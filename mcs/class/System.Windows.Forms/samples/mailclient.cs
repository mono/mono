using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace smtp_csharp
{
  public class frmMain : System.Windows.Forms.Form
  {
    internal System.Windows.Forms.GroupBox GroupBox2;
    internal System.Windows.Forms.Label LabelAuthType;
    internal System.Windows.Forms.ComboBox cmbAuth;
    internal System.Windows.Forms.Label LabelPopServer;
    internal System.Windows.Forms.TextBox txtPOPServer;
    internal System.Windows.Forms.Label LabelPasswd;
    internal System.Windows.Forms.TextBox txtPassword;
    internal System.Windows.Forms.Label LabelUsername;
    internal System.Windows.Forms.TextBox txtUsername;
    internal System.Windows.Forms.Label LabelServer;
    internal System.Windows.Forms.TextBox txtServer;
    internal System.Windows.Forms.GroupBox GroupBox1;
    internal System.Windows.Forms.Button cmdSend;
    internal System.Windows.Forms.Label LabelMessage;
    internal System.Windows.Forms.Label LabelSubject;
    internal System.Windows.Forms.Label LabelSentTo;
    internal System.Windows.Forms.Label LabelMailFrom;
    internal System.Windows.Forms.TextBox txtMessageText;
    internal System.Windows.Forms.TextBox txtMessageSubject;
    internal System.Windows.Forms.TextBox txtSendTo;
    internal System.Windows.Forms.TextBox txtMailFrom;

    private System.ComponentModel.Container components = null;

    public frmMain()
    {
      InitializeComponent();
    }

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

    private void InitializeComponent()
    {
      this.GroupBox2 = new System.Windows.Forms.GroupBox();
      this.LabelAuthType = new System.Windows.Forms.Label();
      this.cmbAuth = new System.Windows.Forms.ComboBox();
      this.LabelPopServer = new System.Windows.Forms.Label();
      this.txtPOPServer = new System.Windows.Forms.TextBox();
      this.LabelPasswd = new System.Windows.Forms.Label();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.LabelUsername = new System.Windows.Forms.Label();
      this.txtUsername = new System.Windows.Forms.TextBox();
      this.LabelServer = new System.Windows.Forms.Label();
      this.txtServer = new System.Windows.Forms.TextBox();
      this.GroupBox1 = new System.Windows.Forms.GroupBox();
      this.cmdSend = new System.Windows.Forms.Button();
      this.LabelMessage = new System.Windows.Forms.Label();
      this.LabelSubject = new System.Windows.Forms.Label();
      this.LabelSentTo = new System.Windows.Forms.Label();
      this.LabelMailFrom = new System.Windows.Forms.Label();
      this.txtMessageText = new System.Windows.Forms.TextBox();
      this.txtMessageSubject = new System.Windows.Forms.TextBox();
      this.txtSendTo = new System.Windows.Forms.TextBox();
      this.txtMailFrom = new System.Windows.Forms.TextBox();
      this.GroupBox2.SuspendLayout();
      this.GroupBox1.SuspendLayout();
      this.SuspendLayout();

      this.GroupBox2.Controls.Add(this.LabelAuthType);
      this.GroupBox2.Controls.Add(this.cmbAuth);
      this.GroupBox2.Controls.Add(this.LabelPopServer);
      this.GroupBox2.Controls.Add(this.txtPOPServer);
      this.GroupBox2.Controls.Add(this.LabelPasswd);
      this.GroupBox2.Controls.Add(this.txtPassword);
      this.GroupBox2.Controls.Add(this.LabelUsername);
      this.GroupBox2.Controls.Add(this.txtUsername);
      this.GroupBox2.Controls.Add(this.LabelServer);
      this.GroupBox2.Controls.Add(this.txtServer);
      this.GroupBox2.Location = new System.Drawing.Point(264, 10);
      this.GroupBox2.Name = "GroupBox2";
      this.GroupBox2.Size = new System.Drawing.Size(240, 216);
      this.GroupBox2.TabIndex = 11;
      this.GroupBox2.TabStop = false;
      this.GroupBox2.Text = "Connection Settings";

      this.LabelAuthType.AutoSize = true;
      this.LabelAuthType.Location = new System.Drawing.Point(24, 98);
      this.LabelAuthType.Name = "LabelAuthType";
      this.LabelAuthType.Size = new System.Drawing.Size(101, 16);
      this.LabelAuthType.TabIndex = 14;
      this.LabelAuthType.Text = "Authentication type";
      this.LabelAuthType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.cmbAuth.Location = new System.Drawing.Point(128, 96);
      this.cmbAuth.Name = "cmbAuth";
      this.cmbAuth.Size = new System.Drawing.Size(96, 21);
      this.cmbAuth.TabIndex = 13;
      this.cmbAuth.Text = "ComboBox1";

      this.LabelPopServer.AutoSize = true;
      this.LabelPopServer.Location = new System.Drawing.Point(24, 122);
      this.LabelPopServer.Name = "LabelPopServer";
      this.LabelPopServer.Size = new System.Drawing.Size(71, 16);
      this.LabelPopServer.TabIndex = 12;
      this.LabelPopServer.Text = "POP3 Server";
      this.LabelPopServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.txtPOPServer.Location = new System.Drawing.Point(104, 120);
      this.txtPOPServer.Name = "txtPOPServer";
      this.txtPOPServer.Size = new System.Drawing.Size(120, 20);
      this.txtPOPServer.TabIndex = 11;
      this.txtPOPServer.Text = "";

      this.LabelPasswd.AutoSize = true;
      this.LabelPasswd.Location = new System.Drawing.Point(24, 74);
      this.LabelPasswd.Name = "LabelPasswd";
      this.LabelPasswd.Size = new System.Drawing.Size(54, 16);
      this.LabelPasswd.TabIndex = 10;
      this.LabelPasswd.Text = "Password";
      this.LabelPasswd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.txtPassword.Location = new System.Drawing.Point(88, 72);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.Size = new System.Drawing.Size(136, 20);
      this.txtPassword.TabIndex = 9;
      this.txtPassword.Text = "";

      this.LabelUsername.AutoSize = true;
      this.LabelUsername.Location = new System.Drawing.Point(24, 50);
      this.LabelUsername.Name = "LabelUsername";
      this.LabelUsername.Size = new System.Drawing.Size(56, 16);
      this.LabelUsername.TabIndex = 8;
      this.LabelUsername.Text = "Username";
      this.LabelUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.txtUsername.Location = new System.Drawing.Point(88, 48);
      this.txtUsername.Name = "txtUsername";
      this.txtUsername.Size = new System.Drawing.Size(136, 20);
      this.txtUsername.TabIndex = 7;
      this.txtUsername.Text = "";

      this.LabelServer.AutoSize = true;
      this.LabelServer.Location = new System.Drawing.Point(24, 26);
      this.LabelServer.Name = "LabelServer";
      this.LabelServer.Size = new System.Drawing.Size(38, 16);
      this.LabelServer.TabIndex = 6;
      this.LabelServer.Text = "Server";
      this.LabelServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.txtServer.Location = new System.Drawing.Point(88, 24);
      this.txtServer.Name = "txtServer";
      this.txtServer.Size = new System.Drawing.Size(136, 20);
      this.txtServer.TabIndex = 5;
      this.txtServer.Text = "localhost";

      this.GroupBox1.Controls.Add(this.cmdSend);
      this.GroupBox1.Controls.Add(this.LabelMessage);
      this.GroupBox1.Controls.Add(this.LabelSubject);
      this.GroupBox1.Controls.Add(this.LabelSentTo);
      this.GroupBox1.Controls.Add(this.LabelMailFrom);
      this.GroupBox1.Controls.Add(this.txtMessageText);
      this.GroupBox1.Controls.Add(this.txtMessageSubject);
      this.GroupBox1.Controls.Add(this.txtSendTo);
      this.GroupBox1.Controls.Add(this.txtMailFrom);
      this.GroupBox1.Location = new System.Drawing.Point(8, 10);
      this.GroupBox1.Name = "GroupBox1";
      this.GroupBox1.Size = new System.Drawing.Size(240, 216);
      this.GroupBox1.TabIndex = 10;
      this.GroupBox1.TabStop = false;
      this.GroupBox1.Text = "Email Editor";

      this.cmdSend.Location = new System.Drawing.Point(80, 184);
      this.cmdSend.Name = "cmdSend";
      this.cmdSend.TabIndex = 8;
      this.cmdSend.Text = "Send";
      this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);

      this.LabelMessage.AutoSize = true;
      this.LabelMessage.Location = new System.Drawing.Point(16, 104);
      this.LabelMessage.Name = "LabelMessage";
      this.LabelMessage.Size = new System.Drawing.Size(50, 16);
      this.LabelMessage.TabIndex = 7;
      this.LabelMessage.Text = "Message";
      this.LabelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.LabelSubject.AutoSize = true;
      this.LabelSubject.Location = new System.Drawing.Point(16, 74);
      this.LabelSubject.Name = "LabelSubject";
      this.LabelSubject.Size = new System.Drawing.Size(42, 16);
      this.LabelSubject.TabIndex = 6;
      this.LabelSubject.Text = "Subject";
      this.LabelSubject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.LabelSentTo.AutoSize = true;
      this.LabelSentTo.Location = new System.Drawing.Point(16, 50);
      this.LabelSentTo.Name = "LabelSentTo";
      this.LabelSentTo.Size = new System.Drawing.Size(43, 16);
      this.LabelSentTo.TabIndex = 5;
      this.LabelSentTo.Text = "Send to";
      this.LabelSentTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.LabelMailFrom.AutoSize = true;
      this.LabelMailFrom.Location = new System.Drawing.Point(16, 26);
      this.LabelMailFrom.Name = "LabelMailFrom";
      this.LabelMailFrom.Size = new System.Drawing.Size(51, 16);
      this.LabelMailFrom.TabIndex = 4;
      this.LabelMailFrom.Text = "Mail from";
      this.LabelMailFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.txtMessageText.Location = new System.Drawing.Point(80, 96);
      this.txtMessageText.Multiline = true;
      this.txtMessageText.Name = "txtMessageText";
      this.txtMessageText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtMessageText.Size = new System.Drawing.Size(136, 80);
      this.txtMessageText.TabIndex = 3;
      this.txtMessageText.Text = "this is the\r\nmulti-line test\r\n";

      this.txtMessageSubject.Location = new System.Drawing.Point(80, 72);
      this.txtMessageSubject.Name = "txtMessageSubject";
      this.txtMessageSubject.Size = new System.Drawing.Size(136, 20);
      this.txtMessageSubject.TabIndex = 2;
      this.txtMessageSubject.Text = "test message";

      this.txtSendTo.Location = new System.Drawing.Point(80, 48);
      this.txtSendTo.Name = "txtSendTo";
      this.txtSendTo.Size = new System.Drawing.Size(136, 20);
      this.txtSendTo.TabIndex = 1;
      this.txtSendTo.Text = "info";

      this.txtMailFrom.Location = new System.Drawing.Point(80, 24);
      this.txtMailFrom.Name = "txtMailFrom";
      this.txtMailFrom.Size = new System.Drawing.Size(136, 20);
      this.txtMailFrom.TabIndex = 0;
      this.txtMailFrom.Text = "test";

      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(512, 229);
      this.Controls.Add(this.GroupBox2);
      this.Controls.Add(this.GroupBox1);
      this.Name = "frmMain";
      this.Text = "Send Email";
      this.Load += new System.EventHandler(this.frmMain_Load);
      this.GroupBox2.ResumeLayout(false);
      this.GroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
	  
    [STAThread]
    static void Main() 
    {
      Application.Run(new frmMain());
    }

    private void cmdSend_Click(object sender, System.EventArgs e)
    {
	    // send mail
    }

    private void frmMain_Load(object sender, System.EventArgs e)
    {
      cmbAuth.Items.Add("None");
      cmbAuth.Items.Add("POP3");
      cmbAuth.Items.Add("Login");
      cmbAuth.Items.Add("Plain");
      cmbAuth.SelectedIndex = 0;
    }
  }
}
