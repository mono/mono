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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

using System.ComponentModel;

namespace System.Windows.Forms {
	public class ApplicationContext : IDisposable
	{
		#region Local Variables
		Form main_form;
		object tag;
		bool thread_exit_raised;

		#endregion	// Local Variables

		#region Public Constructors & Destructors
		public ApplicationContext() : this(null) {
		}

		public ApplicationContext(Form mainForm) {
			MainForm = mainForm; // Use  property to get event handling setup
		}

		~ApplicationContext() {
			this.Dispose(false);
		}
		#endregion	// Public Constructors & Destructors

		#region Public Instance Properties
		public Form MainForm {
			get {
				return main_form;
			}

			set {
				if (main_form != value) {
					// Catch when the form is destroyed so we can fire OnMainFormClosed

					if (main_form != null) {
						main_form.HandleDestroyed -= new EventHandler(OnMainFormClosed);
					}
					main_form = value;
					if (main_form != null) {
						main_form.HandleDestroyed += new EventHandler(OnMainFormClosed);
					}
				}
			}
		}

		[BindableAttribute (true)] 
		[DefaultValue (null)]
		[LocalizableAttribute (false)] 
		[TypeConverterAttribute (typeof(StringConverter))] 
		public Object Tag {
			get { return tag; }
			set { tag = value; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void ExitThread() {
			ExitThreadCore();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void Dispose(bool disposing) {
			MainForm = null;
			tag = null;
		}

		protected virtual void ExitThreadCore() {
			if (Application.MWFThread.Current.Context == this)
				XplatUI.PostQuitMessage(0);
			if (!thread_exit_raised && ThreadExit != null) {
				thread_exit_raised = true;
				ThreadExit(this, EventArgs.Empty);
			}
		}

		protected virtual void OnMainFormClosed(object sender, EventArgs e) {
			if (!MainForm.RecreatingHandle)
				ExitThreadCore();
		}
		#endregion	// Public Instance Methods

		#region Events
		public event EventHandler ThreadExit;
		#endregion	// Events
	}
}
