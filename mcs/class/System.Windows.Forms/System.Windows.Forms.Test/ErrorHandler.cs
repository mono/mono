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
namespace Microsoft.Samples.WinForms.Cs.ErrorHandler {
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Threading;

    //The Error Handler class
    //We need a class because event handling methods can't be static
    internal class CustomExceptionHandler {

        //Handle the exception  event
        public void OnThreadException(object sender, ThreadExceptionEventArgs t) {

            DialogResult result = DialogResult.Cancel;
            try {
                result = this.ShowThreadExceptionDialog(t.Exception);
            }
            catch {
                try {
                    MessageBox.Show("Fatal Error", "Fatal Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                }
                finally {
                    Application.Exit();
                }
            }

            if (result == DialogResult.Abort)
                Application.Exit();

        }

        private DialogResult ShowThreadExceptionDialog(Exception e) {
            string errorMsg = "An error occurred please contact the adminstrator with the following information:\n\n";
            errorMsg = errorMsg + e.Message + "\n\nStack Trace:\n" + e.StackTrace;
            return MessageBox.Show(errorMsg, "Application Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }
    }

    public class ErrorHandler : System.Windows.Forms.Form {
        private System.ComponentModel.Container components;
        private System.Windows.Forms.Button button1;

        public ErrorHandler() {

            // Required by the Win Forms Designer
            InitializeComponent();

        }

        /// <summary>
        ///    Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
           if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
           }
           base.Dispose(disposing);
        }

        /// <summary>
        ///    Required method for Designer support - do not modify
        ///    the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            
            this.button1.Size = new System.Drawing.Size(120, 40);
            this.button1.TabIndex = 1;
            this.button1.Font = new System.Drawing.Font("TAHOMA", 8f, System.Drawing.FontStyle.Bold);
            this.button1.Location = new System.Drawing.Point(256, 64);
            this.button1.Text = "Click Me!";
           
            this.button1.Click += new System.EventHandler(button1_Click);
            
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(392, 117);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {button1});
			this.Text = "Exception Handling Sample";
        }

        private void button1_Click(object sender, System.EventArgs e) {
            throw new ArgumentException("The parameter was invalid");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
            CustomExceptionHandler eh = new CustomExceptionHandler();
            Application.ThreadException += new ThreadExceptionEventHandler(eh.OnThreadException);
            Application.Run(new ErrorHandler());
        }


    }
}










