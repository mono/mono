//
// CertificateViewer.cs: Certificate Viewer for System.Windows.Forms
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Mono.Security.X509;

[assembly: AssemblyTitle("Mono Certificate Viewer")]
[assembly: AssemblyDescription("X.509 Certificate Viewer for SWF")]

namespace Mono.Tools.CertView {

	public class CertificateViewer : System.Windows.Forms.Form {

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button issuerStatementButton;
		private System.Windows.Forms.Label privateKeyLabel;
		private System.Windows.Forms.RichTextBox keyUsageRichTextBox;
		private System.Windows.Forms.Label notAfterLabel;
		private System.Windows.Forms.Label notBeforeLabel;
		private System.Windows.Forms.ComboBox showComboBox;
		private System.Windows.Forms.TextBox detailsTextBox;
		private System.Windows.Forms.ListView fieldListView;
		private System.Windows.Forms.TextBox certStatusTextBox;
		private System.Windows.Forms.TreeView certPathTreeView;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.LinkLabel issuedByLinkLabel;
		private System.Windows.Forms.TextBox issuedToTextBox;
		private System.Windows.Forms.Label certificateLabel;
		private System.Windows.Forms.TextBox issuedByTextBox;
		private System.Windows.Forms.ColumnHeader fieldColumnHeader;
		private System.Windows.Forms.ColumnHeader valueColumnHeader;
		private System.Windows.Forms.ImageList fieldsImageList;
		private System.Windows.Forms.HelpProvider helpProvider;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.LinkLabel subjectAltNameLinkLabel;
		private System.Windows.Forms.ImageList iconImageList;
		private System.Windows.Forms.PictureBox goodPictureBox;
		private System.Windows.Forms.PictureBox badPictureBox;
		private System.ComponentModel.IContainer components;

