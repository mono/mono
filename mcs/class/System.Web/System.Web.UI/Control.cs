//
// System.Web.UI.Control.cs
//
// Authors:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua Javier (gonzalo@ximian.com
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) Bob Smith
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (C) 2004 Novell, Inc. (http://www.novell.com)
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

// This will provide extra information when trace is enabled. Might be too verbose.
#define MONO_TRACE

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Security.Permissions;
using System.Web;
using System.Web.Util;
using System.Globalization;
#if NET_2_0
using System.Web.UI.Adapters;
using System.IO;
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
#if NET_2_0
	[DesignerSerializer ("Microsoft.VisualStudio.Web.WebForms.ControlCodeDomSerializer, " + Consts.AssemblyMicrosoft_VisualStudio_Web,
				"System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	[Bindable (true)]
	[Themeable (false)]
#else
	[DesignerSerializer ("Microsoft.VSDesigner.WebForms.ControlCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner,
				"System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
#endif		
        public partial class Control : IComponent, IDisposable, IParserAccessor, IDataBindingsAccessor
#if NET_2_0
        , IUrlResolutionService, IControlBuilderAccessor, IControlDesignerAccessor, IExpressionsAccessor
#endif
        {
		static readonly object DataBindingEvent = new object();
		static readonly object DisposedEvent = new object();
		static readonly object InitEvent = new object();
		static readonly object LoadEvent = new object();
		static readonly object PreRenderEvent = new object();
		static readonly object UnloadEvent = new object();
		static string[] defaultNameArray;
		/* */
		int event_mask;
		const int databinding_mask = 1;
		const int disposed_mask = 1 << 1;
		const int init_mask = 1 << 2;
		const int load_mask = 1 << 3;
		const int prerender_mask = 1 << 4;
		const int unload_mask = 1 << 5;
		/* */

		string uniqueID;
		string _userId;
		ControlCollection _controls;
		Control _namingContainer;
		Page _page;
		Control _parent;
		ISite _site;
		HttpContext _context;
		StateBag _viewState;
		EventHandlerList _events;
		RenderMethod _renderMethodDelegate;
		int defaultNumberID;
 
		DataBindingCollection dataBindings;
		Hashtable pendingVS; // may hold unused viewstate data from child controls
		

#if NET_2_0
		TemplateControl _templateControl;
		bool _isChildControlStateCleared;
#endif
		/*************/
		int stateMask;
		const int ENABLE_VIEWSTATE 	= 1;
		const int VISIBLE 		= 1 << 1;
		const int AUTOID		= 1 << 2;
		const int CREATING_CONTROLS	= 1 << 3;
		const int BINDING_CONTAINER	= 1 << 4;
		const int AUTO_EVENT_WIREUP	= 1 << 5;
		const int IS_NAMING_CONTAINER	= 1 << 6;
		const int VISIBLE_CHANGED	= 1 << 7;
		const int TRACK_VIEWSTATE	= 1 << 8;
		const int CHILD_CONTROLS_CREATED = 1 << 9;
		const int ID_SET		= 1 << 10;
		const int INITED		= 1 << 11;
		const int INITING		= 1 << 12;
		const int VIEWSTATE_LOADED	= 1 << 13;
		const int LOADED		= 1 << 14;
		const int PRERENDERED		= 1 << 15;
#if NET_2_0
		const int ENABLE_THEMING	= 1 << 16;
#endif
		/*************/
		
		static Control ()
		{
			defaultNameArray = new string [100];
			for (int i = 0 ; i < 100 ; i++)
#if NET_2_0
				defaultNameArray [i] = String.Format("ctl{0:D2}", i);
#else
				defaultNameArray [i] = "_ctl" + i;
#endif
		}

                public Control()
                {
			stateMask = ENABLE_VIEWSTATE | VISIBLE | AUTOID | BINDING_CONTAINER | AUTO_EVENT_WIREUP;
                        if (this is INamingContainer)
				stateMask |= IS_NAMING_CONTAINER;
                }

#if NET_2_0
		[MonoTODO("Not implemented, always returns null")]
		protected ControlAdapter Adapter 
		{
			get {
				// for the time being, fool the
				// Control machinery into thinking we
				// don't have an Adapter.  This will
				// allow us to write all the rest of
				// the Adapter handling code without
				// having to worry about *having*
				// adapters.
				return null;
			}
		}

		string _appRelativeTemplateSourceDirectory = null;

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string AppRelativeTemplateSourceDirectory 
		{
			get {
				if (_appRelativeTemplateSourceDirectory != null && !(this is MasterPage))
					return _appRelativeTemplateSourceDirectory;

				if (Parent != null)
					return Parent.AppRelativeTemplateSourceDirectory;

				return "~/";
			}
			[EditorBrowsable (EditorBrowsableState.Never)]
			set	{ _appRelativeTemplateSourceDirectory = value; }
		}
		
#endif		

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
		public Control BindingContainer {
			get {
				Control container = NamingContainer;
				if (container != null && (container.stateMask & BINDING_CONTAINER) == 0)
					container = container.BindingContainer;
				return container;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("An Identification of the control that is rendered.")]
		public virtual string ClientID {
			get {
				string client = UniqueID;

				if (client != null)
#if NET_2_0
					client = UniqueID2ClientID (client);
#else
					client = client.Replace (':', ClientIDSeparator);
#endif
				
				stateMask |= ID_SET;
				return client;
			}
		}

#if NET_2_0
			internal string UniqueID2ClientID (string uniqueId)
			{
				return uniqueId.Replace (IdSeparator, ClientIDSeparator);
			}

		protected char ClientIDSeparator
#else
		char ClientIDSeparator
#endif		
		{
			get {
				return '_';
			}
		}


		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The child controls of this control.")]
                public virtual ControlCollection Controls //DIT
                {
                        get
                        {
                                if (_controls == null) _controls = CreateControlCollection();
                                return _controls;
                        }
                }

#if NET_2_0
		[MonoTODO ("revisit once we have a real design strategy")]
		protected internal bool DesignMode 
		{
			get { return false; }
		}
#endif		

		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("An Identification of the control that is rendered.")]
#if NET_2_0
		[Themeable (false)]
#endif                
                public virtual bool EnableViewState {
                        get { return ((stateMask & ENABLE_VIEWSTATE) != 0); }
			set { SetMask (ENABLE_VIEWSTATE, value); }
                }
		
		[MergableProperty (false), ParenthesizePropertyName (true)]
		[WebSysDescription ("The name of the control that is rendered.")]
#if NET_2_0
		[Filterable (false), Themeable (false)]
#endif                

                public virtual string ID {
                        get {
				return (((stateMask & ID_SET) != 0) ? _userId : null);
                        }
			
                        set {
				if (value == "")
					value = null;

				stateMask |= ID_SET;
                                _userId = value;
				NullifyUniqueID ();
                        }
                }

#if NET_2_0
		protected char IdSeparator 
		{
			get {
				return '$';
			}
		}

		protected internal bool IsChildControlStateCleared {
			get { return _isChildControlStateCleared; }
		}

		protected internal bool IsViewStateEnabled 
		{
			get {

				for (Control control = this; control != null; control = control.Parent)
					if (!control.EnableViewState)
						return false;

				return true;
			}
		}

		protected bool LoadViewStateByID 
		{
			get {
				return false;
			}
		}
#endif		
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The container that this control is part of. The control's name has to be unique within the container.")]
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
#if NET_2_0
		[Bindable (false)]
#endif                
		public virtual Page Page //DIT
                {
                        get
                        {
                                if (_page == null && _parent != null) _page = _parent.Page;
                                return _page;
                        }
                        set
                        {
                                _page = value;
                        }
                }

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The parent control of this control.")]
                public virtual Control Parent //DIT
                {
                        get
                        {
                                return _parent;
                        }
                }

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced), Browsable (false)]
		[WebSysDescription ("The site this control is part of.")]
                public ISite Site //DIT
                {
                        get
                        {
                                return _site;
                        }
                        set
                        {
                                _site = value;
                        }
                }

#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TemplateControl TemplateControl 
		{
			get {
				return _templateControl;
			}
			[EditorBrowsable (EditorBrowsableState.Never)]
			set {
				_templateControl = value;
			}
		}
#endif		

#if !TARGET_J2EE
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("A virtual directory containing the parent of the control.")]
                public virtual string TemplateSourceDirectory {
			get { return (_parent == null) ? String.Empty : _parent.TemplateSourceDirectory; }
                }
#endif

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The unique ID of the control.")]
                public virtual string UniqueID {
                        get {
				if (uniqueID != null)
					return uniqueID;

				if (_namingContainer == null) {
					if ((stateMask & IS_NAMING_CONTAINER) == 0)
						_namingContainer = NamingContainer;
					if (_namingContainer == null)
						return _userId;
				}

				if (_userId == null)
					_userId = _namingContainer.GetDefaultName ();

				string prefix = _namingContainer.UniqueID;
#if TARGET_J2EE
				// For J2EE portlets we need to add the namespace to the ID.
				if (_namingContainer == _page && _page.PortletNamespace != null)
					prefix = _page.PortletNamespace;
				else
#endif
				if (_namingContainer == _page || prefix == null) {
					uniqueID = _userId;
					return uniqueID;
				}

#if NET_2_0
				uniqueID = prefix + IdSeparator + _userId;
#else
				uniqueID = prefix + ":" + _userId;
#endif
				return uniqueID;
                        }
                }

		void SetMask (int m, bool val)
		{
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
					if (_controls != null)
						_controls.Clear();
				}

				SetMask (CHILD_CONTROLS_CREATED, value);
                        }
                }

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
                protected virtual HttpContext Context //DIT
                {
                        get
                        {
                                HttpContext context;
                                if (_context != null)
                                        return _context;
                                if (_parent == null)
                                        return HttpContext.Current;
                                context = _parent.Context;
                                if (context != null)
                                        return context;
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
                        get {
				return (pendingVS != null && pendingVS.Count > 0);
                        }
                }

                protected bool IsTrackingViewState {
                        get { return ((stateMask & TRACK_VIEWSTATE) != 0); }
                }

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("ViewState")]
                protected virtual StateBag ViewState
                {
                        get
                        {
                        	if(_viewState == null)
	                        	_viewState = new StateBag (ViewStateIgnoresCase);

				if (IsTrackingViewState)
					_viewState.TrackViewState ();

                        	return _viewState;
                        }
                }

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
                protected virtual bool ViewStateIgnoresCase
                {
                        get {
                                return false;
                        }
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
			defaultNumberID = 0;
		}

		string GetDefaultName ()
		{
			string defaultName;
			if (defaultNumberID > 99) {
#if NET_2_0
				defaultName = "ctl" + defaultNumberID++;
#else
				defaultName = "_ctl" + defaultNumberID++;
#endif
			} else {
				defaultName = defaultNameArray [defaultNumberID++];
			}
			return defaultName;
		}

		void NullifyUniqueID ()
		{
			uniqueID = null;
			if (!HasControls ())
				return;

			foreach (Control c in _controls)
				c.NullifyUniqueID ();
		}

		protected internal virtual void AddedControl (Control control, int index)
		{
			/* Ensure the control don't have more than 1 parent */
			if (control._parent != null)
				control._parent.Controls.Remove (control);

			control._parent = this;
			control._page = _page;
			Control nc = ((stateMask & IS_NAMING_CONTAINER) != 0) ? this : NamingContainer;

			if (nc != null) {
				control._namingContainer = nc;
				if (control.AutoID == true && control._userId == null)
					control._userId =  nc.GetDefaultName ();
			}

			if ((stateMask & (INITING | INITED)) != 0)
				control.InitRecursive (nc);

			if ((stateMask & (VIEWSTATE_LOADED | LOADED)) != 0) {
				if (pendingVS != null) {
					object vs = pendingVS [index];
					if (vs != null) {
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

                protected virtual void AddParsedSubObject(object obj) //DIT
                {
                        Control c = obj as Control;
                        if (c != null) Controls.Add(c);
                }

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void ApplyStyleSheetSkin (Page page)
		{
			if (!EnableTheming) /* this enough? */
				return;

			/* apply the style sheet skin here */
			if (page.StyleSheetPageTheme != null) {
				ControlSkin cs = page.StyleSheetPageTheme.GetControlSkin (GetType(), SkinID);
				if (cs != null)
					cs.ApplySkin (this);
			}
		}
#endif		

                protected void BuildProfileTree(string parentId, bool calcViewState)
                {
                        //TODO
                }

#if NET_2_0
		protected void ClearChildControlState ()
		{
			_isChildControlStateCleared = true;
		}

		protected void ClearChildState ()
		{
			ClearChildViewState ();
			ClearChildControlState ();
		}
#endif		

                protected void ClearChildViewState ()
                {
			pendingVS = null;
                }

#if NET_2_0
                protected internal
#else
		protected
#endif		
		virtual void CreateChildControls() {} //DIT
		
                protected virtual ControlCollection CreateControlCollection() //DIT
                {
                        return new ControlCollection(this);
                }

                protected virtual void EnsureChildControls ()
                {
                        if (ChildControlsCreated == false && (stateMask & CREATING_CONTROLS) == 0) {
				stateMask |= CREATING_CONTROLS;
#if NET_2_0
				if (Adapter != null)
					Adapter.CreateChildControls ();
				else
#endif
					CreateChildControls();
                                ChildControlsCreated = true;
				stateMask &= ~CREATING_CONTROLS;
                        }
                }

#if NET_2_0
		protected void EnsureID ()
		{
			if (Page == null)
				return;
			ID = NamingContainer.GetDefaultName ();
		}

		protected bool HasEvents ()
		{
            		return _events != null;
		}
		
#endif
		

		protected bool IsLiteralContent()
		{
			if (HasControls () && _controls.Count == 1 && (_controls [0] is LiteralControl))
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
#if TARGET_J2EE
			if (this == Page && id != null && id == Page.PortletNamespace)
				return this;
#endif
			if (!HasControls ())
				return null;

			Control result = null;
			foreach (Control c in _controls) {
				if (String.Compare (id, c._userId, true, CultureInfo.InvariantCulture) == 0) {
					if (result != null && result != c) {
						throw new HttpException ("1 Found more than one control with ID '" + id + "'");
					}

					result = c;
					continue;
				}

				if ((c.stateMask & IS_NAMING_CONTAINER) == 0 && c.HasControls ()) {
					Control child = c.LookForControlByName (id);
					if (child != null) {
						if (result != null && result != child)
							throw new HttpException ("2 Found more than one control with ID '" + id + "'");

						result = child;
					}
				}
			}

			return result;
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
#if NET_2_0
			int separatorIdx = id.IndexOf (IdSeparator, pathOffset);
#else
			int separatorIdx = id.IndexOf (':', pathOffset);
#endif
			if (separatorIdx == -1)
				return LookForControlByName (id.Substring (pathOffset));
			
			string idfound = id.Substring (pathOffset, separatorIdx - pathOffset);
			namingContainer = LookForControlByName (idfound);
			if (namingContainer == null)
				return null;

			return namingContainer.FindControl (id, separatorIdx + 1);
                }

                protected virtual void LoadViewState(object savedState)
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
                protected string MapPathSecure(string virtualPath)
                {
			string combined = UrlUtils.Combine (TemplateSourceDirectory, virtualPath);
			return Context.Request.MapPath (combined);
                }

                protected virtual bool OnBubbleEvent(object source, EventArgs args) //DIT
                {
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("OnBubbleEvent {0} {1}", _userId, type_name));
			}
#endif
                        return false;
                }

                protected virtual void OnDataBinding (EventArgs e)
                {
			if ((event_mask & databinding_mask) != 0) {
                                EventHandler eh = (EventHandler)(_events [DataBindingEvent]);
                                if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Format ("OnDataBinding {0} {1}", _userId, type_name));
					}
#endif
					eh (this, e);
				}
                        }
                }

