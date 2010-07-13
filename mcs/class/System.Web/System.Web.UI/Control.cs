//
// System.Web.UI.Control.cs
//
// Authors:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//   Marek Habersack <mhabersack@novell.com>
//
// (C) Bob Smith
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (C) 2004-2010 Novell, Inc. (http://www.novell.com)
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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.Adapters;
using System.Web.UI.WebControls;
using System.Web.Util;

#if NET_4_0
using System.Web.Routing;
#endif

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("ID"), DesignerCategory ("Code"), ToolboxItemFilter ("System.Web.UI", ToolboxItemFilterType.Require)]
	[ToolboxItem ("System.Web.UI.Design.WebControlToolboxItem, " + Consts.AssemblySystem_Design)]
	[Designer ("System.Web.UI.Design.ControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DesignerSerializer ("Microsoft.VisualStudio.Web.WebForms.ControlCodeDomSerializer, " + Consts.AssemblyMicrosoft_VisualStudio_Web,
				"System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	[Bindable (true)]
	[Themeable (false)]
	public partial class Control : IComponent, IDisposable, IParserAccessor, IDataBindingsAccessor, IUrlResolutionService, IControlBuilderAccessor, IControlDesignerAccessor, IExpressionsAccessor
	{
		internal static readonly object DataBindingEvent = new object ();
		internal static readonly object DisposedEvent = new object ();
		internal static readonly object InitEvent = new object ();
		internal static readonly object LoadEvent = new object ();
		internal static readonly object PreRenderEvent = new object ();
		internal static readonly object UnloadEvent = new object ();
		internal static string [] defaultNameArray;
		/* */
		int event_mask;
		const int databinding_mask = 1;
		const int disposed_mask = 1 << 1;
		const int init_mask = 1 << 2;
		const int load_mask = 1 << 3;
		const int prerender_mask = 1 << 4;
		const int unload_mask = 1 << 5;
		/* */

		[ThreadStatic]
		static Dictionary <Type, bool> loadViewStateByIDCache;
		bool? loadViewStateByID;
		string uniqueID;
		string clientID;
		string _userId;
		ControlCollection _controls;
		Control _namingContainer;
		Page _page;
		Control _parent;
		ISite _site;
		StateBag _viewState;
		EventHandlerList _events;
		RenderMethod _renderMethodDelegate;
		Hashtable _controlsCache;
		int defaultNumberID;

		DataBindingCollection dataBindings;
		Hashtable pendingVS; // may hold unused viewstate data from child controls
		TemplateControl _templateControl;
		bool _isChildControlStateCleared;
		string _templateSourceDirectory;
#if NET_4_0
		ViewStateMode viewStateMode;
		ClientIDMode? clientIDMode;
		ClientIDMode? effectiveClientIDMode;
		Version renderingCompatibility;
		bool? renderingCompatibilityOld;
#endif
		/*************/
		int stateMask;
		const int ENABLE_VIEWSTATE = 1;
		const int VISIBLE = 1 << 1;
		const int AUTOID = 1 << 2;
		const int CREATING_CONTROLS = 1 << 3;
		const int BINDING_CONTAINER = 1 << 4;
		const int AUTO_EVENT_WIREUP = 1 << 5;
		const int IS_NAMING_CONTAINER = 1 << 6;
		const int VISIBLE_CHANGED = 1 << 7;
		const int TRACK_VIEWSTATE = 1 << 8;
		const int CHILD_CONTROLS_CREATED = 1 << 9;
		const int ID_SET = 1 << 10;
		const int INITED = 1 << 11;
		const int INITING = 1 << 12;
		const int VIEWSTATE_LOADED = 1 << 13;
		const int LOADED = 1 << 14;
		const int PRERENDERED = 1 << 15;
		const int ENABLE_THEMING = 1 << 16;
		const int AUTOID_SET = 1 << 17;
		const int REMOVED = 1 << 18;
		/*************/

		static Control ()
		{
			defaultNameArray = new string [100];
			for (int i = 0; i < 100; i++)
				defaultNameArray [i] = String.Concat ("ctl", i.ToString ("D2"));
		}

		public Control ()
		{
			stateMask = ENABLE_VIEWSTATE | VISIBLE | AUTOID | BINDING_CONTAINER | AUTO_EVENT_WIREUP;
			if (this is INamingContainer)
				stateMask |= IS_NAMING_CONTAINER;
#if NET_4_0
			viewStateMode = ViewStateMode.Inherit;
#endif
		}
		
		ControlAdapter adapter;
		bool did_adapter_lookup;
		protected internal ControlAdapter Adapter {
			get {
				if (!did_adapter_lookup) {
					adapter = ResolveAdapter ();
					if (adapter != null)
						adapter.control = this;
					did_adapter_lookup = true;
				}
				return adapter;
			}
		}

		string _appRelativeTemplateSourceDirectory = null;

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string AppRelativeTemplateSourceDirectory {
			get {
				if (_appRelativeTemplateSourceDirectory != null)
					return _appRelativeTemplateSourceDirectory;

				string tempSrcDir = null;
				TemplateControl templateControl = TemplateControl;
				if (templateControl != null) {
					string templateVirtualPath = templateControl.AppRelativeVirtualPath;
					if (!String.IsNullOrEmpty (templateVirtualPath))
						tempSrcDir = VirtualPathUtility.GetDirectory (templateVirtualPath, false);
				}
				
				_appRelativeTemplateSourceDirectory = (tempSrcDir != null) ? tempSrcDir : VirtualPathUtility.ToAppRelative (TemplateSourceDirectory);
				return _appRelativeTemplateSourceDirectory;
			}
			[EditorBrowsable (EditorBrowsableState.Never)]
			set {
				_appRelativeTemplateSourceDirectory = value;
				_templateSourceDirectory = null;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
#if NET_4_0
		[Bindable (true)]
#endif
		public Control BindingContainer {
			get {
				Control container = NamingContainer;
				if (container != null && container is INonBindingContainer || (stateMask & BINDING_CONTAINER) == 0)
					container = container.BindingContainer;

				return container;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("An Identification of the control that is rendered.")]
		public virtual string ClientID {
			get {
				if (clientID != null)
					return clientID;
#if NET_4_0
				clientID = GetClientID ();
#else
				clientID = UniqueID2ClientID (UniqueID);
#endif				
				stateMask |= ID_SET;
				return clientID;
			}
		}
#if NET_4_0
		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual Version RenderingCompatibility {
			get {
				if (renderingCompatibility == null) {
					var ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
					renderingCompatibility = ps != null ? ps.ControlRenderingCompatibilityVersion : new Version (4, 0);
				}

				return renderingCompatibility;
			}
			
			set {
				renderingCompatibility = value;
				renderingCompatibilityOld = null;
			}
		}

		internal bool RenderingCompatibilityLessThan40 {
			get {
				if (!renderingCompatibilityOld.HasValue)
					renderingCompatibilityOld = RenderingCompatibility < new Version (4, 0);

				return renderingCompatibilityOld.Value;
			}
		}
		
		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public Control DataItemContainer {
			get {
				Control container = NamingContainer;
				if (container == null)
					return null;

				if (container is IDataItemContainer)
					return container;

				return container.DataItemContainer;
			}
		}

		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public Control DataKeysContainer {
			get {
				Control container = NamingContainer;
				if (container == null)
					return null;

				if (container is IDataKeysControl)
					return container;

				return container.DataKeysContainer;
			}
		}

		[Themeable (false)]
		[DefaultValue (ClientIDMode.Inherit)]
		public virtual ClientIDMode ClientIDMode {
			get {
				if (!clientIDMode.HasValue)
					return ClientIDMode.Inherit;

				return clientIDMode.Value;
			}
			
			set {
				if (!clientIDMode.HasValue || clientIDMode.Value != value) {
					ClearCachedClientID ();
					ClearEffectiveClientIDMode ();
					clientIDMode = value;
				}
			}
		}

		internal ClientIDMode EffectiveClientIDMode {
			get {
				if (effectiveClientIDMode.HasValue)
					return effectiveClientIDMode.Value;
				
				ClientIDMode ret = ClientIDMode;
				if (ret != ClientIDMode.Inherit) {
					effectiveClientIDMode = ret;
					return ret;
				}
				
				// not sure about this, but it seems logical as INamingContainer is
				// the top of the hierarchy and it should "reset" the mode.
				Control container = NamingContainer;
				if (container != null) {
					effectiveClientIDMode = container.EffectiveClientIDMode;
					return effectiveClientIDMode.Value;
				}

				var ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
				effectiveClientIDMode = ps.ClientIDMode;

				return effectiveClientIDMode.Value;
			}	
		}

		protected void ClearCachedClientID ()
		{
			clientID = null;
			if (!HasControls ())
				return;

			for (int i = 0; i < _controls.Count; i++)
				_controls [i].ClearCachedClientID ();
		}

		protected void ClearEffectiveClientIDMode ()
		{
			effectiveClientIDMode = null;
			if (!HasControls ())
				return;

			for (int i = 0; i < _controls.Count; i++)
				_controls [i].ClearEffectiveClientIDMode ();
		}

		string GetClientID ()
		{
			switch (EffectiveClientIDMode) {
				case ClientIDMode.AutoID:
					return UniqueID2ClientID (UniqueID);

				case ClientIDMode.Predictable:
					EnsureID ();
					return GeneratePredictableClientID ();

				case ClientIDMode.Static:
					EnsureID ();
					return ID;

				default:
					throw new InvalidOperationException ("Unsupported ClientIDMode value.");
			}
		}
		
		string GeneratePredictableClientID ()
		{
			string myID = ID;
			bool haveMyID = !String.IsNullOrEmpty (myID);
			char separator = ClientIDSeparator;

			var sb = new StringBuilder ();
			Control container = NamingContainer;
			if (this is INamingContainer && !haveMyID) {
				if (container != null)
					EnsureIDInternal ();
				myID = _userId;
			}
			
			if (container != null && container != Page) {
				string containerID = container.ID;
				if (!String.IsNullOrEmpty (containerID)) {
					sb.Append (container.GetClientID ());
					sb.Append (separator);
				} else {
					sb.Append (container.GeneratePredictableClientID ());
					if (sb.Length > 0)
						sb.Append (separator);
				}
			}

			if (!haveMyID) {
				sb.Append (myID);
				return sb.ToString ();
			}
			
			sb.Append (myID);
			IDataItemContainer dataItemContainer = DataItemContainer as IDataItemContainer;
			if (dataItemContainer == null)
				return sb.ToString ();
			
			IDataKeysControl dataKeysContainer = DataKeysContainer as IDataKeysControl;
			GetDataBoundControlFieldValue (sb, separator, dataItemContainer, dataKeysContainer);
			
			return sb.ToString ();
		}

		void GetDataBoundControlFieldValue (StringBuilder sb, char separator, IDataItemContainer dataItemContainer, IDataKeysControl dataKeysContainer)
		{
			if (dataItemContainer is IDataBoundItemControl)
				return;
			
			int index = dataItemContainer.DisplayIndex;
			if (dataKeysContainer == null) {
				if (index >= 0) {
					sb.Append (separator);
					sb.Append (index);
				}
				return;
			}
			
			string[] suffixes = dataKeysContainer.ClientIDRowSuffix;
			DataKeyArray keys = dataKeysContainer.ClientIDRowSuffixDataKeys;
			if (keys == null || suffixes == null || suffixes.Length == 0) {
				sb.Append (separator);
				sb.Append (index);
				return;
			}

			object value;
			DataKey key = keys [index];
			foreach (string suffix in suffixes) {
				sb.Append (separator);
				value = key != null ? key [suffix] : null;
				if (value == null)
					continue;
				sb.Append (value.ToString ());
			}
		}
#endif
		internal string UniqueID2ClientID (string uniqueId)
		{
			if (String.IsNullOrEmpty (uniqueId))
				return null;
			
			return uniqueId.Replace (IdSeparator, ClientIDSeparator);
		}

		protected char ClientIDSeparator {
			get { return '_'; }
		}


		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The child controls of this control.")]
		public virtual ControlCollection Controls { //DIT
			get {
				if (_controls == null)
					_controls = CreateControlCollection ();
				return _controls;
			}
		}

		[MonoTODO ("revisit once we have a real design strategy")]
		protected internal bool DesignMode {
			get { return false; }
		}

		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("An Identification of the control that is rendered.")]
		[Themeable (false)]
		public virtual bool EnableViewState {
			get { return ((stateMask & ENABLE_VIEWSTATE) != 0); }
			set { SetMask (ENABLE_VIEWSTATE, value); }
		}

		[MergableProperty (false), ParenthesizePropertyName (true)]
		[WebSysDescription ("The name of the control that is rendered.")]
		[Filterable (false), Themeable (false)]
		public virtual string ID {
			get { return (((stateMask & ID_SET) != 0) ? _userId : null); }

			set {
				if (value != null && value.Length == 0)
					value = null;

				stateMask |= ID_SET;
				_userId = value;
				NullifyUniqueID ();
			}
		}

		protected internal bool IsChildControlStateCleared {
			get { return _isChildControlStateCleared; }
		}

		protected bool LoadViewStateByID {
			get {
				if (loadViewStateByID == null)
					loadViewStateByID = IsLoadViewStateByID ();

				return (bool)loadViewStateByID;
			}
		}

		protected internal bool IsViewStateEnabled {
			get {
				for (Control control = this; control != null; control = control.Parent) {
					if (!control.EnableViewState)
						return false;
#if NET_4_0
					ViewStateMode vsm = control.ViewStateMode;
					if (vsm != ViewStateMode.Inherit)
						return vsm == ViewStateMode.Enabled;
#endif
				}
				
				return true;
			}
		}

		protected char IdSeparator {
			get { return '$'; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The container that this control is part of. The control's name has to be unique within the container.")]
#if NET_4_0
		[Bindable (true)]
#endif
		public virtual Control NamingContainer {
			get {
				if (_namingContainer == null && _parent != null) {
					if ((_parent.stateMask & IS_NAMING_CONTAINER) == 0)
						_namingContainer = _parent.NamingContainer;
					else
						_namingContainer = _parent;
				}

				return _namingContainer;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The webpage that this control resides on.")]
		[Bindable (false)]
		public virtual Page Page { //DIT
			get {
				if (_page == null){
					if (NamingContainer != null)
						_page = NamingContainer.Page;
					else if (Parent != null)
						_page = Parent.Page;
				}
				return _page;
			}
			
			set { _page = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The parent control of this control.")]
#if NET_4_0
		[Bindable (true)]
#endif
		public virtual Control Parent { //DIT
			get { return _parent; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced), Browsable (false)]
		[WebSysDescription ("The site this control is part of.")]
		public ISite Site { //DIT
			get { return _site; }
			set { _site = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_4_0
		[Bindable (true)]
#endif
		public TemplateControl TemplateControl {
			get { return TemplateControlInternal; }

			[EditorBrowsable (EditorBrowsableState.Never)]
			set { _templateControl = value; }
		}

		internal virtual TemplateControl TemplateControlInternal {
			get {
				if (_templateControl != null)
					return _templateControl;
				if (_parent != null)
					return _parent.TemplateControl;
				return null;
			}
		}

#if !TARGET_J2EE
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("A virtual directory containing the parent of the control.")]
		public virtual string TemplateSourceDirectory {
			get {
				if (_templateSourceDirectory == null) {
					TemplateControl tc = TemplateControl;

					if (tc == null) {
						HttpContext ctx = Context;
						if (ctx != null)
							_templateSourceDirectory = VirtualPathUtility.GetDirectory (ctx.Request.CurrentExecutionFilePath);
					} else if (tc != this)
						_templateSourceDirectory = tc.TemplateSourceDirectory;

					if (_templateSourceDirectory == null && this is TemplateControl) {
						string path = ((TemplateControl) this).AppRelativeVirtualPath;

						if (path != null) {
							string ret = VirtualPathUtility.GetDirectory (VirtualPathUtility.ToAbsolute (path));
							int len = ret.Length;
							if (len <= 1)
								return ret;
							if (ret [--len] == '/')
								_templateSourceDirectory = ret.Substring (0, len);
						} else
							_templateSourceDirectory = String.Empty;
					}
					if (_templateSourceDirectory == null)
						_templateSourceDirectory = String.Empty;
				}

				return _templateSourceDirectory;
			}
		}
#endif

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The unique ID of the control.")]
		public virtual string UniqueID {
			get {
				if (uniqueID != null)
					return uniqueID;

				Control container = NamingContainer;
				if (container == null)
					return _userId;

				EnsureIDInternal ();
				string prefix = container.UniqueID;
				if (container == Page || prefix == null) {
					uniqueID = _userId;
#if TARGET_J2EE
					if (getFacesContext () != null)
						uniqueID = getFacesContext ().getExternalContext ().encodeNamespace (uniqueID);
#endif
					return uniqueID;
				}

				uniqueID = prefix + IdSeparator + _userId;
				return uniqueID;
			}
		}

		void SetMask (int m, bool val) {
			if (val)
				stateMask |= m;
			else
				stateMask &= ~m;
		}

		[DefaultValue (true), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("Visiblity state of the control.")]
		public virtual bool Visible {
			get {
				if ((stateMask & VISIBLE) == 0)
					return false;

				if (_parent != null)
					return _parent.Visible;

				return true;
			}

			set {
				if ((value && (stateMask & VISIBLE) == 0) ||
					(!value && (stateMask & VISIBLE) != 0)) {
					if (IsTrackingViewState)
						stateMask |= VISIBLE_CHANGED;
				}

				SetMask (VISIBLE, value);
			}
		}

		protected bool ChildControlsCreated {
			get { return ((stateMask & CHILD_CONTROLS_CREATED) != 0); }
			set {
				if (value == false && (stateMask & CHILD_CONTROLS_CREATED) != 0) {
					ControlCollection cc = Controls;
					if (cc != null)
						cc.Clear ();
				}

				SetMask (CHILD_CONTROLS_CREATED, value);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected internal virtual HttpContext Context { //DIT
			get {
				Page page = Page;
				if (page != null)
					return page.Context;
				
				return HttpContext.Current;
			}
		}

		protected EventHandlerList Events {
			get {
				if (_events == null)
					_events = new EventHandlerList ();
				return _events;
			}
		}

		protected bool HasChildViewState {
			get { return (pendingVS != null && pendingVS.Count > 0); }
		}

		protected bool IsTrackingViewState {
			get { return ((stateMask & TRACK_VIEWSTATE) != 0); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("ViewState")]
		protected virtual StateBag ViewState {
			get {
				if (_viewState == null)
					_viewState = new StateBag (ViewStateIgnoresCase);

				if (IsTrackingViewState)
					_viewState.TrackViewState ();

				return _viewState;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected virtual bool ViewStateIgnoresCase {
			get { return false; }
		}

		internal bool AutoEventWireup {
			get { return (stateMask & AUTO_EVENT_WIREUP) != 0; }
			set { SetMask (AUTO_EVENT_WIREUP, value); }
		}
		
		internal void SetBindingContainer (bool isBC)
		{
			SetMask (BINDING_CONTAINER, isBC);
		}

		internal void ResetChildNames ()
		{
			ResetChildNames (-1);
		}

		internal void ResetChildNames (int value)
		{
			if (value < 0)
				defaultNumberID = 0;
			else
				defaultNumberID = value;
		}

		internal int GetDefaultNumberID ()
		{
			return defaultNumberID;
		}

		string GetDefaultName ()
		{
			string defaultName;
			if (defaultNumberID > 99)
				defaultName = "ctl" + defaultNumberID++;
			else
				defaultName = defaultNameArray [defaultNumberID++];
			return defaultName;
		}

		void NullifyUniqueID ()
		{
			uniqueID = null;
#if NET_4_0
			ClearCachedClientID ();
#endif
			if (!HasControls ())
				return;

			for (int i = 0; i < _controls.Count; i++)
				_controls [i].NullifyUniqueID ();
		}

		bool IsLoadViewStateByID ()
		{
			if (loadViewStateByIDCache == null)
				loadViewStateByIDCache = new Dictionary <Type, bool> ();

			bool ret;
			Type myType = GetType ();
			if (loadViewStateByIDCache.TryGetValue (myType, out ret))
				return ret;

			System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes (myType);
			if (attrs != null || attrs.Count > 0) {
				ret = false;
				foreach (Attribute attr in attrs) {
					if (attr is ViewStateModeByIdAttribute) {
						ret = true;
						break;
					}
				}
			} else
				ret = false;
			
			loadViewStateByIDCache.Add (myType, ret);
			return ret;
		}
		
		protected internal virtual void AddedControl (Control control, int index)
		{
			ResetControlsCache ();

			/* Ensure the control don't have more than 1 parent */
			if (control._parent != null)
				control._parent.Controls.Remove (control);

			control._parent = this;
			Control nc = ((stateMask & IS_NAMING_CONTAINER) != 0) ? this : NamingContainer;

			if ((stateMask & (INITING | INITED)) != 0) {
				control.InitRecursive (nc);
				control.SetMask (REMOVED, false);
			} else {
				control.SetNamingContainer (nc);
				control.SetMask (REMOVED, false);
				return;
			}

			if ((stateMask & (VIEWSTATE_LOADED | LOADED)) != 0) {
				if (pendingVS != null) {
					object vs;
					bool byId = LoadViewStateByID;
					string id;
					
					if (byId) {
						control.EnsureID ();
						id = control.ID;
						vs = pendingVS [id];
					} else {
						id = null;
						vs = pendingVS [index];
					}
					
					if (vs != null) {
						if (byId)
							pendingVS.Remove (id);
						else
							pendingVS.Remove (index);
						
						if (pendingVS.Count == 0)
							pendingVS = null;

						control.LoadViewStateRecursive (vs);
					}
				}
			}

			if ((stateMask & LOADED) != 0)
				control.LoadRecursive ();

			if ((stateMask & PRERENDERED) != 0)
				control.PreRenderRecursiveInternal ();
		}
		
		void SetNamingContainer (Control nc)
		{
			if (nc != null) {
				_namingContainer = nc;
				if (AutoID)
					EnsureIDInternal ();
			}
		}

		protected virtual void AddParsedSubObject (object obj) //DIT
		{
			Control c = obj as Control;
			if (c != null)
				Controls.Add (c);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void ApplyStyleSheetSkin (Page page)
		{
			if (page == null)
				return;

			if (!EnableTheming) /* this enough? */
				return;

			/* apply the style sheet skin here */
			if (page.StyleSheetPageTheme != null) {
				ControlSkin cs = page.StyleSheetPageTheme.GetControlSkin (GetType (), SkinID);
				if (cs != null)
					cs.ApplySkin (this);
			}
		}

		[MonoTODO]
		protected void BuildProfileTree (string parentId, bool calcViewState)
		{
		}

		protected void ClearChildControlState ()
		{
			_isChildControlStateCleared = true;
		}

		protected void ClearChildState ()
		{
			ClearChildViewState ();
			ClearChildControlState ();
		}

		protected void ClearChildViewState ()
		{
			pendingVS = null;
		}

		protected internal virtual void CreateChildControls () //DIT
		{
		}

		protected virtual ControlCollection CreateControlCollection () //DIT
		{
			return new ControlCollection (this);
		}

		protected virtual void EnsureChildControls ()
		{
			if (ChildControlsCreated == false && (stateMask & CREATING_CONTROLS) == 0) {
				stateMask |= CREATING_CONTROLS;
				if (Adapter != null)
					Adapter.CreateChildControls ();
				else
					CreateChildControls ();
				ChildControlsCreated = true;
				stateMask &= ~CREATING_CONTROLS;
			}
		}

		void EnsureIDInternal ()
		{
			if (_userId != null)
				return;

			_userId = NamingContainer.GetDefaultName ();
			SetMask (AUTOID_SET, true);
		}

		protected void EnsureID ()
		{
			if (NamingContainer == null)
				return;
			EnsureIDInternal ();
			SetMask (ID_SET, true);
		}

		protected bool HasEvents ()
		{
			return _events != null;
		}

		void ResetControlsCache ()
		{
			_controlsCache = null;

			if ((this.stateMask & IS_NAMING_CONTAINER) == 0 && Parent != null)
				Parent.ResetControlsCache ();
		}

		Hashtable InitControlsCache ()
		{
			if (_controlsCache != null)
				return _controlsCache;

			if ((this.stateMask & IS_NAMING_CONTAINER) != 0 || Parent == null)
				//LAMESPEC: MS' docs don't mention it, but FindControl is case insensitive.
				_controlsCache = new Hashtable (StringComparer.OrdinalIgnoreCase);
			else
				_controlsCache = Parent.InitControlsCache ();

			return _controlsCache;
		}

		void EnsureControlsCache ()
		{
			if (_controlsCache != null)
				return;

			InitControlsCache ();

			FillControlCache (_controls);
		}

		void FillControlCache (ControlCollection controls)
		{
			if (controls == null || controls.Count == 0)
				return;
			
			foreach (Control c in controls) {
				try {
					if (c._userId != null)
						_controlsCache.Add (c._userId, c);
				} catch (ArgumentException) {
					throw new HttpException (
						"Multiple controls with the same ID '" + 
						c._userId + 
						"' were found. FindControl requires that controls have unique IDs. ");
				}

				if ((c.stateMask & IS_NAMING_CONTAINER) == 0 && c.HasControls ())
					FillControlCache (c.Controls);
			}
		}

		protected bool IsLiteralContent ()
		{
			if (_controls != null && _controls.Count == 1 && _controls [0] is LiteralControl)
				return true;

			return false;
		}

		[WebSysDescription ("")]
		public virtual Control FindControl (string id)
		{
			return FindControl (id, 0);
		}

		Control LookForControlByName (string id)
		{
			EnsureControlsCache ();
			return (Control) _controlsCache [id];
		}

		protected virtual Control FindControl (string id, int pathOffset)
		{
			EnsureChildControls ();
			Control namingContainer = null;
			if ((stateMask & IS_NAMING_CONTAINER) == 0) {
				namingContainer = NamingContainer;
				if (namingContainer == null)
					return null;
				
				return namingContainer.FindControl (id, pathOffset);
			}

			if (!HasControls ())
				return null;
			
			int separatorIdx = id.IndexOf (IdSeparator, pathOffset);
			if (separatorIdx == -1)
				return LookForControlByName (id.Substring (pathOffset));

			string idfound = id.Substring (pathOffset, separatorIdx - pathOffset);
			namingContainer = LookForControlByName (idfound);
			if (namingContainer == null)
				return null;

			return namingContainer.FindControl (id, separatorIdx + 1);
		}

		protected virtual void LoadViewState (object savedState)
		{
			if (savedState != null) {
				ViewState.LoadViewState (savedState);
				object o = ViewState ["Visible"];
				if (o != null) {
					SetMask (VISIBLE, (bool) o);
					stateMask |= VISIBLE_CHANGED;
				}
			}
		}

		// [MonoTODO("Secure?")]
		protected string MapPathSecure (string virtualPath)
		{
			string combined = UrlUtils.Combine (TemplateSourceDirectory, virtualPath);
			return Context.Request.MapPath (combined);
		}

		protected virtual bool OnBubbleEvent (object source, EventArgs args) //DIT
		{
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("OnBubbleEvent ", _userId, " ", type_name));
			}
#endif
			return false;
		}

		protected virtual void OnDataBinding (EventArgs e)
		{
			if ((event_mask & databinding_mask) != 0) {
				EventHandler eh = (EventHandler) (_events [DataBindingEvent]);
				if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Concat ("OnDataBinding ", _userId, " ", type_name));
					}
#endif
					eh (this, e);
				}
			}
		}

		protected internal virtual void OnInit (EventArgs e)
		{
			if ((event_mask & init_mask) != 0) {
				EventHandler eh = (EventHandler) (_events [InitEvent]);
				if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Concat ("OnInit ", _userId, " ", type_name));
					}
#endif
					eh (this, e);
				}
			}
		}

		protected internal virtual void OnLoad (EventArgs e)
		{
			if ((event_mask & load_mask) != 0) {
				EventHandler eh = (EventHandler) (_events [LoadEvent]);
				if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Concat ("OnLoad ", _userId, " ", type_name));
					}
#endif
					eh (this, e);
				}
			}
		}

		protected internal virtual void OnPreRender (EventArgs e)
		{
			if ((event_mask & prerender_mask) != 0) {
				EventHandler eh = (EventHandler) (_events [PreRenderEvent]);
				if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Concat ("OnPreRender ", _userId, " ", type_name));
					}
#endif
					eh (this, e);
				}
			}
		}

		protected internal virtual void OnUnload (EventArgs e)
		{
			if ((event_mask & unload_mask) != 0) {
				EventHandler eh = (EventHandler) (_events [UnloadEvent]);
				if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Concat ("OnUnload ", _userId, " ", type_name));
					}
#endif
					eh (this, e);
				}
			}
		}

		protected internal Stream OpenFile (string path)
		{
			try {
				string filePath = Context.Server.MapPath (path);
				return File.OpenRead (filePath);
			}
			catch (UnauthorizedAccessException) {
				throw new HttpException ("Access to the specified file was denied.");
			}
		}

		internal string GetPhysicalFilePath (string virtualPath)
		{
			Page page = Page;

			if (VirtualPathUtility.IsAbsolute (virtualPath))
				return page != null ? page.MapPath (virtualPath) : Context.Server.MapPath (virtualPath);

			// We need to determine whether one of our parents is a
			// master page. If so, we need to map the path
			// relatively to the master page and not our containing
			// page/control. This is necessary for cases when a
			// relative path is used in a control placed in a master
			// page and the master page is referenced from a
			// location other than its own. In such cases MS.NET looks
			// for the file in the directory where the master page
			// is.
			//
			// An example of where it is needed is at
			//
			// http://quickstarts.asp.net/QuickStartv20/aspnet/samples/masterpages/masterpages_cs/pages/default.aspx
			//
			MasterPage master = null;
			Control ctrl = Parent;

			while (ctrl != null) {
				if (ctrl is MasterPage) {
					master = ctrl as MasterPage;
					break;
				}
				ctrl = ctrl.Parent;
			}

			string path;
			if (master != null)
				path = VirtualPathUtility.Combine (master.TemplateSourceDirectory + "/", virtualPath);
			else
				path = VirtualPathUtility.Combine (TemplateSourceDirectory + "/", virtualPath);
			
			return page != null ? page.MapPath (path) : Context.Server.MapPath (path);
		}

		protected void RaiseBubbleEvent (object source, EventArgs args)
		{
			Control c = Parent;
			while (c != null) {
#if MONO_TRACE
				TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
				string type_name = null;
				if (trace != null) {
					type_name = GetType ().Name;
					trace.Write ("control", String.Concat ("RaiseBubbleEvent ", _userId, " ", type_name));
				}
#endif
				if (c.OnBubbleEvent (source, args)) {
#if MONO_TRACE
					if (trace != null)
						trace.Write ("control", String.Concat ("End RaiseBubbleEvent (false) ", _userId, " ", type_name));
#endif
					break;
				}
#if MONO_TRACE
				if (trace != null)
					trace.Write ("control", String.Concat ("End RaiseBubbleEvent (true) ", _userId, " ", type_name));
#endif
				c = c.Parent;
			}
		}

		protected internal virtual void Render (HtmlTextWriter writer) //DIT
		{
			RenderChildren (writer);
		}

		protected internal virtual void RenderChildren (HtmlTextWriter writer) //DIT
		{
			if (_renderMethodDelegate != null) {
				_renderMethodDelegate (writer, this);
				return;
			}

			if (_controls == null)
				return;

			int len = _controls.Count;
			Control c;
			for (int i = 0; i < len; i++) {
				c = _controls [i];
				if (c == null)
					continue;
				ControlAdapter tmp = c.Adapter;
				if (tmp != null)
					c.RenderControl (writer, tmp);
				else
					c.RenderControl (writer);
			}
		}

		protected virtual ControlAdapter ResolveAdapter ()
		{
			HttpContext context = Context;

			if (context == null)
				return null;

			if (!context.Request.BrowserMightHaveAdapters)
				return null;
				
			// Search up the type hierarchy until we find a control with an adapter.
			IDictionary typeMap = context.Request.Browser.Adapters;
			Type controlType = GetType ();
			Type adapterType = (Type)typeMap [controlType];
			while (adapterType == null && controlType != typeof (Control)) {
				controlType = controlType.BaseType;
				adapterType = (Type)typeMap [controlType];
			}

			ControlAdapter a = null;
			if (adapterType != null)
				a = (ControlAdapter)Activator.CreateInstance (adapterType);
			return a;
		}

		protected virtual object SaveViewState ()
		{
			if ((stateMask & VISIBLE_CHANGED) != 0) {
				ViewState ["Visible"] = (stateMask & VISIBLE) != 0;
			} else if (_viewState == null) {
				return null;
			}

			return _viewState.SaveViewState ();
		}

		protected virtual void TrackViewState ()
		{
			if (_viewState != null)
				_viewState.TrackViewState ();

			stateMask |= TRACK_VIEWSTATE;
		}

		public virtual void Dispose ()
		{
			if ((event_mask & disposed_mask) != 0) {
				EventHandler eh = (EventHandler) (_events [DisposedEvent]);
				if (eh != null)
					eh (this, EventArgs.Empty);
			}
		}

		[WebCategory ("FIXME")]
		[WebSysDescription ("Raised when the contols databound properties are evaluated.")]
		public event EventHandler DataBinding {
			add {
				event_mask |= databinding_mask;
				Events.AddHandler (DataBindingEvent, value);
			}
			remove { Events.RemoveHandler (DataBindingEvent, value); }
		}

		[WebSysDescription ("Raised when the contol is disposed.")]
		public event EventHandler Disposed {
			add {
				event_mask |= disposed_mask;
				Events.AddHandler (DisposedEvent, value);
			}
			remove { Events.RemoveHandler (DisposedEvent, value); }
		}

		[WebSysDescription ("Raised when the page containing the control is initialized.")]
		public event EventHandler Init {
			add {
				event_mask |= init_mask;
				Events.AddHandler (InitEvent, value);
			}
			remove { Events.RemoveHandler (InitEvent, value); }
		}

		[WebSysDescription ("Raised after the page containing the control has been loaded.")]
		public event EventHandler Load {
			add {
				event_mask |= load_mask;
				Events.AddHandler (LoadEvent, value);
			}
			remove { Events.RemoveHandler (LoadEvent, value); }
		}

		[WebSysDescription ("Raised before the page containing the control is rendered.")]
		public event EventHandler PreRender {
			add {
				event_mask |= prerender_mask;
				Events.AddHandler (PreRenderEvent, value);
			}
			remove { Events.RemoveHandler (PreRenderEvent, value); }
		}

		[WebSysDescription ("Raised when the page containing the control is unloaded.")]
		public event EventHandler Unload {
			add {
				event_mask |= unload_mask;
				Events.AddHandler (UnloadEvent, value);
			}
			remove { Events.RemoveHandler (UnloadEvent, value); }
		}

		public virtual void DataBind () //DIT
		{
			DataBind (true);
		}

		protected virtual void DataBindChildren ()
		{
			if (!HasControls ())
				return;

			int len = _controls != null ? _controls.Count : 0;
			for (int i = 0; i < len; i++) {
				Control c = _controls [i];
				c.DataBind ();
			}
		}

		public virtual bool HasControls ()
		{
			return (_controls != null && _controls.Count > 0);
		}
		
		public virtual void RenderControl (HtmlTextWriter writer)
		{
			if (this.adapter != null) {
				RenderControl (writer, this.adapter);
				return;
			}

			if ((stateMask & VISIBLE) != 0) {
				HttpContext ctx = Context;
				TraceContext trace = (ctx != null) ? ctx.Trace : null;
				int pos = 0;
				if ((trace != null) && trace.IsEnabled)
					pos = ctx.Response.GetOutputByteCount ();

				Render (writer);
				if ((trace != null) && trace.IsEnabled) {
					int size = ctx.Response.GetOutputByteCount () - pos;
					trace.SaveSize (this, size >= 0 ? size : 0);
				}
			}
		}

		protected void RenderControl (HtmlTextWriter writer, ControlAdapter adapter)
		{
			if ((stateMask & VISIBLE) != 0) {
				adapter.BeginRender (writer);
				adapter.Render (writer);
				adapter.EndRender (writer);
			}
		}

		public string ResolveUrl (string relativeUrl)
		{
			if (relativeUrl == null)
				throw new ArgumentNullException ("relativeUrl");

			if (relativeUrl == String.Empty)
				return relativeUrl;

			if (VirtualPathUtility.IsAbsolute (relativeUrl))
				return relativeUrl;

			if (relativeUrl [0] == '#')
				return relativeUrl;

			string ts = AppRelativeTemplateSourceDirectory;
			HttpContext ctx = Context;
			HttpResponse resp = ctx != null ? ctx.Response : null;
			if (ts == null || ts.Length == 0 || resp == null || relativeUrl.IndexOf (':') >= 0)
				return relativeUrl;
			
			if (!VirtualPathUtility.IsAppRelative (relativeUrl))
				relativeUrl = VirtualPathUtility.Combine (VirtualPathUtility.AppendTrailingSlash (ts), relativeUrl);
			
			return resp.ApplyAppPathModifier (relativeUrl);
		}


		public string ResolveClientUrl (string relativeUrl)
		{
			if (relativeUrl == null)
				throw new ArgumentNullException ("relativeUrl");

			if (relativeUrl.Length == 0)
				return String.Empty;

#if TARGET_J2EE
			relativeUrl = ResolveClientUrlInternal (relativeUrl);

			javax.faces.context.FacesContext faces = getFacesContext ();
			if (faces == null)
				return relativeUrl;

			string url;
			if (relativeUrl.IndexOf (':') >= 0)
				url = ResolveAppRelativeFromFullPath (relativeUrl);
			else if (VirtualPathUtility.IsAbsolute (relativeUrl))
				url = VirtualPathUtility.ToAppRelative (relativeUrl);
			else
				return faces.getApplication ().getViewHandler ().getResourceURL (faces, relativeUrl);

			if (VirtualPathUtility.IsAppRelative (url)) {
				url = url.Substring (1);
				url = url.Length == 0 ? "/" : url;
				return faces.getApplication ().getViewHandler ().getResourceURL (faces, url);
			}
			return relativeUrl;
		}
		
		string ResolveClientUrlInternal (string relativeUrl) {
			if (relativeUrl.StartsWith (J2EE.J2EEConsts.ACTION_URL_PREFIX, StringComparison.Ordinal))
				return CreateActionUrl (relativeUrl.Substring (J2EE.J2EEConsts.ACTION_URL_PREFIX.Length));

			if (relativeUrl.StartsWith (J2EE.J2EEConsts.RENDER_URL_PREFIX, StringComparison.Ordinal))
				return ResolveClientUrl (relativeUrl.Substring (J2EE.J2EEConsts.RENDER_URL_PREFIX.Length));

#endif
			if (VirtualPathUtility.IsAbsolute (relativeUrl) || relativeUrl.IndexOf (':') >= 0)
				return relativeUrl;

			HttpContext context = Context;
			HttpRequest req = context != null ? context.Request : null;
			if (req != null) {
				string templateSourceDirectory = TemplateSourceDirectory;
				if (templateSourceDirectory == null || templateSourceDirectory.Length == 0)
					return relativeUrl;

				string basePath = req.ClientFilePath;

				if (basePath.Length > 1 && basePath [basePath.Length - 1] != '/')
					basePath = VirtualPathUtility.GetDirectory (basePath, false);

				if (VirtualPathUtility.IsAppRelative (relativeUrl))
					return VirtualPathUtility.MakeRelative (basePath, relativeUrl);

				string templatePath = VirtualPathUtility.AppendTrailingSlash (templateSourceDirectory);

				if (basePath.Length == templatePath.Length && String.CompareOrdinal (basePath, templatePath) == 0)
					return relativeUrl;

				relativeUrl = VirtualPathUtility.Combine (templatePath, relativeUrl);
				return VirtualPathUtility.MakeRelative (basePath, relativeUrl);
			}
			return relativeUrl;
		}

		internal bool HasRenderMethodDelegate ()
		{
			return _renderMethodDelegate != null;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void SetRenderMethodDelegate (RenderMethod renderMethod) //DIT
		{
			_renderMethodDelegate = renderMethod;
		}

		internal void LoadRecursive ()
		{
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("LoadRecursive ", _userId, " ", type_name));
			}
#endif
			if (Adapter != null)
				Adapter.OnLoad (EventArgs.Empty);
			else
				OnLoad (EventArgs.Empty);
			int ccount = _controls != null ? _controls.Count : 0;
			for (int i = 0; i < ccount; i++) {
				Control c = _controls [i];
				c.LoadRecursive ();
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Concat ("End LoadRecursive ", _userId, " ", type_name));
#endif
			stateMask |= LOADED;
		}

		internal void UnloadRecursive (Boolean dispose)
		{
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("UnloadRecursive ", _userId, " ", type_name));
			}
