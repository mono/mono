//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.IO;
using System.Web.J2EE;
using System.Xml;
using vmw.common;
using System.Web.Util;
using System.Collections.Generic;

namespace System.Web.UI {

	public abstract class TemplateControl : Control, INamingContainer
	{
		static readonly object abortTransaction = new object ();
		static readonly object commitTransaction = new object ();
		static readonly object error = new object ();
		static readonly string [] methodNames = { "Page_Init",
#if NET_2_0
						 "Page_PreInit",
						 "Page_PreLoad",
						 "Page_LoadComplete",
						 "Page_PreRenderComplete",
						 "Page_SaveStateComplete",
						 "Page_InitComplete",
#endif
						 "Page_Load",
						 "Page_DataBind",
						 "Page_PreRender",
						 "Page_Disposed",
						 "Page_Unload",
						 "Page_Error",
						 "Page_AbortTransaction",
						 "Page_CommitTransaction" };

		static readonly object [] EventKeys = {
						 Control.InitEvent,
#if NET_2_0
						 Page.PreInitEvent,
						 Page.PreLoadEvent,
						 Page.LoadCompleteEvent,
						 Page.PreRenderCompleteEvent,
						 Page.SaveStateCompleteEvent,
						 Page.InitCompleteEvent,
#endif
						Control.LoadEvent,
						Control.DataBindingEvent,
						Control.PreRenderEvent,
						Control.DisposedEvent,
						Control.UnloadEvent,
						error,
						abortTransaction,
						commitTransaction
		};

		enum LifeCycleEvent
		{
			Init,
#if NET_2_0
			PreInit,
			PreLoad,
			LoadComplete,
			PreRenderComplete,
			SaveStateComplete,
			InitComplete,
#endif
			Load,
			DataBinding,
			PreRender,
			Disposed,
			Unload,
			Error,
			AbortTransaction,
			CommitTransaction
		}

		const BindingFlags bflags = BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance;

		byte [] GetResourceBytes (Type type)
		{
			Hashtable table = (Hashtable) AppDomain.CurrentDomain.GetData ("TemplateControl.RES_BYTES");
			if (table == null) {
				return null;
			}
			return (byte []) table [type];
		}
		void SetResourceBytes (Type type, byte [] bytes)
		{
			Hashtable table = (Hashtable) AppDomain.CurrentDomain.GetData ("TemplateControl.RES_BYTES");
			if (table == null) {
				table = new Hashtable ();
				AppDomain.CurrentDomain.SetData ("TemplateControl.RES_BYTES", table);
			}
			table [type] = bytes;
			return;
		}

		Hashtable ResourceHash
		{
			get
			{
				Hashtable table = (Hashtable) AppDomain.CurrentDomain.GetData ("TemplateControl.RES_STRING");
				if (table == null) {
					table = new Hashtable ();
					AppDomain.CurrentDomain.SetData ("TemplateControl.RES_STRING", table);
				}
				return table;
			}
			set
			{
				AppDomain.CurrentDomain.SetData ("TemplateControl.RES_STRING", value);
			}
		}

		string CachedString (Type type, int offset, int size)
		{
			CacheKey key = new CacheKey (type, offset, size);

			string strObj = (string) ResourceHash [key];
			if (strObj == null) {
				char [] tmp = System.Text.Encoding.UTF8.GetChars (GetResourceBytes (this.GetType ()), offset, size);
				strObj = new string (tmp);

				Hashtable tmpResourceHash = (Hashtable) ResourceHash.Clone ();
				tmpResourceHash.Add (key, strObj);
				ResourceHash = tmpResourceHash;
			}
			return strObj;
		}
		public virtual string TemplateSourceDirectory_Private
		{
			get { return null; }
		}

		#region Constructor
		protected TemplateControl ()
		{
			Construct ();
		}

		#endregion

