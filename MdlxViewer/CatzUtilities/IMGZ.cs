using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace MdlxViewer
{
    public class IMGZ : ImageContainer
    {
        public static readonly IList<string> extensions = new List<string> {"imz", "imd"}.AsReadOnly();

        private readonly int internalCount;
        private readonly uint magic;
        private BinaryStream file;

        public IMGZ(Stream file)
        {
            if (!file.CanRead || !file.CanSeek)
            {
                throw new NotSupportedException("Cannot read or seek in stream");
            }
            this.file = new BinaryStream(file);
            switch (magic = this.file.ReadUInt32())
            {
                case 0x5A474D49: //IMGZ
#if DEBUG
                    Debug.WriteLine(String.Format("num0: {0}\nnum1: {1}", this.file.ReadUInt32(), this.file.ReadUInt32()));
#else
                        file.Position += 8;
#endif
                    internalCount = this.file.ReadInt32();
                    Debug.Write("IMGZ ");
                    break;
                case 0x44474D49: //IMGD
                    internalCount = 1;
                    Debug.Write("IMGD ");
                    break;
                default:
                    throw new InvalidDataException("Invalid signature");
            }
            Debug.WriteLine("has " + internalCount + " items");
        }

        private long getIMGDOffset(int index = 0)
        {
            switch (magic)
            {
                case 0x5A474D49: //IMGZ
                    if (index < 0 || index >= internalCount)
                    {
                        return -1;
                    }
                    file.Seek(16 + 8*index, SeekOrigin.Begin);
                    return file.ReadUInt32();
                case 0x44474D49: //IMGD
                    return 0;
                default:
                    throw new NotSupportedException("Invalid signature.");
            }
        }

        private void parseD(long pos, long len, int index)
        {
            if (pos + len > file.BaseStream.Length)
            {
                throw new IndexOutOfRangeException("IMGD goes past file bounds");
            }
            file.Seek(pos, SeekOrigin.Begin);
            if (file.ReadUInt32() != 0x44474D49)
            {
                throw new InvalidDataException("IMGD has bad signature");
            }
#if DEBUG
            uint pixelOffset, pixelLength, paletteOffset, paletteLength;
            ushort width, height, type;
            byte encode;
            Debug.WriteLine(
                String.Format(
                    "---IMGD---\nN0: {0}\npixelOffset: {1}\npixelLength: {2}\npaletteOffset: {3}\npaletteLength: {4}\nN5: {5}\nN6: {6}\nwidth: {7}\nheight: {8}\nN9: {9}\nN10: {10}\ntype: {11}\nN12: {12}\nN13: {13}\nN14: {14}\nN15: {15}\nN16: {16}\nencode: {17}\n---IMGD End---",
                    file.ReadUInt32(),
                    pixelOffset = file.ReadUInt32(),
                    pixelLength = file.ReadUInt32(),
                    paletteOffset = file.ReadUInt32(),
                    paletteLength = file.ReadUInt32(),
                    file.ReadUInt16(),
                    file.ReadUInt16(),
                    width = file.ReadUInt16(),
                    height = file.ReadUInt16(),
                    file.ReadUInt32(),
                    file.ReadUInt16(),
                    type = file.ReadUInt16(),
                    file.ReadUInt32(),
                    file.ReadUInt32(),
                    file.ReadUInt32(),
                    file.ReadUInt32(),
                    file.ReadUInt32(),
                    encode = file.ReadByte()
                    ));
#else
            this.file.Seek(4, SeekOrigin.Current);
            uint pixelOffset = this.file.ReadUInt32(),
                pixelLength = this.file.ReadUInt32(),
                paletteOffset = this.file.ReadUInt32(),
                paletteLength = this.file.ReadUInt32();
            this.file.Seek(4, SeekOrigin.Current);
            ushort width = this.file.ReadUInt16(),
                height = this.file.ReadUInt16();
            this.file.Seek(6, SeekOrigin.Current);
            ushort type = this.file.ReadUInt16();
            this.file.Seek(20, SeekOrigin.Current);
            byte encode = this.file.ReadByte();
#endif
            if (pixelOffset + pixelLength > len)
            {
                throw new IndexOutOfRangeException("IMGD pixel data goes past file bounds");
            }
            if (paletteOffset + paletteLength > len)
            {
                throw new IndexOutOfRangeException("IMGD palette data goes past file bounds");
            }

            PixelFormat pf;
            switch (type)
            {
                case 19:
                    pf = PixelFormat.Format8bppIndexed;
                    break;
                case 20:
                    pf = PixelFormat.Format4bppIndexed;
                    break;
                default:
                    throw new NotSupportedException("Unsupported IMGD type");
            }
            var bmp = new Bitmap(width, height, pf);
            {
                file.Seek(pos + pixelOffset, SeekOrigin.Begin);
                byte[] buffer = file.ReadBytes((int) pixelLength);
                switch (pf)
                {
                    case PixelFormat.Format8bppIndexed:
                        if (encode == 7)
                        {
                            buffer = Reform.Decode8(Reform.Encode32(buffer, width/128, height/64), width/128, height/64);
                        }
                        break;
                    case PixelFormat.Format4bppIndexed:
                        if (encode == 7)
                        {
                            buffer = Reform.Decode4(Reform.Encode32(buffer, width/128, height/128), width/128,
                                height/128);
                        }
                        else
                        {
                            Reform.swapHLUT(buffer);
                        }
                        break;
                }
                BitmapData pix = bmp.LockBits(Rectangle.FromLTRB(0, 0, width, height), ImageLockMode.WriteOnly, pf);
                try
                {
                    Marshal.Copy(buffer, 0, pix.Scan0, (int) pixelLength);
                }
                finally
                {
                    bmp.UnlockBits(pix);
                }
            }
            {
                file.Seek(pos + paletteOffset, SeekOrigin.Begin);
                byte[] buffer = file.ReadBytes((int) paletteLength);
                ColorPalette palette = bmp.Palette;
                //Because of rounding, when reading values back use Math.Ceiling(input/2)
                switch (pf)
                {
                    case PixelFormat.Format8bppIndexed:
                        for (int i = 0; i < 256; i++)
                        {
                            palette.Entries[Reform.paletteSwap34(i)] = Color.FromArgb(
                                Math.Min(buffer[(i*4) + 3]*2, 255), buffer[i*4], buffer[(i*4) + 1], buffer[(i*4) + 2]);
                            Debug.WriteLineIf(buffer[(i*4) + 3] > 128,
                                "Transparency before transform is over 128: " + buffer[(i*4) + 3]);
                        }
                        break;
                    case PixelFormat.Format4bppIndexed:
                        for (int i = 0; i < 16; i++)
                        {
                            palette.Entries[i] = Color.FromArgb(Math.Min(buffer[(i*4) + 3]*2, 255), buffer[i*4],
                                buffer[(i*4) + 1], buffer[(i*4) + 2]);
                            Debug.WriteLineIf(buffer[(i*4) + 3] > 128,
                                "Transparency before transform is over 128: " + buffer[(i*4) + 3]);
                        }
                        break;
                }
                bmp.Palette = palette;
            }
            bmps.Add(bmp);
        }

        protected override void Dispose(bool disposing)
        {
            file.Dispose();
            base.Dispose(disposing);
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
            bmps.Capacity = internalCount;
            switch (magic)
            {
                case 0x5A474D49: //IMGZ
                    file.Seek(16, SeekOrigin.Begin);
                    for (int i = 0, j = internalCount; i < j; i++)
                    {
                        long lpos = file.Tell() + 8;
                        parseD(file.ReadUInt32(), file.ReadUInt32(), i);
                        file.Seek(lpos, SeekOrigin.Begin);
                    }
                    break;
                case 0x44474D49: //IMGD
                    parseD(0, file.BaseStream.Length, 0);
                    break;
                default:
                    throw new NotSupportedException("Invalid signature");
            }
        }

        protected override void setBMPInternal(int index, ref Bitmap bmp)
        {
            if (!file.CanWrite)
            {
                throw new NotSupportedException("Stream is readonly.");
            }
            long pos = getIMGDOffset(index);
            file.Seek(pos, SeekOrigin.Begin);
            if (file.ReadUInt32() != 0x44474D49)
            {
                throw new InvalidDataException("IMGD has bad signature.");
            }
            file.Seek(4, SeekOrigin.Current);
            uint pixelOffset = file.ReadUInt32(),
                pixelLength = file.ReadUInt32(),
                paletteOffset = file.ReadUInt32(),
                paletteLength = file.ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            ushort width = file.ReadUInt16(),
                height = file.ReadUInt16();
            file.Seek(6, SeekOrigin.Current);
            ushort type = file.ReadUInt16();
            file.Seek(20, SeekOrigin.Current);
            byte encode = file.ReadByte();

            if (bmp.Width != width || bmp.Height != height)
            {
                throw new NotSupportedException("New image has different dimensions.");
            }
            PixelFormat pf;
            switch (type)
            {
                case 19:
                    pf = PixelFormat.Format8bppIndexed;
                    break;
                case 20:
                    pf = PixelFormat.Format4bppIndexed;
                    break;
                default:
                    throw new NotSupportedException("Unsupported IMGD type");
            }
            if (bmp.PixelFormat != pf)
            {
                requestQuantize(ref bmp, pf);
            }
            {
                BitmapData pix = bmp.LockBits(Rectangle.FromLTRB(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    pf);
                var buffer = new byte[pixelLength];
                try
                {
                    Marshal.Copy(pix.Scan0, buffer, 0, (int) pixelLength);
                }
                finally
                {
                    bmp.UnlockBits(pix);
                }
                switch (pf)
                {
                    case PixelFormat.Format8bppIndexed:
                        if (encode == 7)
                        {
                            buffer = Reform.Decode32(Reform.Encode8(buffer, width/128, height/64), width/128, height/64);
                        }
                        break;
                    case PixelFormat.Format4bppIndexed:
                        if (encode == 7)
                        {
                            buffer = Reform.Decode32(Reform.Encode4(buffer, width/128, height/128), width/128,
                                height/128);
                        }
                        else
                        {
                            Reform.swapHLUT(buffer);
                        }
                        break;
                }
                file.Seek(pos + pixelOffset, SeekOrigin.Begin);
                file.Write(buffer, 0, (int) pixelLength);
            }
            {
                ColorPalette palette = bmp.Palette;
                var buffer = new byte[paletteLength];
                switch (pf)
                {
                    case PixelFormat.Format8bppIndexed:
                        for (int i = 0; i < 256; i++)
                        {
                            var argb = (uint) palette.Entries[Reform.paletteSwap34(i)].ToArgb();
                            buffer[(i*4) + 3] = (byte) Math.Ceiling((double) (argb >> 24)/2);
                            buffer[i*4] = (byte) (argb >> 16);
                            buffer[i*4 + 1] = (byte) (argb >> 8);
                            buffer[i*4 + 2] = (byte) argb;
                        }
                        break;
                    case PixelFormat.Format4bppIndexed:
                        for (int i = 0; i < 16; i++)
                        {
                            var argb = (uint) palette.Entries[i].ToArgb();
                            buffer[(i*4) + 3] = (byte) Math.Ceiling((double) (argb >> 24)/2);
                            buffer[i*4] = (byte) (argb >> 16);
                            buffer[i*4 + 1] = (byte) (argb >> 8);
                            buffer[i*4 + 2] = (byte) argb;
                        }
                        break;
                }
                file.Seek(pos + paletteOffset, SeekOrigin.Begin);
                file.Write(buffer, 0, (int) paletteLength);
            }
        }
    }
}