#endif
			int ccount = _controls != null ? _controls.Count : 0;
			for (int i = 0; i < ccount; i++) {
				Control c = _controls [i];
				c.UnloadRecursive (dispose);
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Concat ("End UnloadRecursive ", _userId, " ", type_name));
#endif
			ControlAdapter tmp = Adapter;
			if (tmp != null)
				tmp.OnUnload (EventArgs.Empty);
			else
				OnUnload (EventArgs.Empty);
			if (dispose)
				Dispose ();
		}

		internal void PreRenderRecursiveInternal ()
		{
			bool visible;

			visible = Visible;			
			if (visible) {
				SetMask (VISIBLE, true);
				EnsureChildControls ();
#if MONO_TRACE
				TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
				string type_name = null;
				if (trace != null) {
					type_name = GetType ().Name;
					trace.Write ("control", String.Concat ("PreRenderRecursive ", _userId, " ", type_name));
				}
#endif
				if (Adapter != null)
					Adapter.OnPreRender (EventArgs.Empty);
				else
					OnPreRender (EventArgs.Empty);
				if (!HasControls ())
					return;

				int len = _controls != null ? _controls.Count : 0;
				for (int i = 0; i < len; i++) {
					Control c = _controls [i];
					c.PreRenderRecursiveInternal ();
				}
#if MONO_TRACE
				if (trace != null)
					trace.Write ("control", String.Concat ("End PreRenderRecursive ", _userId, " ", type_name));
#endif
			} else
				SetMask (VISIBLE, false);
			
			stateMask |= PRERENDERED;
		}

		internal void InitRecursive (Control namingContainer)
		{
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("InitRecursive ", _userId, " ", type_name));
			}
