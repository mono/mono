//
// System.Web.UI.TemplateControl.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Web.Compilation;
using System.Web.Util;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class TemplateControl : Control, INamingContainer, IFilterResolutionService
	{
		static readonly Assembly _System_Web_Assembly = typeof (TemplateControl).Assembly;
		static object abortTransaction = new object ();
		static object commitTransaction = new object ();
		static object error = new object ();
		static string [] methodNames = { "Page_Init",
						 "Page_PreInit",
						 "Page_PreLoad",
						 "Page_LoadComplete",
						 "Page_PreRenderComplete",
						 "Page_SaveStateComplete",
						 "Page_InitComplete",
						 "Page_Load",
						 "Page_DataBind",
						 "Page_PreRender",
						 "Page_Disposed",
						 "Page_Error",
						 "Page_Unload",
						 "Page_AbortTransaction",
						 "Page_CommitTransaction"};

		const BindingFlags bflags = BindingFlags.Public |
					    BindingFlags.NonPublic |
					    BindingFlags.Instance;

		string _appRelativeVirtualPath;
		StringResourceData resource_data;
		
		#region Constructor
		protected TemplateControl ()
		{
			TemplateControl = this;
			Construct ();
		}

		#endregion

		#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		protected virtual int AutoHandlers {
			get { return 0; }
			set { }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected virtual bool SupportAutoEvents {
			get { return true; }
		}

		public string AppRelativeVirtualPath {
			get { return _appRelativeVirtualPath; }
			set { _appRelativeVirtualPath = value; }
		}

		#endregion

		#region Methods

		protected virtual void Construct ()
		{
		}

		protected LiteralControl CreateResourceBasedLiteralControl (int offset, int size, bool fAsciiOnly)
		{
			if (resource_data == null)
				return null;

			if (offset > resource_data.MaxOffset - size)
				throw new ArgumentOutOfRangeException ("size");

			IntPtr ptr = AddOffset (resource_data.Ptr, offset);
			return new ResourceBasedLiteralControl (ptr, size);
		}

		class EvtInfo {
			public MethodInfo method;
			public string methodName;
			public EventInfo evt;
			public bool noParams;
		}

		static SplitOrderedList<Type, ArrayList> auto_event_info = new SplitOrderedList<Type, ArrayList> (EqualityComparer<Type>.Default);

		internal void WireupAutomaticEvents ()
		{
			if (!SupportAutoEvents || !AutoEventWireup)
				return;

			/* Avoid expensive reflection operations by computing the event info only once */
			Type type = GetType ();
			ArrayList events = auto_event_info.InsertOrGet ((uint)type.GetHashCode (), type, null, CollectAutomaticEventInfo);

			for (int i = 0; i < events.Count; ++i) {
				EvtInfo evinfo = (EvtInfo)events [i];
				if (evinfo.noParams) {
					NoParamsInvoker npi = new NoParamsInvoker (this, evinfo.method);
					evinfo.evt.AddEventHandler (this, npi.FakeDelegate);
				} else
					evinfo.evt.AddEventHandler (this, Delegate.CreateDelegate (typeof (EventHandler), this, evinfo.method));
			}
		}

		ArrayList CollectAutomaticEventInfo () {
			ArrayList events = new ArrayList ();

			foreach (string methodName in methodNames) {
				MethodInfo method = null;
				Type type;
				for (type = GetType (); type.Assembly != _System_Web_Assembly; type = type.BaseType) {
					method = type.GetMethod (methodName, bflags);
					if (method != null)
						break;
				}
				if (method == null)
					continue;

				if (method.DeclaringType != type) {
					if (!method.IsPublic && !method.IsFamilyOrAssembly &&
					    !method.IsFamilyAndAssembly && !method.IsFamily)
						continue;
				}

				if (method.ReturnType != typeof (void))
					continue;

				ParameterInfo [] parms = method.GetParameters ();
				int length = parms.Length;
				bool noParams = (length == 0);
				if (!noParams && (length != 2 ||
				    parms [0].ParameterType != typeof (object) ||
				    parms [1].ParameterType != typeof (EventArgs)))
				    continue;

				int pos = methodName.IndexOf ('_');
				string eventName = methodName.Substring (pos + 1);
				EventInfo evt = type.GetEvent (eventName);
				if (evt == null) {
					/* This should never happen */
					continue;
				}

				EvtInfo evinfo = new EvtInfo ();
				evinfo.method = method;
				evinfo.methodName = methodName;
				evinfo.evt = evt;
				evinfo.noParams = noParams;

				events.Add (evinfo);
			}

			return events;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected virtual void FrameworkInitialize ()
		{
		}

		Type GetTypeFromControlPath (string virtualPath)
		{
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");

			string vpath = UrlUtils.Combine (TemplateSourceDirectory, virtualPath);
			return BuildManager.GetCompiledType (vpath);
		}

		public Control LoadControl (string virtualPath)
		{
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");
			Type type = GetTypeFromControlPath (virtualPath);
			
			return LoadControl (type, null);
		}

		public Control LoadControl (Type type, object[] parameters) 
		{
			object [] attrs = null;

			if (type != null)
				type.GetCustomAttributes (typeof (PartialCachingAttribute), true);
			if (attrs != null && attrs.Length == 1) {
				PartialCachingAttribute attr = (PartialCachingAttribute) attrs [0];
				PartialCachingControl ctrl = new PartialCachingControl (type, parameters);
				ctrl.VaryByParams = attr.VaryByParams;
				ctrl.VaryByControls = attr.VaryByControls;
				ctrl.VaryByCustom = attr.VaryByCustom;
				return ctrl;
			}

			object control = Activator.CreateInstance (type, parameters);
			if (control is UserControl)
				((UserControl) control).InitializeAsUserControl (Page);

			return (Control) control;
		}

		public ITemplate LoadTemplate (string virtualPath)
		{
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");
			Type t = GetTypeFromControlPath (virtualPath);
			return new SimpleTemplate (t);
		}

		protected virtual void OnAbortTransaction (EventArgs e)
		{
			EventHandler eh = Events [abortTransaction] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCommitTransaction (EventArgs e)
		{
			EventHandler eh = Events [commitTransaction] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnError (EventArgs e)
		{
			EventHandler eh = Events [error] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		public Control ParseControl (string content)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			// FIXME: This method needs to be rewritten in some sane way - the way it is now,
			// is a kludge. New version should not use
			// UserControlParser.GetCompiledType, but instead resort to some other way
			// of creating the content (template instantiation? BuildManager? TBD)
			TextReader reader = new StringReader (content);
			Type control = UserControlParser.GetCompiledType (reader, content.GetHashCode (), HttpContext.Current);
			if (control == null)
				return null;

			TemplateControl parsedControl = Activator.CreateInstance (control, null) as TemplateControl;
			if (parsedControl == null)
				return null;

			if (this is System.Web.UI.Page)
				parsedControl.Page = (System.Web.UI.Page) this;
			parsedControl.FrameworkInitialize ();
			
			Control ret = new Control ();
			int count = parsedControl.Controls.Count;
			Control[] parsedControlControls = new Control [count];
			parsedControl.Controls.CopyTo (parsedControlControls, 0);

			for (int i = 0; i < count; i++)
				ret.Controls.Add (parsedControlControls [i]);

			parsedControl = null;
			return ret;
		}

	        [MonoTODO ("Parser filters not implemented yet. Calls ParseControl (string) for now.")]
		public Control ParseControl (string content, bool ignoreParserFilter)
		{
			return ParseControl (content);
		}
	
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object ReadStringResource ()
		{
			return ReadStringResource (GetType ());
		}

		class StringResourceData {
			public IntPtr Ptr;
			public int Length;
			public int MaxOffset;
		}

		protected object GetGlobalResourceObject (string className, string resourceKey)
		{
			return HttpContext.GetGlobalResourceObject (className, resourceKey);
		}

		protected object GetGlobalResourceObject (string className, string resourceKey, Type objType, string propName)
		{
			if (String.IsNullOrEmpty (resourceKey) || String.IsNullOrEmpty (propName) ||
			    String.IsNullOrEmpty (className) || objType == null)
				return null;

			object globalObject = GetGlobalResourceObject (className, resourceKey);
			if (globalObject == null)
				return null;
			
			TypeConverter converter = TypeDescriptor.GetProperties (objType) [propName].Converter;
			if (converter == null || !converter.CanConvertFrom (globalObject.GetType ()))
				return null;
			
			return converter.ConvertFrom (globalObject);
		}

		protected object GetLocalResourceObject (string resourceKey)
		{
			return HttpContext.GetLocalResourceObject (VirtualPathUtility.ToAbsolute (this.AppRelativeVirtualPath),
								   resourceKey);
		}
		
		protected object GetLocalResourceObject (string resourceKey, Type objType, string propName)
		{
			if (String.IsNullOrEmpty (resourceKey) || String.IsNullOrEmpty (propName) || objType == null)
				return null;

			object localObject = GetLocalResourceObject (resourceKey);
			if (localObject == null)
				return null;
			
			TypeConverter converter = TypeDescriptor.GetProperties (objType) [propName].Converter;
			if (converter == null || !converter.CanConvertFrom (localObject.GetType ()))
				return null;
			
			return converter.ConvertFrom (localObject);
		}

		internal override TemplateControl TemplateControlInternal {
			get { return this; }
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public static object ReadStringResource (Type t)
		{
			StringResourceData data = new StringResourceData ();
			if (ICalls.GetUnmanagedResourcesPtr (t.Assembly, out data.Ptr, out data.Length))
				return data;

			throw new HttpException ("Unable to load the string resources.");
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected void SetStringResourcePointer (object stringResourcePointer,
							 int maxResourceOffset)
		{
			StringResourceData rd = stringResourcePointer as StringResourceData;
			if (rd == null)
				return;

			if (maxResourceOffset < 0 || maxResourceOffset > rd.Length)
				throw new ArgumentOutOfRangeException ("maxResourceOffset");

			resource_data = new StringResourceData ();
			resource_data.Ptr = rd.Ptr;
			resource_data.Length = rd.Length;
			resource_data.MaxOffset = maxResourceOffset > 0 ? Math.Min (maxResourceOffset, rd.Length) : rd.Length;
		}

		static IntPtr AddOffset (IntPtr ptr, int offset)
		{
			if (offset == 0)
				return ptr;

			if (IntPtr.Size == 4) {
				int p = ptr.ToInt32 () + offset;
				ptr = new IntPtr (p);
			} else {
				long p = ptr.ToInt64 () + offset;
				ptr = new IntPtr (p);
			}
			return ptr;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected void WriteUTF8ResourceString (HtmlTextWriter output, int offset, int size, bool fAsciiOnly)
		{
			if (resource_data == null)
				return; // throw?
			if (output == null)
				throw new ArgumentNullException ("output");
			if (offset > resource_data.MaxOffset - size)
				throw new ArgumentOutOfRangeException ("size");

			//TODO: fAsciiOnly?
			IntPtr ptr = AddOffset (resource_data.Ptr, offset);
			HttpWriter writer = output.GetHttpWriter ();
			
			if (writer == null || writer.Response.ContentEncoding.CodePage != 65001) {
				byte [] bytes = new byte [size];
				Marshal.Copy (ptr, bytes, 0, size);
				output.Write (Encoding.UTF8.GetString (bytes));
				bytes = null;
				return;
			}

			writer.WriteUTF8Ptr (ptr, size);
		}

		#endregion

		#region Events

		[WebSysDescription ("Raised when the user aborts a transaction.")]
		public event EventHandler AbortTransaction {
			add { Events.AddHandler (abortTransaction, value); }
			remove { Events.RemoveHandler (abortTransaction, value); }
		}

		[WebSysDescription ("Raised when the user initiates a transaction.")]
		public event EventHandler CommitTransaction {
			add { Events.AddHandler (commitTransaction, value); }
			remove { Events.RemoveHandler (commitTransaction, value); }
		}

		[WebSysDescription ("Raised when an exception occurs that cannot be handled.")]
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

		protected internal object Eval (string expression)
		{
			return DataBinder.Eval (Page.GetDataItem(), expression);
		}
	
		protected internal string Eval (string expression, string format)
		{
			return DataBinder.Eval (Page.GetDataItem(), expression, format);
		}
	
		protected internal object XPath (string xpathexpression)
		{
			return XPathBinder.Eval (Page.GetDataItem(), xpathexpression);
		}
	
		protected internal object XPath (string xpathexpression, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, null, resolver);
		}

		protected internal string XPath (string xpathexpression, string format)
		{
			return XPathBinder.Eval (Page.GetDataItem(), xpathexpression, format);
		}
	
		protected internal string XPath (string xpathexpression, string format, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, format, resolver);
		}

		protected internal IEnumerable XPathSelect (string xpathexpression)
		{
			return XPathBinder.Select (Page.GetDataItem(), xpathexpression);
		}

		protected internal IEnumerable XPathSelect (string xpathexpression, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Select (Page.GetDataItem (), xpathexpression, resolver);
		}

		// IFilterResolutionService

		[MonoTODO ("Not implemented")]
		int IFilterResolutionService.CompareFilters (string filter1, string filter2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		bool IFilterResolutionService.EvaluateFilter (string filterName)
		{
			throw new NotImplementedException ();
		}
	}
}
