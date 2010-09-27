namespace WindowsFormsApplication1
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components;

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
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea ();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series ();
			System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint (0D, 6D);
			System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint2 = new System.Windows.Forms.DataVisualization.Charting.DataPoint (0D, 9D);
			System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint3 = new System.Windows.Forms.DataVisualization.Charting.DataPoint (0D, 3D);
			System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint4 = new System.Windows.Forms.DataVisualization.Charting.DataPoint (0D, 5D);
			this.chart1 = new WindowsFormsApplication1.Form1.MyChart ();
			this.SuspendLayout ();
			// 
			// chart1
			// 
			chartArea1.Name = "ChartArea1";
			this.chart1.ChartAreas.Add (chartArea1);
			this.chart1.Location = new System.Drawing.Point (12, 44);
			this.chart1.Name = "chart1";
			series1.ChartArea = "ChartArea1";
			series1.Name = "Series1";
			series1.Points.Add (dataPoint1);
			series1.Points.Add (dataPoint2);
			series1.Points.Add (dataPoint3);
			series1.Points.Add (dataPoint4);
			this.chart1.Series.Add (series1);
			this.chart1.Size = new System.Drawing.Size (506, 303);
			this.chart1.TabIndex = 0;
			this.chart1.Text = "chart1";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.SlateGray;
			this.ClientSize = new System.Drawing.Size (530, 386);
			this.Controls.Add (this.chart1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout (false);

		}

		#endregion

		private Form1.MyChart chart1;

	}
}