#endif
			SetNamingContainer (namingContainer);

			if (HasControls ()) {
				if ((stateMask & IS_NAMING_CONTAINER) != 0)
					namingContainer = this;

				int len = _controls != null ? _controls.Count : 0;
				for (int i = 0; i < len; i++) {
					Control c = _controls [i];
					c.InitRecursive (namingContainer);
				}
			}

			if ((stateMask & REMOVED) == 0 && (stateMask & INITED) != INITED) {
				stateMask |= INITING;
				ApplyTheme ();
				ControlAdapter tmp = Adapter;
				if (tmp != null)
					tmp.OnInit (EventArgs.Empty);
				else
					OnInit (EventArgs.Empty);
				TrackViewState ();
				stateMask |= INITED;
				stateMask &= ~INITING;
			}
			
#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Concat ("End InitRecursive ", _userId, " ", type_name));
#endif
		}

		internal object SaveViewStateRecursive ()
		{
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
#if MONO_TRACE
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("SaveViewStateRecursive ", _userId, " ", type_name));
			}
#endif

			ArrayList controlStates = null;
			bool byId = LoadViewStateByID;
			if (HasControls ()) {
				int len = _controls != null ? _controls.Count : 0;
				for (int i = 0; i < len; i++) {
					Control ctrl = _controls [i];
					object ctrlState = ctrl.SaveViewStateRecursive ();
					if (ctrlState == null)
						continue;

					if (controlStates == null)
						controlStates = new ArrayList ();
					if (byId) {
						ctrl.EnsureID ();
						controlStates.Add (new Pair (ctrl.ID, ctrlState));
					} else
						controlStates.Add (new Pair (i, ctrlState));
				}
			}

			object thisAdapterViewState = null;
			if (Adapter != null)
				thisAdapterViewState = Adapter.SaveAdapterViewState ();

			object thisState = null;

			if (IsViewStateEnabled)
				thisState = SaveViewState ();

			if (thisState == null && controlStates == null) {
				if (trace != null) {
#if MONO_TRACE
					trace.Write ("control", "End SaveViewStateRecursive " + _userId + " " + type_name + " saved nothing");
#endif
					trace.SaveViewState (this, null);
				}
				return null;
			}

			if (trace != null) {
#if MONO_TRACE
				trace.Write ("control", "End SaveViewStateRecursive " + _userId + " " + type_name + " saved a Triplet");
#endif
				trace.SaveViewState (this, thisState);
			}
			thisState = new object[] { thisState, thisAdapterViewState };
			return new Pair (thisState, controlStates);
		}

		internal void LoadViewStateRecursive (object savedState)
		{
			if (savedState == null)
				return;

#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("LoadViewStateRecursive ", _userId, " ", type_name));
			}
