using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
//using System.Data;
using System.IO;

namespace NotePadExample
{
        public class Notepad : System.Windows.Forms.Form
        {
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItemFile;
		private System.Windows.Forms.MenuItem menuItemNew;
		private System.Windows.Forms.MenuItem menuItemOpen;
		private System.Windows.Forms.MenuItem menuItemSave;
		private System.Windows.Forms.MenuItem menuItemExit;
		private System.Windows.Forms.MenuItem menuItemEdit;
		private System.Windows.Forms.MenuItem menuItemCut;
		private System.Windows.Forms.MenuItem menuItemCopy;
		private System.Windows.Forms.MenuItem menuItemPaste;
		private System.Windows.Forms.MenuItem menuItemSelectAll;
		private System.Windows.Forms.MenuItem menuItemHelp;
		private System.Windows.Forms.MenuItem menuItemAbout;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.MenuItem menuItemSep2;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.MenuItem menuItemSep1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.MenuItem menuItemBlank;
		private System.Windows.Forms.MenuItem menuItemGeneral;
		
		private System.ComponentModel.Container components = null;

		public Notepad()
		 {
		     InitializeComponent();
		 }

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

		private void InitializeComponent()
		 {
			 this.mainMenu1 = new System.Windows.Forms.MainMenu();
			 this.menuItemFile = new System.Windows.Forms.MenuItem();
			 this.menuItemNew = new System.Windows.Forms.MenuItem();
			 this.menuItemOpen = new System.Windows.Forms.MenuItem();
			 this.menuItemSep1 = new System.Windows.Forms.MenuItem();
			 this.menuItemSave = new System.Windows.Forms.MenuItem();
			 this.menuItemSep2 = new System.Windows.Forms.MenuItem();
			 this.menuItemExit = new System.Windows.Forms.MenuItem();
			 this.menuItemEdit = new System.Windows.Forms.MenuItem();
			 this.menuItemCut = new System.Windows.Forms.MenuItem();
			 this.menuItemCopy = new System.Windows.Forms.MenuItem();
			 this.menuItemPaste = new System.Windows.Forms.MenuItem();
			 this.menuItemSelectAll = new System.Windows.Forms.MenuItem();
			 this.menuItemHelp = new System.Windows.Forms.MenuItem();
			 this.menuItemAbout = new System.Windows.Forms.MenuItem();
			 this.menuItemBlank = new System.Windows.Forms.MenuItem();
			 this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			 this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			 this.textBox1 = new System.Windows.Forms.TextBox();
			 this.menuItemGeneral = new System.Windows.Forms.MenuItem();
			 this.SuspendLayout();

			 this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				 this.menuItemFile,
				   this.menuItemEdit,
				   this.menuItemHelp,
				   this.menuItemBlank});
			 
