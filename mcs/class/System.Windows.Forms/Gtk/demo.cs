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
	    	private Button button1 = new Button(); 
			private Label label1 = new Label();
	    	private TextBox text1 = new TextBox(); 
	    	private ProgressBar bar1 = new ProgressBar();
			private CheckBox check1 = new CheckBox();
	    	private RadioButton opt1 = new RadioButton();
			private RadioButton opt2 = new RadioButton();
			private GroupBox frame1 = new GroupBox();
			private PictureBox pbox = new PictureBox();
			
	    	private void InitializeWidgets()
	    	{
    		  	button1.Location = new Point(150, 28);
       	  	button1.Name = "button1";
    	  	  	button1.Size = new Size(128, 44);
       	 	button1.Text = "Apply";
    	  		button1.Click += new EventHandler(this.button1_Click);    
            
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
			
				check1.Location = new Point(28, 140);
				check1.Size = new Size(180, 20);
				check1.Text = "Stretch Image";
				check1.Checked = false;
		
				opt1.Location = new Point(280, 155);
				opt1.Size = new Size(180, 20);
				opt1.Text = "Option";

				opt2.Location = new Point(280,180);
				opt2.Size = new Size(180, 20);
				opt2.Text = "Option2";

				frame1.Location = new Point(260, 130);
				frame1.Size = new Size (110, 85);
				frame1.Text = "Frame1";
				
				pbox.Location = new Point (25, 28);
				pbox.Size = new Size(100, 100);
				//
				//Add you image name and path below
				pbox.File = "/home/jstrike/Shared/7804.jpg";

          this.Controls.AddRange(new System.Windows.Forms.Control[] { 
									this.button1,
                     	this.text1, 
									this.bar1, 
									this.check1,
									this.opt1,
									this.opt2,
									this.frame1,
									this.pbox,
									this.label1 });
          this.Size = new Size(512, 250);
    		}
    	
    	public GtkForm()
    	{
    	   	InitializeWidgets();

    	}

		private void button1_Click(object sender, EventArgs e){ 
			
				if (this.check1.Checked) { 
					this.pbox.Stretch = true;
				}
				if (!this.check1.Checked){
					this.pbox.Stretch = false;
				}			
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
