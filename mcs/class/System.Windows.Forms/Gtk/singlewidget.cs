//
// System.Windows.Forms Demo app
//
// Authors: 
//    Philip Van Hoof (me@freax.org)
//
// I use this sample for testing a single widget
// 

using System;
using System.Drawing;
using System.Windows.Forms;

namespace singlewidget
{

	public class GtkForm : System.Windows.Forms.Form
	{
		private HScrollBar hScrollBar1 = new HScrollBar();

		private void InitializeWidgets()
		{

			this.hScrollBar1.Location = new System.Drawing.Point(50, 60);
			this.hScrollBar1.Maximum = 200;
			this.hScrollBar1.Minimum = 10;
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.Size = new System.Drawing.Size(360, 24);


			this.Controls.AddRange(new System.Windows.Forms.Control[] { 
						this.hScrollBar1 });

			this.Size = new Size(512, 250);
		}

		public GtkForm()
		{
			InitializeWidgets();
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
}
