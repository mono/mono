// summaryform.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using Mono.Document.Library;

	public class SummaryForm : QVBox {

		QLineEdit edit;
		DocType document;
		
		public SummaryForm (DocType document, QWidget parent) : base (parent)
		{
			this.document = document;
			new QLabel (Global.Summary, this);
			edit = new QLineEdit (this);
			Connect (parent, SIGNAL ("Sync ()"), this, SLOT ("OnSync ()"));
			Connect (parent, SIGNAL ("Flush ()"), this, SLOT ("OnFlush ()"));
		}
		public void OnSync ()
		{
			edit.SetText (document.Summary);
		}
		public void OnFlush ()
		{
			document.Summary = edit.Text ();
		}
	}
}