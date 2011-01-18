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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;

namespace System.Windows.Forms
{
	
	public class PrintControllerWithStatusDialog : PrintController {
		#region Local variables
		PrintController underlyingController;
		PrintingDialog dialog;
		int currentPage;
		#endregion // Local variables

		#region Public Constructors

		public PrintControllerWithStatusDialog(PrintController underlyingController) {
			this.underlyingController = underlyingController;
			dialog = new PrintingDialog();
			dialog.Text = "Printing";
		}

		public PrintControllerWithStatusDialog(PrintController underlyingController, string dialogTitle) : this(underlyingController) {
			dialog.Text = dialogTitle;
		}
		#endregion // Public Constructors
		
		#region	Protected Instance Methods
		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e) {
			if (dialog.DialogResult == DialogResult.Cancel) {
				e.Cancel = true;
				dialog.Hide();
				return;
			}
			underlyingController.OnEndPage (document, e);
		}

		public override void OnEndPrint(PrintDocument document, PrintEventArgs e) {
			dialog.Hide();
			underlyingController.OnEndPrint (document, e);
		}

		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e) {
			if (dialog.DialogResult == DialogResult.Cancel) {
				e.Cancel = true;
				dialog.Hide();
				return null;
			}
			dialog.LabelText = string.Format("Page {0} of document", ++currentPage);
			return underlyingController.OnStartPage (document, e);
		}

		void Set_PrinterSettings_PrintFileName (PrinterSettings settings, string filename)
		{
			settings.PrintFileName = filename;
		}

		public override void OnStartPrint(PrintDocument document, PrintEventArgs e) {
			try {
				currentPage = 0;
				dialog.Show();
				if (document.PrinterSettings.PrintToFile) {
					SaveFileDialog d = new SaveFileDialog ();
					if (d.ShowDialog () != DialogResult.OK)
						// Windows throws a Win32Exception here.
						throw new Exception ("The operation was canceled by the user");
					Set_PrinterSettings_PrintFileName (document.PrinterSettings, d.FileName);
				}
				underlyingController.OnStartPrint (document, e);
			}
			catch {
				dialog.Hide ();
				throw;
			}
		}

		#endregion	// Protected Instance Methods

		#region Public Properties
		public override bool IsPreview {
			get { return underlyingController.IsPreview; }
		}
		#endregion
	
		#region Internal Class
		class PrintingDialog : Form {
			private Button buttonCancel;
			private Label label;

			public PrintingDialog() {
				buttonCancel = new System.Windows.Forms.Button();
				label = new System.Windows.Forms.Label();
				SuspendLayout();

				buttonCancel.Location = new System.Drawing.Point(88, 88);
				buttonCancel.Name = "buttonCancel";
				buttonCancel.TabIndex = 0;
				buttonCancel.Text = "Cancel";

				label.Location = new System.Drawing.Point(0, 40);
				label.Name = "label";
				label.Size = new System.Drawing.Size(257, 23);
				label.TabIndex = 1;
				label.Text = "Page 1 of document";
				label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

				AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				CancelButton = buttonCancel;
				ClientSize = new System.Drawing.Size(258, 124);
				ControlBox = false;
				Controls.Add(label);
				Controls.Add(buttonCancel);
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
				Name = "PrintingDialog";
				ShowInTaskbar = false;
				Text = "Printing";
				ResumeLayout(false);
			}

			public string LabelText {
				get { return label.Text; }
				set { label.Text = value; }
			}
		}
		#endregion Internal Class
	}
}
