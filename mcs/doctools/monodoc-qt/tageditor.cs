// doceditbar.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;

	public class TagContext : QPopupMenu {

		QTextEdit edit;

		public TagContext (QTextEdit edit) : base (edit)
		{
			this.edit = edit;
			InsertItem ("<see>", this, SLOT ("OnSee ()"));
			InsertItem ("<code>", this, SLOT ("OnCode ()"));
			InsertItem ("<data>", this, SLOT ("OnData ()"));
			InsertItem ("<list>", this, SLOT ("OnList ()"));
			InsertItem ("<example>", this, SLOT ("OnExample ()"));
			InsertItem ("<exception>", this, SLOT ("OnException ()"));
		}

		public void OnSee ()
		{
			edit.Insert ("<see></see>");
			BackUp ("</see>");
		}
		
		public void OnCode ()
		{
			edit.Insert ("<code></code>");
			BackUp ("</code>");
		}
		
		public void OnData ()
		{
			edit.Insert ("<data></data>");
			BackUp ("</data>");
		}
		
		public void OnList ()
		{
			edit.Insert ("<list></list>");
			BackUp ("</list>");
		}
		
		public void OnExample ()
		{
			edit.Insert ("<example></example>");
			BackUp ("</example>");
		}
		
		public void OnException ()
		{
			edit.Insert ("<exception></exception>");
			BackUp ("</exception>");
		}
		
		public void BackUp (string back)
		{
			foreach (char c in back)
				edit.MoveCursor (QTextEdit.CursorAction.MoveBackward, false);
		}
	}
}
