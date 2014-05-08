using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Drawing.Imaging;
using System.IO;

// Program to display a 16-bit raw image file.
// Written by Amarnath S, Bangalore, India, December 2008.

namespace ImageDraw
{
	/// <summary>
	/// Summary description for Form1.
	/// 
	/// It is assumed that the pixels comprising the image are stored 
	/// from left to right, and top to bottom, as unsigned short values.
	/// It is also assumed that the width and height of the image are 
	/// identical, so that the image dimensions can be computed by just 
	/// using the square root function.
	/// The algorithm is briefly as follows:
	///  1. Read the raw image file and store the pixel values into an 
	///     ArrayList called pixel16.
	///  2. Create a bitmap of the required dimensions. Create the bitmap 
	///     with the pixel format of Format24bppRgb, and set the red, green 
	///     and blue colors to be identical, since the images are grayscale. 
	///     Scale the 16-bit grayscale interval of 0 - 65535 to an 8-bit 
	///     interval of 0 - 255, since each color is represented by 8 bits. 
	///     Use the BitmapData class to populate the bitmap pixel values 
	///     from the abovementioned pixel16 array list. Use LockBits and 
	///     UnlockBits appropriately. Display the image. It is also 
	///     possible to use SetPixel to set the values, but this method 
	///     is painfully slow compared to what is used here.	
	///  3. Override Paint to draw the image.
	///  
	///  Compile with the unsafe flag on.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
    {
		private System.Windows.Forms.Button bnOpenImage;
	    private PictureBox pictureBox1;                 // Bitmap object

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.bnOpenImage = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // bnOpenImage
            // 
            this.bnOpenImage.Location = new System.Drawing.Point(14, 12);
            this.bnOpenImage.Name = "bnOpenImage";
            this.bnOpenImage.Size = new System.Drawing.Size(105, 33);
            this.bnOpenImage.TabIndex = 0;
            this.bnOpenImage.Text = "Open Raw Image";
            this.bnOpenImage.Click += new System.EventHandler(this.bnOpenImage_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(899, 530);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(899, 530);
            this.Controls.Add(this.bnOpenImage);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Display 32-bits ARGB Raw Image Files";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		// Open the Raw image file for reading.
		private void bnOpenImage_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Raw Files(*.*)|*.*";
			ofd.ShowDialog();
			if( ofd.FileName.Length > 0 )
			{
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

				pictureBox1.Image = getRawImageFmt32(ofd.FileName, 1280, 800);
 
            }
			ofd.Dispose();		
		}

        private Bitmap getRawImageFmt32(String rawFileName, int imgWidth, int imgHeight)
        {
            Bitmap bitmap = null;
            BinaryReader reader = new BinaryReader(File.Open(rawFileName, FileMode.Open));
            if (reader.BaseStream.Length != imgWidth * imgHeight * 4)
            {
                MessageBox.Show("图像格式不正确!");
                goto __END__;
            }

            bitmap = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, imgWidth, imgHeight),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // This 'unsafe' part of the code populates the bitmap bmp with data stored in pixel16.
            // It does so using pointers, and therefore the need for 'unsafe'. 
            unsafe
            {
                int i, j;
                for (i = 0; i < bmpData.Height; ++i)
                {
                    byte* row = (byte*)bmpData.Scan0 + (i * bmpData.Stride);
                    for (j = 0; j < bmpData.Width; ++j)
                    {
                        UInt32 iArgb = (UInt32)reader.ReadInt32();
                        byte[] bytes = System.BitConverter.GetBytes(iArgb);

                        row[j * 4] = bytes[0];                // Blue 
                        row[j * 4 + 1] = bytes[1];            // Green
                        row[j * 4 + 2] = bytes[2];            // Red
                        row[j * 4 + 3] = bytes[3];            // Alpha
                    }
                }
            }

            bitmap.UnlockBits(bmpData);

        __END__:
            reader.Close();
            return bitmap;
        }	

		// Create the Bitmap object and populate its pixel data with the stored pixel values.
        //private void CreateBitmap()
        //{
        //    bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, width, height),
        //        System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

        //    // This 'unsafe' part of the code populates the bitmap bmp with data stored in pixel16.
        //    // It does so using pointers, and therefore the need for 'unsafe'. 
        //    unsafe
        //    {
        //        int    i, j, j1, i1;
        //        for (i = 0; i < bmd.Width; ++i)
        //        {
        //            byte* row = (byte*)bmd.Scan0 + (i * bmd.Stride);
        //            i1 = i * bmd.Width;

        //            for (j = 0; j < bmd.Height; ++j)
        //            {
        //                UInt32 iArgb = (UInt32)pixels32[bmd.Width * j + i];
        //                byte[] bytes = System.BitConverter.GetBytes(iArgb);
        //                j1 = j * 4;

        //                //row[j1] = bytes[3]; //Alpha
        //                //row[j1 + 1] = bytes[2];            // Red
        //                //row[j1 + 2] = bytes[1];            // Green
        //                //row[j1 + 3] = bytes[0];            // Blue

        //                row[j1] = bytes[0];                // Blue 
        //                row[j1 + 1] = bytes[1];            // Green
        //                row[j1 + 2] = bytes[2];            // Red
        //                row[j1 + 3] = bytes[3];            // Alpha
        //            }

        //        //for (i = 0; i < bmd.Height; ++i)
        //        //{
        //        //    byte* row = (byte*)bmd.Scan0 + (i * bmd.Stride);
        //        //    i1 = i * bmd.Height;

        //        //    for (j = 0; j < bmd.Width; ++j)
        //        //    {
        //        //        //sVal        = (UInt32)(pixels32[i1 + j]);


        //        //        UInt32 iArgb = (UInt32)pixels32[i1 + j];
        //        //        byte[] bytes = System.BitConverter.GetBytes(iArgb);
        //        //        j1 = j * pixelSize;

        //        //        //row[j1] = bytes[3]; //Alpha
        //        //        //row[j1 + 1] = bytes[2];            // Red
        //        //        //row[j1 + 2] = bytes[1];            // Green
        //        //        //row[j1 + 3] = bytes[0];            // Blue

        //        //        row[j1] = bytes[0];                // Blue 
        //        //        row[j1 + 1] = bytes[1];            // Green
        //        //        row[j1 + 2] = bytes[2];            // Red
        //        //        row[j1 + 3] = bytes[3];            // Alpha
        //        //    }
        //        }
        //    }            
        //    bmp.UnlockBits(bmd);
        //}	
	}
}