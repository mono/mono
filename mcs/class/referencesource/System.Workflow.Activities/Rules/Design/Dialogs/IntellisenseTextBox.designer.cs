namespace System.Workflow.Activities.Rules.Design
{
    partial class IntellisenseTextBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntellisenseTextBox));
            this.autoCompletionImageList = new System.Windows.Forms.ImageList(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // autoCompletionImageList
            // 
            this.autoCompletionImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("autoCompletionImageList.ImageStream")));
            this.autoCompletionImageList.TransparentColor = System.Drawing.Color.Magenta;
            this.autoCompletionImageList.Images.SetKeyName(0, "");
            this.autoCompletionImageList.Images.SetKeyName(1, "");
            this.autoCompletionImageList.Images.SetKeyName(2, "");
            this.autoCompletionImageList.Images.SetKeyName(3, "");
            this.autoCompletionImageList.Images.SetKeyName(4, "");
            this.autoCompletionImageList.Images.SetKeyName(5, "");
            this.autoCompletionImageList.Images.SetKeyName(6, "");
            this.autoCompletionImageList.Images.SetKeyName(7, "");
            this.autoCompletionImageList.Images.SetKeyName(8, "");
            this.autoCompletionImageList.Images.SetKeyName(9, "");
            this.autoCompletionImageList.Images.SetKeyName(10, "");
            this.autoCompletionImageList.Images.SetKeyName(11, "");
            this.autoCompletionImageList.Images.SetKeyName(12, "");
            this.autoCompletionImageList.Images.SetKeyName(13, "");
            this.autoCompletionImageList.Images.SetKeyName(14, "Keyword.bmp");
            this.autoCompletionImageList.Images.SetKeyName(15, "MethodExtension.bmp");
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 0;
            this.toolTip.UseAnimation = false;
            // 
            // IntellisenseTextBox
            // 
            this.Enter += new System.EventHandler(this.IntellisenseTextBox_Enter);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.IntellisenseTextBox_MouseClick);
            this.Leave += new System.EventHandler(this.IntellisenseTextBox_Leave);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.IntellisenseTextBox_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList autoCompletionImageList;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