			 this.menuItemFile.Index = 0;
			 this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				 this.menuItemNew,
				   this.menuItemOpen,
				   this.menuItemSep1,
				   this.menuItemSave,
				   this.menuItemSep2,
				   this.menuItemExit}
							   );
			 this.menuItemFile.Text = "File";
			 
			 this.menuItemNew.Index = 0;
			 this.menuItemNew.Text = "&New";
			 this.menuItemNew.Click += new System.EventHandler(this.menuItemNew_Click);

			 this.menuItemOpen.Index = 1;
			 this.menuItemOpen.Text = "&Open";
			 this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);

			 this.menuItemSep1.Index = 2;
			 this.menuItemSep1.Text = "-";

			 this.menuItemSave.Index = 3;
			 this.menuItemSave.Text = "&Save";
			 this.menuItemSave.Click += new System.EventHandler(this.menuItemSave_Click);
			 
			 this.menuItemSep2.Index = 4;
			 this.menuItemSep2.Text = "-";

			 this.menuItemExit.Index = 5;
			 this.menuItemExit.Text = "Exit";
			 this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click_1);

			 this.menuItemEdit.Index = 1;
			 this.menuItemEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				 this.menuItemCut,
				   this.menuItemCopy,
				   this.menuItemPaste,
				   this.menuItemSelectAll}
							   );
			 this.menuItemEdit.Text = "Edit";

			 this.menuItemCut.Index = 0;
			 this.menuItemCut.Text = "&Cut";
			 this.menuItemCut.Click += new System.EventHandler(this.menuItemCut_Click);

			 this.menuItemCopy.Index = 1;
			 this.menuItemCopy.Text = "&Copy";
			 this.menuItemCopy.Click += new System.EventHandler(this.menuItemCopy_Click);

			 this.menuItemPaste.Index = 2;
			 this.menuItemPaste.Text = "Paste";
			 this.menuItemPaste.Click += new System.EventHandler(this.menuItemPaste_Click);

			 this.menuItemSelectAll.Index = 3;
			 this.menuItemSelectAll.Text = "&Select All";
			 this.menuItemSelectAll.Click += new System.EventHandler(this.menuItemSelectAll_Click);
			 
			 this.menuItemHelp.Index = 2;
			 this.menuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				 this.menuItemAbout,
				   this.menuItemGeneral}
							    );
			 this.menuItemHelp.Text = "Help";
			 
			 this.menuItemAbout.Index = 0;
			 this.menuItemAbout.Text = "About";
			 this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);

			 this.menuItemBlank.Index = 3;
			 this.menuItemBlank.Text = "";

			 this.saveFileDialog1.FileName = "doc1";

			 this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			 this.textBox1.Multiline = true;
			 this.textBox1.Name = "textBox1";
			 this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			 this.textBox1.Size = new System.Drawing.Size(544, 321);
			 this.textBox1.TabIndex = 0;
			 this.textBox1.Text = "";
			 this.textBox1.TextChanged += new
			   System.EventHandler(this.textBox1_TextChanged_1);
			 
			 this.menuItemGeneral.Index = 1;
			 this.menuItemGeneral.Text = "General";
			 this.menuItemGeneral.Click += new System.EventHandler(this.menuItemGeneral_Click);
			 
			 this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			 this.ClientSize = new System.Drawing.Size(544, 321);
			 this.Controls.AddRange(new System.Windows.Forms.Control[] {
				 this.textBox1}
						);
			 this.ImeMode = System.Windows.Forms.ImeMode.Off;
			 this.Menu = this.mainMenu1;
			 this.Name = "Notepad";
			 this.Text = "Notepad MWF Example";
			 this.Load += new System.EventHandler(this.Form1_Load);
			 this.ResumeLayout(false);			 
		 }
		
		[STAThread]
		public static void Main()
		{
			Application.Run(new Notepad());
		}
		
		private void Form1_Load(object sender, System.EventArgs e)
		{
			 
		}
		
		private void menuItemOpen_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.ShowDialog();
			StreamReader sr = new StreamReader(openFileDialog1.FileName);
			textBox1.Text = sr.ReadToEnd();
			sr.Close();			 
		}
		
		private void menuItemCut_Click(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(textBox1.SelectedText);
			textBox1.SelectedText="";
			
		}
		
		private void menuItemPaste_Click(object sender, System.EventArgs e)
		{
			textBox1.Paste();
		}
		
		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
			
		}
		
		private void menuItemAbout_Click(object sender, System.EventArgs e)
		{
			Form helpnew = new Form();
			helpnew.Show();
			
		}
		
		private void textBox1_TextChanged_1(object sender, System.EventArgs e)
		{
			
		}
		
		private void menuItemExit_Click(object sender, System.EventArgs e)
		{
			
		}
		
		private void menuItemExit_Click_1(object sender, System.EventArgs e)
		{
			Dispose(true);
		}
		
		private void menuItemCopy_Click(object sender, System.EventArgs e)
		{
			textBox1.Copy();
		}
		
		private void menuItemSelectAll_Click(object sender, System.EventArgs e)
		{
			textBox1.SelectAll();			
		}
		
		private void menuItemNew_Click(object sender, System.EventArgs e)
		{
			textBox1.Clear();			
		}
		
		private void menuItemSave_Click(object sender, System.EventArgs e)
		{
			saveFileDialog1.FileName = "*.txt";
			//saveFileDialog1.FilterIndex ="*.txt";
			saveFileDialog1.ShowDialog();
			//MessageBox.Show (saveFileDialog1.FileName);
			StreamWriter sw = new StreamWriter(saveFileDialog1.FileName);
			sw.Write (textBox1.Text);
			sw.Close();
		}
		
		private void menuItemGeneral_Click(object sender, System.EventArgs e)
		{
			Form gen = new Form ();
			gen.Show();
			
		}		
	}
}
