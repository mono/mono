//		
//			System.Windows.Forms Demo app
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
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
		
	    	private void InitializeWidgets()
	    	{
    		button1.Location = new Point(150, 28);
       	  	button1.Name = "button1";
    	  	button1.Size = new Size(128, 44);
       	 	button1.Text = "Apply";
    	  	button1.Click += new EventHandler(this.button1_Click);    
         	button1.Enabled = false;
  
			button2.Location = new Point(150, 85);
       	  	button2.Name = "button2";
    	  	button2.Size = new Size(128, 44);
       	 	button2.Text = "File";
    	  	button2.Click += new EventHandler(this.button2_Click); 

  	  	copybutton.Click += new EventHandler(this.copybutton_Click); 
  	  	pastebutton.Click += new EventHandler(this.pastebutton_Click); 
  	  	cutbutton.Click += new EventHandler(this.cutbutton_Click); 

		copybutton.Location = new Point(320, 80); 
		pastebutton.Location = new Point(320, 100); 
		cutbutton.Location = new Point(320, 120);

		copybutton.Size = new Size(150, 20); 
		pastebutton.Size = new Size(150, 20); 
		cutbutton.Size = new Size(150, 20); 

		copybutton.Text ="Copy";
		pastebutton.Text ="Paste";
		cutbutton.Text ="Cut";

    	    	text1.Location = new Point(320,48);
    	    	text1.Name = "textBox1";
   	    		text1.Size = new Size(150, 22);
	 			text1.Text = this.button1.Name;

	    		bar1.Location = new Point(0, 230);
	    		bar1.Size = new Size(512, 20);
	    		bar1.Text = "This is a ProgressBar";
				bar1.Value = 25;

  	    		label1.Location = new Point(330, 20);
	    		label1.Text = "This is a Label";	
			
				check1.Location = new Point(150, 160);
				check1.Size = new Size(180, 20);
				check1.Text = "arbitrary CheckBox";
				check1.Checked = false;
		
				opt1.Location = new Point(20, 160);
				opt1.Size = new Size(100, 20);
				opt1.Text = "CenterImage";

				opt2.Location = new Point(20,180);
				opt2.Size = new Size(100, 20);
				opt2.Text = "StretchImage";
				
				opt3.Location = new Point(20,200);
				opt3.Size = new Size(100, 20);
				opt3.Text = "Normal";

				frame1.Location = new Point(15, 140);
				frame1.Size = new Size (110, 85);
				frame1.Text = "Properties";
				
				pbox.Location = new Point (25, 28);
				pbox.Size = new Size(100, 100);
				//
				//Add you image name and path below
				//pbox.File = "/home/jstrike/Shared/7804.jpg";

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
									this.label1 });
          this.Size = new Size(512, 250);
    		}
    	
    	public GtkForm()
    	{
    	   	InitializeWidgets();

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
