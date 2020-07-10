using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace Aiv.Draw
{
	/// <summary>
	/// Class able to manage a Window and drawing to it
	/// </summary>	
	public class Window
	{
		/// <summary>
		/// Used to draw into the form
		/// </summary>
		public byte[] bitmap;
		public Bitmap workingBitmap;

		/// <summary>
		/// Window width
		/// </summary>
		public int Width { get; }
		/// <summary>
		/// Window height
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// Return the pixel format for this sprite
		/// </summary>
		public PixelFormat Format { get; }

		/// <summary>
		/// Time (in seconds) passed since the last <c>Blit()</c>
		/// </summary>
		public float DeltaTime { get; internal set; }

		/// <summary>
		/// Sets or get if window is opened or closed;
		/// </summary>
		public bool opened = true;

		private Form form;
		private PictureBox pbox;
		private Rectangle rect;
		private Dictionary<KeyCode, bool> keyboardTable;
		private readonly int deltaW;
		private readonly int deltaH;
		private readonly Stopwatch timer;

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
		/// <param name="path">path to the icon. Should be an .ico file</param>
		public void SetIcon(string path)
		{
			this.form.Icon = new Icon(path);
		}

		/// <summary>
		/// Sets Window's Title
		/// </summary>
		/// <param name="text">text to be set as window title</param>
		public void SetTitle(string text)
		{
			this.form.Text = text;
		}

		/// <summary>
		/// Sets mouse cursor visibility
		/// </summary>
		/// <param name="enabled"><c>true</c> to show mouse, <c>false</c> otherwise</param>
		public void SetMouseVisible(bool enabled) 
		{ 
			if (enabled)
				Cursor.Show();
			else
				Cursor.Hide();	
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

			this.Width = width;
			this.Height = height;
			this.Format = format;

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
			this.pbox.Size = new Size(this.Width, this.Height);
			this.form.Controls.Add(this.pbox);

			this.pbox.MouseUp += new MouseEventHandler(this.MouseUp);
			this.pbox.MouseDown += new MouseEventHandler(this.MouseDown);

			timer = new Stopwatch();

			this.keyboardTable = new Dictionary<KeyCode, bool>();

			this.form.Show();
			this.form.Activate();
		}

		/// <summary>
		/// Returns mouse X position relative to the form
		/// </summary>
		public int MouseX
		{
			get
			{
				return this.form.PointToClient(Cursor.Position).X;
			}
		}

		/// <summary>
		/// Returns mouse Y position relative to the form
		/// </summary>
		public int MouseY
		{
			get
			{
				return this.form.PointToClient(Cursor.Position).Y;
			}
		}

		/// <summary>
		/// Returns <c>true</c> if mouse left button is pressed, otherwise <c>false</c>
		/// </summary>
		public bool MouseLeft { get; internal set; }

		/// <summary>
		/// Returns <c>true</c> if mouse right button is pressed, otherwise <c>false</c>
		/// </summary>
		public bool MouseRight { get; internal set; }

		/// <summary>
		/// Returns <c>true</c> if mouse middle button is pressed, otherwise <c>false</c>
		/// </summary>
		public bool MouseMiddle { get; internal set; }

		private void MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				MouseLeft = true;
			if (e.Button == MouseButtons.Right)
				MouseRight = true;
			if (e.Button == MouseButtons.Middle)
				MouseMiddle = true;
		}

		private void MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				MouseLeft = false;
			if (e.Button == MouseButtons.Right)
				MouseRight = false;
			if (e.Button == MouseButtons.Middle)
				MouseMiddle = false;
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
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width * 3) + (x * 3);
					int dpos = (y * this.Width * 4) + (x * 4);
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
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width * 4) + (x * 4);
					int dpos = (y * this.Width * 4) + (x * 4);
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
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width) + x;
					int dpos = (y * this.Width * 4) + (x * 4);
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
			if (!this.timer.IsRunning)
				this.timer.Start();

			switch (this.Format)
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

			DeltaTime = (float)this.timer.Elapsed.TotalSeconds;

			this.timer.Reset();
			this.timer.Start();
		}
	}
}

