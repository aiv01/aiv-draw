using System;
using System.Drawing;

namespace Aiv.Draw
{
	public class Sprite
	{

		public byte[] bitmap;
		private int _width;
		private int _height;

		public int width {
			get {
				return _width;
			}
		}

		public int height {
			get {
				return _height;
			}
		}

		private unsafe void ConvertToRGBA(Bitmap bitmap) {
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			System.Drawing.Imaging.BitmapData bdata = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;

			// fix ordering as in windows all is little endian ARGB -> BGRA -> RGBA
			for (int y = 0; y < this._height; y++) {
				for (int x = 0; x < this._width; x++) {
					int spos = (y * this._width * 4) + (x * 4);
					int dpos = (y * this._width * 4) + (x * 4);

					// R
					this.bitmap[dpos] = data[spos+2];
					// G
					this.bitmap[dpos+1] = data[spos+1];
					// B
					this.bitmap[dpos + 2] = data[spos];
					// A
					this.bitmap[dpos + 3] = data[spos+3];

				}
			}

			bitmap.UnlockBits(bdata);
		}

		public Sprite (string fileName)
		{
			Bitmap _bitmap = new Bitmap (fileName);
			if (_bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
				throw new Exception ("invalid sprite format, must be RGBA");
			this._width = _bitmap.Width;
			this._height = _bitmap.Height;

			this.bitmap = new byte[this._width * this._height * 4];

			this.ConvertToRGBA (_bitmap);

		}
	}
}

