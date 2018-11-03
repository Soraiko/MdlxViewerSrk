using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using BAR_Editor;
using Microsoft.Xna.Framework.Graphics;

namespace MdlxViewer
{
    class Tim_t7
    {
        CatzTim ct;
        SrkTim st;
        bool catzMode;

        BAR file;
        int t7_index = -1;
        SrkBinary binary;

        public Tim_t7(ref BAR b)
        {
            this.file = b;
            this.Patches = new List<Bitmap>(0);
            this.PatchesPositions = new List<Point>(0);
            this.PatchesSizes = new List<Size>(0);
            this.PatchesCounts = new List<int>(0);
            this.PatchesDestinations = new List<int>(0);

            bool containsTexture = false;

            for (int i=0;i<b.fileList.Count;i++)
            {
                if (b.fileList[i].type == 7)
                {
                    this.binary = new SrkBinary(ref b.fileList[i].data);

                    this.t7_index = i;
                    this.st = new SrkTim(b.fileList[i]);
                    this.st.ParseTim();

                    this.ct = new CatzTim(b.fileList[i]);
                    this.ct.parse();

                    catzMode = this.st.Patches.Count == 0 || (ct.imageCount > st.Bitmaps.Count);

                    if (catzMode)
                    {
                        this.textures = new Texture2D[this.ct.imageCount];
                        UpdateCatzT2D();
                    }
                    else
                    {
                        this.Patches = this.st.Patches;
                        this.PatchesPositions = this.st.PatchLocs;
                        this.PatchesSizes = this.st.PatchSizes;
                        this.PatchesCounts = this.st.PatchCounts;
                        this.PatchesDestinations = this.st.PatchTextureIndexes;
                    }

                    containsTexture = true;
                    break;
                }
            }
            if (!containsTexture)
            {
                Console.WriteLine("No TIM2 found inside that file.");
                Console.WriteLine("Exiting...");
                System.Threading.Thread.Sleep(2000);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        public void UpdateCatzT2D()
        {
            for (int i = 0; i < this.ct.imageCount; i++)
            {
                this.textures[i] = Tex2Dbmp.GetTexture2DFromBitmap(this.ct.getBMP(i));
            }
        }
        
        public List<Bitmap> Patches
        {
            get; set;
        }

        public List<Point> PatchesPositions
        {
            get; set;
        }

        public List<Size> PatchesSizes
        {
            get; set;
        }

        public List<int> PatchesCounts
        {
            get; set;
        }

        public List<int> PatchesDestinations
        {
            get; set;
        }

        Texture2D[] textures;

        public Bitmap getBitmap(int index)
        {
            if (catzMode)
            {
                return this.ct.getBMP(index);
            }
            else
            {
                return this.st.Bitmaps[index];
            }
            return SrkBinary.EmptyBMP;
        }

        public Texture2D getTexture(int index)
        {
            if (View.AllowTextures)
            if (catzMode)
            {
                if (index>-1&&index < this.textures.Length)
                    return this.textures[index];
            }
            else
            {
                //this.st.GetDisplayTextures();
                if (index > -1 && index < this.st.DisplayTextures.Length)
                    return this.st.DisplayTextures[index];
            }
            return SrkBinary.EmptyT2D;
        }

        public int TextureCount
        {
            get
            {
                int count = 0;
                if (catzMode)
                {
                    count = this.ct.imageCount;
                }
                else
                {
                    count = this.st.Bitmaps.Count;
                }
                return count;
            }
        }
    }
}
