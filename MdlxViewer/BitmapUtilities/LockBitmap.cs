using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MdlxViewer
{
	public class LockBitmap
	{
		Bitmap source = null;
		IntPtr Iptr = IntPtr.Zero;
		BitmapData bitmapData = null;
		
		public byte[] Pixels { get; set; }
		public int Depth { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		
		public LockBitmap(Bitmap source)
		{
			this.source = source;
		}
		
		/// <summary>
		/// Lock bitmap data
		/// </summary>
		public void LockBits()
		{
			try
			{
				Width = source.Width;
				Height = source.Height;
				int PixelCount = Width * Height;
				Rectangle rect = new Rectangle(0, 0, Width, Height);
				Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);
				if (Depth != 8 && Depth != 24 && Depth != 32)
				{
					throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
				}
				bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
				                             source.PixelFormat);
				int step = Depth / 8;
				Pixels = new byte[PixelCount * step];
				Iptr = bitmapData.Scan0;
				Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
			}
			catch
            {

            }
		}
		
		/// <summary>
		/// Unlock bitmap data
		/// </summary>
		public void UnlockBits()
		{
			try
			{
				// Copy data from byte array to pointer
				Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);
				
				// Unlock bitmap data
				source.UnlockBits(bitmapData);
			}
			catch
            {

            }
		}
		
		/// <summary>
		/// Get the color of the specified pixel
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public System.Windows.Media.Color GetPixel(int x, int y)
		{
			System.Windows.Media.Color clr = System.Windows.Media.Colors.Transparent;
			
			// Get color components count
			int cCount = Depth / 8;
			
			// Get start index of the specified pixel
			int i = ((y * Width) + x) * cCount;
			
			if (i > Pixels.Length - cCount)
				throw new IndexOutOfRangeException();
			
			if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
			{
				byte b = Pixels[i];
				byte g = Pixels[i + 1];
				byte r = Pixels[i + 2];
				byte a = Pixels[i + 3]; // a
				clr = System.Windows.Media.Color.FromArgb(a, r, g, b);
			}
			if (Depth == 24) // For 24 bpp get Red, Green and Blue
			{
				byte b = Pixels[i];
				byte g = Pixels[i + 1];
				byte r = Pixels[i + 2];
				clr = System.Windows.Media.Color.FromRgb(r, g, b);
			}
			if (Depth == 8)
				// For 8 bpp get color value (Red, Green and Blue values are the same)
			{
				byte c = Pixels[i];
				clr = System.Windows.Media.Color.FromRgb(c, c, c);
			}
			return clr;
		}
		
		/// <summary>
		/// Set the color of the specified pixel
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="color"></param>
		public void SetPixel(int x, int y, System.Drawing.Color color)
		{
			int cCount = Depth / 8;
			int i = ((y * Width) + x) * cCount;
			
			if (Depth == 32)
			{
				Pixels[i] = color.B;
				Pixels[i + 1] = color.G;
				Pixels[i + 2] = color.R;
				Pixels[i + 3] = color.A;
			}
			if (Depth == 24)
			{
				Pixels[i] = color.B;
				Pixels[i + 1] = color.G;
				Pixels[i + 2] = color.R;
			}
			if (Depth == 8)
				// For 8 bpp set color value (Red, Green and Blue values are the same)
			{
				Pixels[i] = color.B;
			}
		}
	}
}
