// options.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;

	[DeclareQtSignal ("OkClicked ()")]
	public class Options : QDialog {

		QCheckBox openPrev;

		public Options (QWidget parent) : base (parent, "options", true)
		{
			SetCaption ("Configure MonoDoc");
			SetIcon (Global.IMono);
			openPrev = new QCheckBox (this);
			openPrev.SetText ("Open previous Master.xml file upon startup");
			QPushButton ok = new QPushButton ("OK", this);
			QPushButton cancel = new QPushButton ("Cancel", this);

			Connect (ok, QtSupport.SIGNAL("clicked()"), this, QtSupport.SLOT("OkClicked()"));
			Connect (cancel, QtSupport.SIGNAL("clicked()"), this, SLOT("CancelClicked()"));

			QVBoxLayout dialogLayout = new QVBoxLayout (this);

			QHBoxLayout mainLayout = new QHBoxLayout (dialogLayout);
			mainLayout.AddWidget (openPrev);

			QHBoxLayout actionLayout = new QHBoxLayout (dialogLayout);
			actionLayout.AddWidget (ok);
			actionLayout.AddWidget (cancel);
		}

		public void CancelClicked ()
		{
			Reject ();
		}

		public void OkClicked ()
		{
			Emit ("OkClicked ()", null);
			Accept ();
		}

		public bool OpenPrevious
		{
			get { return openPrev.IsOn (); }
		}

		public void IsChecked (bool value)
		{
			openPrev.SetChecked (value);
		}
	}
}