#endif
			Pair savedInfo = (Pair) savedState;
			object[] controlAndAdapterViewStates = (object [])savedInfo.First;
			if (Adapter != null)
				Adapter.LoadAdapterViewState (controlAndAdapterViewStates [1]);
			LoadViewState (controlAndAdapterViewStates [0]);

			ArrayList controlStates = savedInfo.Second as ArrayList;
			if (controlStates == null)
				return;

			int nControls = controlStates.Count;
			bool byId = LoadViewStateByID;
			for (int i = 0; i < nControls; i++) {
				Pair p = controlStates [i] as Pair;
				if (p == null)
					continue;

				if (byId) {
					string id = (string)p.First;
					bool found = false;
					
					foreach (Control c in Controls) {
						c.EnsureID ();
						if (c.ID == id) {
							found = true;
							c.LoadViewStateRecursive (p.Second);
							break;
						}
					}

					if (!found) {
						if (pendingVS == null)
							pendingVS = new Hashtable ();
						pendingVS [id] = p.Second;
					}
				} else {
					int k = (int) p.First;
					if (k < Controls.Count) {
						Control c = Controls [k];
						c.LoadViewStateRecursive (p.Second);
					} else {
						if (pendingVS == null)
							pendingVS = new Hashtable ();

						pendingVS [k] = p.Second;
					}
				}
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Concat ("End LoadViewStateRecursive ", _userId, " ", type_name));
#endif
			stateMask |= VIEWSTATE_LOADED;
		}

		internal ControlSkin controlSkin;

		internal void ApplyTheme ()
		{
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Concat ("ApplyThemeRecursive ", _userId, " ", type_name));
			}
