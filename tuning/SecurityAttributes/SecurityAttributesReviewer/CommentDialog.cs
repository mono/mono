using System;
using System.Windows.Forms;

namespace SecurityAttributesReviewer
{
	public partial class CommentDialog : Form
	{
		public CommentDialog()
		{
			InitializeComponent();
		}

		public string Prompt
		{
			get { return _prompt.Text; }
			set { _prompt.Text = value; }
		}

		public string Comment
		{
			get { return _commentTextBox.Text; }
		}
	}
}
