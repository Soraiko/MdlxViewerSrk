using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BAR_Editor;

namespace MdlxViewer
{
    public class CatzTim : ImageContainer
    {
        public static readonly IList<string> extensions = new List<string> { "mdlx", "map" }.AsReadOnly();
        private BAR.BARFile barFile;

        /// <summary>
        ///     Signals that some fix was applied to load the image. This makes saving MUCH harder, so don't save these for
        ///     now.
        /// </summary>
        private bool compatFixes = false;
        
        private List<parsedImgData> imgDatas = new List<parsedImgData>();

        public CatzTim(BAR.BARFile bar)
        {
            this.barFile = bar;
        }

        private static byte[] getData(BinaryReader br, long baseP)
        {
            long s, t;
            return getDataRF(br, baseP, out s, out t);
        }

        private static byte[] getDataR(BinaryReader br, long baseP, out long ramOffset)
        {
            long t;
            return getDataRF(br, baseP, out ramOffset, out t);
        }

        private static byte[] getDataF(BinaryReader br, long baseP, out long fileOffset)
        {
            long t;
            return getDataRF(br, baseP, out t, out fileOffset);
        }

        private static byte[] getDataRF(BinaryReader br, long baseP, out long ramOffset, out long fileOffset)
        {
            ulong num = br.ReadUInt64();
            ramOffset = 256u * (((uint)(num >> 0x20)) & 0x3FFFu);
            int bw = ((int)(num >> 0x30)) & 0x3F;
            uint type = ((uint)(num >> 0x38)) & 0x3F;

#if TRACE
            br.BaseStream.Position += 8;
            Trace.Assert((num & 0x3fff) == 0);
            Trace.Assert(((num >> 16) & 0x3f) == 0);
            Trace.Assert(((num >> 0x18) & 0x3f) == 0);
            num = br.ReadUInt64();
            br.BaseStream.Position += 8;
            Trace.Assert((num & 0x7ff) == 0);
            Trace.Assert(((num >> 0x10) & 0x7ff) == 0);
            Trace.Assert(((num >> 0x20) & 0x7ff) == 0);
            Trace.Assert(((num >> 0x30) & 0x7ff) == 0);
            Trace.Assert(((num >> 0x3b) & 3) == 0);
            br.BaseStream.Position += 16;
            Trace.Assert((br.ReadUInt64() & 2) == 0);
            br.BaseStream.Position += 8;
#else
            br.BaseStream.Position += 8 + 16 + 16 + 16;
#endif
            int size = (br.ReadUInt16() & 0x7FFF) << 4;
            br.BaseStream.Position += 0x12L;
            br.BaseStream.Position = fileOffset = baseP + br.ReadInt32();
            var buffer = new byte[size];
            Debug.WriteLine("Reading " + size + " bytes...");
#if DEBUG
            {
                int t = br.BaseStream.Read(buffer, 0, size);
                Debug.WriteLineIf(t != size, "Expected " + size + " bytes, but only got " + t + " bytes!");
            }
#else
            br.BaseStream.Read(buffer, 0, size);
#endif
            Debug.WriteLine("bw = " + bw + "; type = " + type + "; size = " + size);
            size /= 8192;
            switch (type)
            {
                case 0:
                    if (bw > 0)
                    {
                        buffer = Reform.Encode32(buffer, bw, size / bw);
                    }
                    break;
                case 19:
                    bw /= 2;
                    if (bw > 0)
                    {
                        buffer = Reform.Encode8(buffer, bw, size / bw);
                    }
                    break;
                case 20:
                    bw /= 2;
                    if (bw > 0)
                    {
                        buffer = Reform.Encode4(buffer, bw, size / bw);
                    }
                    break;
                default:
                    throw new NotSupportedException("Unknown type: " + type);
            }
            return buffer;
        }

