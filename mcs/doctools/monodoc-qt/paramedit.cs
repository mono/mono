// paramedit.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using Mono.Document.Library;

	public class ParamEdit : QVGroupBox, IEditForm {

		DocMember member;

		public ParamEdit (DocMember member) : base (member.FullName)
		{
			this.member = member;
			foreach (DocParam param in member.Params)
			{
				QHBox hbox = new QHBox (this);
				QLabel label = new QLabel (hbox);
				label.SetText (param.Name);
				QLineEdit edit = new QLineEdit (hbox);
			}
		}

		public void Sync ()
		{
			Console.WriteLine (member.FullName);
		}

		public void Flush ()
		{
			Console.WriteLine ("Flush IO");
		}
	}
}