		#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected virtual int AutoHandlers
		{
			get { return 0; }
			set { }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
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
		protected LiteralControl CreateResourceBasedLiteralControl (int offset,
											int size,
											bool fAsciiOnly)
		{
			string str = CachedString (this.GetType (), offset, size);
			return new LiteralControl (str);
		}

		sealed class EventMethodMap
		{
			public EventMethodMap (LifeCycleEvent EventKeyIndex, MethodInfo Method, bool NoParameters)
			{
				this.EventKeyIndex = EventKeyIndex;
				this.Method = Method;
				this.NoParameters = NoParameters;
			}

			public readonly LifeCycleEvent EventKeyIndex;
			public readonly MethodInfo Method;
			public readonly bool NoParameters;
		}

		// This hashtable cashes methods and events found in user code
		const string eventMethodCacheKey = "eventMethodCacheKey";
		static Hashtable EventMethodCache
		{
			get { return (Hashtable) AppDomain.CurrentDomain.GetData (eventMethodCacheKey); }
			set { AppDomain.CurrentDomain.SetData (eventMethodCacheKey, value); }
		}

		internal void WireupAutomaticEvents ()
		{
			Type cacheKey = this.GetType ();
			Hashtable eventMethodCache = EventMethodCache;
			ArrayList eventMethodList = eventMethodCache == null ? null : (ArrayList) eventMethodCache [cacheKey];

			if (eventMethodList == null) {
				eventMethodList = new ArrayList ();

				if (!SupportAutoEvents || !AutoEventWireup)
					return;

				Type thisType = typeof (TemplateControl);
				Type voidType = typeof (void);
				Type [] DefaultParams = new Type [] {
				typeof (object),
				typeof (EventArgs) };

                LifeCycleEvent[] _pageEvents = new LifeCycleEvent[] { 
                    LifeCycleEvent.PreInit,
                    LifeCycleEvent.PreLoad,
                    LifeCycleEvent.LoadComplete,
                    LifeCycleEvent.PreRenderComplete,
                    LifeCycleEvent.SaveStateComplete,
                    LifeCycleEvent.InitComplete
                };
                List<LifeCycleEvent> pageEvents = new List<LifeCycleEvent>(_pageEvents);

                bool isPage = Page.GetType().IsAssignableFrom(GetType());

				for (int i = 0; i < methodNames.Length; i++) {
                    
                    // Don't look for page-only events in non-page controls.
                    if (!isPage && pageEvents.Contains((LifeCycleEvent)i))
                        continue;

					string methodName = methodNames [i];
					MethodInfo method;
					bool noParams = false;
					Type type = GetType ();
					do {
						method = type.GetMethod (methodName, bflags, null, DefaultParams, null);
						if (method != null) {
							break;
						}

						type = type.BaseType;
					}
					while (type != thisType);

					if (method == null) {
						type = GetType ();
						do {
							method = type.GetMethod (methodName, bflags, null, Type.EmptyTypes, null);
							if (method != null) {
								noParams = true;
								break;
							}

							type = type.BaseType;
						}
						while (type != thisType);

						if (method == null)
							continue;
					}
					if (method.ReturnType != voidType)
						continue;

					eventMethodList.Add (new EventMethodMap ((LifeCycleEvent) i, method, noParams));
				}
				// We copy to not lock

				Hashtable newEventMethodCache = eventMethodCache == null ? new Hashtable () : (Hashtable) eventMethodCache.Clone ();
				newEventMethodCache [cacheKey] = eventMethodList;
				EventMethodCache = newEventMethodCache;
			}

			foreach (EventMethodMap eventMethod in eventMethodList) {
				EventHandler handler = eventMethod.NoParameters ?
					new NoParamsInvoker (this, eventMethod.Method).FakeDelegate :
					(EventHandler)Delegate.CreateDelegate (typeof (EventHandler), this, eventMethod.Method);

				object eventKey = EventKeys [(int) eventMethod.EventKeyIndex];

				Delegate existing = Events [eventKey];
				if (existing != null && handler.Equals(existing))
					continue;

				switch (eventMethod.EventKeyIndex) {
				case LifeCycleEvent.Init:
					Init += handler;
					break;
#if NET_2_0
				case LifeCycleEvent.PreInit:
					((Page)this).PreInit += handler;
					break;
				case LifeCycleEvent.PreLoad:
					((Page) this).PreLoad += handler;
					break;
				case LifeCycleEvent.LoadComplete:
					((Page) this).LoadComplete += handler;
					break;
				case LifeCycleEvent.PreRenderComplete:
					((Page) this).PreRenderComplete += handler;
					break;
				case LifeCycleEvent.SaveStateComplete:
					((Page) this).SaveStateComplete += handler;
					break;
				case LifeCycleEvent.InitComplete:
					((Page) this).InitComplete += handler;
					break;
#endif
				case LifeCycleEvent.Load:
					Load += handler;
					break;
				case LifeCycleEvent.DataBinding:
					DataBinding += handler;
					break;
				case LifeCycleEvent.PreRender:
					PreRender += handler;
					break;
				case LifeCycleEvent.Disposed:
					Disposed += handler;
					break;
				case LifeCycleEvent.Unload:
					Unload += handler;
					break;
				case LifeCycleEvent.Error:
					Error += handler;
					break;
				case LifeCycleEvent.AbortTransaction:
					AbortTransaction += handler;
					break;
				case LifeCycleEvent.CommitTransaction:
					CommitTransaction += handler;
					break;
				}
			}
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
			return PageMapper.GetObjectType (Context, vpath);
		}

		public Control LoadControl (string virtualPath)
		{
#if NET_2_0
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");
#else
			if (virtualPath == null)
				throw new HttpException ("virtualPath is null");
#endif
			Type type = GetTypeFromControlPath (virtualPath);
			return LoadControl (type, null);
		}

		public Control LoadControl (Type type, object [] parameters)
		{
			object [] attrs = type.GetCustomAttributes (typeof (PartialCachingAttribute), true);
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

		[MonoNotSupported ("Not supported")]
		public Control ParseControl (string content)
		{
			throw new NotSupportedException ();
		}

		[MonoLimitation ("Always returns false")]
		public virtual bool TestDeviceFilter (string filterName)
		{
			return false;
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public static object ReadStringResource (Type t)
		{
			return t;
		}
#if NET_2_0
		[MonoTODO ("is this correct?")]
		public Object ReadStringResource ()
		{
			return this.GetType ();
		}
#endif
		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected void SetStringResourcePointer (object stringResourcePointer,
							 int maxResourceOffset)
		{
			if (GetResourceBytes (this.GetType ()) != null)
				return;

			java.lang.Class c = vmw.common.TypeUtils.ToClass (stringResourcePointer);
			java.lang.ClassLoader contextClassLoader = c.getClassLoader ();

			//TODO:move this code to page mapper
			string assemblyName = PageMapper.GetAssemblyResource (Context, VirtualPathUtility.ToAbsolute (AppRelativeVirtualPath));
			if (assemblyName == null)
				throw new HttpException (404, "The requested resource (" + this.AppRelativeVirtualPath + ") is not available.");

			java.io.InputStream inputStream = contextClassLoader.getResourceAsStream (assemblyName);

			System.IO.Stream strim = null;
			if (inputStream == null) {
				string descPath = String.Join ("/", new string [] { "assemblies", this.GetType ().Assembly.GetName ().Name, assemblyName });
				try {
					strim = new StreamReader (HttpContext.Current.Request.MapPath ("/" + descPath)).BaseStream;
				}
				catch (Exception ex) {
					throw new System.IO.IOException ("couldn't open resource file:" + assemblyName, ex);
				}
				if (strim == null)
					throw new System.IO.IOException ("couldn't open resource file:" + assemblyName);
			}

			try {
				if (strim == null)
					strim = (System.IO.Stream) vmw.common.IOUtils.getStream (inputStream);
				int capacity = (int) strim.Length;
				byte [] resourceBytes = new byte [capacity];
				strim.Read (resourceBytes, 0, capacity);
				SetResourceBytes (this.GetType (), resourceBytes);
			}
			catch (Exception e) {
				throw new HttpException ("problem with dll.ghres file", e);
			}
			finally {
				if (strim != null)
					strim.Close ();
				if (inputStream != null)
					inputStream.close ();
			}
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected void WriteUTF8ResourceString (HtmlTextWriter output, int offset,
							int size, bool fAsciiOnly)
		{
			string str = CachedString (this.GetType (), offset, size);
			output.Write (str);
		}

		#endregion

		#region Events

		[WebSysDescription ("Raised when the user aborts a transaction.")]
		public event EventHandler AbortTransaction
		{
			add { Events.AddHandler (abortTransaction, value); }
			remove { Events.RemoveHandler (abortTransaction, value); }
		}

		[WebSysDescription ("Raised when the user initiates a transaction.")]
		public event EventHandler CommitTransaction
		{
			add { Events.AddHandler (commitTransaction, value); }
			remove { Events.RemoveHandler (commitTransaction, value); }
		}

		[WebSysDescription ("Raised when an exception occurs that cannot be handled.")]
		public event EventHandler Error
		{
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

		sealed class CacheKey
		{
			readonly Type _type;
			readonly int _offset;
			readonly int _size;

			public CacheKey (Type type, int offset, int size)
			{
				_type = type;
				_offset = offset;
				_size = size;
			}

			public override int GetHashCode ()
			{
				return _type.GetHashCode () ^ _offset ^ _size;
			}

			public override bool Equals (object obj)
			{
				if (obj == null || !(obj is CacheKey))
					return false;

				CacheKey key = (CacheKey) obj;
				return key._type == _type && key._offset == _offset && key._size == _size;
			}
		}

#if NET_2_0

		string _appRelativeVirtualPath = null;

		public string AppRelativeVirtualPath
		{
			get { return _appRelativeVirtualPath; }
			set
			{
				if (value == null)
					throw new ArgumentNullException ("value");
				if (!UrlUtils.IsRooted (value) && !(value.Length > 0 && value [0] == '~'))
					throw new ArgumentException ("The path that is set is not rooted");
				_appRelativeVirtualPath = value;

				int lastSlash = _appRelativeVirtualPath.LastIndexOf ('/');
				AppRelativeTemplateSourceDirectory = (lastSlash > 0) ? _appRelativeVirtualPath.Substring (0, lastSlash + 1) : "~/";
			}
		}

		internal override TemplateControl TemplateControlInternal {
			get { return this; }
		}

		protected internal object Eval (string expression)
		{
			return DataBinder.Eval (Page.GetDataItem (), expression);
		}

		protected internal string Eval (string expression, string format)
		{
			return DataBinder.Eval (Page.GetDataItem (), expression, format);
		}

		protected internal object XPath (string xpathexpression)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression);
		}

		protected internal object XPath (string xpathexpression, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, null, resolver);
		}

		protected internal string XPath (string xpathexpression, string format)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, format);
		}