#if NET_2_0
		protected internal
#else		
                protected
#endif		
		virtual void OnInit (EventArgs e)
                {
			if ((event_mask & init_mask) != 0) {
                                EventHandler eh = (EventHandler)(_events [InitEvent]);
                                if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Format ("OnInit {0} {1}", _userId, type_name));
					}
#endif
					eh (this, e);
				}
                        }
                }

#if NET_2_0
		protected internal
#else
		protected
#endif		
		virtual void OnLoad (EventArgs e)
                {
			if ((event_mask & load_mask) != 0) {
                                EventHandler eh = (EventHandler)(_events [LoadEvent]);
                                if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Format ("OnLoad {0} {1}", _userId, type_name));
					}
#endif
					eh (this, e);
				}
                        }
                }

#if NET_2_0
		protected internal
#else
		protected
#endif
                virtual void OnPreRender (EventArgs e)
                {
			if ((event_mask & prerender_mask) != 0) {
                                EventHandler eh = (EventHandler)(_events [PreRenderEvent]);
                                if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Format ("OnPreRender {0} {1}", _userId, type_name));
					}
#endif
					eh (this, e);
				}
                        }
                }

#if NET_2_0
		protected internal
#else
		protected
#endif
                virtual void OnUnload(EventArgs e)
                {
			if ((event_mask & unload_mask) != 0) {
                                EventHandler eh = (EventHandler)(_events [UnloadEvent]);
                                if (eh != null) {
#if MONO_TRACE
					TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
					string type_name = null;
					if (trace != null) {
						type_name = GetType ().Name;
						trace.Write ("control", String.Format ("OnUnload {0} {1}", _userId, type_name));
					}
#endif
					eh (this, e);
				}
                        }
                }

