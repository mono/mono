//
// System.ComponentModel.Design.DesignerTransaction.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.ComponentModel.Design
{
	public abstract class DesignerTransaction : IDisposable
	{
		private string description;
		private bool committed;
		private bool canceled;

		protected DesignerTransaction () 
			: this ("")
		{
		}

		protected DesignerTransaction (string description)
		{
			this.description = description;
			this.committed = false;
			this.canceled = false;
		}
		
		void IDisposable.Dispose () 
		{ 
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			Cancel ();
			if (disposing) {
				GC.SuppressFinalize (true);
			}
		}

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
			Dispose (false);
		}
	}
}
