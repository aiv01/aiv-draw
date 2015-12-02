using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace Aiv.Draw
{

	public enum PixelFormat
	{
		BW,
		Grayscale,
		RGB,
		RGBA,
	}

	public enum KeyCode {
		A=Keys.A,
		B=Keys.B,
		C=Keys.C,
		D=Keys.D,
		E=Keys.E,
		F=Keys.F,
		G=Keys.G,
		H=Keys.H,
		I=Keys.I,
		J=Keys.J,
		K=Keys.K,
		L=Keys.L,
		M=Keys.M,
		N=Keys.N,
		O=Keys.O,
		P=Keys.P,
		Q=Keys.Q,
		R=Keys.R,
		S=Keys.S,
		T=Keys.T,
		U=Keys.U,
		V=Keys.V,
		W=Keys.W,
		X=Keys.X,
		Y=Keys.Y,
		Z=Keys.Z,

		Space=Keys.Space,
		Return=Keys.Return,
		Esc=Keys.Escape,

		Up=Keys.Up,
		Down=Keys.Down,
		Left=Keys.Left,
		Right=Keys.Right,
	}

	public class Window
	{
		private Form form;
		private PictureBox pbox;
		private Rectangle rect;

		public byte[] bitmap;
		public Bitmap workingBitmap;

		public int width;
		public int height;

		private PixelFormat pixelFormat;

		private Stopwatch watch;

		private float _deltaTime;
		public float deltaTime {
			get {
				return _deltaTime;
			}
		}

		public bool opened = true;
		private Dictionary<KeyCode, bool> keyboardTable;


		private bool _mouseLeft;
		private bool _mouseRight;
		private bool _mouseMiddle;

		private int deltaW;
		private int deltaH;

		private class WindowDraw : Form {
			public WindowDraw() {
				StartPosition = FormStartPosition.CenterScreen;
				FormBorderStyle = FormBorderStyle.FixedSingle;
				MaximizeBox = false;
				MinimizeBox = false;

				this.SetStyle (ControlStyles.AllPaintingInWmPaint, true);
				this.SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
				this.SetStyle (ControlStyles.UserPaint, false);
				this.SetStyle (ControlStyles.FixedWidth, true);
				this.SetStyle (ControlStyles.FixedHeight, true);

			}
		}

		public Window (int width, int height, string title, PixelFormat format)
		{

			this.form = new WindowDraw ();
			this.form.Text = title;
			this.form.Size = new Size (width, height);
			Size clientSize = this.form.ClientSize;
			this.deltaW = width - clientSize.Width;
			this.deltaH = height - clientSize.Height;
			this.form.Size = new Size (width + this.deltaW, height + this.deltaH);

			this.form.FormClosed += new FormClosedEventHandler (this.Close);
			this.form.KeyDown += new KeyEventHandler (this.KeyDown);
			this.form.KeyUp += new KeyEventHandler (this.KeyUp);


			this.width = width;
			this.height = height;

			this.pixelFormat = format;

			this.rect = new Rectangle (0, 0, width, height);

			switch (format) {
			case PixelFormat.BW:
				this.bitmap = new byte[width * height / 8];
				break;
			case PixelFormat.Grayscale:
				this.bitmap = new byte[width * height];
				break;
			case PixelFormat.RGB:
				this.bitmap = new byte[width * height * 3];
				break;
			case PixelFormat.RGBA:
				this.bitmap = new byte[width * height * 4];
				break;
			default:
				throw new Exception ("Unsupported PixelFormat");
			}

			this.workingBitmap = new Bitmap (width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			this.pbox = new PictureBox ();
			this.pbox.Size = new Size (this.width, this.height);
			this.form.Controls.Add (this.pbox);

			this.pbox.MouseUp += new MouseEventHandler (this.MouseUp);
			this.pbox.MouseDown += new MouseEventHandler (this.MouseDown);

			watch = new Stopwatch ();

			this.keyboardTable = new Dictionary<KeyCode, bool>();

			this.form.Show ();
		}

		public int mouseX {
			get {
				return Cursor.Position.X - this.form.Location.X - this.deltaW;
			}
		}

		public int mouseY {
			get {
				return Cursor.Position.Y - this.form.Location.Y - this.deltaH;
			}
		}

		public bool mouseLeft {
			get {
				return this._mouseLeft;
			}
		}

		public bool mouseRight {
			get {
				return this._mouseRight;
			}
		}

		public bool mouseMiddle {
			get {
				return this._mouseMiddle;
			}
		}

		private void MouseDown (object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				this._mouseLeft = true;
			if (e.Button == MouseButtons.Right)
				this._mouseRight = true;
			if (e.Button == MouseButtons.Middle)
				this._mouseMiddle = true;
		}

		private void MouseUp (object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				this._mouseLeft = false;
			if (e.Button == MouseButtons.Right)
				this._mouseRight = false;
			if (e.Button == MouseButtons.Middle)
				this._mouseMiddle = false;
		}


		private void Close(object sender, FormClosedEventArgs e) {
			this.opened = false;
		}

		private void KeyDown (object sender, KeyEventArgs e)
		{
			this.keyboardTable [(KeyCode)e.KeyCode] = true;
		}

		private void KeyUp (object sender, KeyEventArgs e)
		{
			this.keyboardTable [(KeyCode)e.KeyCode] = false;
		}

		unsafe private void BlitRGB ()
		{
			
			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits (this.rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.height; y++) {
				for (int x = 0; x < this.width; x++) {
					int spos = (y * this.width * 3) + (x * 3);
					int dpos = (y * this.width * 4) + (x * 4);
					//B
					data [dpos] = this.bitmap [spos + 2];
					//G
					data [dpos + 1] = this.bitmap [spos + 1];
					//R
					data [dpos + 2] = this.bitmap [spos];
					//A
					data [dpos + 3] = 0xff;
				}
			}
			this.workingBitmap.UnlockBits (bdata);

		}

		unsafe private void BlitRGBA ()
		{
			
			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits (this.rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.height; y++) {
				for (int x = 0; x < this.width; x++) {
					int spos = (y * this.width * 4) + (x * 4);
					int dpos = (y * this.width * 4) + (x * 4);
					//B
					data [dpos] = this.bitmap [spos + 2];
					//G
					data [dpos + 1] = this.bitmap [spos + 1];
					//R
					data [dpos + 2] = this.bitmap [spos];
					//A
					data [dpos + 3] = this.bitmap [spos + 3];
				}
			}
			this.workingBitmap.UnlockBits (bdata);

		}

		unsafe private void BlitGrayscale ()
		{
			
			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits (this.rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.height; y++) {
				for (int x = 0; x < this.width; x++) {
					int spos = (y * this.width) + x;
					int dpos = (y * this.width * 4) + (x * 4);
					//B
					data [dpos] = this.bitmap [spos];
					//G
					data [dpos + 1] = this.bitmap [spos];
					//R
					data [dpos + 2] = this.bitmap [spos];
					//A
					data [dpos + 3] = 0xff;
				}
			}
			this.workingBitmap.UnlockBits (bdata);

		}

		public bool GetKey(KeyCode key) {
			if (!this.keyboardTable.ContainsKey (key))
				return false;
			return this.keyboardTable [key];
		}

		public void Blit ()
		{


			if (!this.watch.IsRunning)
				this.watch.Start ();

			switch (this.pixelFormat) {
			case PixelFormat.RGB:
				this.BlitRGB ();
				break;
			case PixelFormat.RGBA:
				this.BlitRGBA ();
				break;
			case PixelFormat.Grayscale:
				this.BlitGrayscale ();
				break;
			default:
				throw new Exception ("Unsupported PixelFormat");
			}

			// this invalidates the picturebox
			this.pbox.Image = this.workingBitmap;

			Application.DoEvents ();

			this._deltaTime = (float)this.watch.Elapsed.TotalSeconds;

			this.watch.Reset ();
			this.watch.Start ();
		}
	}
}