#if NET_2_0
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
#endif		

                protected void RaiseBubbleEvent(object source, EventArgs args)
                {
			Control c = Parent;
			while (c != null) {
#if MONO_TRACE
				TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
				string type_name = null;
				if (trace != null) {
					type_name = GetType ().Name;
					trace.Write ("control", String.Format ("RaiseBubbleEvent {0} {1}", _userId, type_name));
				}
#endif
				if (c.OnBubbleEvent (source, args)) {
#if MONO_TRACE
					if (trace != null)
						trace.Write ("control", String.Format ("End RaiseBubbleEvent (false) {0} {1}", _userId, type_name));
#endif
					break;
				}
#if MONO_TRACE
				if (trace != null)
					trace.Write ("control", String.Format ("End RaiseBubbleEvent (true) {0} {1}", _userId, type_name));
#endif
				c = c.Parent;
			}
                }

#if NET_2_0
		protected internal
#else
		protected
#endif
                virtual void Render(HtmlTextWriter writer) //DIT
                {
			RenderChildren(writer);
                }

#if NET_2_0
		protected internal
#else
		protected
#endif
                virtual void RenderChildren (HtmlTextWriter writer) //DIT
                {
                        if (_renderMethodDelegate != null) {
                                _renderMethodDelegate (writer, this);
			} else if (_controls != null) {
				int len = _controls.Count;
				for (int i = 0; i < len; i++) {
					Control c = _controls [i];
#if NET_2_0
					if (c.Adapter != null)
						c.RenderControl (writer, c.Adapter);
					else
#endif
						c.RenderControl (writer);
				}
			}
                }