		protected internal string XPath (string xpathexpression, string format, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, format, resolver);
		}

		protected internal IEnumerable XPathSelect (string xpathexpression)
		{
			return XPathBinder.Select (Page.GetDataItem (), xpathexpression);
		}

		protected internal IEnumerable XPathSelect (string xpathexpression, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Select (Page.GetDataItem (), xpathexpression, resolver);
		}

		protected object GetGlobalResourceObject (string className, string resourceKey)
		{
			return HttpContext.GetGlobalResourceObject (className, resourceKey);
		}

		protected object GetGlobalResourceObject (string className, string resourceKey, Type objType, string propName)
		{
			return ConvertResource (GetGlobalResourceObject (className, resourceKey), objType, propName);
		}

		protected Object GetLocalResourceObject (string resourceKey)
		{
			return HttpContext.GetLocalResourceObject (Context.Request.Path, resourceKey);
		}

		protected Object GetLocalResourceObject (string resourceKey, Type objType, string propName)
		{
			return ConvertResource (GetLocalResourceObject (resourceKey), objType, propName);
		}

		static Object ConvertResource (Object resource, Type objType, string propName) {
			if (resource == null)
				return resource;

			PropertyDescriptor pdesc = TypeDescriptor.GetProperties (objType) [propName];
			if (pdesc == null)
				return resource;

			TypeConverter converter = pdesc.Converter;
			if (converter == null)
				return resource;

			return resource is string ?
				converter.ConvertFromInvariantString ((string) resource) :
				converter.ConvertFrom (resource);
		}

#endif

	}
}
