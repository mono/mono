// listview.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using System.IO;
	using System.Collections;
	using Mono.Document.Library;

	public class ListView : QListView {

		QListViewItem _namespace;
		Progress progress;

		public ListView (QWidget parent) : base (parent)
		{
			SetRootIsDecorated (true);
			AddColumn ("Namespace");
			SetSorting (-1);
			progress = new Progress (this);
		}

		public void LoadFile (string filename)
		{
			if(File.Exists (filename)) {
				progress.Show ();
				DocParser parser = new DocParser (filename);
				progress.SetTotalSteps (parser.DocTypes.Count - 1);
				progress.SetLabelText ("Loading Master.xml file");
				SetUpdatesEnabled (false);
				int i = 0;
				foreach(DocType type in parser.DocTypes) {
					type.FileRoot = Global.InitDir;
					type.Language = "en";
					progress.SetProgress (i++);
					ListItem typeitem = new ListItem (GetNamespaceItem (type.Namespace), type.Name, type);
					ProcessMember (type.Dtors, typeitem);
					ProcessMember (type.Events, typeitem);
					ProcessMember (type.Fields, typeitem);
					ProcessMember (type.Properties, typeitem);
					ProcessMember (type.Methods, typeitem);
					ProcessMember (type.Ctors, typeitem);
				}
				SetUpdatesEnabled (true);
				Repaint ();
			}
		}

		public void ProcessMember (ArrayList array, QListViewItem parent)
		{
			foreach (DocMember member in array) {
				if (parent != null && member != null) {
					new ListItem (parent, member.FullName, member);
				}
			}
		}

		public QListViewItem GetNamespaceItem (string name)
		{
			_namespace = FindItem (name, 0);
			if (_namespace == null) {
				SetUpdatesEnabled (true);
				_namespace = new ListItem (this, name);
				Repaint ();
				SetUpdatesEnabled (false);
			}
			return _namespace;
		}
	}
}