using System;

namespace System.Windows.Forms.PropertyGridInternal {
	public class PropertyGridTextBox : System.Windows.Forms.UserControl {
		private TextBox textbox;
		private Button dialog_button;
		private Button dropdown_button;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PropertyGridTextBox() {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.dialog_button = new System.Windows.Forms.Button();
			this.dropdown_button = new System.Windows.Forms.Button();
			this.textbox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// dialog_button
			// 
			this.dialog_button.Dock = System.Windows.Forms.DockStyle.Right;
			this.dialog_button.Location = new System.Drawing.Point(256, 0);
			this.dialog_button.Name = "dialog_button";
			this.dialog_button.Size = new System.Drawing.Size(16, 16);
			this.dialog_button.TabIndex = 1;
			this.dialog_button.Text = "D";
			this.dialog_button.Visible = false;
			this.dialog_button.Click += new System.EventHandler(this.dialog_button_Click);
			// 
			// dropdown_button
			// 
			this.dropdown_button.Dock = System.Windows.Forms.DockStyle.Right;
			this.dropdown_button.Location = new System.Drawing.Point(240, 0);
			this.dropdown_button.Name = "dropdown_button";
			this.dropdown_button.Size = new System.Drawing.Size(16, 16);
			this.dropdown_button.TabIndex = 2;
			this.dropdown_button.Text = "P";
			this.dropdown_button.Visible = false;
			this.dropdown_button.Click += new System.EventHandler(this.dropdown_button_Click);
			// 
			// textbox
			// 
			this.textbox.AutoSize = false;
			this.textbox.BackColor = System.Drawing.SystemColors.Window;
			this.textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textbox.Location = new System.Drawing.Point(0, 0);
			this.textbox.Name = "textbox";
			this.textbox.Size = new System.Drawing.Size(240, 16);
			this.textbox.TabIndex = 3;
			this.textbox.Text = "textbox";
			// 
			// PropertyGridTextBox
			// 
			this.Controls.Add(this.textbox);
			this.Controls.Add(this.dropdown_button);
			this.Controls.Add(this.dialog_button);
			this.Name = "PropertyGridTextBox";
			this.Size = new System.Drawing.Size(272, 16);
			this.ResumeLayout(false);

		}
		#endregion

		private void dropdown_button_Click(object sender, System.EventArgs e) {
			if (DropDownButtonClicked != null)
				DropDownButtonClicked(this, EventArgs.Empty);
		}

		private void dialog_button_Click(object sender, System.EventArgs e) {
			if (DialogButtonClicked != null)
				DialogButtonClicked(this, EventArgs.Empty);
		}

		
		public event EventHandler DropDownButtonClicked;
		public event EventHandler DialogButtonClicked;

		public bool DialogButtonVisible {
			get{
				return dialog_button.Visible;
			}
			set {
				dialog_button.Visible = value;
				dropdown_button.Redraw();
			}
		}
		public bool DropDownButtonVisible {
			get{
				return dropdown_button.Visible;
			}
			set {
				dropdown_button.Visible = value;
				dropdown_button.Redraw();
			}
		}

		public bool ReadOnly {
			get {
				return textbox.ReadOnly;
			}
			set {
				textbox.ReadOnly = value;
			}
		}

		public string TextBoxText {
			get {
				return textbox.Text;
			}
			set {
				textbox.Text = value;
			}
		}

	}
}
