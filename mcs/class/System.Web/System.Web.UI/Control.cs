//
// System.Web.UI.Control.cs
//
// Authors:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua Javier (gonzalo@ximian.com
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Bob Smith
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
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
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Web;
using System.Web.Util;

namespace System.Web.UI
{
	[DefaultProperty ("ID"), DesignerCategory ("Code"), ToolboxItemFilter ("System.Web.UI", ToolboxItemFilterType.Require)]
	[ToolboxItem ("System.Web.UI.Design.WebControlToolboxItem, " + Consts.AssemblySystem_Design)]
	[Designer ("System.Web.UI.Design.ControlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DesignerSerializer ("Microsoft.VSDesigner.WebForms.ControlCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner,
				"System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
        public class Control : IComponent, IDisposable, IParserAccessor, IDataBindingsAccessor
        {
		static readonly object DataBindingEvent = new object();
		static readonly object DisposedEvent = new object();
		static readonly object InitEvent = new object();
		static readonly object LoadEvent = new object();
		static readonly object PreRenderEvent = new object();
		static readonly object UnloadEvent = new object();
		static string[] defaultNameArray;

		string uniqueID;
		string _userId;
		ControlCollection _controls;
		IDictionary _childViewStates;
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
		/*************/
		
		static Control ()
		{
			defaultNameArray = new string [100];
			for (int i = 0 ; i < 100 ; i++)
				defaultNameArray [i] = "_ctrl" + i;
		}

                public Control()
                {
			stateMask = ENABLE_VIEWSTATE | VISIBLE | AUTOID | BINDING_CONTAINER | AUTO_EVENT_WIREUP;
                        if (this is INamingContainer)
				stateMask |= IS_NAMING_CONTAINER;
                }

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
		public Control BindingContainer {
			get {
				Control container = NamingContainer;
				if ((container.stateMask & BINDING_CONTAINER) == 0)
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
					client = client.Replace (':', '_');

				return client;
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

		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("An Identification of the control that is rendered.")]
                public virtual bool EnableViewState {
                        get { return ((stateMask & ENABLE_VIEWSTATE) != 0); }
			set { SetMask (ENABLE_VIEWSTATE, value); }
                }
		
		[MergableProperty (false), ParenthesizePropertyName (true)]
		[WebSysDescription ("The name of the control that is rendered.")]
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

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("A virtual directory containing the parent of the control.")]
                public virtual string TemplateSourceDirectory {
			get { return (_parent == null) ? String.Empty : _parent.TemplateSourceDirectory; }
                }

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The unique ID of the control.")]
                public virtual string UniqueID {
                        get {
				if (uniqueID != null)
					return uniqueID;

				if (_namingContainer == null) {
					return _userId;
				}

				if (_userId == null)
					_userId = _namingContainer.GetDefaultName ();

				string prefix = _namingContainer.UniqueID;
				if (_namingContainer == _page || prefix == null) {
					uniqueID = _userId;
					return uniqueID;
				}

				uniqueID = prefix + ":" + _userId;
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
				if (value == false && (stateMask & CHILD_CONTROLS_CREATED) != 0)
					Controls.Clear();

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
                protected EventHandlerList Events //DIT
                {
                        get
                        {
                                if (_events == null)
                                {
                                	_events = new EventHandlerList();
                                }
                                return _events;
                        }
                }
                protected bool HasChildViewState //DIT
                {
                        get
                        {
                                if (_childViewStates == null) return false;
                                return true;
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
				defaultName = "_ctrl" + defaultNumberID++;
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

			foreach (Control c in Controls)
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
					control._userId =  nc.GetDefaultName () + "a";
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
			WebTrace.PushContext ("Control.AddParsedSubobject ()");
                        Control c = obj as Control;
			WebTrace.WriteLine ("Start: {0} -> {1}", obj, (c != null) ? c.ID : String.Empty);
                        if (c != null) Controls.Add(c);
			WebTrace.WriteLine ("End");
			WebTrace.PopContext ();
                }

                protected void BuildProfileTree(string parentId, bool calcViewState)
                {
                        //TODO
                }

                protected void ClearChildViewState ()
                {
			pendingVS = null;
                }

                protected virtual void CreateChildControls() {} //DIT
                protected virtual ControlCollection CreateControlCollection() //DIT
                {
                        return new ControlCollection(this);
                }

                protected virtual void EnsureChildControls ()
                {
                        if (ChildControlsCreated == false && (stateMask & CREATING_CONTROLS) == 0) {
				stateMask |= CREATING_CONTROLS;
                                CreateChildControls();
                                ChildControlsCreated = true;
				stateMask &= ~CREATING_CONTROLS;
                        }
                }

		protected bool IsLiteralContent()
		{
			if (HasControls () && Controls.Count == 1 && (Controls [0] is LiteralControl))
				return true;

			return false;
		}

                public virtual Control FindControl (string id)
                {
			return FindControl (id, 0);
                }

		Control LookForControlByName (string id)
		{
			if (!HasControls ())
				return null;

			foreach (Control c in Controls) {
				if (String.Compare (id, c._userId, true) == 0)
					return c;

				if ((c.stateMask & IS_NAMING_CONTAINER) == 0 && c.HasControls ()) {
					Control child = c.LookForControlByName (id);
					if (child != null)
						return child;
				}
			}

			return null;
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

			int colon = id.IndexOf (':', pathOffset);
			if (colon == -1)
				return LookForControlByName (id.Substring (pathOffset));
			
			string idfound = id.Substring (pathOffset, colon - pathOffset);
			namingContainer = LookForControlByName (idfound);
			if (namingContainer == null)
				return null;

			return namingContainer.FindControl (id, colon + 1);
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

		[MonoTODO("Secure?")]
                protected string MapPathSecure(string virtualPath)
                {
			string combined = UrlUtils.Combine (TemplateSourceDirectory, virtualPath);
			return Context.Request.MapPath (combined);
                }

                protected virtual bool OnBubbleEvent(object source, EventArgs args) //DIT
                {
                        return false;
                }
                protected virtual void OnDataBinding(EventArgs e) //DIT
                {
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[DataBindingEvent]);
                                if (eh != null) eh(this, e);
                        }
                }
                protected virtual void OnInit(EventArgs e) //DIT
                {
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[InitEvent]);
                                if (eh != null) eh(this, e);
                        }
                }
                protected virtual void OnLoad(EventArgs e) //DIT
                {
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[LoadEvent]);
                                if (eh != null) eh(this, e);
                        }
                }
                protected virtual void OnPreRender(EventArgs e) //DIT
                {
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[PreRenderEvent]);
                                if (eh != null) eh(this, e);
                        }
                }
                protected virtual void OnUnload(EventArgs e) //DIT
                {
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[UnloadEvent]);
                                if (eh != null) eh(this, e);
                        }
                }
                
                protected void RaiseBubbleEvent(object source, EventArgs args)
                {
			Control c = Parent;
			while (c != null) {
				if (c.OnBubbleEvent (source, args))
					break;
				c = c.Parent;
			}
                }

                protected virtual void Render(HtmlTextWriter writer) //DIT
                {
                        RenderChildren(writer);
                }

                protected virtual void RenderChildren (HtmlTextWriter writer) //DIT
                {
                        if (_renderMethodDelegate != null) {
                                _renderMethodDelegate (writer, this);
			} else if (HasControls ()) {
				int len = Controls.Count;
				for (int i = 0; i < len; i++) {
					Control c = Controls [i];
					c.RenderControl (writer);
				}
			}
                }

                protected virtual object SaveViewState ()
                {
			if ((stateMask & VISIBLE_CHANGED) != 0) {
				ViewState ["Visible"] = Visible;
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
                
                public virtual void Dispose()
                {
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler) _events [DisposedEvent];
                                if (eh != null)
					eh(this, EventArgs.Empty);
                        }
                }

		[WebCategory ("FIXME")]
		[WebSysDescription ("Raised when the contols databound properties are evaluated.")]
                public event EventHandler DataBinding //DIT
                {
                        add
                        {
                                Events.AddHandler(DataBindingEvent, value);
                        }
                        remove
                        {
                                Events.RemoveHandler(DataBindingEvent, value);
                        }
                }

		[WebSysDescription ("Raised when the contol is disposed.")]
                public event EventHandler Disposed //DIT
                {
                        add
                        {
                                Events.AddHandler(DisposedEvent, value);
                        }
                        remove
                        {
                                Events.RemoveHandler(DisposedEvent, value);
                        }
                }

		[WebSysDescription ("Raised when the page containing the control is initialized.")]
                public event EventHandler Init //DIT
                {
                        add
                        {
                                Events.AddHandler(InitEvent, value);
                        }
                        remove
                        {
                                Events.RemoveHandler(InitEvent, value);
                        }
                }

		[WebSysDescription ("Raised after the page containing the control has been loaded.")]
                public event EventHandler Load //DIT
                {
                        add
                        {
                                Events.AddHandler(LoadEvent, value);
                        }
                        remove
                        {
                                Events.RemoveHandler(LoadEvent, value);
                        }
                }

		[WebSysDescription ("Raised before the page containing the control is rendered.")]
                public event EventHandler PreRender //DIT
                {
                        add
                        {
                                Events.AddHandler(PreRenderEvent, value);
                        }
                        remove
                        {
                                Events.RemoveHandler(PreRenderEvent, value);
                        }
                }

		[WebSysDescription ("Raised when the page containing the control is unloaded.")]
                public event EventHandler Unload //DIT
                {
                        add
                        {
                                Events.AddHandler(UnloadEvent, value);
                        }
                        remove
                        {
                                Events.RemoveHandler(UnloadEvent, value);
                        }
                }

                public virtual void DataBind() //DIT
                {
			#if NET_2_0
			bool foundDataItem = false;
			
			if ((stateMask & IS_NAMING_CONTAINER) != 0 && Page != null) {
				object o = DataBinder.GetDataItem (this, out foundDataItem);
				if (foundDataItem)
					Page.PushDataItemContext (o);
			}
			
			try {
			#endif
				
				OnDataBinding (EventArgs.Empty);
				DataBindChildren();
			
			#if NET_2_0
			} finally {
				if (foundDataItem)
					Page.PopDataItemContext ();
			}
			#endif
                }
		
		#if NET_2_0
		protected virtual
		#endif
		
		void DataBindChildren ()
		{
			if (!HasControls ())
				return;
			
			int len = Controls.Count;
			for (int i = 0; i < len; i++) {
				Control c = Controls [i];
				c.DataBind ();
			}
		}


                public virtual bool HasControls ()
                {
                        return (_controls != null && _controls.Count > 0);
                }

                public void RenderControl(HtmlTextWriter writer)
                {
                        if ((stateMask & VISIBLE) != 0)
                                Render(writer);
                }

                public string ResolveUrl(string relativeUrl)
                {
			if (relativeUrl == null)
				throw new ArgumentNullException ("relativeUrl");

			if (relativeUrl == "")
				return "";

			if (relativeUrl [0] == '#')
				return relativeUrl;
			
			string ts = TemplateSourceDirectory;
			if (ts == "" || !UrlUtils.IsRelativeUrl (relativeUrl))
				return relativeUrl;

			if (relativeUrl.IndexOf ('/') == -1 && relativeUrl [0] != '.' && relativeUrl != "..")
				return relativeUrl;

			HttpResponse resp = Context.Response;
			return resp.ApplyAppPathModifier (UrlUtils.Combine (ts, relativeUrl));
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
                public void SetRenderMethodDelegate(RenderMethod renderMethod) //DIT
                {
                        _renderMethodDelegate = renderMethod;
                }

                internal void LoadRecursive()
                {
                        OnLoad (EventArgs.Empty);
                        if (HasControls ()) {
				int len = Controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = Controls[i];
					c.LoadRecursive ();
				}
			}
			stateMask |= LOADED;
                }

                internal void UnloadRecursive(Boolean dispose)
                {
			if (HasControls ()) {
				int len = Controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = Controls[i];					
					c.UnloadRecursive (dispose);
				}
			}

                        OnUnload (EventArgs.Empty);
                        if (dispose)
				Dispose();
                }

                internal void PreRenderRecursiveInternal()
                {
			if ((stateMask & VISIBLE) != 0) {
				EnsureChildControls ();
				OnPreRender (EventArgs.Empty);
				if (!HasControls ())
					return;
				
				int len = Controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = Controls[i];
					c.PreRenderRecursiveInternal ();
				}
			}
			stateMask |= PRERENDERED;
                }

                internal void InitRecursive(Control namingContainer)
                {
                        if (HasControls ()) {
				if ((stateMask & IS_NAMING_CONTAINER) != 0)
					namingContainer = this;

				if (namingContainer != null && 
				    namingContainer._userId == null &&
				    namingContainer.AutoID)
					namingContainer._userId = namingContainer.GetDefaultName () + "b";

				int len = Controls.Count;
				for (int i=0;i<len;i++)
				{
					Control c = Controls[i];
					c._page = Page;
					c._namingContainer = namingContainer;
					if (namingContainer != null && c._userId == null && c.AutoID)
						c._userId = namingContainer.GetDefaultName () + "c";
					c.InitRecursive (namingContainer);	
				}
			}

			stateMask |= INITING;
                        OnInit (EventArgs.Empty);
			TrackViewState ();
			stateMask |= INITED;
			stateMask &= ~INITING;
                }

                internal object SaveViewStateRecursive ()
                {
			if (!EnableViewState)
				return null;

			ArrayList controlList = null;
			ArrayList controlStates = null;

			int idx = -1;
			if (HasControls ())
			{
				int len = Controls.Count;
				for (int i=0;i<len;i++)
				{
					Control ctrl = Controls[i];
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
			if (thisState == null && controlList == null && controlStates == null)
				return null;

			return new Triplet (thisState, controlList, controlStates);
                }
                
		internal void LoadViewStateRecursive (object savedState)
                {
			if (!EnableViewState || savedState == null)
				return;

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

			stateMask |= VIEWSTATE_LOADED;
                }
                
                void IParserAccessor.AddParsedSubObject(object obj)
                {
                	AddParsedSubObject(obj);
                }
                
                DataBindingCollection IDataBindingsAccessor.DataBindings
                {
                	get
                	{
                		if(dataBindings == null)
                			dataBindings = new DataBindingCollection();
                		return dataBindings;
                	}
                }
                
                bool IDataBindingsAccessor.HasDataBindings
                {
                	get
                	{
                		return (dataBindings!=null && dataBindings.Count>0);
                	}
                }
 
		internal bool AutoID
		{
			get { return (stateMask & AUTOID) != 0; }
			set {
				if (value == false && (stateMask & IS_NAMING_CONTAINER) != 0)
					return;

				SetMask (AUTOID, value);
			}
		}

                internal void PreventAutoID()
                {
			AutoID = false;
                }
                
		protected internal virtual void RemovedControl (Control control)
		{
			control.UnloadRecursive (false);
			control._parent = null;
			control._page = null;
			control._namingContainer = null;
		}

		#if NET_2_0
		protected string GetWebResourceUrl (string resourceName)
		{
			return Page.GetWebResourceUrl (GetType(), resourceName); 
		} 

		#endif
        }
}