#if NET_2_0
		protected virtual ControlAdapter ResolveAdapter ()
		{
			throw new NotImplementedException ();
		}
#endif		

                protected virtual object SaveViewState ()
                {
			if ((stateMask & VISIBLE_CHANGED) != 0) {
				ViewState ["Visible"] = (stateMask & VISIBLE) != 0;
			} else if (_viewState == null) {
				return null;
			}

			return _viewState.SaveViewState ();
                }

                protected virtual void TrackViewState()
                {
			if (_viewState != null)
				_viewState.TrackViewState ();

                        stateMask |= TRACK_VIEWSTATE;
                }

                public virtual void Dispose ()
                {
			if ((event_mask & disposed_mask) != 0) {
                                EventHandler eh = (EventHandler)(_events [DisposedEvent]);
                                if (eh != null) eh (this, EventArgs.Empty);
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

		public virtual void DataBind() //DIT
		{
			#if NET_2_0
			DataBind (true);
			#else
			OnDataBinding (EventArgs.Empty);
			DataBindChildren();
			#endif
		}

		#if NET_2_0
		protected virtual
		#endif
		
		void DataBindChildren ()
		{
			if (!HasControls ())
				return;
			
			int len = _controls.Count;
			for (int i = 0; i < len; i++) {
				Control c = _controls [i];
				c.DataBind ();
			}
		}


		public virtual bool HasControls ()
		{
		    return (_controls != null && _controls.Count > 0);
		}

#if NET_2_0
		public virtual
#else
		public
#endif
		void RenderControl (HtmlTextWriter writer)
		{
			if ((stateMask & VISIBLE) != 0) {
				HttpContext ctx = Context;
				TraceContext trace = (ctx != null) ? ctx.Trace : null;
				int pos = 0;
				if ((trace != null) && trace.IsEnabled)
					pos = ctx.Response.GetOutputByteCount ();

				Render(writer);
				if ((trace != null) && trace.IsEnabled) {
					int size = ctx.Response.GetOutputByteCount () - pos;
					trace.SaveSize (this, size >= 0 ? size : 0);
				}
			}
		}

#if NET_2_0
		protected void RenderControl (HtmlTextWriter writer,
					      ControlAdapter adapter)
		{
			if ((stateMask & VISIBLE) != 0) {
				adapter.BeginRender (writer);
				adapter.Render (writer);
				adapter.EndRender (writer);
			}
		}
#endif		

		public string ResolveUrl (string relativeUrl)
		{
			if (relativeUrl == null)
				throw new ArgumentNullException ("relativeUrl");

			if (relativeUrl == "")
				return "";

			if (relativeUrl [0] == '#')
				return relativeUrl;
			
			string ts = TemplateSourceDirectory;
			if (ts == null || ts.Length == 0 ||
				Context == null || Context.Response == null ||
				!UrlUtils.IsRelativeUrl (relativeUrl))
				return relativeUrl;

			HttpResponse resp = Context.Response;
			return resp.ApplyAppPathModifier (UrlUtils.Combine (ts, relativeUrl));
		}


#if NET_2_0		
		public
#else
		internal
#endif
		string ResolveClientUrl (string relativeUrl)
		{
#if TARGET_J2EE
			// There are no relative paths when rendering a J2EE portlet
			if (Page != null && Page.PortletNamespace != null)
				return ResolveUrl (relativeUrl);
#endif
			if (relativeUrl == null)
				throw new ArgumentNullException ("relativeUrl");

			if (relativeUrl.Length == 0)
				return String.Empty;

			if (VirtualPathUtility.IsAbsolute (relativeUrl) || relativeUrl.IndexOf (':') >= 0)
				return relativeUrl;

			if (Context != null && Context.Request != null && TemplateSourceDirectory != null && TemplateSourceDirectory.Length > 0) {
				string basePath = Context.Request.FilePath;
				if (basePath.Length > 1 && basePath [basePath.Length - 1] != '/') {
					basePath = VirtualPathUtility.GetDirectory (basePath);
				}
				
				if(VirtualPathUtility.IsAppRelative(relativeUrl))
					return VirtualPathUtility.MakeRelative (basePath, relativeUrl);

				string templatePath = VirtualPathUtility.AppendTrailingSlash (TemplateSourceDirectory);
				
				if (basePath.Length == templatePath.Length && String.CompareOrdinal (basePath, templatePath) == 0)
					return relativeUrl;

				relativeUrl = VirtualPathUtility.Combine (templatePath, relativeUrl);
				return VirtualPathUtility.MakeRelative (basePath, relativeUrl);
			}
			return relativeUrl;
		}
		
		internal bool HasRenderMethodDelegate () {
			return _renderMethodDelegate != null;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
                public void SetRenderMethodDelegate(RenderMethod renderMethod) //DIT
                {
                        _renderMethodDelegate = renderMethod;
                }

                internal void LoadRecursive()
                {
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("LoadRecursive {0} {1}", _userId, type_name));
			}
#endif
#if NET_2_0
			if (Adapter != null)
				Adapter.OnLoad (EventArgs.Empty);
			else
#endif
				OnLoad (EventArgs.Empty);
                        if (HasControls ()) {
				int len = _controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = _controls[i];
					c.LoadRecursive ();
				}
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Format ("End LoadRecursive {0} {1}", _userId, type_name));
#endif
			stateMask |= LOADED;
                }

                internal void UnloadRecursive(Boolean dispose)
                {
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("UnloadRecursive {0} {1}", _userId, type_name));
			}
