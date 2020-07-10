using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Aiv.Draw
{
	/// <summary>
	/// Class able to load an image
	/// </summary>
	public class Sprite
	{
		/// <summary>
		/// Return raw sprite data in bytes
		/// </summary>
		public byte[] Bitmap { get; }
		
		/// <summary>
		/// Sprite's width in pixel
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Sprite's height in pixel
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// Return the pixel format for this sprite
		/// </summary>
		public PixelFormat Format { get; }

		/// <summary>
		/// Depth in bits. E.g.: 24bit, 32bit 
		/// </summary>
		public int Depth { get; }

		private unsafe void StoreBitmapAsRGBA(Bitmap bitmap) {
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			System.Drawing.Imaging.BitmapData bdata = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;

			// fix ordering as in windows all is little endian ARGB -> BGRA -> RGBA
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					int spos = (y * Width * 4) + (x * 4);
					int dpos = (y * Width * 4) + (x * 4);

					// R
					this.Bitmap[dpos] = data[spos+2];
					// G
					this.Bitmap[dpos+1] = data[spos+1];
					// B
					this.Bitmap[dpos + 2] = data[spos];
					// A
					this.Bitmap[dpos + 3] = data[spos+3];
				}
			}
			bitmap.UnlockBits(bdata);
		}

		private void StoreBitmapAsRGB(Bitmap bitmap)
		{
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					int dpos = (y * Width * 3) + (x * 3);
					Color sourcePixel = bitmap.GetPixel(x, y);

					// R
					this.Bitmap[dpos + 0] = sourcePixel.R;
					// G
					this.Bitmap[dpos + 1] = sourcePixel.G;
					// B
					this.Bitmap[dpos + 2] = sourcePixel.B;
				}
			}
		}

		/// <summary>
		/// Create a Sprite object loading an image from a filesystem path
		/// </summary>
		/// <param name="fileName">path to the image</param>
		/// <exception cref="FileNotFoundException">if file doesn't not exits</exception>
		/// <exception cref="ArgumentException">if the the image is not in valid format</exception>
		public Sprite(string fileName)
		{
			Bitmap rawBitmap = null;
			try
			{
				rawBitmap = new Bitmap(fileName);
			} catch (FileNotFoundException e)
			{
				throw new FileNotFoundException(fileName, e);
			} catch (ArgumentException e)
			{
				//Bitmap should throw only FileNotFoundException,
				//but sometime ArgumentException appear instead.
				throw new FileNotFoundException(fileName, e);
			}

			Width = rawBitmap.Width;
			Height = rawBitmap.Height;

			switch (rawBitmap.PixelFormat)
			{
				case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
					Depth = 32;
					Format = PixelFormat.RGBA;
					this.Bitmap = new byte[Width * Height * Depth/8];
					this.StoreBitmapAsRGBA(rawBitmap);
					break;
				case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
					Depth = 24;
					Format = PixelFormat.RGB;
					this.Bitmap = new byte[Width * Height * Depth/8];
					this.StoreBitmapAsRGB(rawBitmap);
					break;
				default: throw new ArgumentException("Invalid sprite format, must be RGB or RGBA");
			}
		}
	}
}

