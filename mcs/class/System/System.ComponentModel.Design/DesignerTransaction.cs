//
// System.ComponentModel.Design.DesignerTransaction.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System;

namespace System.ComponentModel.Design
{
	public abstract class DesignerTransaction : IDisposable
	{
		private string description;
		private bool committed;
		private bool canceled;

		public DesignerTransaction () 
			: this ("")
		{
		}

		public DesignerTransaction (string description)
		{
			this.description = description;
			this.committed = false;
			this.canceled = false;
		}
		
		void IDisposable.Dispose () 
		{ 
			this.Dispose (true); 
		}
		
		public abstract void Dispose (bool disposing);

		protected abstract void OnCancel ();

		protected abstract void OnCommit ();

		public void Cancel ()
		{
			// LAMESPEC Cannot find anything about the exact behavior, but I do some checks that
			// seem to make sense
			if (this.Canceled == false && this.Committed == false) {
				this.canceled = true;
				OnCancel ();
			}
		}

		public void Commit ()
		{
			// LAMESPEC Cannot find anything about the exact behavior, but I do some checks that
			// seem to make sense
			if (this.Canceled == false && this.Committed == false) {
 				this.committed = true;
				OnCommit ();
			}
		}

		public bool Canceled {
			get {
				return canceled;
			}
		}

		public bool Committed {
			get {
				return committed;
			}
		}
		
		public string Description {
			get {
				return description;
			}
		}

		~DesignerTransaction ()
		{
		}
	}
}
