//
// System.Windows.Forms Demo app
//
// Authors: 
//    Joel Basson (jstrike@mweb.co.za)
//    Philip Van Hoof (me@freax.org)
//
//

using System;
using System.Drawing;
using System.Windows.Forms;

namespace demo
{
	
	public class GtkForm : System.Windows.Forms.Form
	{
		private Button copybutton = new Button();
		private Button pastebutton = new Button();
		private Button cutbutton = new Button();
		private Button button1 = new Button(); 
		private Button button2 = new Button(); 
		private Label label1 = new Label();
		private TextBox text1 = new TextBox(); 
		private ProgressBar bar1 = new ProgressBar();
		private CheckBox check1 = new CheckBox();
		private RadioButton opt1 = new RadioButton();
		private RadioButton opt2 = new RadioButton();
		private RadioButton opt3 = new RadioButton();
		private GroupBox frame1 = new GroupBox();
		private PictureBox pbox = new PictureBox();
		private FileDialog fdialog = new FileDialog();
		private VScrollBar vScrollBar1 = new VScrollBar();
		private HScrollBar hScrollBar1 = new HScrollBar();

		private void InitializeWidgets()
		{
			this.vScrollBar1.Location = new Point(10, 10);
			this.vScrollBar1.Maximum = 200;
			this.vScrollBar1.Name = "vScrollBar1";
			this.vScrollBar1.Size = new Size(24, 200);
			this.vScrollBar1.TabIndex = 0;
			this.vScrollBar1.ValueChanged += new EventHandler(this.vScrollBar1_ValueChanged);

			this.hScrollBar1.Location = new System.Drawing.Point(50, 60);
			this.hScrollBar1.Maximum = 200;
			this.hScrollBar1.Minimum = 10;
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.Size = new System.Drawing.Size(360, 24);
			this.hScrollBar1.ValueChanged += new EventHandler(this.hScrollBar1_ValueChanged);

			this.button1.Location = new Point(150, 28);
			this.button1.Name = "button1";
			this.button1.Size = new Size(128, 44);
			this.button1.Text = "Apply";
			this.button1.Click += new EventHandler(this.button1_Click);    
			this.button1.Enabled = false;

			this.button2.Location = new Point(150, 85);
			this.button2.Name = "button2";
			this.button2.Size = new Size(128, 44);
			this.button2.Text = "File";
			this.button2.Click += new EventHandler(this.button2_Click); 

			this.copybutton.Click += new EventHandler(this.copybutton_Click); 
			this.pastebutton.Click += new EventHandler(this.pastebutton_Click); 
			this.cutbutton.Click += new EventHandler(this.cutbutton_Click); 

			this.copybutton.Location = new Point(320, 80); 
			this.pastebutton.Location = new Point(320, 100); 
			this.cutbutton.Location = new Point(320, 120);

			this.copybutton.Size = new Size(150, 20); 
			this.pastebutton.Size = new Size(150, 20); 
			this.cutbutton.Size = new Size(150, 20); 

			this.copybutton.Text ="Copy";
			this.pastebutton.Text ="Paste";
			this.cutbutton.Text ="Cut";

			this.text1.Location = new Point(320,48);
			this.text1.Name = "textBox1";
			this.text1.Size = new Size(150, 22);
			this.text1.Text = this.button1.Name;

			this.bar1.Location = new Point(0, 230);
			this.bar1.Size = new Size(512, 20);
			this.bar1.Text = "This is a ProgressBar";
			this.bar1.Value = 25;

			this.label1.Location = new Point(330, 20);
			this.label1.Text = "This is a Label";	

			this.check1.Location = new Point(150, 160);
			this.check1.Size = new Size(180, 20);
			this.check1.Text = "arbitrary CheckBox";
			this.check1.Checked = false;

			this.opt1.Location = new Point(20, 160);
			this.opt1.Size = new Size(100, 20);
			this.opt1.Text = "CenterImage";

			this.opt2.Location = new Point(20,180);
			this.opt2.Size = new Size(100, 20);
			this.opt2.Text = "StretchImage";

			this.opt3.Location = new Point(20,200);
			this.opt3.Size = new Size(100, 20);
			this.opt3.Text = "Normal";

			this.frame1.Location = new Point(15, 140);
			this.frame1.Size = new Size (110, 85);
			this.frame1.Text = "Properties";

			this.pbox.Location = new Point (25, 28);
			this.pbox.Size = new Size(100, 100);

			//
			// Add you image name and path below
			// pbox.File = "/home/jstrike/Shared/7804.jpg";
			//

			this.Controls.AddRange(new System.Windows.Forms.Control[] { 
						this.button1,
						this.button2,
						this.copybutton,
						this.pastebutton,
						this.cutbutton,
						this.text1, 
						this.bar1, 
						this.check1,
						this.opt1,
						this.opt2,
						this.opt3,
						this.frame1,
						this.pbox,
						this.fdialog,
						this.vScrollBar1,
						this.hScrollBar1,
						this.label1 });

			this.Size = new Size(512, 250);
		}

		public GtkForm()
		{
			InitializeWidgets();
		}

		private void set_Text1_to_scrollbarvalues() {
			this.text1.Text = String.Format ("{0}, {1}", this.vScrollBar1.Value, this.hScrollBar1.Value);
		}

		private void vScrollBar1_ValueChanged (object sender, EventArgs e) { 
			this.set_Text1_to_scrollbarvalues ();
		}

		private void hScrollBar1_ValueChanged (object sender, EventArgs e) { 
			this.set_Text1_to_scrollbarvalues ();
		}


		private void copybutton_Click(object sender, EventArgs e){ 
			//text1.Select (1, 4);
			text1.Copy();
		}

		private void pastebutton_Click(object sender, EventArgs e){ 
			//text1.SelectAll();
			text1.Paste();
		}

		private void cutbutton_Click(object sender, EventArgs e){ 
			text1.Cut();
		}

		private void button1_Click(object sender, EventArgs e){ 
		
			pbox.File = fdialog.OpenFile;
				if (this.opt2.Checked) { 
					this.pbox.SizeMode = PictureBoxSizeMode.StretchImage;
				}
				if (this.opt1.Checked){
					this.pbox.SizeMode = PictureBoxSizeMode.CenterImage;
				}	
				if (this.opt3.Checked){
					this.pbox.SizeMode = PictureBoxSizeMode.Normal;
				}	
  		}

		private void button2_Click(object sender, EventArgs e){ 							
  			fdialog.ShowDialog();
			button1.Enabled = true;
		}

		}
	
	public class GtkMain
	{
		public static void Main()
		{
			GtkForm form1 = new GtkForm ();
			form1.Text = "System.Windows.Forms at work!";			
			Application.Run(form1);
		}
	}
}
