// docedit.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;

 	public class DocEdit : QWidgetStack {

		public DocEdit (QWidget parent) : base (parent)
		{
			SetMargin (10);
		}

		public void ListViewChanged (QListViewItem item)
		{
			ListItem listItem = item as ListItem;

			if (listItem.IsNamespace)
				return;
			if (listItem != null && !listItem.IsBuilt)
				AddWidget (listItem.BuildEditForm () as QWidget);

			IEditForm edit = VisibleWidget () as IEditForm;
			if (edit != null)
				edit.Flush ();

			listItem.EditForm.Sync ();
			RaiseWidget (listItem.EditForm as QWidget);
		}
	}
}