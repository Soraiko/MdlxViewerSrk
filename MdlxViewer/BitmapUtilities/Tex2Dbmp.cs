using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BAR_Editor;
using System.IO;
using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MdlxViewer
{
    class Tex2Dbmp
    {
        public static Texture2D GetTexture2DFromBitmap(System.Drawing.Bitmap bitmap)
        {
            Texture2D tex = new Texture2D(Program.MainView.GraphicsDevice, bitmap.Width, bitmap.Height);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int bufferSize = data.Height * data.Stride;
            byte[] bytes = new byte[bufferSize];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (int i = 0; i < bytes.Length; i += 4)
            {
                byte red = bytes[i];
                bytes[i] = bytes[i + 2];
                bytes[i + 2] = red;
            }
            tex.SetData(bytes);
            bitmap.UnlockBits(data);
            return tex;
        }
    }
}
