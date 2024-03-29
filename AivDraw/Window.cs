﻿using System;
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
		/// Used to draw pixel bytes into the form
		/// </summary>
		public byte[] Bitmap { get; }

		/// <summary>
		/// Window width in pixel
		/// </summary>
		public int Width { get; }
		/// <summary>
		/// Window height in pixel
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
		/// Check if window is opened or closed;
		/// </summary>
		public bool IsOpened { get; internal set; }

		/// <summary>
		/// Check if window has focus;
		/// </summary>
		public bool HasFocus { get; internal set; }

		private Form form;
		private PictureBox pbox;
		private Rectangle workingRect;
		private Bitmap workingBitmap;
		
		private Dictionary<KeyCode, bool> keyboardTable;
		private readonly Stopwatch timer;
		
		/// <summary>
		/// Creates a new Window
		/// </summary>
		/// <param name="width">internal window's width</param>
		/// <param name="height">internal window's height</param>
		/// <param name="title">window's title</param>
		/// <param name="format">Pixel Format</param>
		public Window(int width, int height, string title, PixelFormat format)
		{
			form = new FormAdapter(width, height, title);
			
			form.FormClosed += new FormClosedEventHandler(this.CloseHandler);
			form.KeyDown += new KeyEventHandler(this.KeyDownHandler);
			form.KeyUp += new KeyEventHandler(this.KeyUpHandler);

			Width = width;
			Height = height;
			Format = format;

			timer = new Stopwatch();
			this.keyboardTable = new Dictionary<KeyCode, bool>();

			this.pbox = new PictureBox();
			this.pbox.Size = new Size(this.Width, this.Height);
			this.pbox.MouseUp += new MouseEventHandler(this.MouseUpHandler);
			this.pbox.MouseDown += new MouseEventHandler(this.MouseDownHandler);
			this.form.Controls.Add(this.pbox);
			this.form.GotFocus += new EventHandler(this.GainFocusHandler);
			this.form.LostFocus += new EventHandler(this.LostFocusHandler);

			switch (format)
			{
				case PixelFormat.BlackWhite:
					int byteSize = width * height;
					int bitSize = width * height / 8;
					if (bitSize * 8 != byteSize) throw new Exception("In BlackWhite format, Width * Height must be divisible by 8");
					this.Bitmap = new byte[bitSize];
					break;
				case PixelFormat.GrayScale:
					this.Bitmap = new byte[width * height];
					break;
				case PixelFormat.RGB:
					this.Bitmap = new byte[width * height * 3];
					break;
				case PixelFormat.RGBA:
					this.Bitmap = new byte[width * height * 4];
					break;
				default:
					throw new Exception("Unsupported PixelFormat");
			}

			this.workingRect = new Rectangle(0, 0, width, height);
			this.workingBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						
			this.form.Show(); //make the form visible
			this.form.Activate(); //make sure the form is on front and ready to catch events

			IsOpened = true;
		}

        /// <summary>
        /// Close this window
        /// </summary>
        public void Close()
		{
			form.Close();
			form.Dispose();
			IsOpened = false;
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
			switch (this.Format)
			{
				case PixelFormat.RGB:
					this.BlitRGB();
					break;
				case PixelFormat.RGBA:
					this.BlitRGBA();
					break;
				case PixelFormat.GrayScale:
					this.BlitGrayScale();
					break;
				case PixelFormat.BlackWhite:
					this.BlitBlackWhite();
					break;
				default:
					throw new Exception("Unsupported PixelFormat");
			}

			// invalidate and update the picturebox
			this.pbox.Image = this.workingBitmap;

			Application.DoEvents();

			DeltaTime = (float)this.timer.Elapsed.TotalSeconds;
			this.timer.Restart();
		}

		/// <summary>
		/// This class it's necessary because <c>SetStyle</c> method is <c>protected</c>
		/// </summary>
		private class FormAdapter : Form
		{
			public FormAdapter(int width, int height, string title)
			{
				StartPosition = FormStartPosition.CenterScreen;
				FormBorderStyle = FormBorderStyle.FixedSingle;
				MinimizeBox = true;
				MaximizeBox = false;

				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
				SetStyle(ControlStyles.UserPaint, false);
				SetStyle(ControlStyles.FixedWidth, true);
				SetStyle(ControlStyles.FixedHeight, true);

				Text = title;

				//Calculate form size taking into account delta
				//to have a real width x height window
				Size = new Size(width, height);
				int deltaW = width - ClientSize.Width;
				int deltaH = height - ClientSize.Height;
				Size = new Size(width + deltaW, height + deltaH);
			}
		}

		private void MouseDownHandler(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				MouseLeft = true;
			else if (e.Button == MouseButtons.Right)
				MouseRight = true;
			else if (e.Button == MouseButtons.Middle)
				MouseMiddle = true;
		}

		private void MouseUpHandler(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				MouseLeft = false;
			else if (e.Button == MouseButtons.Right)
				MouseRight = false;
			else if (e.Button == MouseButtons.Middle)
				MouseMiddle = false;
		}

		private void CloseHandler(object sender, FormClosedEventArgs e)
		{
			this.IsOpened = false;
		}

		private void KeyDownHandler(object sender, KeyEventArgs e)
		{
			this.keyboardTable[(KeyCode)e.KeyCode] = true;
		}

		private void KeyUpHandler(object sender, KeyEventArgs e)
		{
			this.keyboardTable[(KeyCode)e.KeyCode] = false;
		}

		private void GainFocusHandler(object sender, EventArgs e)
		{
			HasFocus = true;
		}

		private void LostFocusHandler(object sender, EventArgs e)
		{
			HasFocus = false;
			//Clear Input state when window loose focus
			this.keyboardTable.Clear();
			MouseLeft = false;
			MouseRight = false;
			MouseMiddle = false;
		}

		private unsafe void BlitRGB()
		{
			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.workingRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width * 3) + (x * 3);
					int dpos = (y * this.Width * 4) + (x * 4);
					//B
					data[dpos] = this.Bitmap[spos + 2];
					//G
					data[dpos + 1] = this.Bitmap[spos + 1];
					//R
					data[dpos + 2] = this.Bitmap[spos];
					//A
					data[dpos + 3] = 0xff;
				}
			}
			this.workingBitmap.UnlockBits(bdata);

		}

		private unsafe void BlitRGBA()
		{

			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.workingRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width * 4) + (x * 4);
					int dpos = (y * this.Width * 4) + (x * 4);
					//B
					data[dpos] = this.Bitmap[spos + 2];
					//G
					data[dpos + 1] = this.Bitmap[spos + 1];
					//R
					data[dpos + 2] = this.Bitmap[spos];
					//A
					data[dpos + 3] = this.Bitmap[spos + 3];
				}
			}
			this.workingBitmap.UnlockBits(bdata);

		}

		private unsafe void BlitGrayScale()
		{

			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.workingRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width) + x;
					int dpos = (y * this.Width * 4) + (x * 4);
					//B
					data[dpos] = this.Bitmap[spos];
					//G
					data[dpos + 1] = this.Bitmap[spos];
					//R
					data[dpos + 2] = this.Bitmap[spos];
					//A
					data[dpos + 3] = 0xff;
				}
			}
			this.workingBitmap.UnlockBits(bdata);
		}

		private unsafe void BlitBlackWhite()
		{

			System.Drawing.Imaging.BitmapData bdata = this.workingBitmap.LockBits(this.workingRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, this.workingBitmap.PixelFormat);
			byte* data = (byte*)bdata.Scan0;

			int bitShift = 7;
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					int spos = (y * this.Width + x ) / 8; //arrotonda sempre all'intero inferire
					int dpos = (y * this.Width * 4) + (x * 4);

					byte sourceByte = this.Bitmap[spos];
					byte value = (byte)(((sourceByte >> bitShift) & 1) * 255);  //value will be 255 or 0

					//B
					data[dpos + 0] = value;
					//G
					data[dpos + 1] = value;
					//R
					data[dpos + 2] = value;
					//A
					data[dpos + 3] = 0xff;

					bitShift--;
					if (bitShift == -1) bitShift = 7;
				}
			}
			this.workingBitmap.UnlockBits(bdata);

		}

	}
}