#endif
			if (HasControls ()) {
				int len = _controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = _controls[i];					
					c.UnloadRecursive (dispose);
				}
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Format ("End UnloadRecursive {0} {1}", _userId, type_name));
#endif
#if NET_2_0
			if (Adapter != null)
				Adapter.OnUnload (EventArgs.Empty);
			else
#endif
                        	OnUnload (EventArgs.Empty);
                        if (dispose)
				Dispose();
                }

                internal void PreRenderRecursiveInternal()
                {
			if ((stateMask & VISIBLE) != 0) {
				EnsureChildControls ();
#if MONO_TRACE
				TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
				string type_name = null;
				if (trace != null) {
					type_name = GetType ().Name;
					trace.Write ("control", String.Format ("PreRenderRecursive {0} {1}", _userId, type_name));
				}
#endif
#if NET_2_0
				if (Adapter != null)
					Adapter.OnPreRender (EventArgs.Empty);
				else
#endif
					OnPreRender (EventArgs.Empty);
				if (!HasControls ())
					return;
				
				int len = _controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = _controls[i];
					c.PreRenderRecursiveInternal ();
				}
#if MONO_TRACE
				if (trace != null)
					trace.Write ("control", String.Format ("End PreRenderRecursive {0} {1}", _userId, type_name));
#endif
			}
			stateMask |= PRERENDERED;
                }

                internal void InitRecursive(Control namingContainer)
                {
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("InitRecursive {0} {1}", _userId, type_name));
			}