		public CertificateViewer (string filename)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			LoadCertificate (filename);
			helpProvider.SetHelpString (issuedToTextBox, CertificateFormatter.Help.IssuedTo);
			helpProvider.SetHelpString (issuedByTextBox, CertificateFormatter.Help.IssuedBy);
			helpProvider.SetHelpString (notBeforeLabel, CertificateFormatter.Help.ValidFrom);
			helpProvider.SetHelpString (notAfterLabel, CertificateFormatter.Help.ValidUntil);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CertificateViewer));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.issuerStatementButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.badPictureBox = new System.Windows.Forms.PictureBox();
			this.goodPictureBox = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.subjectAltNameLinkLabel = new System.Windows.Forms.LinkLabel();
			this.keyUsageRichTextBox = new System.Windows.Forms.RichTextBox();
			this.issuedByTextBox = new System.Windows.Forms.TextBox();
			this.issuedToTextBox = new System.Windows.Forms.TextBox();
			this.issuedByLinkLabel = new System.Windows.Forms.LinkLabel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel5 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.privateKeyLabel = new System.Windows.Forms.Label();
			this.notAfterLabel = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.notBeforeLabel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.certificateLabel = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.label8 = new System.Windows.Forms.Label();
			this.showComboBox = new System.Windows.Forms.ComboBox();
			this.detailsTextBox = new System.Windows.Forms.TextBox();
			this.fieldListView = new System.Windows.Forms.ListView();
			this.fieldColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.valueColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.fieldsImageList = new System.Windows.Forms.ImageList(this.components);
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.label9 = new System.Windows.Forms.Label();
			this.certStatusTextBox = new System.Windows.Forms.TextBox();
			this.certPathTreeView = new System.Windows.Forms.TreeView();
			this.okButton = new System.Windows.Forms.Button();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.helpProvider = new System.Windows.Forms.HelpProvider();
			this.iconImageList = new System.Windows.Forms.ImageList(this.components);
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel2.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
												      this.tabPage1,
												      this.tabPage2,
												      this.tabPage3});
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(384, 408);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.AddRange(new System.Windows.Forms.Control[] {
												   this.issuerStatementButton,
												   this.panel1});
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(376, 382);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			// 
			// issuerStatementButton
			// 
			this.issuerStatementButton.Enabled = false;
			this.issuerStatementButton.Location = new System.Drawing.Point(264, 344);
			this.issuerStatementButton.Name = "issuerStatementButton";
			this.issuerStatementButton.Size = new System.Drawing.Size(104, 23);
			this.issuerStatementButton.TabIndex = 2;
			this.issuerStatementButton.Text = "Issuer Statement";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Window;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
												 this.badPictureBox,
												 this.goodPictureBox,
												 this.label1,
												 this.subjectAltNameLinkLabel,
												 this.keyUsageRichTextBox,
												 this.issuedByTextBox,
												 this.issuedToTextBox,
												 this.issuedByLinkLabel,
												 this.panel4,
												 this.panel2,
												 this.privateKeyLabel,
												 this.notAfterLabel,
												 this.label6,
												 this.notBeforeLabel,
												 this.label4,
												 this.label3,
												 this.label2,
												 this.certificateLabel});
			this.panel1.Location = new System.Drawing.Point(8, 8);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(360, 328);
			this.panel1.TabIndex = 0;
			// 
			// badPictureBox
			// 
			this.badPictureBox.Image = ((System.Drawing.Bitmap)(resources.GetObject("badPictureBox.Image")));
			this.badPictureBox.Name = "badPictureBox";
			this.badPictureBox.Size = new System.Drawing.Size(64, 64);
			this.badPictureBox.TabIndex = 21;
			this.badPictureBox.TabStop = false;
			// 
			// goodPictureBox
			// 
			this.goodPictureBox.Image = ((System.Drawing.Bitmap)(resources.GetObject("goodPictureBox.Image")));
			this.goodPictureBox.Name = "goodPictureBox";
			this.goodPictureBox.Size = new System.Drawing.Size(64, 64);
			this.goodPictureBox.TabIndex = 20;
			this.goodPictureBox.TabStop = false;
			this.goodPictureBox.Visible = false;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(64, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(288, 56);
			this.label1.TabIndex = 0;
			this.label1.Text = "Certificate Information";
			// 
			// subjectAltNameLinkLabel
			// 
			this.helpProvider.SetHelpString(this.subjectAltNameLinkLabel, "Subject Alternative Name (e.g. email)");
			this.subjectAltNameLinkLabel.Location = new System.Drawing.Point(73, 208);
			this.subjectAltNameLinkLabel.Name = "subjectAltNameLinkLabel";
			this.helpProvider.SetShowHelp(this.subjectAltNameLinkLabel, true);
			this.subjectAltNameLinkLabel.Size = new System.Drawing.Size(272, 16);
			this.subjectAltNameLinkLabel.TabIndex = 19;
			this.subjectAltNameLinkLabel.TabStop = true;
			this.subjectAltNameLinkLabel.Text = "mailto:spouliot@motus.com";
			this.subjectAltNameLinkLabel.Visible = false;
			this.subjectAltNameLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.subjectAltNameLinkLabel_LinkClicked);
			// 
			// keyUsageRichTextBox
			// 
			this.keyUsageRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.keyUsageRichTextBox.Location = new System.Drawing.Point(16, 96);
			this.keyUsageRichTextBox.Name = "keyUsageRichTextBox";
			this.keyUsageRichTextBox.Size = new System.Drawing.Size(328, 88);
			this.keyUsageRichTextBox.TabIndex = 9;
			this.keyUsageRichTextBox.TabStop = false;
			this.keyUsageRichTextBox.Text = "";
			// 
			// issuedByTextBox
			// 
			this.issuedByTextBox.AcceptsReturn = true;
			this.issuedByTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.issuedByTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.issuedByTextBox.Location = new System.Drawing.Point(75, 232);
			this.issuedByTextBox.Name = "issuedByTextBox";
			this.issuedByTextBox.ReadOnly = true;
			this.issuedByTextBox.Size = new System.Drawing.Size(269, 14);
			this.issuedByTextBox.TabIndex = 18;
			this.issuedByTextBox.TabStop = false;
			this.issuedByTextBox.Text = "issued by";
			// 
			// issuedToTextBox
			// 
			this.issuedToTextBox.AcceptsReturn = true;
			this.issuedToTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.issuedToTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.issuedToTextBox.Location = new System.Drawing.Point(75, 192);
			this.issuedToTextBox.Name = "issuedToTextBox";
			this.issuedToTextBox.ReadOnly = true;
			this.issuedToTextBox.Size = new System.Drawing.Size(269, 14);
			this.issuedToTextBox.TabIndex = 17;
			this.issuedToTextBox.TabStop = false;
			this.issuedToTextBox.Text = "issued to";
			// 
			// issuedByLinkLabel
			// 
			this.issuedByLinkLabel.Location = new System.Drawing.Point(72, 248);
			this.issuedByLinkLabel.Name = "issuedByLinkLabel";
			this.issuedByLinkLabel.Size = new System.Drawing.Size(272, 16);
			this.issuedByLinkLabel.TabIndex = 15;
			this.issuedByLinkLabel.TabStop = true;
			this.issuedByLinkLabel.Text = "http://www.mono-project.com/";
			this.issuedByLinkLabel.Visible = false;
			// 
			// panel4
			// 
			this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel4.Controls.AddRange(new System.Windows.Forms.Control[] {
												 this.panel5});
			this.panel4.Location = new System.Drawing.Point(8, 64);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(336, 1);
			this.panel4.TabIndex = 13;
			// 
			// panel5
			// 
			this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel5.Location = new System.Drawing.Point(-1, 0);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(344, 1);
			this.panel5.TabIndex = 13;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
												 this.panel3});
			this.panel2.Location = new System.Drawing.Point(8, 184);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(336, 1);
			this.panel2.TabIndex = 12;
			// 
			// panel3
			// 
			this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel3.Location = new System.Drawing.Point(-1, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(344, 1);
			this.panel3.TabIndex = 13;
			// 
			// privateKeyLabel
			// 
			this.privateKeyLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.privateKeyLabel.Location = new System.Drawing.Point(32, 298);
			this.privateKeyLabel.Name = "privateKeyLabel";
			this.privateKeyLabel.Size = new System.Drawing.Size(312, 16);
			this.privateKeyLabel.TabIndex = 11;
			this.privateKeyLabel.Text = "You have a private key that match this certificate";
			this.privateKeyLabel.Visible = false;
			// 
			// notAfterLabel
			// 
			this.notAfterLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.notAfterLabel.Location = new System.Drawing.Point(156, 272);
			this.notAfterLabel.Name = "notAfterLabel";
			this.notAfterLabel.Size = new System.Drawing.Size(64, 16);
			this.notAfterLabel.TabIndex = 8;
			this.notAfterLabel.Text = "9999-99-99";
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(136, 272);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(16, 16);
			this.label6.TabIndex = 7;
			this.label6.Text = "to";
			// 
			// notBeforeLabel
			// 
			this.notBeforeLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.notBeforeLabel.Location = new System.Drawing.Point(72, 272);
			this.notBeforeLabel.Name = "notBeforeLabel";
			this.notBeforeLabel.Size = new System.Drawing.Size(64, 16);
			this.notBeforeLabel.TabIndex = 6;
			this.notBeforeLabel.Text = "9999-99-99";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 232);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "Issued by:";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 192);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "Issued to:";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 272);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Valid from";
			// 
			// certificateLabel
			// 
			this.certificateLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.certificateLabel.Location = new System.Drawing.Point(8, 72);
			this.certificateLabel.Name = "certificateLabel";
			this.certificateLabel.Size = new System.Drawing.Size(344, 112);
			this.certificateLabel.TabIndex = 10;
			this.certificateLabel.Text = "This certificate is intended to:";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.AddRange(new System.Windows.Forms.Control[] {
												   this.label8,
												   this.showComboBox,
												   this.detailsTextBox,
												   this.fieldListView});
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(376, 382);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Details";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 12);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(40, 16);
			this.label8.TabIndex = 3;
			this.label8.Text = "Show:";
			// 
			// showComboBox
			// 
			this.showComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.showComboBox.Items.AddRange(new object[] {
									      "<All>",
									      "Version 1 Fields Only",
									      "Extensions Only",
									      "Critical Extensions Only",
									      "Properties Only"});
			this.showComboBox.Location = new System.Drawing.Point(48, 8);
			this.showComboBox.MaxDropDownItems = 5;
			this.showComboBox.Name = "showComboBox";
			this.showComboBox.Size = new System.Drawing.Size(320, 21);
			this.showComboBox.TabIndex = 2;
			this.showComboBox.SelectedIndexChanged += new System.EventHandler(this.showComboBox_SelectedIndexChanged);
			// 
			// detailsTextBox
			// 
			this.detailsTextBox.AcceptsReturn = true;
			this.detailsTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.detailsTextBox.Location = new System.Drawing.Point(8, 216);
			this.detailsTextBox.Multiline = true;
			this.detailsTextBox.Name = "detailsTextBox";
			this.detailsTextBox.ReadOnly = true;
			this.detailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.detailsTextBox.Size = new System.Drawing.Size(360, 120);
			this.detailsTextBox.TabIndex = 1;
			this.detailsTextBox.Text = "";
			// 
			// fieldListView
			// 
			this.fieldListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
													    this.fieldColumnHeader,
													    this.valueColumnHeader});
			this.fieldListView.FullRowSelect = true;
			this.fieldListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.fieldListView.HideSelection = false;
			this.fieldListView.Location = new System.Drawing.Point(8, 40);
			this.fieldListView.MultiSelect = false;
			this.fieldListView.Name = "fieldListView";
			this.fieldListView.Size = new System.Drawing.Size(360, 168);
			this.fieldListView.SmallImageList = this.fieldsImageList;
			this.fieldListView.TabIndex = 0;
			this.fieldListView.View = System.Windows.Forms.View.Details;
			this.fieldListView.SelectedIndexChanged += new System.EventHandler(this.fieldListView_SelectedIndexChanged);
			// 
			// fieldColumnHeader
			// 
			this.fieldColumnHeader.Text = "Field";
			this.fieldColumnHeader.Width = 140;
			// 
			// valueColumnHeader
			// 
			this.valueColumnHeader.Text = "Value";
			this.valueColumnHeader.Width = 200;
			// 
			// fieldsImageList
			// 
			this.fieldsImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.fieldsImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.fieldsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fieldsImageList.ImageStream")));
			this.fieldsImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.AddRange(new System.Windows.Forms.Control[] {
												   this.label9,
												   this.certStatusTextBox,
												   this.certPathTreeView});
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(376, 382);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Certificate Path";
			// 
			// label9
			// 
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label9.Location = new System.Drawing.Point(8, 296);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(96, 16);
			this.label9.TabIndex = 7;
			this.label9.Text = "Certificate Status";
			// 
			// certStatusTextBox
			// 
			this.certStatusTextBox.AcceptsReturn = true;
			this.certStatusTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.certStatusTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.certStatusTextBox.Location = new System.Drawing.Point(8, 312);
			this.certStatusTextBox.Multiline = true;
			this.certStatusTextBox.Name = "certStatusTextBox";
			this.certStatusTextBox.ReadOnly = true;
			this.certStatusTextBox.Size = new System.Drawing.Size(360, 56);
			this.certStatusTextBox.TabIndex = 2;
			this.certStatusTextBox.Text = "This certificate is OK.";
			// 
			// certPathTreeView
			// 
			this.certPathTreeView.ImageIndex = -1;
			this.certPathTreeView.Location = new System.Drawing.Point(8, 8);
			this.certPathTreeView.Name = "certPathTreeView";
			this.certPathTreeView.SelectedImageIndex = -1;
			this.certPathTreeView.Size = new System.Drawing.Size(360, 280);
			this.certPathTreeView.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(317, 424);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// iconImageList
			// 
			this.iconImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.iconImageList.ImageSize = new System.Drawing.Size(64, 64);
			this.iconImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconImageList.ImageStream")));
			this.iconImageList.TransparentColor = System.Drawing.Color.Black;
			// 
			// CertificateViewer
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(400, 453);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
											  this.okButton,
											  this.tabControl1});
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.HelpButton = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CertificateViewer";
			this.Text = "Mono Certificate Viewer";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private CertificateFormatter cf;
		private Font genericFont;
		private Font monospaceFont;

		private ListViewItem itemVersion;
		private ListViewItem itemSerial;
		private ListViewItem itemSignatureAlgorithm;
		private ListViewItem itemIssuer;
		private ListViewItem itemValidFrom;
		private ListViewItem itemValidUntil;
		private ListViewItem itemSubject;
		private ListViewItem itemPublicKey;
		private ListViewItem[] itemExtensions;
		private ListViewItem itemThumbprintAlgorithm;
		private ListViewItem itemThumbprint;

		public void LoadCertificate (string filename) 
		{
			cf = new CertificateFormatter (filename);

			genericFont = detailsTextBox.Font;
			monospaceFont = new Font (FontFamily.GenericMonospace, 10);

			issuedToTextBox.Text = cf.Subject (false);
			toolTip.SetToolTip (issuedToTextBox, issuedToTextBox.Text);

			subjectAltNameLinkLabel.Text = cf.SubjectAltName (false);
			subjectAltNameLinkLabel.Visible = (subjectAltNameLinkLabel.Text != String.Empty);

			issuedByTextBox.Text = cf.Issuer (false);
			toolTip.SetToolTip (issuedByTextBox, issuedByTextBox.Text);

			notBeforeLabel.Text = cf.Certificate.ValidFrom.ToString ("yyyy-MM-dd");
			notAfterLabel.Text = cf.Certificate.ValidUntil.ToString ("yyyy-MM-dd");

			if (cf.Certificate.Version == 1) {
				// not in certificate so it's a property
				itemVersion = new ListViewItem (CertificateFormatter.FieldNames.Version, 0);
				itemVersion.ForeColor = Color.Blue;
			}
			else
				itemVersion = new ListViewItem (CertificateFormatter.FieldNames.Version, 2);
			itemVersion.SubItems.Add (cf.Version (false));
			itemVersion.SubItems.Add (cf.Version (true));

			itemSerial = new ListViewItem (CertificateFormatter.FieldNames.SerialNumber, 1);
			itemSerial.SubItems.Add (cf.SerialNumber (false));
			itemSerial.SubItems.Add (cf.SerialNumber (true));
			itemSerial.Tag = monospaceFont;

			itemSignatureAlgorithm = new ListViewItem (CertificateFormatter.FieldNames.SignatureAlgorithm, 1);
			itemSignatureAlgorithm.SubItems.Add (cf.SignatureAlgorithm (false));
			itemSignatureAlgorithm.SubItems.Add (cf.SignatureAlgorithm (true));

			itemIssuer = new ListViewItem (CertificateFormatter.FieldNames.Issuer, 1);
			itemIssuer.SubItems.Add (cf.Issuer (false));
			itemIssuer.SubItems.Add (cf.Issuer (true));

			itemValidFrom = new ListViewItem (CertificateFormatter.FieldNames.ValidFrom, 1);
			itemValidFrom.SubItems.Add (cf.ValidFrom (false));
			itemValidFrom.SubItems.Add (cf.ValidFrom (true));

			itemValidUntil = new ListViewItem (CertificateFormatter.FieldNames.ValidUntil,1);
			itemValidUntil.SubItems.Add (cf.ValidUntil (false));
			itemValidUntil.SubItems.Add (cf.ValidUntil (true));

			itemSubject = new ListViewItem (CertificateFormatter.FieldNames.Subject, 1);
			itemSubject.SubItems.Add (cf.Subject (false));
			itemSubject.SubItems.Add (cf.Subject (true));

			itemPublicKey = new ListViewItem (CertificateFormatter.FieldNames.PublicKey, 1);
			itemPublicKey.SubItems.Add (cf.PublicKey (false));
			itemPublicKey.SubItems.Add (cf.PublicKey (true));
			itemPublicKey.Tag = monospaceFont;

			itemExtensions = new ListViewItem [cf.Certificate.Extensions.Count];
			for (int i=0; i < cf.Certificate.Extensions.Count; i++) {
				X509Extension xe = cf.GetExtension (i);
				int critical = (xe.Critical ? 4 : 3);
				string name = xe.Name;
				object tag = null;
				if (name == xe.Oid)
					tag = monospaceFont;
				ListViewItem lvi = new ListViewItem (name, critical);
				lvi.Tag = tag;
				if (critical == 4)
					lvi.ForeColor = Color.Red;
				string exts = xe.ToString ();
				if (xe.Name == xe.Oid)
					lvi.SubItems.Add (cf.Extension (i, false));
				else 
					lvi.SubItems.Add (CertificateFormatter.OneLine (exts));
				lvi.SubItems.Add (exts);
				itemExtensions [i] = lvi;
			}

			// properties (calculated)
			itemThumbprintAlgorithm = new ListViewItem (CertificateFormatter.PropertyNames.ThumbprintAlgorithm, 0);
			itemThumbprintAlgorithm.SubItems.Add (cf.ThumbprintAlgorithm);
			itemThumbprintAlgorithm.SubItems.Add (cf.ThumbprintAlgorithm);
			itemThumbprintAlgorithm.ForeColor = Color.Blue;

			itemThumbprint = new ListViewItem (CertificateFormatter.PropertyNames.Thumbprint, 0);
			string tb = CertificateFormatter.Array2Word (cf.Thumbprint);
			itemThumbprint.SubItems.Add (tb);
			itemThumbprint.SubItems.Add (tb);
			itemThumbprint.Tag = monospaceFont;
			itemThumbprint.ForeColor = Color.Blue;

			showComboBox.SelectedIndex = 0;

			if (cf.Status != null) {
				badPictureBox.Visible = true;
				keyUsageRichTextBox.Visible = false;
				certificateLabel.Text = cf.Status;
				certificateLabel.ForeColor = Color.Red;
				certStatusTextBox.Text = cf.Status;
				certStatusTextBox.ForeColor = Color.Red;
			}
			else
				badPictureBox.Visible = false;
			goodPictureBox.Visible = !badPictureBox.Visible;

			keyUsageRichTextBox.SelectionBullet = true;
			keyUsageRichTextBox.Text = "No restrictions";
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main (string[] args) 
		{
			string filename = ((args.Length > 0) ? args[0] : null);
			if ((filename != null) && (File.Exists (filename)))
				Application.Run (new CertificateViewer (filename));
		}

		private void okButton_Click (object sender, System.EventArgs e) 
		{
			Application.Exit ();
		}

		private void fieldListView_SelectedIndexChanged (object sender, System.EventArgs e) 
		{
			if (sender is ListView) {
				ListView lv = (sender as ListView);
				if (lv.SelectedItems.Count > 0) {
					ListViewItem lvi = lv.SelectedItems [0];
					if (lvi.Tag is Font)
						detailsTextBox.Font = (lvi.Tag as Font);
					else
						detailsTextBox.Font = genericFont;
					detailsTextBox.Text = lvi.SubItems [2].Text;
				}
			}
		}

		private void UpdateListView (int filter) 
		{
			fieldListView.Items.Clear ();

			if ((filter == 0) || ((filter == 4) && (itemVersion.ImageIndex == 0)))
				fieldListView.Items.Add (itemVersion);

			if (filter < 2)
				fieldListView.Items.AddRange (new ListViewItem[] {itemSerial, itemSignatureAlgorithm, itemIssuer, itemValidFrom, itemValidUntil, itemSubject, itemPublicKey });

			if ((filter != 1) && (filter != 4)) {
				for (int i=0; i < itemExtensions.Length; i++) {
					if ((filter != 3) || ((filter == 3) && (cf.Certificate.Extensions [i].Critical))) {
						fieldListView.Items.Add (itemExtensions [i]);
					}
				}
			}

			if ((filter == 0) || (filter == 4)) {
				fieldListView.Items.Add (itemThumbprintAlgorithm);
				fieldListView.Items.Add (itemThumbprint);
			}

			detailsTextBox.Text = "";
		}

		private void showComboBox_SelectedIndexChanged (object sender, System.EventArgs e) 
		{
			UpdateListView ((sender as ComboBox).SelectedIndex);
		}

		private void subjectAltNameLinkLabel_LinkClicked (object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) 
		{
			System.Diagnostics.Process.Start ((sender as LinkLabel).Text);
		}
	}
}
