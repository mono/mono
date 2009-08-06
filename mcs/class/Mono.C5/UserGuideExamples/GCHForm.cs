/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using C5;

namespace GConvexHull
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
  public class TesterForm : System.Windows.Forms.Form
  {
    //My data

    //My GUI stuff
    private System.Windows.Forms.Panel drawarea;

    private Graphics drawg;

    //Std stuff
    private System.Windows.Forms.Button runButton;
    private TextBox pointCount;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;


    public TesterForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
      drawg = drawarea.CreateGraphics();
      reset();
    }


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }

      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.drawarea = new System.Windows.Forms.Panel();
      this.runButton = new System.Windows.Forms.Button();
      this.pointCount = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // drawarea
      // 
      this.drawarea.BackColor = System.Drawing.Color.White;
      this.drawarea.Location = new System.Drawing.Point(8, 9);
      this.drawarea.Name = "drawarea";
      this.drawarea.Size = new System.Drawing.Size(500, 500);
      this.drawarea.TabIndex = 0;
      this.drawarea.Paint += new System.Windows.Forms.PaintEventHandler(this.drawarea_Paint);
      this.drawarea.Invalidated += new System.Windows.Forms.InvalidateEventHandler(this.drawarea_Invalidated);
      this.drawarea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.drawarea_MouseMove);
      this.drawarea.MouseClick += new System.Windows.Forms.MouseEventHandler(this.drawarea_MouseClick);
      // 
      // runButton
      // 
      this.runButton.Location = new System.Drawing.Point(8, 516);
      this.runButton.Name = "runButton";
      this.runButton.Size = new System.Drawing.Size(42, 20);
      this.runButton.TabIndex = 1;
      this.runButton.Text = "Run";
      this.runButton.Click += new System.EventHandler(this.runButton_Click);
      // 
      // pointCount
      // 
      this.pointCount.Location = new System.Drawing.Point(97, 517);
      this.pointCount.Name = "pointCount";
      this.pointCount.Size = new System.Drawing.Size(55, 20);
      this.pointCount.TabIndex = 5;
      // 
      // TesterForm
      // 
      this.ClientSize = new System.Drawing.Size(524, 550);
      this.Controls.Add(this.pointCount);
      this.Controls.Add(this.runButton);
      this.Controls.Add(this.drawarea);
      this.Name = "TesterForm";
      this.Text = "C5 Tester";
      this.Load += new System.EventHandler(this.TesterForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.Run(new TesterForm());
    }

    Point[] pts;
    Point[] chpts;

    private void runButton_Click(object sender, System.EventArgs e)
    {
      int N = int.Parse(pointCount.Text);
      pts = new Point[N];
      for (int i = 0; i < N; i++)
        pts[i] = Point.Random(500, 500);
      chpts = Convexhull.ConvexHull(pts);

      drawarea.Invalidate();
    }


    private void drawarea_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
    {
      mydraw();
    }


    private void resetButton_Click(object sender, System.EventArgs e)
    {
      reset();
    }


    private void reset()
    {
      drawarea.Invalidate();//(new Rectangle(0, 0, 40, 40));
    }



    public void mydraw()
    {
      if (pts == null)
      {
        return;
      }
      for (int i = 0; i < pts.Length; i++)
      {
        Point p = pts[i];
        drawg.DrawEllipse(new Pen(Color.Red), transx(p.x) - 2, transy(p.y) - 2, 4, 4);
      }
      for (int i = 0; i < chpts.Length; i++)
      {
        int j = i + 1 < chpts.Length ? i + 1 : 0;
        drawg.DrawEllipse(new Pen(Color.Blue), transx(chpts[i].x) - 2, transy(chpts[i].y) - 2, 4, 4);
        drawg.DrawLine(new Pen(Color.LawnGreen), transx(chpts[i].x), transx(chpts[i].y), transx(chpts[j].x), transx(chpts[j].y));
      }
    }



    private int transx(double x)
    {
      return (int)x;
    }


    private int transy(double y)
    {
      return (int)y;
    }


    private void dumpButton_Click(object sender, System.EventArgs e)
    {
      Debug.WriteLine("###############");
      Debug.WriteLine("###############");
    }


    private void graphTypeControlArray_Click(object sender, System.EventArgs e)
    {
      Debug.WriteLine(e.GetType());
      Debug.WriteLine(sender.GetType());
      drawarea.Invalidate();
    }


    private void drawarea_MouseMove(object sender, MouseEventArgs e)
    {
    }


    private void drawarea_MouseClick(object sender, MouseEventArgs e)
    {
      //double x = untransx(e.X), y = untransy(e.Y);

    }


    private void drawarea_Invalidated(object sender, InvalidateEventArgs e)
    {
      //msg.Text = e.InvalidRect + "";
      //mydraw();
    }


    private void preparedFigureSelection_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }

    private void voronoiButton_CheckedChanged(object sender, EventArgs e)
    {
      graphTypeControlArray_Click(sender, e);
    }

    private void delaunayButton_CheckedChanged(object sender, EventArgs e)
    {
      graphTypeControlArray_Click(sender, e);
    }

    private void TesterForm_Load(object sender, EventArgs e)
    {

    }

  }
}
