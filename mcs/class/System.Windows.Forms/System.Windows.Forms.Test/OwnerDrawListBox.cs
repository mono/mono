//------------------------------------------------------------------------------
/// <copyright from='1997' to='2001' company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///
///    This source code is intended only as a supplement to Microsoft
///    Development Tools and/or on-line documentation.  See these other
///    materials for detailed information regarding Microsoft code samples.
///
/// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.WinForms.Cs.OwnerDrawListBox {
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Reflection;
    using System.Windows.Forms;

    // <doc>
    // <desc>
    //     This sample control demonstrates various properties and
    //     methods for the ListBox control.
    // </desc>
    // </doc>
    //
    public class OwnerDrawListBox : System.Windows.Forms.Form {

        //Used to paint the list box
        private Brush[] listBoxBrushes ;
        private int[] listBoxHeights = new int[] {50, 25, 33, 15} ;
        private FontFamily sansSerifFontFamily;

        // <doc>
        // <desc>
        //     Public Constructor
        // </desc>
        // </doc>
        public OwnerDrawListBox() : base() {
            sansSerifFontFamily = new FontFamily (GenericFontFamilies.SansSerif);

            // This call is required for support of the Windows Forms Form Designer.
            InitializeComponent();


            //Set up the brushes we are going to use

            //Load the image to be used for the background from the exe's resource fork
/*            
            Image backgroundImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("colorbars.jpg"));
            //Now create the brush we are going to use to paint the background
            Brush backgroundBrush = new TextureBrush(backgroundImage);
*/            
			Brush backgroundBrush = new SolidBrush(Color.Blue);
            Rectangle r = new Rectangle(0, 0, listBox1.Width, 100);
            LinearGradientBrush lb = new LinearGradientBrush(r, Color.Red, Color.Yellow,LinearGradientMode.Horizontal);


            listBoxBrushes = new Brush[]
                {
                    backgroundBrush,
                    Brushes.LemonChiffon,
                    lb,
                    Brushes.PeachPuff
                };

        }

        // <doc>
        // <desc>
        //     OwnerDrawListBox overrides dispose so it can clean up the
        //     component list.
        // </desc>
        // </doc>
        //
        protected override void Dispose(bool disposing)
        {
           if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
           }
           base.Dispose(disposing);
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e) {

            // The following method should generally be called before drawing.
            // It is actually superfluous here, since the subsequent drawing
            // will completely cover the area of interest.
            e.DrawBackground();

            //The system provides the context
            //into which the owner custom-draws the required graphics.
            //The context into which to draw is e.graphics.
            //The index of the item to be painted is e.Index.
            //The painting should be done into the area described by e.Bounds.
            Brush brush = listBoxBrushes[e.Index];
            e.Graphics.FillRectangle(brush, e.Bounds);
            e.Graphics.DrawRectangle(SystemPens.WindowText, e.Bounds);

            bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? true : false;

            string displayText = "ITEM #" + e.Index;
            displayText = displayText + (selected ? " SELECTED" : "");

            e.Graphics.DrawString(displayText, this.Font, Brushes.Black, e.Bounds);

            e.DrawFocusRectangle();
        }

        //Return the height of the item to be drawn
        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e) {
            //Work out what the text will be
            string displayText = "ITEM #" + e.Index;

            //Get width & height of string
            SizeF stringSize=e.Graphics.MeasureString(displayText, this.Font);

            //Account for top margin
            stringSize.Height += 6;

            //Now set height to taller of default and text height
            if (listBoxHeights[e.Index] > stringSize.Height)
                e.ItemHeight = listBoxHeights[e.Index];
            else
                e.ItemHeight = (int)stringSize.Height;
        }

        // NOTE: The following code is required by the Windows Forms Form Designer
        // It can be modified using the Windows Forms Form Designer.
        // Do not modify it using the code editor.
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ListBox listBox1;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox1.ColumnWidth = 144;
            this.listBox1.ForeColor = (System.Drawing.Color)SystemColors.WindowText;
            this.listBox1.IntegralHeight = false;
            this.listBox1.Items.AddRange(new object[4] {"First", "Second", "Third", "Fourth"});
            this.listBox1.Location = new System.Drawing.Point(8, 24);
            this.listBox1.Size = new System.Drawing.Size(232, 200);
            this.listBox1.TabIndex = 0;
            this.listBox1.UseTabStops = true;
            this.listBox1.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(listBox1_MeasureItem);
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(listBox1_DrawItem);
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.AutoScale = false;
            this.ClientSize = new System.Drawing.Size(248, 248);
            this.Font = new System.Drawing.Font(sansSerifFontFamily, 12);
            this.Text = "ListBox";
            this.Controls.AddRange(new System.Windows.Forms.Control[] {this.listBox1});

        }

        // The main entry point for the application.
        [STAThread]
        public static void Main(string[] args) {
            Application.Run(new OwnerDrawListBox());
        }
    }

}




