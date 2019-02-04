using System;
using System.Drawing;
using System.Windows.Forms;

namespace Test
{
	static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}

	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.linkLabel3 = new System.Windows.Forms.LinkLabel();
			this.linkLabel4 = new System.Windows.Forms.LinkLabel();
			this.linkLabel5 = new System.Windows.Forms.LinkLabel();
			this.linkLabel6 = new System.Windows.Forms.LinkLabel();
			this.linkLabel7 = new System.Windows.Forms.LinkLabel();
			this.linkLabel8 = new System.Windows.Forms.LinkLabel();
			this.linkLabel9 = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// linkLabel1
			//
			this.linkLabel1.AutoSize = false;
			this.linkLabel1.Location = new System.Drawing.Point(50, 20);
			this.linkLabel1.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(100, 20);
			this.linkLabel1.TabIndex = 1;
			this.linkLabel1.Text = "TopLeft";
			this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(0, 7);
			//
			// linkLabel2
			//
			this.linkLabel2.AutoSize = false;
			this.linkLabel2.Location = new System.Drawing.Point(50, 50);
			this.linkLabel2.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.Size = new System.Drawing.Size(100, 20);
			this.linkLabel2.TabIndex = 2;
			this.linkLabel2.Text = "TopCenter";
			this.linkLabel2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.linkLabel2.LinkArea = new System.Windows.Forms.LinkArea(0, 9);
			//
			// linkLabel3
			//
			this.linkLabel3.AutoSize = false;
			this.linkLabel3.Location = new System.Drawing.Point(50, 80);
			this.linkLabel3.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel3.Name = "linkLabel3";
			this.linkLabel3.Size = new System.Drawing.Size(100, 20);
			this.linkLabel3.TabIndex = 3;
			this.linkLabel3.Text = "TopRight";
			this.linkLabel3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.linkLabel3.LinkArea = new System.Windows.Forms.LinkArea(0, 8);
			//
			// linkLabel4
			//
			this.linkLabel4.AutoSize = false;
			this.linkLabel4.Location = new System.Drawing.Point(50, 110);
			this.linkLabel4.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel4.Name = "linkLabel4";
			this.linkLabel4.Size = new System.Drawing.Size(100, 20);
			this.linkLabel4.TabIndex = 4;
			this.linkLabel4.Text = "MiddleLeft";
			this.linkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.linkLabel4.LinkArea = new System.Windows.Forms.LinkArea(0, 10);
			//
			// linkLabel5
			//
			this.linkLabel5.AutoSize = false;
			this.linkLabel5.Location = new System.Drawing.Point(50, 140);
			this.linkLabel5.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel5.Name = "linkLabel5";
			this.linkLabel5.Size = new System.Drawing.Size(100, 20);
			this.linkLabel5.TabIndex = 5;
			this.linkLabel5.Text = "MiddleCenter";
			this.linkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkLabel5.LinkArea = new System.Windows.Forms.LinkArea(0, 12);
			//
			// linkLabel6
			//
			this.linkLabel6.AutoSize = false;
			this.linkLabel6.Location = new System.Drawing.Point(50, 170);
			this.linkLabel6.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel6.Name = "linkLabel6";
			this.linkLabel6.Size = new System.Drawing.Size(100, 20);
			this.linkLabel6.TabIndex = 6;
			this.linkLabel6.Text = "MiddleRight";
			this.linkLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.linkLabel6.LinkArea = new System.Windows.Forms.LinkArea(0, 11);
			//
			// linkLabel7
			//
			this.linkLabel7.AutoSize = false;
			this.linkLabel7.Location = new System.Drawing.Point(50, 200);
			this.linkLabel7.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel7.Name = "linkLabel7";
			this.linkLabel7.Size = new System.Drawing.Size(100, 20);
			this.linkLabel7.TabIndex = 4;
			this.linkLabel7.Text = "BottomLeft";
			this.linkLabel7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.linkLabel7.LinkArea = new System.Windows.Forms.LinkArea(0, 10);
			//
			// linkLabel8
			//
			this.linkLabel8.AutoSize = false;
			this.linkLabel8.Location = new System.Drawing.Point(50, 230);
			this.linkLabel8.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel8.Name = "linkLabel8";
			this.linkLabel8.Size = new System.Drawing.Size(100, 20);
			this.linkLabel8.TabIndex = 5;
			this.linkLabel8.Text = "BottomCenter";
			this.linkLabel8.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.linkLabel8.LinkArea = new System.Windows.Forms.LinkArea(0, 12);
			//
			// linkLabel9
			//
			this.linkLabel9.AutoSize = false;
			this.linkLabel9.Location = new System.Drawing.Point(50, 260);
			this.linkLabel9.Margin = new System.Windows.Forms.Padding(3);
			this.linkLabel9.Name = "linkLabel9";
			this.linkLabel9.Size = new System.Drawing.Size(100, 20);
			this.linkLabel9.TabIndex = 6;
			this.linkLabel9.Text = "BottomRight";
			this.linkLabel9.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.linkLabel9.LinkArea = new System.Windows.Forms.LinkArea(0, 11);
			//
			// Form1
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(200, 300);
			this.Controls.AddRange(new Control[] {
				this.linkLabel1,
				this.linkLabel2,
				this.linkLabel3,
				this.linkLabel4,
				this.linkLabel5,
				this.linkLabel6,
				this.linkLabel7,
				this.linkLabel8,
				this.linkLabel9});
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.LinkLabel linkLabel2;
		private System.Windows.Forms.LinkLabel linkLabel3;
		private System.Windows.Forms.LinkLabel linkLabel4;
		private System.Windows.Forms.LinkLabel linkLabel5;
		private System.Windows.Forms.LinkLabel linkLabel6;
		private System.Windows.Forms.LinkLabel linkLabel7;
		private System.Windows.Forms.LinkLabel linkLabel8;
		private System.Windows.Forms.LinkLabel linkLabel9;
	}
}