#endif
                        if (HasControls ()) {
				if ((stateMask & IS_NAMING_CONTAINER) != 0)
					namingContainer = this;

				if (namingContainer != null && 
				    namingContainer._userId == null &&
				    namingContainer.AutoID)
					namingContainer._userId = namingContainer.GetDefaultName () + "b";

				int len = _controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = _controls[i];
					c._page = Page;
					c._namingContainer = namingContainer;
					if (namingContainer != null && c._userId == null && c.AutoID)
						c._userId = namingContainer.GetDefaultName () + "c";
					c.InitRecursive (namingContainer);	
				}
			}

			stateMask |= INITING;
#if NET_2_0
			ApplyTheme ();
			
			if (Adapter != null)
				Adapter.OnInit (EventArgs.Empty);
			else
#endif
				OnInit (EventArgs.Empty);
#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Format ("End InitRecursive {0} {1}", _userId, type_name));
#endif
			TrackViewState ();
			stateMask |= INITED;
			stateMask &= ~INITING;
                }

                internal object SaveViewStateRecursive ()
                {
			if (!EnableViewState)
				return null;

#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("SaveViewStateRecursive {0} {1}", _userId, type_name));
			}
#endif

			ArrayList controlList = null;
			ArrayList controlStates = null;

			int idx = -1;
			if (HasControls ())
			{
				int len = _controls.Count;
				for (int i=0;i<len;i++)
				{
					Control ctrl = _controls[i];
					object ctrlState = ctrl.SaveViewStateRecursive ();
					idx++;
					if (ctrlState == null)
						continue;

					if (controlList == null) 
					{
						controlList = new ArrayList ();
						controlStates = new ArrayList ();
					}

					controlList.Add (idx);
					controlStates.Add (ctrlState);
				}
			}

			object thisState = SaveViewState ();
			if (thisState == null && controlList == null && controlStates == null) {
#if MONO_TRACE
				if (trace != null) {
					trace.Write ("control", String.Format ("End SaveViewStateRecursive {0} {1} saved nothing", _userId, type_name));
					trace.SaveViewState (this, null);
				}
#endif
				return null;
			}

