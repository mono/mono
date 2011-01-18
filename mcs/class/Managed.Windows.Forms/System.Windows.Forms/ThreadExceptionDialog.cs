//
// System.Windows.Forms.ThreadExceptionDialog.cs
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//		Marek Safar     marek.safar@seznam.cz
//
// Copyright (C) Novell Inc., 2004

// COMPLETE - BUT DISABLED TEXTBOX

using System;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	public class ThreadExceptionDialog: Form
	{
		Exception e;
		bool details;

		private System.Windows.Forms.Button buttonIgnore;
		private System.Windows.Forms.Button buttonAbort;
		private System.Windows.Forms.Button buttonDetails;
		private System.Windows.Forms.Label labelException;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxDetails;
		private System.Windows.Forms.Label helpText;

		private void InitializeComponent()
		{
			this.helpText = new System.Windows.Forms.Label();
			this.buttonAbort = new System.Windows.Forms.Button();
			this.buttonIgnore = new System.Windows.Forms.Button();
			this.buttonDetails = new System.Windows.Forms.Button();
			this.labelException = new System.Windows.Forms.Label();
			this.textBoxDetails = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// helpText
			// 
			this.helpText.Location = new System.Drawing.Point(60, 8);
			this.helpText.Name = "helpText";
			this.helpText.Size = new System.Drawing.Size(356, 40);
			this.helpText.TabIndex = 0;
			this.helpText.Text = "An unhandled exception has occurred in you application. If you click Ignore the a" +
				"pplication will ignore this error and attempt to continue. If you click Abort, t" +
				"he application will quit immediately.";
			// 
			// buttonAbort
			// 
			this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.buttonAbort.Location = new System.Drawing.Point(332, 112);
			this.buttonAbort.Name = "buttonAbort";
			this.buttonAbort.Size = new System.Drawing.Size(85, 23);
			this.buttonAbort.TabIndex = 4;
			this.buttonAbort.Text = "&Abort";
			this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
			// 
			// buttonIgnore
			// 
			this.buttonIgnore.DialogResult = System.Windows.Forms.DialogResult.Ignore;
			this.buttonIgnore.Location = new System.Drawing.Point(236, 112);
			this.buttonIgnore.Name = "buttonIgnore";
			this.buttonIgnore.Size = new System.Drawing.Size(85, 23);
			this.buttonIgnore.TabIndex = 3;
			this.buttonIgnore.Text = "&Ignore";
			// 
			// buttonDetails
			// 
			this.buttonDetails.Location = new System.Drawing.Point(140, 112);
			this.buttonDetails.Name = "buttonDetails";
			this.buttonDetails.Size = new System.Drawing.Size(85, 23);
			this.buttonDetails.TabIndex = 2;
			this.buttonDetails.Text = "Show &Details";
			this.buttonDetails.Click += new System.EventHandler(this.buttonDetails_Click);
			// 
			// labelException
			// 
			this.labelException.Location = new System.Drawing.Point(60, 64);
			this.labelException.Name = "labelException";
			this.labelException.Size = new System.Drawing.Size(356, 32);
			this.labelException.TabIndex = 1;
			// 
			// textBoxDetails
			// 
			this.textBoxDetails.Location = new System.Drawing.Point(8, 168);
			this.textBoxDetails.Multiline = true;
			this.textBoxDetails.Name = "textBoxDetails";
			this.textBoxDetails.ReadOnly = true;
			this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxDetails.Size = new System.Drawing.Size(408, 196);
			this.textBoxDetails.TabIndex = 5;
			this.textBoxDetails.TabStop = false;
			this.textBoxDetails.Text = "";
			this.textBoxDetails.WordWrap = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 148);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Exception details";
			// 
			// ThreadExceptionDialog
			// 
			this.AcceptButton = this.buttonIgnore;
			this.CancelButton = this.buttonAbort;
			this.ClientSize = new System.Drawing.Size(428, 374);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxDetails);
			this.Controls.Add(this.labelException);
			this.Controls.Add(this.buttonDetails);
			this.Controls.Add(this.buttonIgnore);
			this.Controls.Add(this.buttonAbort);
			this.Controls.Add(this.helpText);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ThreadExceptionDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.TopMost = true;
			this.Paint += new PaintEventHandler (PaintHandler);
			this.ResumeLayout(false);
		}
	
		public ThreadExceptionDialog (Exception t)
		{
			this.e = t;
			InitializeComponent ();

			this.labelException.Text = t.Message;
			if (Form.ActiveForm != null)
				this.Text = Form.ActiveForm.Text;
			else
				this.Text = "Mono";
			this.buttonAbort.Enabled = Application.AllowQuit;
			RefreshDetails ();
			FillExceptionDetails ();
		}

		void buttonDetails_Click(object sender, System.EventArgs e)
		{
			details = !details;
			RefreshDetails ();
		}

		void FillExceptionDetails ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (e.ToString ());
			sb.Append (Environment.NewLine + Environment.NewLine);
			sb.Append ("Loaded assemblies:" + Environment.NewLine + Environment.NewLine);

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies ()) {
				AssemblyName an = a.GetName ();
				sb.AppendFormat ("Name:\t{0}" + Environment.NewLine, an.Name);
				sb.AppendFormat ("Version:\t{0}" + Environment.NewLine, an.Version);
				sb.AppendFormat ("Location:\t{0}" + Environment.NewLine, an.CodeBase);
				sb.Append (Environment.NewLine);
			}
			textBoxDetails.Text = sb.ToString ();
		}

		void RefreshDetails ()
		{
			if (details) {
				buttonDetails.Text = "Hide &Details";
				Height = 410;
				label1.Visible = true;
				textBoxDetails.Visible = true;
				return;
			}
			buttonDetails.Text = "Show &Details";
			label1.Visible = false;
			textBoxDetails.Visible = false;
			Height = 180;
		}

		void buttonAbort_Click(object sender, System.EventArgs e)
		{
			Application.Exit ();
		}

		void PaintHandler (object o, PaintEventArgs args)
		{
			Graphics g = args.Graphics;
			g.DrawIcon (SystemIcons.Error, 15, 10);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}
	}
}
