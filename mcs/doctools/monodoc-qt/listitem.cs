// listitem.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using Mono.Document.Library;

	public class ListItem : QListViewItem {

		DocType type;
		DocMember member;
		public bool IsBuilt, IsNamespace = false;
		public IEditForm EditForm;

		public ListItem (QListView parent, string text) : base (parent, text)
		{
			IsNamespace = true;
			SetPixmap (0, Global.IName);
		}

		public ListItem (QListViewItem parent, string text, DocMember member) : base (parent, text)
		{
			this.member = member;
			if (member.IsConstructor || member.IsMethod)
				SetPixmap (0, Global.IMethod);
			else if (member.IsEvent)
				SetPixmap (0, Global.IEvent);
			else if (member.IsField)
				SetPixmap (0, Global.IField);
			else if (member.IsOperator)
				SetPixmap (0, Global.IOperator);
			else if (member.IsProperty)
				SetPixmap (0, Global.IProperty);

		}

		public ListItem (QListViewItem parent, string text, DocType type) : base (parent, text)
		{
			this.type = type;
			if (type.IsClass)
				SetPixmap (0, Global.IClass);
			else if (type.IsDelegate)
				SetPixmap (0, Global.IDelegate);
			else if (type.IsEnum)
				SetPixmap (0, Global.IEnum);
			else if (type.IsInterface)
				SetPixmap (0, Global.IInterface);
			else if (type.IsStructure)
				SetPixmap (0, Global.IStructure);
		}

		public IEditForm BuildEditForm ()
		{
			if (type != null)
				EditForm = new TypeEdit (type);
			else if (member != null)
				EditForm = new ParamEdit (member);
			IsBuilt = true;
			return EditForm;
		}
	}
}