#if MONO_TRACE
			if (trace != null) {
				trace.Write ("control", String.Format ("End SaveViewStateRecursive {0} {1} saved a Triplet", _userId, type_name));
				trace.SaveViewState (this, thisState);
			}
#endif
			return new Triplet (thisState, controlList, controlStates);
                }
                
		internal void LoadViewStateRecursive (object savedState)
                {
			if (!EnableViewState || savedState == null)
				return;

#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("LoadViewStateRecursive {0} {1}", _userId, type_name));
			}
#endif
			Triplet savedInfo = (Triplet) savedState;
			LoadViewState (savedInfo.First);

			ArrayList controlList = savedInfo.Second as ArrayList;
			if (controlList == null)
				return;
			ArrayList controlStates = savedInfo.Third as ArrayList;
			int nControls = controlList.Count;
			for (int i = 0; i < nControls; i++) {
				int k = (int) controlList [i];
				if (k < Controls.Count && controlStates != null) {
					Control c = Controls [k];
					c.LoadViewStateRecursive (controlStates [i]);
				} else {
					if (pendingVS == null)
						pendingVS = new Hashtable ();

					pendingVS [k] = controlStates [i];
				}
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Format ("End LoadViewStateRecursive {0} {1}", _userId, type_name));
#endif
			stateMask |= VIEWSTATE_LOADED;
                }

