using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace swf_texture {
	public partial class TextureForm: Form {

		TextureBrush tb;
		private string filename;

		public TextureForm (string[] args)
		{
			filename = args.Length > 0 ? args [1] : String.Empty;

			InitializeComponent ();

			tb = new TextureBrush (pictureBox1.Image);
		}

		private void textBox1_TextChanged (object sender, EventArgs e)
		{
			try {
				float f = Single.Parse (rotationTextBox.Text);
				tb.RotateTransform (f);
				rotationTextBox.BackColor = SystemColors.Window;
			}
			catch {
				rotationTextBox.BackColor = Color.Red;
			}
			finally {
				Invalidate ();
				Update ();
			}
		}

		private void TextureForm_Paint (object sender, PaintEventArgs e)
		{
			e.Graphics.FillRectangle (tb, e.Graphics.VisibleClipBounds);
		}

		private void resetButton_Click (object sender, EventArgs e)
		{
			tb.Transform = new Matrix ();
			Invalidate ();
			Update ();
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
//			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (TextureForm));
			this.pictureBox1 = new System.Windows.Forms.PictureBox ();
			this.rotationTextBox = new System.Windows.Forms.TextBox ();
			this.label1 = new System.Windows.Forms.Label ();
			this.resetButton = new System.Windows.Forms.Button ();
			((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit ();
			this.SuspendLayout ();
			// 
			// pictureBox1
			// 
//			this.pictureBox1.Image = ((System.Drawing.Image) (resources.GetObject ("pictureBox1.Image")));
			if (filename.Length > 0) {
				this.pictureBox1.Image = Image.FromFile (filename);
			} else {
				Bitmap b = new Bitmap (32, 32);
				using (Graphics g = Graphics.FromImage (b)) {
					g.DrawLine (Pens.Red, 0, 0, 32, 32);
				}
				this.pictureBox1.Image = b;
			}
			this.pictureBox1.Location = new System.Drawing.Point (12, 217);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size (73, 80);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// rotationTextBox
			// 
			this.rotationTextBox.Location = new System.Drawing.Point (94, 233);
			this.rotationTextBox.Name = "rotationTextBox";
			this.rotationTextBox.Size = new System.Drawing.Size (44, 20);
			this.rotationTextBox.TabIndex = 1;
			this.rotationTextBox.Text = "0";
			this.rotationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.rotationTextBox.TextChanged += new System.EventHandler (this.textBox1_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point (91, 217);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (47, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Rotation";
			// 
			// resetButton
			// 
			this.resetButton.Location = new System.Drawing.Point (94, 274);
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size (75, 23);
			this.resetButton.TabIndex = 3;
			this.resetButton.Text = "Reset";
			this.resetButton.UseVisualStyleBackColor = true;
			this.resetButton.Click += new System.EventHandler (this.resetButton_Click);
			// 
			// TextureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (292, 309);
			this.Controls.Add (this.resetButton);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.rotationTextBox);
			this.Controls.Add (this.pictureBox1);
			this.Name = "TextureForm";
			this.Text = "SWF Texture Demo";
			this.Paint += new System.Windows.Forms.PaintEventHandler (this.TextureForm_Paint);
			((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit ();
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox rotationTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button resetButton;

		static void Main (string[] args)
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new TextureForm (args));
		}
	}
}
