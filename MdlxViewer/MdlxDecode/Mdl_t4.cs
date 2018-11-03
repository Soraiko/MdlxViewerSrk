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
    class Mdl_t4
    {
        int t4_index = -1;
        public SrkBinary binary;

        public Mdl_t4(ref BAR b)
        {
            bool containsModel = false;

            for (int i = 0; i < b.fileList.Count; i++)
            {
                if (b.fileList[i].type == 4)
                {
                    this.binary = new SrkBinary(ref b.fileList[i].data);

                    t4_index = i;
                    containsModel = true;
                    break;
                }
            }
            if (!containsModel)
            {
                Console.WriteLine("No model found inside that file.");
                Console.WriteLine("Exiting...");
                System.Threading.Thread.Sleep(2000);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
        
    }
}