#endif
			Page page = Page;
			if (page != null && page.PageTheme != null && EnableTheming) {
				ControlSkin controlSkin = page.PageTheme.GetControlSkin (GetType (), SkinID);
				if (controlSkin != null)
					controlSkin.ApplySkin (this);
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Concat ("End ApplyThemeRecursive ", _userId, " ", type_name));
#endif
		}

		internal bool AutoID {
			get { return (stateMask & AUTOID) != 0; }
			set {
				if (value == false && (stateMask & IS_NAMING_CONTAINER) != 0)
					return;

				SetMask (AUTOID, value);
			}
		}

		protected internal virtual void RemovedControl (Control control)
		{
			control.UnloadRecursive (false);
			control._parent = null;
			control._page = null;
			control._namingContainer = null;
			if ((control.stateMask & AUTOID_SET) != 0) {
				control._userId = null;
				control.SetMask (ID_SET, false);
			}
			control.NullifyUniqueID ();
			control.SetMask (REMOVED, true);
			ResetControlsCache ();
		}
		
		string skinId = string.Empty;
		bool _enableTheming = true;

		[Browsable (false)]
		[Themeable (false)]
		[DefaultValue (true)]
		public virtual bool EnableTheming {
			get {
				if ((stateMask & ENABLE_THEMING) != 0)
					return _enableTheming;

				if (_parent != null)
					return _parent.EnableTheming;

				return true;
			}
			set {
				SetMask (ENABLE_THEMING, true);
				_enableTheming = value;
			}
		}

		[Browsable (false)]
		[DefaultValue ("")]
		[Filterable (false)]
		public virtual string SkinID {
			get { return skinId; }
			set { skinId = value; }
		}

		ControlBuilder IControlBuilderAccessor.ControlBuilder {
			get { throw new NotImplementedException (); }
		}

		IDictionary IControlDesignerAccessor.GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}

		void IControlDesignerAccessor.SetDesignModeState (IDictionary designData)
		{
			SetDesignModeState (designData);
		}

		void IControlDesignerAccessor.SetOwnerControl (Control control)
		{
			throw new NotImplementedException ();
		}

		IDictionary IControlDesignerAccessor.UserData {
			get { throw new NotImplementedException (); }
		}

		ExpressionBindingCollection expressionBindings;

		ExpressionBindingCollection IExpressionsAccessor.Expressions {
			get {
				if (expressionBindings == null)
					expressionBindings = new ExpressionBindingCollection ();
				return expressionBindings;
			}
		}

		bool IExpressionsAccessor.HasExpressions {
			get { return (expressionBindings != null && expressionBindings.Count > 0); }
		}

		public virtual void Focus ()
		{
			Page.SetFocus (this);
		}

		protected internal virtual void LoadControlState (object state)
		{
		}

		protected internal virtual object SaveControlState ()
		{
			return null;
		}

		protected virtual void DataBind (bool raiseOnDataBinding)
		{
			bool foundDataItem = false;

			if ((stateMask & IS_NAMING_CONTAINER) != 0 && Page != null) {
				object o = DataBinder.GetDataItem (this, out foundDataItem);
				if (foundDataItem)
					Page.PushDataItemContext (o);
			}

			try {
				if (raiseOnDataBinding)
					OnDataBinding (EventArgs.Empty);
				DataBindChildren ();
			} finally {
				if (foundDataItem)
					Page.PopDataItemContext ();
			}
		}

		protected virtual IDictionary GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void SetDesignModeState (IDictionary data)
		{
			throw new NotImplementedException ();
		}

		internal bool IsInited {
			get { return (stateMask & INITED) != 0; }
		}

		internal bool IsLoaded {
			get { return (stateMask & LOADED) != 0; }
		}

		internal bool IsPrerendered {
			get { return (stateMask & PRERENDERED) != 0; }
		}

		bool CheckForValidationSupport ()
		{
			return GetType ().GetCustomAttributes (typeof (SupportsEventValidationAttribute), false).Length > 0;
		}

		//
		// Apparently this is where .NET routes validation from all the controls which
		// support it. See:
		//
		//  http://odetocode.com/Blogs/scott/archive/2006/03/20/3145.aspx
		//    Sample in here contains ValidateEvent in the stack trace
		//
		//  http://odetocode.com/blogs/scott/archive/2006/03/21/3153.aspx
		//
		//  http://www.alexthissen.nl/blogs/main/archive/2005/12/13/event-validation-of-controls-in-asp-net-2-0.aspx
		//
		// It also seems that it's the control's responsibility to call this method or
		// validation won't take place. Also, the SupportsEventValidation attribute must be
		// present on the control for validation to take place.
		//
		internal void ValidateEvent (String uniqueId, String argument)
		{
			Page page = Page;
			
			if (page != null && CheckForValidationSupport ())
				page.ClientScript.ValidateEvent (uniqueId, argument);
		}

		void IParserAccessor.AddParsedSubObject (object obj)
		{
			this.AddParsedSubObject (obj);
		}

		DataBindingCollection IDataBindingsAccessor.DataBindings {
			get {
				if (dataBindings == null) {
					dataBindings = new DataBindingCollection ();
				}
				return dataBindings;
			}
		}

		bool IDataBindingsAccessor.HasDataBindings {
			get {
				if (dataBindings != null && dataBindings.Count > 0) {
					return true;
				}
				return false;
			}
		}
