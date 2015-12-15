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

	public enum KeyCode
	{
		A = Keys.A,
		B = Keys.B,
		C = Keys.C,
		D = Keys.D,
		E = Keys.E,
		F = Keys.F,
		G = Keys.G,
		H = Keys.H,
		I = Keys.I,
		J = Keys.J,
		K = Keys.K,
		L = Keys.L,
		M = Keys.M,
		N = Keys.N,
		O = Keys.O,
		P = Keys.P,
		Q = Keys.Q,
		R = Keys.R,
		S = Keys.S,
		T = Keys.T,
		U = Keys.U,
		V = Keys.V,
		W = Keys.W,
		X = Keys.X,
		Y = Keys.Y,
		Z = Keys.Z,

		Space = Keys.Space,
		Return = Keys.Return,
		Esc = Keys.Escape,

		Up = Keys.Up,
		Down = Keys.Down,
		Left = Keys.Left,
		Right = Keys.Right,
	}

	public class Window
	{
		private Form form;
		private PictureBox pbox;
		private Rectangle rect;

		/// <summary>
		/// Used to draw into the form
		/// </summary>
		public byte[] bitmap;
		public Bitmap workingBitmap;

		/// <summary>
		/// Window width
		/// </summary>
		public int width;
		/// <summary>
		/// Window height
		/// </summary>
		public int height;

		/// <summary>
		/// Get or sets the cursor visibility
		/// </summary>
		public bool CursorVisible
		{
			set
			{
				if (value)
					Cursor.Show();
				else
					Cursor.Hide();
			}
		}

		private PixelFormat pixelFormat;

		private Stopwatch watch;

		private float _deltaTime;

		/// <summary>
		/// Time (in seconds) passed since the last <c>Blit()</c>
		/// </summary>
		public float deltaTime
		{
			get
			{
				return _deltaTime;
			}
		}

		/// <summary>
		/// Sets or get if window is opened or closed;
		/// </summary>
		public bool opened = true;


		private Dictionary<KeyCode, bool> keyboardTable;


		private bool _mouseLeft;
		private bool _mouseRight;
		private bool _mouseMiddle;

		private int deltaW;
		private int deltaH;

		private class WindowDraw : Form
		{
			public WindowDraw()
			{
				StartPosition = FormStartPosition.CenterScreen;
				FormBorderStyle = FormBorderStyle.FixedSingle;
				MaximizeBox = false;
				MinimizeBox = false;

				this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
				this.SetStyle(ControlStyles.UserPaint, false);
				this.SetStyle(ControlStyles.FixedWidth, true);
				this.SetStyle(ControlStyles.FixedHeight, true);

			}
		}

		/// <summary>
		/// Sets Window's Icon
		/// </summary>
		/// <param name="path">path to the icon</param>
		/// <param name="isRelative">if <c>true</c>, the path will be relative to the application location, otherwise the path will be absoulte</param>
		public void SetIcon(string path, bool isRelative)
		{
			if (isRelative)
				this.form.Icon = new Icon(AppDomain.CurrentDomain.BaseDirectory + path);
			else
				this.form.Icon = new Icon(path);
		}

		/// <summary>
		/// Creates a new Window
		/// </summary>
		/// <param name="width">internal window's width</param>
		/// <param name="height">internal window's height</param>
		/// <param name="title">window's title</param>
		/// <param name="format">Pixel Format</param>
		public Window(int width, int height, string title, PixelFormat format)
		{
			this.form = new WindowDraw();
			this.form.Text = title;
			this.form.MinimizeBox = true;
			this.form.StartPosition = FormStartPosition.CenterScreen;
			this.form.Size = new Size(width, height);
			Size clientSize = this.form.ClientSize;
			this.deltaW = width - clientSize.Width;
			this.deltaH = height - clientSize.Height;
			this.form.Size = new Size(width + this.deltaW, height + this.deltaH);

			this.form.FormClosed += new FormClosedEventHandler(this.Close);
			this.form.KeyDown += new KeyEventHandler(this.KeyDown);
			this.form.KeyUp += new KeyEventHandler(this.KeyUp);

			this.width = width;
			this.height = height;

			this.pixelFormat = format;

			this.rect = new Rectangle(0, 0, width, height);

			switch (format)
			{
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
					throw new Exception("Unsupported PixelFormat");
			}

			this.workingBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			this.pbox = new PictureBox();
			this.pbox.Size = new Size(this.width, this.height);
			this.form.Controls.Add(this.pbox);

			this.pbox.MouseUp += new MouseEventHandler(this.MouseUp);
			this.pbox.MouseDown += new MouseEventHandler(this.MouseDown);

			watch = new Stopwatch();

			this.keyboardTable = new Dictionary<KeyCode, bool>();

			this.form.Show();
			this.form.Activate();
		}

		/// <summary>
		/// Returns mouse X position relative to the form
		/// </summary>
		public int mouseX
		{
			get
			{
				return this.form.PointToClient(Cursor.Position).X;
			}
		}

		/// <summary>
		/// Returns mouse Y position relative to the form
		/// </summary>
		public int mouseY
		{
			get
			{
				return this.form.PointToClient(Cursor.Position).Y;
			}
		}

		/// <summary>
		/// Returns <c>true</c> if mouse left button is pressed, otherwise <c>false</c>
		/// </summary>
		public bool mouseLeft
		{
			get
			{
				return this._mouseLeft;
			}
		}

		/// <summary>
		/// Returns <c>true</c> if mouse right button is pressed, otherwise <c>false</c>
		/// </summary>
		public bool mouseRight
		{
			get
			{
				return this._mouseRight;
			}
		}

		/// <summary>
		/// Returns <c>true</c> if mouse middle button is pressed, otherwise <c>false</c>
		/// </summary>
		public bool mouseMiddle
		{
			get
			{
				return this._mouseMiddle;
			}
		}

		private void MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				this._mouseLeft = true;
			if (e.Button == MouseButtons.Right)
				this._mouseRight = true;
			if (e.Button == MouseButtons.Middle)
				this._mouseMiddle = true;
		}

		private void MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				this._mouseLeft = false;
			if (e.Button == MouseButtons.Right)
				this._mouseRight = false;
			if (e.Button == MouseButtons.Middle)
				this._mouseMiddle = false;
		}


		private void Close(object sender, FormClosedEventArgs e)
		{
			this.opened = false;
		}

		private void KeyDown(object sender, KeyEventArgs e)
		{
			this.keyboardTable[(KeyCode)e.KeyCode] = true;
		}

		private void KeyUp(object sender, KeyEventArgs e)
		{
			this.keyboardTable[(KeyCode)e.KeyCode] = false;
		}

		private unsafe void BlitRGB()
		{

			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.height; y++)
			{
				for (int x = 0; x < this.width; x++)
				{
					int spos = (y * this.width * 3) + (x * 3);
					int dpos = (y * this.width * 4) + (x * 4);
					//B
					data[dpos] = this.bitmap[spos + 2];
					//G
					data[dpos + 1] = this.bitmap[spos + 1];
					//R
					data[dpos + 2] = this.bitmap[spos];
					//A
					data[dpos + 3] = 0xff;
				}
			}
			this.workingBitmap.UnlockBits(bdata);

		}

		private unsafe void BlitRGBA()
		{

			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.height; y++)
			{
				for (int x = 0; x < this.width; x++)
				{
					int spos = (y * this.width * 4) + (x * 4);
					int dpos = (y * this.width * 4) + (x * 4);
					//B
					data[dpos] = this.bitmap[spos + 2];
					//G
					data[dpos + 1] = this.bitmap[spos + 1];
					//R
					data[dpos + 2] = this.bitmap[spos];
					//A
					data[dpos + 3] = this.bitmap[spos + 3];
				}
			}
			this.workingBitmap.UnlockBits(bdata);

		}

		private unsafe void BlitGrayscale()
		{

			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.height; y++)
			{
				for (int x = 0; x < this.width; x++)
				{
					int spos = (y * this.width) + x;
					int dpos = (y * this.width * 4) + (x * 4);
					//B
					data[dpos] = this.bitmap[spos];
					//G
					data[dpos + 1] = this.bitmap[spos];
					//R
					data[dpos + 2] = this.bitmap[spos];
					//A
					data[dpos + 3] = 0xff;
				}
			}
			this.workingBitmap.UnlockBits(bdata);

		}

		/// <summary>
		/// Returns true when <c>key</c> is pressed
		/// </summary>
		/// <param name="key">key to check if is pressed</param>
		public bool GetKey(KeyCode key)
		{
			if (!this.keyboardTable.ContainsKey(key))
				return false;
			return this.keyboardTable[key];
		}

		/// <summary>
		/// Draws the current <c>Window.bitmap</c> into the form
		/// </summary>
		public void Blit()
		{
			if (!this.watch.IsRunning)
				this.watch.Start();

			switch (this.pixelFormat)
			{
				case PixelFormat.RGB:
					this.BlitRGB();
					break;
				case PixelFormat.RGBA:
					this.BlitRGBA();
					break;
				case PixelFormat.Grayscale:
					this.BlitGrayscale();
					break;
				default:
					throw new Exception("Unsupported PixelFormat");
			}

			// this invalidates the picturebox
			this.pbox.Image = this.workingBitmap;

			Application.DoEvents();

			this._deltaTime = (float)this.watch.Elapsed.TotalSeconds;

			this.watch.Reset();
			this.watch.Start();
		}
	}
}

