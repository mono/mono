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
		#region Constructor
		protected TemplateControl ()
		{
		}

		#endregion

		#region Properties

		[MonoTODO]
		protected virtual int AutoHandlers {
			get { return 1; }
			set { }
		}

		[MonoTODO]
		protected virtual bool SupportAutoEvents {
			get { return false; }
		}

		#endregion

		#region Methods

		protected virtual void Construct ()
		{
		}

		[MonoTODO]
		protected virtual LiteralControl CreateResourceBasedLiteralControl (
			int offset, int size, bool fAsciiOnly)
		{
			return null;
		}

		[MonoTODO]
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

		[MonoTODO]
		protected virtual void OnAbortTransaction (EventArgs e)
		{
		}

		[MonoTODO]
		protected virtual void OnCommitTransaction (EventArgs e)
		{
		}

		[MonoTODO]
		protected virtual void OnError (EventArgs e)
		{
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
		protected void SetStringresourcePointer (object stringResourcePointer,
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

		public event EventHandler AbortTransaction;
		public event EventHandler CommitTransaction;
		public event EventHandler Error;
		#endregion
	}
}
