using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using BAR_Editor;

namespace MdlxViewer
{
    public class SrkTim
    {
        private BAR.BARFile barFile;

        SrkBinary timBinary;

        public int dmyCount = 0;
        public int DMY = 0;
        public List<System.Drawing.Color[]> Palettes;
        public List<Bitmap> Bitmaps;
        public List<int> textureAddresses;
        public List<int> paletteAddresses;
        
        public SrkTim(BAR.BARFile b)
        {
            this.barFile = b;
            this.timBinary = new SrkBinary(ref this.barFile.data);

            this.DisplayTextures = new Microsoft.Xna.Framework.Graphics.Texture2D[0];
            this.Palettes = new List<System.Drawing.Color[]>(0);
            this.Bitmaps = new List<Bitmap>(0);
            this.textureAddresses = new List<int>(0);
            this.paletteAddresses = new List<int>(0);
            this.Patches = new List<Bitmap>(0);
            this.PatchIndexes = new List<int>(0);
            this.PatchTextureIndexes = new List<int>(0);
            this.PatchCounts = new List<int>(0);
            this.PatchSizes = new List<Size>(0);
            this.PatchLocs = new List<Point>(0);
        }
        

        public static Bitmap Convert(Bitmap oldbmp)
        {
            Bitmap newbmp = new Bitmap(oldbmp.Width, oldbmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics gr = Graphics.FromImage(newbmp);

            gr.PageUnit = GraphicsUnit.Pixel;
            gr.DrawImageUnscaled(oldbmp, 0, 0);

            return newbmp;
        }


        public void ParseTim()
        {
            Palettes.Clear();
            Bitmaps.Clear();
            textureAddresses.Clear();
            paletteAddresses.Clear();


            int max = timBinary.ReadInt(12,false);
            int startPalette =  + timBinary.ReadInt(timBinary.ReadUInt(0x14, false) + 0x74, false);
            int[] offs = new int[] {
                0x0000,
                0x0040,
                0x1000,
                0x1040,

                0x0080,
                0x00C0,
                0x1080,
                0x10C0};

            for (int i = 0; i < max; i++)
            {
                byte iI = timBinary.Buffer[timBinary.ReadUInt(0x10, false) + i];
                ulong num = timBinary.ReadUInt64(timBinary.ReadUInt(0x18, false) + 0xA0 * i + 0x70, false);
                int paletteOffset = startPalette + offs[i % 8] + ((i / 8) * 0x2000);

                System.Drawing.Color[] paletteSwap = GetPalette((paletteOffset));
                paletteAddresses.Insert(paletteAddresses.Count, paletteOffset);
                Palettes.Insert(Palettes.Count, paletteSwap);
                textureAddresses.Insert(textureAddresses.Count,  + timBinary.ReadInt( + timBinary.ReadInt(0x14, false) + 0x104 + 0x90 * iI, false));
                int wByte = 2 * timBinary.ReadInt(timBinary.ReadInt( 0x14, false) + 0xD0 + 0x90 * iI, false);
                int hByte = 2 * timBinary.ReadInt(timBinary.ReadInt( 0x14, false) + 0xD4 + 0x90 * iI, false);
                ushort textureWidth = (ushort)(1u << ((int)(num >> 0x1A) & 0x0F));
                ushort textureHeight = (ushort)(1u << ((int)(num >> 0x1E) & 0x0F));


                //ushort textureWidth = (ushort)(2 * timBinary.ReadUShort(timBinary.ReadUInt(0x14) + 0x40 + 0x90 * (iI + 0)));
                //ushort textureHeight = (ushort)(2 * timBinary.ReadUShort(timBinary.ReadUInt(0x14) + 0x44 + 0x90 * (iI + 0)));

                byte palCs = (byte)((uint)(num << 0x41) & 0x1Fu);
                uint type = (uint)(num >> 20) & 0x3fu;
                long pixelsPerByte = (textureWidth * textureHeight / (wByte * hByte));

                if (pixelsPerByte!=1)
                {
                    return;
                }
                
                Bitmaps.Add(GetTextureBitmap(textureAddresses[textureAddresses.Count - 1], Palettes[textureAddresses.Count - 1], textureWidth, textureHeight));

            }
            this.Patches = new List<Bitmap>(0);
            this.PatchIndexes = new List<int>(0);
            this.PatchTextureIndexes = new List<int>(0);
            this.PatchCounts = new List<int>(0);
            this.PatchSizes = new List<Size>(0);
            this.PatchLocs = new List<Point>(0);


            dmyCount = 0;
            int DMYReach = startPalette;
            DMYReach = 16 * (DMYReach / 16);
            while (BitConverter.ToInt32(timBinary.Buffer, DMYReach) != 0x594D445F)
            {
                DMYReach += 16;
                if (DMYReach + 16 >= timBinary.Buffer.Length)
                {
                    GetDisplayTextures();
                    return;
                }
            }
            DMY = DMYReach;


            while (DMYReach + 16 <  + timBinary.Buffer.Length && BitConverter.ToInt32(timBinary.Buffer, DMYReach) == 0x594D445F)
            {
                int next = DMYReach + 16 + timBinary.ReadInt(DMYReach + 12,false);
                int startPatch = DMYReach + 16 + timBinary.ReadInt(DMYReach + 48, false);
                ushort maxPatch = timBinary.ReadUShort(DMYReach + 0x1E, false);

                ushort patchX = timBinary.ReadUShort(DMYReach + 0x20, false);
                ushort patchY = timBinary.ReadUShort(DMYReach + 0x22, false);

                ushort patchWidth = timBinary.ReadUShort(DMYReach + 0x24, false);
                ushort patchHeight = (ushort)(timBinary.ReadUShort(DMYReach + 0x26, false) * maxPatch);

                    if (patchWidth <1 || patchHeight <1 || patchWidth > 512 || patchHeight > 512 || startPatch + patchWidth * patchHeight >  + timBinary.Buffer.Length)
                        break;

                short paletteIndex = timBinary.ReadShort(DMYReach + 0x12, false);

                dmyCount++;
                this.textureAddresses.Insert(textureAddresses.Count, (startPatch));
                System.Drawing.Color[][] currPal = Palettes.ToArray();
                this.Palettes.Insert(Palettes.Count, currPal[paletteIndex]);
                this.Bitmaps.Insert(Bitmaps.Count, GetPatchBitmap(textureAddresses[textureAddresses.Count - 1], Palettes[Palettes.Count - 1], patchWidth, patchHeight));

                this.Patches.Insert(this.Patches.Count, this.Bitmaps[this.Bitmaps.Count - 1]);
                this.PatchIndexes.Insert(this.PatchIndexes.Count, -1);

                this.PatchTextureIndexes.Insert(this.PatchTextureIndexes.Count, paletteIndex);
                this.PatchSizes.Insert(this.PatchSizes.Count, new Size(patchWidth, timBinary.ReadUShort(DMYReach + 0x26, false)));
                this.PatchCounts.Insert(this.PatchCounts.Count, maxPatch);
                this.PatchLocs.Insert(this.PatchLocs.Count, new Point(patchX, patchY));
                this.paletteAddresses.Insert(paletteAddresses.Count, paletteAddresses[paletteIndex]);
                DMYReach = next;
            }
            
            GetDisplayTextures();
        }

        public static string tempPath = System.IO.Path.GetTempPath() + @"\";


        public void UpdateBitmaps(string name)
        {
            if (Directory.Exists(tempPath+"Okiaros"))
                foreach (string currFile in Directory.GetFiles(tempPath + "Okiaros"))
                    File.Delete(currFile);
            else
                Directory.CreateDirectory(tempPath + "Okiaros");

            string[] pngs = new string[Bitmaps.Count];
            for (int i = 0; i < pngs.Length; i++)
                pngs[i] = name + @"\texture-" + i.ToString("d3") + ".png";

            for (int i = 0; i < Bitmaps.Count; i++)
            {
                FileStream cFS = new FileStream(pngs[i], System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                Bitmaps[i] = new Bitmap(cFS);
                cFS.Close();
            }

            var palettes = new List<string>(0);
            var toQuant = new List<Bitmap>(0);
            var filenames = new List<string>(0);

            for (int i = 0; i < paletteAddresses.Count; i++)
            {
                if (!palettes.Contains(paletteAddresses[i].ToString("X")))
                {
                    toQuant.Add(Bitmaps[i]);
                    palettes.Add(paletteAddresses[i].ToString("X"));
                    filenames.Add(paletteAddresses[i].ToString() + "@" + textureAddresses[i].ToString() + ",0,0," + Bitmaps[i].Width.ToString() + "," + Bitmaps[i].Height.ToString());
                }
                else
                    for (int k = 0; k < paletteAddresses.Count; k++)
                        if (paletteAddresses[i].ToString() == paletteAddresses[k].ToString())
                        {
                            int wi = toQuant[k].Width;
                            int he = toQuant[k].Height;
                            if (Bitmaps[i].Height > he)
                                he = Bitmaps[i].Height;
                            wi = wi + Bitmaps[i].Width;
                            var merge = new Bitmap(wi, he);
                            var mergeGr = Graphics.FromImage(merge);
                            mergeGr.DrawImage(toQuant[k], new Rectangle(0, 0, toQuant[k].Width, toQuant[k].Height));
                            mergeGr.DrawImage(Bitmaps[i], new Rectangle(toQuant[k].Width, 0, Bitmaps[i].Width, Bitmaps[i].Height));
                            filenames[k] = filenames[k] + "@" + textureAddresses[i].ToString() + "," + toQuant[k].Width.ToString() + ",0," + Bitmaps[i].Width.ToString() + "," + Bitmaps[i].Height.ToString();
                            toQuant[k] = merge;
                            k = paletteAddresses.Count;
                        }
            }

            pngs = Directory.GetFiles(tempPath + "Okiaros", "*.png");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "pngquant.exe";
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.Arguments = "pngquant.exe ";
            string[] toQuantFilenames = new string[toQuant.Count];

            for (int i = 0; i < toQuant.Count; i++)
            {
                toQuantFilenames[i] = tempPath + @"Okiaros\" + filenames[i];
                startInfo.Arguments += "\"" + toQuantFilenames[i] + ".png" + "\" ";
                toQuant[i].Save(toQuantFilenames[i] + ".png");
            }

            Process.Start(startInfo);
            while (Process.GetProcessesByName("pngquant").Length > 0) { }

            for (int i = 0; i < toQuantFilenames.Length; i++)
            {
                File.Delete(toQuantFilenames[i] + ".png");
                toQuantFilenames[i] += "-fs8.png";
            }

            for (int g = 0; g < toQuantFilenames.Length; g++)
            {
                string curr = toQuantFilenames[g];
                string filename_ = Path.GetFileName(curr).Split('-')[0];
                string[] bounds = filename_.Split('@');
                int newPaletteAddress = (int)(int.Parse(bounds[0]));
                var stream = new System.IO.FileStream(curr, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                var bitm2Palette = new Bitmap(stream);
                var decoder = new PngBitmapDecoder(stream,
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                var bitmapSource = decoder.Frames[0];
                var palette = new List<System.Windows.Media.Color>(bitmapSource.Palette.Colors);
                while (palette.Count < 256)
                    palette.Add(System.Windows.Media.Colors.Black);

                /*if (checkBox1.Checked)
				palette.Sort((System.Windows.Media.Color left, System.Windows.Media.Color right) => (BrightNess(left)).CompareTo(BrightNess(right)));*/

                for (int c = 0; c < 256; c++)
                {
                    int colorSlot = ((64 * ((c / 8) % 2) + ((c / 32) * 128) + (8 * ((c / 16) % 2)) + (c % 8)) * 4);
                    if (colorSlot < timBinary.Buffer.Length - 3)
                    {
                        timBinary.Buffer[newPaletteAddress + colorSlot] = (byte)palette[c].R;
                        timBinary.Buffer[newPaletteAddress + colorSlot + 1] = (byte)palette[c].G;
                        timBinary.Buffer[newPaletteAddress + colorSlot + 2] = (byte)palette[c].B;
                        timBinary.Buffer[newPaletteAddress + colorSlot + 3] = (byte)((palette[c].A + 1) / 2);
                    }


                }

                for (int i = 1; i < bounds.Length; i++)
                {
                    string[] splited = bounds[i].Split(',');
                    int pixAddress = (int)(int.Parse(splited[0]));
                    Bitmap CropBitmap = bitm2Palette.Clone(new Rectangle(int.Parse(splited[1]), int.Parse(splited[2]), int.Parse(splited[3]), int.Parse(splited[4])), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    LockBitmap CropBitmapLK = new LockBitmap(CropBitmap);
                    CropBitmapLK.LockBits();
                    if (dmyCount > 0 && pixAddress >= DMY)
                    {
                        for (ushort x = 0; x < CropBitmap.Width; x++)
                            for (ushort y = 0; y < CropBitmap.Height; y++)
                            {
                                System.Windows.Media.Color currCol = CropBitmapLK.GetPixel(x, y);
                                int pos = pixAddress + (y * CropBitmap.Width) + x;
                                if (pos > timBinary.Buffer.Length - 1)
                                    pos = timBinary.Buffer.Length - 1;
                                timBinary.Buffer[pos] = (byte)palette.IndexOf(currCol); //433
                            }
                    }
                    else
                    {
                        int textW2 = (CropBitmap.Width * 2);
                        int textW4 = (CropBitmap.Width * 4);
                        for (int x = 0; x < CropBitmap.Width; x++)
                        {
                            ushort xMOD16 = (ushort)(x % 16);
                            ushort xMOD8 = (ushort)(x % 8);
                            ushort xOVER16 = (ushort)(x / 16);
                            for (int y = 0; y < CropBitmap.Height; y++)
                            {
                                System.Windows.Media.Color currCol = CropBitmapLK.GetPixel(x, y);
                                ushort yOVER4 = (ushort)(y / 4);
                                ushort yMOD2 = (ushort)(y % 2);
                                ushort yMOD4 = (ushort)(y % 4);
                                ushort yMOD8 = (ushort)(y % 8);
                                int offset = 32 * (xOVER16) + (4 * (xMOD16)) - (30 * ((xMOD16) >> 3)) + ((System.Byte)(y & 1)) * textW2 + ((System.Byte)(y >> 2)) * textW4 + ((((System.Byte)(y & 3)) >> 1) + (((xMOD8) >> 2) * -1 + (1 - ((xMOD8) >> 2))) * 16 * (((System.Byte)(y & 3)) >> 1) + (((xMOD8) >> 2) * -1 + (1 - ((xMOD8) >> 2))) * 16 * (((System.Byte)(y & 7)) >> 2)) * (1 - (((System.Byte)(y & 7)) >> 2) * (((System.Byte)(y & 3)) >> 1)) + (((System.Byte)(y & 7)) >> 2) * (((System.Byte)(y & 3)) >> 1);
                                if (pixAddress + offset< timBinary.Buffer.Length)
                                timBinary.Buffer[pixAddress + offset] = (byte)palette.IndexOf(currCol);
                            }
                        }
                    }
                }
                stream.Close();
                stream = null;
                decoder = null;
                bitmapSource = null;
                palette = null;
                toQuant = null;
                filenames = null;
            }
        }

        public List<Bitmap> Patches { get; set; }
        public List<int> PatchTextureIndexes;
        public List<int> PatchIndexes { get; set; }
        public List<int> PatchCounts { get; set; }
        public List<Size> PatchSizes { get; set; }
        public List<Point> PatchLocs { get; set; }
        public Bitmap InOneTextureBMP;
        public Point[] InOneTextureLocs;
        public Size[] InOneTextureSizes;

        public Microsoft.Xna.Framework.Graphics.Texture2D[] DisplayTextures;
        public Bitmap[] EmptyPatches;

        public void GetDisplayTextures()
        {
            if (this.Bitmaps.Count == 0)
                return;
            if (this.DisplayTextures.Length == 0)
            {
                this.DisplayTextures = new Microsoft.Xna.Framework.Graphics.Texture2D[Bitmaps.Count - Patches.Count];
                this.EmptyPatches = new Bitmap[Patches.Count];
            }

            for (int i = 0; i < this.EmptyPatches.Length; i++)
            {
                Bitmap img = this.Bitmaps[this.PatchTextureIndexes[i]].Clone(new Rectangle(0, 0, this.Bitmaps[this.PatchTextureIndexes[i]].Width, this.Bitmaps[this.PatchTextureIndexes[i]].Height), this.Bitmaps[this.PatchTextureIndexes[i]].PixelFormat);
                if (this.PatchIndexes[i] > -1)
                {
                    Bitmap img2 = new Bitmap(img.Width, img.Height);
                    Graphics img2Gr = Graphics.FromImage(img2);

                    img2Gr.DrawImage(this.Patches[i], new Rectangle(this.PatchLocs[i].X, this.PatchLocs[i].Y, this.PatchSizes[i].Width, this.PatchSizes[i].Height),
                    new Rectangle(0, this.PatchSizes[i].Height * this.PatchIndexes[i], this.PatchSizes[i].Width, this.PatchSizes[i].Height), GraphicsUnit.Pixel);

                    Rectangle rect;
                    if (this.PatchLocs[i].X > 0)
                    {
                        rect = new Rectangle(0, 0, this.PatchLocs[i].X, img.Height);
                        img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                    }
                    if (this.PatchLocs[i].Y > 0)
                    {
                        rect = new Rectangle(0, 0, img.Width, this.PatchLocs[i].Y);
                        img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                    }
                    if (this.PatchLocs[i].X + this.PatchSizes[i].Width < img.Width)
                    {
                        rect = new Rectangle(this.PatchLocs[i].X + this.PatchSizes[i].Width, 0, img.Width - this.PatchLocs[i].X - this.PatchSizes[i].Width, img.Height);
                        img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                    }
                    if (this.PatchLocs[i].Y + this.PatchSizes[i].Height < img.Height)
                    {
                        rect = new Rectangle(0, this.PatchLocs[i].Y + this.PatchSizes[i].Height, img.Width, img.Height - this.PatchLocs[i].Y - this.PatchSizes[i].Height);
                        img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                    }
                    img = img2;
                }

                Graphics grPatch = Graphics.FromImage(img);

                grPatch.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), new Rectangle(0, 0, img.Width, img.Height - 1));

                grPatch.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 100, 100)), new Rectangle(this.PatchLocs[i].X, this.PatchLocs[i].Y, this.PatchSizes[i].Width, this.PatchSizes[i].Height - 1));
                grPatch.DrawRectangle(new Pen(Color.Red, 1), new Rectangle(this.PatchLocs[i].X, this.PatchLocs[i].Y, this.PatchSizes[i].Width, this.PatchSizes[i].Height - 1));

                this.EmptyPatches[i] = img;
            }

            for (int i = 0; i < this.DisplayTextures.Length; i++)
            {
                Bitmap img = this.Bitmaps[i].Clone(new Rectangle(0, 0, this.Bitmaps[i].Width, this.Bitmaps[i].Height), this.Bitmaps[i].PixelFormat);
                for (int j = 0; j < this.PatchTextureIndexes.Count; j++)
                {
                    if (this.PatchTextureIndexes[j] == i)
                    {
                        if (this.PatchIndexes[j] < 0) continue;

                        Bitmap img2 = new Bitmap(img.Width, img.Height);
                        Graphics img2Gr = Graphics.FromImage(img2);

                        img2Gr.DrawImage(this.Patches[j], new Rectangle(this.PatchLocs[j].X, this.PatchLocs[j].Y, this.PatchSizes[j].Width, this.PatchSizes[j].Height),
                        new Rectangle(0, this.PatchSizes[j].Height * this.PatchIndexes[j], this.PatchSizes[j].Width, this.PatchSizes[j].Height), GraphicsUnit.Pixel);
                        Rectangle rect;
                        if (this.PatchLocs[j].X > 0)
                        {
                            rect = new Rectangle(0, 0, this.PatchLocs[j].X, img.Height);
                            img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                        }
                        if (this.PatchLocs[j].Y > 0)
                        {
                            rect = new Rectangle(0, 0, img.Width, this.PatchLocs[j].Y);
                            img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                        }
                        if (this.PatchLocs[j].X + this.PatchSizes[j].Width < img.Width)
                        {
                            rect = new Rectangle(this.PatchLocs[j].X + this.PatchSizes[j].Width, 0, img.Width - this.PatchLocs[j].X - this.PatchSizes[j].Width, img.Height);
                            img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                        }
                        if (this.PatchLocs[j].Y + this.PatchSizes[j].Height < img.Height)
                        {
                            rect = new Rectangle(0, this.PatchLocs[j].Y + this.PatchSizes[j].Height, img.Width, img.Height - this.PatchLocs[j].Y - this.PatchSizes[j].Height);
                            img2Gr.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
                        }
                        img = img2;
                    }
                }
                this.DisplayTextures[i] = Tex2Dbmp.GetTexture2DFromBitmap(img);
                Bitmap imgHalfOp = new Bitmap(img.Width, img.Height);
                Graphics imgHalfGR = Graphics.FromImage(imgHalfOp);
                System.Drawing.Imaging.ColorMatrix cmxPic = new System.Drawing.Imaging.ColorMatrix();
                cmxPic.Matrix33 = 0.333f;
                System.Drawing.Imaging.ImageAttributes iaPic = new System.Drawing.Imaging.ImageAttributes();
                iaPic.SetColorMatrix(cmxPic, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                imgHalfGR.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, iaPic);
            }
        }

