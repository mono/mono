// typeedit.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using System.IO;
	using Mono.Document.Library;

	[DeclareQtSignal ("Sync ()")]
	[DeclareQtSignal ("Flush ()")]
	public class TypeEdit : QVGroupBox, IEditForm {

		DocType document;

		public TypeEdit (DocType document) : base (document.Name)
		{
			this.document = document;
			SetInsideMargin (20);
			SummaryForm sum = new SummaryForm (document, this);
			RemarksForm rem = new RemarksForm (document, this);
		}

		public void Sync ()
		{
			if (!File.Exists (document.FilePath))
				return;
			DocParser.Parse (document);
			Emit ("Sync ()", null);
			//Console.WriteLine ("Found doc for: "+document.Name);
		}

		public void Flush ()
		{
			Emit ("Flush ()", null);
			DocArchiver.Archive (document);
			//Console.WriteLine ("Wrote doc for:"+document.Name);
		}
	}
}
