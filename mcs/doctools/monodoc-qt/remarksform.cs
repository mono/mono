// remarksform.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using Mono.Document.Library;

	public class RemarksForm : QVBox {

		QTextEdit edit;
		DocType document;
		
		public RemarksForm (DocType document, QWidget parent) : base (parent)
		{
			this.document = document;
			new QLabel (Global.Remarks, this);
			edit = new QTextEdit (this);
			Connect (parent, SIGNAL ("Sync ()"), this, SLOT ("OnSync ()"));
			Connect (parent, SIGNAL ("Flush ()"), this, SLOT ("OnFlush ()"));
		}
		public void OnSync ()
		{
			edit.SetText (document.Remarks);
		}
		public void OnFlush ()
		{
			document.Remarks = edit.Text ();
		}
	}
}