//
// System.Web.UI.TemplateControl.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	public abstract class TemplateControl : Control, INamingContainer
	{
		private object abortTransaction = new object ();
		private object commitTransaction = new object ();
		private object error = new object ();

		#region Constructor
		protected TemplateControl ()
		{
			Construct ();
		}

		#endregion

		#region Properties

		protected virtual int AutoHandlers
		{
			get { return 0; }
			set { }
		}

		protected virtual bool SupportAutoEvents
		{
			get { return true; }
		}

		#endregion

		#region Methods

		protected virtual void Construct ()
		{
		}

		[MonoTODO]
		protected virtual LiteralControl CreateResourceBasedLiteralControl (int offset,
										    int size,
										    bool fAsciiOnly)
		{
			return null;
		}

		protected virtual void FrameworkInitialize ()
		{
		}

		[MonoTODO]
		public Control LoadControl (string virtualPath)
		{
			return null;
		}

		[MonoTODO]
		public ITemplate LoadTemplate (string virtualPath)
		{
			return null;
		}

		protected virtual void OnAbortTransaction (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [error];
			if (eh != null)
				eh.Invoke (this, e);
		}

		protected virtual void OnCommitTransaction (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [commitTransaction];
			if (eh != null)
				eh.Invoke (this, e);
		}

		protected virtual void OnError (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [abortTransaction];
			if (eh != null)
				eh.Invoke (this, e);
		}

		[MonoTODO]
		public Control ParseControl (string content)
		{
			return null;
		}

		[MonoTODO]
		public static object ReadStringResource (Type t)
		{
			return null;
		}

		[MonoTODO]
		protected void SetStringResourcePointer (object stringResourcePointer,
							 int maxResourceOffset)
		{
		}

		[MonoTODO]
		protected void WriteUTF8ResourceString (HtmlTextWriter output, int offset,
							int size, bool fAsciiOnly)
		{
		}

		#endregion

		#region Events

		public event EventHandler AbortTransaction
		{
			add {
				Events.AddHandler (abortTransaction, value);
			}

			remove {
				Events.RemoveHandler (abortTransaction, value);
			}
		}

		public event EventHandler CommitTransaction
		{
			add {
				Events.AddHandler (commitTransaction, value);
			}

			remove {
				Events.RemoveHandler (commitTransaction, value);
			}
		}

		public event EventHandler Error
		{
			add {
				Events.AddHandler (error, value);
			}

			remove {
				Events.RemoveHandler (error, value);
			}
		}

		#endregion
	}
}
