//
// System.Web.UI.TemplateControl.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI {

	public abstract class TemplateControl : Control, INamingContainer
	{
		static object abortTransaction = new object ();
		static object commitTransaction = new object ();
		static object error = new object ();
		static string [] methodNames = { "Page_Init",
						 "Page_Load",
						 "Page_DataBind",
						 "Page_PreRender",
						 "Page_Dispose",
						 "Page_Error" };

		const BindingFlags bflags = BindingFlags.Public |
					    BindingFlags.NonPublic |
					    BindingFlags.DeclaredOnly |
					    BindingFlags.Static |
					    BindingFlags.Instance;

		#region Constructor
		protected TemplateControl ()
		{
			Construct ();
		}

		#endregion

		#region Properties

		protected virtual int AutoHandlers {
			get { return 0; }
			set { }
		}

		protected virtual bool SupportAutoEvents {
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

		internal void WireupAutomaticEvents ()
		{
			if (!SupportAutoEvents || !AutoEventWireup)
				return;

			Type type = GetType ();
			foreach (MethodInfo method in type.GetMethods (bflags)) {
				int pos = Array.IndexOf (methodNames, method.Name);
				if (pos == -1)
					continue;

				string name = methodNames [pos];
				pos = name.IndexOf ("_");
				if (pos == -1 || pos + 1 == name.Length)
					continue;

				if (method.ReturnType != typeof (void))
					continue;

				ParameterInfo [] parms = method.GetParameters ();
				if (parms.Length != 2 ||
				    parms [0].ParameterType != typeof (object) ||
				    parms [1].ParameterType != typeof (EventArgs))
				    continue;

				string eventName = name.Substring (pos + 1);
				EventInfo evt = type.GetEvent (eventName);
				if (evt == null)
					continue;

				evt.AddEventHandler (this, Delegate.CreateDelegate (typeof (EventHandler), method));
			}
		}

		protected virtual void FrameworkInitialize ()
		{
		}

		Type GetTypeFromControlPath (string virtualPath)
		{
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");

			return UserControlParser.GetCompiledType (TemplateSourceDirectory, virtualPath, Context);
		}

		public Control LoadControl (string virtualPath)
		{
			object control = Activator.CreateInstance (GetTypeFromControlPath (virtualPath));
			if (control is UserControl)
				((UserControl) control).InitializeAsUserControl (Page);

			return (Control) control;
		}

		public ITemplate LoadTemplate (string virtualPath)
		{
			Type t = GetTypeFromControlPath (virtualPath);
			return new SimpleTemplate (t);
		}

		protected virtual void OnAbortTransaction (EventArgs e)
		{
			EventHandler eh = Events [error] as EventHandler;
			if (eh != null)
				eh.Invoke (this, e);
		}

		protected virtual void OnCommitTransaction (EventArgs e)
		{
			EventHandler eh = Events [commitTransaction] as EventHandler;
			if (eh != null)
				eh.Invoke (this, e);
		}

		protected virtual void OnError (EventArgs e)
		{
			EventHandler eh = Events [abortTransaction] as EventHandler;
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

		public event EventHandler AbortTransaction {
			add { Events.AddHandler (abortTransaction, value); }
			remove { Events.RemoveHandler (abortTransaction, value); }
		}

		public event EventHandler CommitTransaction {
			add { Events.AddHandler (commitTransaction, value); }
			remove { Events.RemoveHandler (commitTransaction, value); }
		}

		public event EventHandler Error {
			add { Events.AddHandler (error, value); }
			remove { Events.RemoveHandler (error, value); }
		}

		#endregion

		class SimpleTemplate : ITemplate
		{
			Type type;

			public SimpleTemplate (Type type)
			{
				this.type = type;
			}

			public void InstantiateIn (Control control)
			{
				Control template = Activator.CreateInstance (type) as Control;
				template.SetBindingContainer (false);
				control.Controls.Add (template);
			}
		}
	}
}