        private static void setData(BinaryReader br, long baseP, byte[] data)
        {
            ulong num = br.ReadUInt64();
            int bw = ((int)(num >> 0x30)) & 0x3F;
            uint type = ((uint)(num >> 0x38)) & 0x3F;
            br.BaseStream.Position += 8 + 16 + 16 + 16;
            int size = (br.ReadUInt16() & 0x7FFF) << 4;
            br.BaseStream.Position += 0x12L;
            br.BaseStream.Position = baseP + br.ReadInt32();

            Debug.WriteLine("writing:bw = " + bw + "; type = " + type + "; size = " + size);
            int ds = size / 8192;
            switch (type)
            {
                case 0:
                    if (bw > 0)
                    {
                        data = Reform.Decode32(data, bw, ds / bw);
                    }
                    break;
                case 19:
                    bw /= 2;
                    if (bw > 0)
                    {
                        data = Reform.Decode8(data, bw, ds / bw);
                    }
                    break;
                case 20:
                    bw /= 2;
                    if (bw > 0)
                    {
                        data = Reform.Decode4(data, bw, ds / bw);
                    }
                    break;
                default:
                    throw new NotSupportedException("Unknown type: " + type);
            }
            Debug.WriteLine("Writing " + size + " bytes...");
            Debug.WriteLineIf(size < data.Length,
                "Expected " + size + " bytes, but got " + data.Length + " bytes; Truncating!");
            Debug.WriteLineIf(size > data.Length, "Expected " + size + " bytes, but only got " + data.Length + " bytes!");
            br.BaseStream.Write(data, 0,
                Math.Min(size, (int)Math.Min(data.Length, br.BaseStream.Length - br.BaseStream.Position)));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private static Bitmap getImage(BinaryReader br, ref byte[] imgData, byte[] palData, long palOffs,
            parsedImgData info, out byte[] unSwizzledPalette)
        {
#if TRACE
            Trace.Assert(br.ReadUInt64() == 0x3fL);
            br.BaseStream.Position += 8;
            Trace.Assert(br.ReadUInt64() == 0x34L);
            br.BaseStream.Position += 8;
            Trace.Assert(br.ReadUInt64() == 0x36L);
            ulong num = br.ReadUInt64();
            Trace.Assert(br.ReadUInt64() == 0x16L);
            Trace.Assert(((num >> 20) & 0x3f) == 0x13);
            Trace.Assert(((num >> 0x33) & 15) == 0);
            Trace.Assert(((num >> 0x37) & 1) == 0);
            Trace.Assert(((num >> 0x38) & 0x1f) == 0);
            Trace.Assert(((num >> 0x3d) & 7) == 4);
            br.BaseStream.Position += 8;
            Trace.Assert(br.ReadUInt64() == 0x14L);
#else
            br.BaseStream.Position += 8 + 16 + 16 + 16 + 16;
#endif
            num = br.ReadUInt64();
#if TRACE
            Trace.Assert(br.ReadUInt64() == 0x06L);
            Trace.Assert(((num >> 0x22) & 1) == 1);
            Trace.Assert(((num >> 0x33) & 15) == 0);
            Trace.Assert(((num >> 0x37) & 1) == 0);
            Trace.Assert(((num >> 0x3d) & 7) == 0);
            br.BaseStream.Position += 8;
            Trace.Assert(br.ReadUInt64() == 8L);
#else
    //br.BaseStream.Position += 8 + 16;
#endif
            info.type = (uint)(num >> 20) & 0x3fu;
            if (info.type != 19 && info.type != 20)
            {
                throw new NotSupportedException("Unknown t0PSM: " + info.type);
            }
            info.palOffs = (256 * ((uint)(num >> 0x25) & 0x3FFFu)) - palOffs;
            info.width = (ushort)(1u << ((int)(num >> 0x1A) & 0x0F));
            info.height = (ushort)(1u << ((int)(num >> 0x1E) & 0x0F));
            info.palCs = (byte)((uint)(num >> 0x38) & 0x1Fu);
            var palette = new byte[1024];
            if (info.palOffs < 0)
            {
                throw new NotSupportedException("Image palette located before block address.");
            }
            if (info.palOffs + palette.Length > palData.Length)
            {
                throw new NotSupportedException("Image palette located after block address.");
            }
            Array.Copy(palData, info.palOffs, palette, 0, palette.Length);
            int size = info.width * info.height;
            if (info.type == 20)
            {
                size /= 2;
            }
            if (imgData.Length < size)
            {
                Debug.WriteLine("Expected size = " + size + "; got = " + imgData.Length);
                Array.Resize(ref imgData, size);
            }
            return TexUt2.Decode(imgData, palette, info.type, info.width, info.height, out unSwizzledPalette, info.palCs);
        }

        private void ParseTIM(BinaryReader br, long baseP, int barPos)
        {
            br.BaseStream.Position = baseP + 8 + 4;
            int count = br.ReadInt32();
            int offsetTableOff = br.ReadInt32();
            int dataOff = br.ReadInt32();
            int mkImgOff = br.ReadInt32();

            br.BaseStream.Position = baseP + offsetTableOff;
            var offs = new int[count];
            for (int i = 0; i < count; ++i)
            {
                offs[i] = br.ReadByte();
            }

            long palOffs, palPos = br.BaseStream.Position = baseP + dataOff + 32;
            byte[] palData = getDataR(br, baseP, out palOffs);

            for (int i = 0; i < count; ++i)
            {
                var info = new parsedImgData
                {
                    barFile = barPos,
                    baseP = baseP,
                    palBOffs = palPos
                };

                br.BaseStream.Position = info.imgBOffs = baseP + dataOff + 32 + 144 + (144 * offs[i]);
                byte[] imgData = getData(br, baseP);

                br.BaseStream.Position = baseP + mkImgOff + 32 + (160 * i) + 8;
                byte[] palette = new byte[1024];

                Bitmap img = getImage(br, ref imgData, palData, palOffs, info, out palette);

                bmps.Add(Convert(img));
                Palettes.Add(new Color[256]);

                for (int c = 0; c < 256; c++)
                {
                    int alpha = palette[c * 4 + 3] * 2;
                    if (alpha > 255)
                        alpha = 255;

                    Palettes[Palettes.Count - 1][c] = Color.FromArgb((byte)alpha, palette[c * 4], palette[c * 4 + 1], palette[c * 4 + 2]);
                }
                imgDatas.Add(info);
            }

            /*br.BaseStream.Position = 0;
            
            for (int dmyReach = 0; dmyReach+16 < br.BaseStream.Length; dmyReach += 16)
            {
                br.BaseStream.Position = dmyReach;

                if (br.ReadUInt32() == 0x594D445F)
                {
                    br.BaseStream.Position = dmyReach + 0x0C;
                    int nextPatch = br.ReadInt32();

                    br.BaseStream.Position = dmyReach + 0x12;
                    int patchPalette = br.ReadUInt16();

                    br.BaseStream.Position = dmyReach + 0x1E;
                    int patchCount = br.ReadUInt16();

                    br.BaseStream.Position = dmyReach + 0x20;
                    int patchX = br.ReadUInt16();
                    int patchY = br.ReadUInt16();
                    int patchWidth = br.ReadUInt16();
                    int patchHeight = br.ReadUInt16();

                    br.BaseStream.Position = dmyReach + 0x30;
                    int patchStart = br.ReadInt32();

                    nextPatch += 16 + dmyReach;
                    patchStart += 16 + dmyReach;

                    Bitmap patch = new Bitmap(patchWidth, patchHeight * patchCount);
                    int patchSize = patchWidth * patchHeight * patchCount;
                    byte[] patchPixels = new byte[patchSize];

                    br.BaseStream.Position = patchStart;
                    br.BaseStream.Read(patchPixels, 0, patchPixels.Length);
                    
                    for (int of = 0; of < patchSize; of++)
                    {
                        patch.SetPixel(of % patchWidth, of / patchWidth, Palettes[patchPalette][patchPixels[of]]);
                    }

                    Patches.Add(patch);
                    PatchesCounts.Add(patchCount);
                    PatchesSizes.Add(new Size(patchWidth, patchHeight));
                    PatchesPositions.Add(new Point(patchX, patchY));
                    PatchesDestinations.Add(patchPalette);
                }
            }*/
        }

        public static Bitmap Convert(Bitmap oldbmp)
        {
            Bitmap newbmp = new Bitmap(oldbmp.Width, oldbmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics gr = Graphics.FromImage(newbmp);

            gr.PageUnit = GraphicsUnit.Pixel;
            gr.DrawImageUnscaled(oldbmp, 0, 0);

            return newbmp;
        }


        private void parseTexture(BAR.BARFile file, int barPos)
        {
            using (var br = new BinaryReader(new MemoryStream(file.data, false)))
            {
                int v0 = br.ReadInt32();
                if (v0 == 0)
                {
                    ParseTIM(br, 0, barPos);
                }
                else if (v0 == -1)
                {
                    v0 = br.ReadInt32();
                    for (int j = 0; j < v0; j++)
                    {
                        long lPos = br.BaseStream.Position + 4;
                        ParseTIM(br, br.ReadInt32(), barPos);
                        br.BaseStream.Position = lPos;
                    }
                }
                else
                {
                    throw new NotSupportedException("Unknown v0: " + v0);
                }
            }
        }

        public override void parse()
        {
            if (bmps.Count != 0)
            {
                foreach (Bitmap bmp in bmps)
                {
                    bmp.Dispose();
                }
                bmps.Clear();
            }
            parseTexture(this.barFile, 0);
            /*for (int i = 0; i < BAR.fileList.Count; ++i)
            {
                BAR.BARFile f = BAR.fileList[i];
                if (f.type == 7)
                {
                    Debug.WriteLine("BAR file \"" + f.id + "\" is textures");
                }
            }*/
        }

        protected override void setBMPInternal(int index, ref Bitmap bmp)
        {
            parsedImgData imgInfo = imgDatas[index];
            if (bmp.Width != imgInfo.width || bmp.Height != imgInfo.height)
            {
                throw new NotSupportedException("New image has different dimensions");
            }
            {
                PixelFormat pf;
                switch (imgInfo.type)
                {
                    case 19:
                        pf = PixelFormat.Format8bppIndexed;
                        break;
                    case 20:
                        pf = PixelFormat.Format4bppIndexed;
                        break;
                    default:
                        throw new NotSupportedException("Unsupported type");
                }
                if (bmp.PixelFormat != pf)
                {
                    requestQuantize(ref bmp, pf);
                }
            }
            /*Get the BAR subfile*/
            byte[] barFile = this.barFile.data;
            using (var br = new BinaryReader(new MemoryStream(barFile, true)))
            {
                br.BaseStream.Position = imgInfo.palBOffs;
                byte[] palData = getData(br, imgInfo.baseP), imgData;
                {
                    var palette = new byte[1024];
                    Array.Copy(palData, imgInfo.palOffs, palette, 0, palette.Length);
                    imgData = TexUt2.Encode(bmp, ref palette, imgInfo.palCs);
                    Array.Copy(palette, 0, palData, imgInfo.palOffs, palette.Length);
                }
                br.BaseStream.Position = imgInfo.palBOffs;
                setData(br, imgInfo.baseP, palData);
                br.BaseStream.Position = imgInfo.imgBOffs;
                setData(br, imgInfo.baseP, imgData);
            }
            this.barFile.data = barFile;
        }

        private class parsedImgData
        {
            /// <summary>Index of the file containing this image</summary>
            public int barFile;

            public long baseP;

            /// <summary>Image height</summary>
            public ushort height;

            /// <summary>Image block offset</summary>
            public long imgBOffs;

            /// <summary>Palette block offset</summary>
            public long palBOffs;

            /// <summary>Palette CSA (minor offset)</summary>
            public byte palCs;

            /// <summary>Offset inside the palette block</summary>
            public long palOffs;

            /// <summary>Image type (19=8bit; 20=4bit)</summary>
            public uint type;

            /// <summary>Image width</summary>
            public ushort width;
        }
    }
}