#if NET_2_0
		internal ControlSkin controlSkin;

                internal void ApplyTheme ()
                {
#if MONO_TRACE
			TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
			string type_name = null;
			if (trace != null) {
				type_name = GetType ().Name;
				trace.Write ("control", String.Format ("ApplyThemeRecursive {0} {1}", _userId, type_name));
			}
#endif
			if (Page.PageTheme != null && EnableTheming) {
				ControlSkin controlSkin = Page.PageTheme.GetControlSkin (GetType (), SkinID);
				if (controlSkin != null)
					controlSkin.ApplySkin (this);
			}

#if MONO_TRACE
			if (trace != null)
				trace.Write ("control", String.Format ("End ApplyThemeRecursive {0} {1}", _userId, type_name));
#endif
                }
#endif
                
		internal bool AutoID
		{
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
		}


#if NET_2_0

		string skinId = string.Empty;
		bool _enableTheming = true;
		
		[Browsable (false)]
		[Themeable (false)]
		[DefaultValue (true)]
		public virtual bool EnableTheming
		{
			get
			{
				if ((stateMask & ENABLE_THEMING) != 0)
					return _enableTheming;

				if (_parent != null)
					return _parent.EnableTheming;

				return true;
			}
			set 
			{ 
				SetMask (ENABLE_THEMING, true);
				_enableTheming = value;
			}
		}
		
		[Browsable (false)]
		[DefaultValue ("")]
		[Filterable (false)]
		public virtual string SkinID
		{
			get { return skinId; }
			set { skinId = value; }
		}

		ControlBuilder IControlBuilderAccessor.ControlBuilder { 
			get {throw new NotImplementedException (); }
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
			get {
				return (expressionBindings != null && expressionBindings.Count > 0);
			}
		}

		public virtual void Focus()
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
				DataBindChildren();
			
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
#endif
		void IParserAccessor.AddParsedSubObject (object obj) {
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
        }
}
