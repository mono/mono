// progress.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;

	public class Progress : QProgressDialog {

		QPushButton pb;

		public Progress (QWidget parent) : base (parent, "", true)
		{
			SetLabelText ("Parsing Master.xml file");
			SetTotalSteps (500);
			pb = new QPushButton (null);
			SetCancelButton (pb);
			pb.Hide ();
		}

		new public void Show ()
		{
			ForceShow ();
			SetProgress (1);
		}
	}
}