        public System.Drawing.Color GetPaletteColor(int paletteStartAddress, int pIndex)
        {
            System.Drawing.Color output = System.Drawing.Color.Transparent;
            try
            {
                int colorSlot = ((64 * ((pIndex / 8) % 2) + ((pIndex / 32) * 128) + (8 * ((pIndex / 16) % 2)) + (pIndex % 8)) * 4);
                double alpha = timBinary.Buffer[paletteStartAddress + colorSlot + 3] * 2;
                if (alpha > 255) alpha = 255;

                output = System.Drawing.Color.FromArgb((int)alpha, timBinary.Buffer[paletteStartAddress + colorSlot],
                timBinary.Buffer[paletteStartAddress + colorSlot + 1], timBinary.Buffer[paletteStartAddress + colorSlot + 2]);
            }
            catch
            {

            }
            return output;
        }

        public System.Drawing.Color[] GetPalette(int paletteStartAddress)
        {
            System.Drawing.Color[] outputPalette = new System.Drawing.Color[256];
            for (int c = 0; c < 256; c++)
                outputPalette[c] = GetPaletteColor(paletteStartAddress, c);
            return outputPalette;
        }

        public Bitmap GetPatchBitmap(int startPixel, System.Drawing.Color[] palette, uint width, uint height)
        {
            Bitmap Texture = new Bitmap((int)width, (int)height);
            LockBitmap lockBitmap = new LockBitmap(Texture);
            lockBitmap.LockBits();
            for (ushort x = 0; x < width; x++)
                for (ushort y = 0; y < height; y++)
                    lockBitmap.SetPixel(x, y, palette[timBinary.Buffer[startPixel + (y * width) + x]]);
            lockBitmap.UnlockBits();
            return Texture;
        }
        public Bitmap GetTextureBitmap(int startPixel, System.Drawing.Color[] palette, uint width, uint height)
        {
            Bitmap Texture = new Bitmap((int)width, (int)height);
            LockBitmap lockBitmap = new LockBitmap(Texture);
            lockBitmap.LockBits();
            ushort textW2 = (ushort)(Texture.Width * 2);
            ushort textW4 = (ushort)(Texture.Width * 4);
            for (ushort x = 0; x < width; x++)
            {
                byte xMOD16 = (byte)(x & 15);
                byte xMOD8 = (byte)(x & 7);
                byte xOVER16 = (byte)(x >> 4);
                for (ushort y = 0; y < height; y++)
                {
                    int offset = 32 * (xOVER16) + (4 * (xMOD16)) - (30 * ((xMOD16) >> 3)) + ((System.Byte)(y & 1)) * textW2 +
                        ((System.Byte)(y >> 2)) * textW4 + ((((System.Byte)(y & 3)) >> 1) +
                        (((xMOD8) >> 2) * -1 + (1 - ((xMOD8) >> 2))) * 16 * (((System.Byte)(y & 3)) >> 1) + (((xMOD8) >> 2) * -1 +
                        (1 - ((xMOD8) >> 2))) * 16 * (((System.Byte)(y & 7)) >> 2)) * (1 - (((System.Byte)(y & 7)) >> 2) * (((System.Byte)(y & 3)) >> 1)) +
                        (((System.Byte)(y & 7)) >> 2) * (((System.Byte)(y & 3)) >> 1);

                    if (startPixel + offset < timBinary.Buffer.Length)
                    lockBitmap.SetPixel(x, y, palette[timBinary.Buffer[startPixel + offset]]);
                }
            }
            lockBitmap.UnlockBits();
            return Texture;
        }
    }
}