#if NET_4_0
		[ThemeableAttribute(false)]
		[DefaultValue ("0")]
		public virtual ViewStateMode ViewStateMode {
			get { return viewStateMode;  }
			set {
				if (value < ViewStateMode.Inherit || value > ViewStateMode.Disabled)
					throw new ArgumentOutOfRangeException ("An attempt was made to set this property to a value that is not in the ViewStateMode enumeration.");

				viewStateMode = value;
			}
		}

		public string GetRouteUrl (object routeParameters)
		{
			return GetRouteUrl (null, new RouteValueDictionary (routeParameters));
		}

		public string GetRouteUrl (RouteValueDictionary routeParameters)
		{
			return GetRouteUrl (null, routeParameters);
		}

		public string GetRouteUrl (string routeName, object routeParameters)
		{
			return GetRouteUrl (routeName, new RouteValueDictionary (routeParameters));
		}

		public string GetRouteUrl (string routeName, RouteValueDictionary routeParameters)
		{
			HttpContext ctx = Context ?? HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;

			if (req == null)
				return null;

			VirtualPathData vpd = RouteTable.Routes.GetVirtualPath (req.RequestContext, routeName, routeParameters);
			if (vpd == null)
				return null;

			return vpd.VirtualPath;
		}

		public string GetUniqueIDRelativeTo (Control control)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			Control parent = this;
			Page page = Page;
			Control namingContainer = control.NamingContainer;
			
			if (namingContainer != null)
				while (parent != null && parent != namingContainer)
					parent = parent.Parent;

			if (parent != namingContainer)
				throw new InvalidOperationException (
					String.Format ("This control is not a descendant of the NamingContainer of '{0}'", control.UniqueID)
				);

			int idx = control.UniqueID.LastIndexOf (IdSeparator);
			if (idx < 0)
				return UniqueID;

			return UniqueID.Substring (idx + 1);
		}
#endif
	}
}
