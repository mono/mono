//
// System.Web.UI.Control.cs
//
// Authors:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Bob Smith
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

/*
 * Maintainer: bob@thestuff.net, gvaish@iitk.ac.in
 * (C) Bob Smith, Gaurav Vaish
 */

//notes: view state only tracks changes after OnInit method is executed for the page request. You can read from it at any time, but cant write to it during rendering.
//even more notes: view state info in trackviewstate method description. read later.
//Ok, enough notes: what the heck is different between enable view state, and track view state.
//Well, maybe not. How does the ViewState know when to track changes? Does it look at the property
//on the owning control, or does it have a method/property of its own that gets called?
// I think this last question is solved in the Interface for it. Look into this.

//cycle:
//init is called when control is first created.
//load view state ic called right after init to populate the view state.
//loadpostdata is called if ipostbackdatahandler is implemented.
//load is called when control is loaded into a page
//raisepostdatachangedevent if ipostbackdatahandler is implemented.
//raisepostbackevent if ipostbackeventhandler is implemented.
//prerender is called when the server is about to render its page object
//SaveViewState is called.
//Unload then dispose it apears. :)

//Naming Container MUST have some methods. What are they? No clue. Help? (updated: the doc says that it's just a marker interface)

//read this later. http://gotdotnet.com/quickstart/aspplus/
//This to: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguidnf/html/cpconattributesdesign-timesupport.asp
//http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguidnf/html/cpcontracefunctionality.asp

// Isnt life grand? :)
// See the undocumented methods? Gota love um. ;)
// ASP.test4_aspx.Page_Load(Object Sender, EventArgs e) in \\genfs2\www24\bobsmith11\test4.aspx:6
// System.Web.UI.Control.OnLoad(EventArgs e) +67
// System.Web.UI.Control.LoadRecursive() +73
// System.Web.UI.Page.ProcessRequestMain() +394

// ASP.test4_aspx.Page_Unload(Object Sender, EventArgs e) in \\genfs2\www24\bobsmith11\test4.aspx:6
// System.EventHandler.Invoke(Object sender, EventArgs e) +0
// System.Web.UI.Control.OnUnload(EventArgs e) +67
// System.Web.UI.Control.UnloadRecursive(Boolean dispose) +78
// System.Web.UI.Page.ProcessRequest() +194
// System.Web.UI.Page.ProcessRequest(HttpContext context) +18
// System.Web.CallHandlerExecutionStep.Execute() +179
// System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously) +87


// ASP.test4_aspx.Page_Unload(Object Sender, EventArgs e) in \\genfs2\www24\bobsmith11\test4.aspx:6
// System.Web.UI.Control.OnUnload(EventArgs e) +67
// System.Web.UI.Control.UnloadRecursive(Boolean dispose) +78
// System.Web.UI.Page.ProcessRequest()

// ASP.test4_aspx.Page_Kill(Object Sender, EventArgs e) in \\genfs2\www24\bobsmith11\test4.aspx:6
// System.Web.UI.Control.OnPreRender(EventArgs e) +67
// System.Web.UI.Control.PreRenderRecursiveInternal() +61
// System.Web.UI.Page.ProcessRequestMain() +753

// ASP.test4_aspx.OnInit(EventArgs e) in \\genfs2\www24\bobsmith11\test4.aspx:6
// System.Web.UI.Control.InitRecursive(Control namingContainer) +202
// System.Web.UI.Page.ProcessRequestMain() +120

// ASP.test4_aspx.SaveViewState() in \\genfs2\www24\bobsmith11\test4.aspx:12
// System.Web.UI.Control.SaveViewStateRecursive() +51
// System.Web.UI.Page.SavePageViewState() +174
// System.Web.UI.Page.ProcessRequestMain() +861

// ASP.test_aspx.LoadViewState(Object t) +28
// System.Web.UI.Control.LoadViewStateRecursive(Object savedState) +125
// System.Web.UI.Page.LoadPageViewState() +182
// System.Web.UI.Page.ProcessRequestMain() +256

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
	[DesignerSerializer ("Microsoft.VSDesigner.WebForms.ControlCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
        public class Control : IComponent, IDisposable, IParserAccessor, IDataBindingsAccessor
        {
                private static readonly object DataBindingEvent = new object();
                private static readonly object DisposedEvent = new object();
                private static readonly object InitEvent = new object();
                private static readonly object LoadEvent = new object();
                private static readonly object PreRenderEvent = new object();
                private static readonly object UnloadEvent = new object();
		private string uniqueID;
                private string _userId = null;
                private string _cachedClientId = null;
                private ControlCollection _controls = null;
                private bool _enableViewState = true;
                private IDictionary _childViewStates = null; //TODO: Not sure datatype. Placeholder guess.
                private bool _isNamingContainer = false;
                private Control _namingContainer = null;
                private Page _page = null;
                private Control _parent = null;
                private ISite _site = null;
                private bool _visible = true;
                private HttpContext _context = null;
                private bool _childControlsCreated = false;
                private StateBag _viewState = null;
                private bool _trackViewState = false;
                private EventHandlerList _events = new EventHandlerList();
                private RenderMethod _renderMethodDelegate = null;
		private bool autoID = true;
		private bool creatingControls = false;
		private bool bindingContainer = true;
		private bool autoEventWireup = true;

		bool inited = false;
		bool loaded = false;
		bool prerendered = false;
		int defaultNumberID = 0;
        	
		private DataBindingCollection dataBindings = null;

                public Control()
                {
                        if (this is INamingContainer) _isNamingContainer = true;
                }

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
		public Control BindingContainer
		{
			get {
				Control container = NamingContainer;
				if (!container.bindingContainer)
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

		[DefaultValue (true), WebCategory ("FIXME")]
		[WebSysDescription ("An Identification of the control that is rendered.")]
                public virtual bool EnableViewState //DIT
                {
                        get
                        {
                                return _enableViewState;
                        }
                        set
                        {
                                _enableViewState = value;
                        }
                }
		
		[MergableProperty (false), ParenthesizePropertyName (true)]
		[WebSysDescription ("The name of the control that is rendered.")]
                public virtual string ID {
                        get {
                                return _userId;
                        }
			
                        set {
				if (value == "")
					value = null;

                                _userId = value;
				NullifyUniqueID ();
                        }
                }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("The container that this control is part of. The control's name has to be unique within the container.")]
                public virtual Control NamingContainer //DIT
                {
                        get
                        {
                                if (_namingContainer == null && _parent != null)
                                {
                                        if (_parent._isNamingContainer == false)
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

		[DefaultValue (true), Bindable (true), WebCategory ("FIXME")]
		[WebSysDescription ("Visiblity state of the control.")]
                public virtual bool Visible
                {
                        get
                        {
				if (_visible == false)
					return false;

				if (_parent != null)
					return _parent.Visible;

                                return true;
                        }
                        set
                        {
                                _visible = value;
                        }
                }
                protected bool ChildControlsCreated //DIT
                {
                        get
                        {
                                return _childControlsCreated;
                        }
                        set
                        {
                                if (value == false && _childControlsCreated == true)
                                        _controls.Clear();
                                _childControlsCreated = value;
                        }
                }
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
                protected bool IsTrackingViewState //DIT
                {
                        get
                        {
                                return _trackViewState;
                        }
                }
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

                protected virtual bool ViewStateIgnoresCase
                {
                        get {
                                return false;
                        }
                }

		internal bool AutoEventWireup {
			get { return autoEventWireup; }
			set { autoEventWireup = value; }
		}

		internal void SetBindingContainer (bool isBC)
		{
			bindingContainer = isBC;
		}
		
		internal void ResetChildNames ()
		{
			defaultNumberID = 0;
		}
		
		string GetDefaultName ()
		{
			return "_ctrl" + defaultNumberID++;
		}
		
		void NullifyUniqueID ()
		{
			uniqueID = null;
			if (_controls == null)
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
			Control nc = _isNamingContainer ? this : NamingContainer;

			if (nc != null) {
				control._namingContainer = nc;
				if (control.AutoID == true && control._userId == null)
					control._userId =  nc.GetDefaultName () + "a";
			}

			if (inited)
				control.InitRecursive (nc);

			if (loaded)
				control.LoadRecursive ();

			if (prerendered)
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
                protected void ClearChildViewState()
                {
                        //TODO
                        //Not quite sure about this. an example clears children then calls this, so I think
                        //view state is local to the current object, not children.
                }
                protected virtual void CreateChildControls() {} //DIT
                protected virtual ControlCollection CreateControlCollection() //DIT
                {
                        return new ControlCollection(this);
                }

                protected virtual void EnsureChildControls () //DIT
                {
                        if (ChildControlsCreated == false && !creatingControls) {
				creatingControls = true;
                                CreateChildControls();
                                ChildControlsCreated = true;
				creatingControls = false;
                        }
                }

		protected bool IsLiteralContent()
		{
			if (_controls != null) 
				if (_controls.Count == 1)
					if (_controls[0] is LiteralControl)
						return true;
			return false;
		}

                public virtual Control FindControl (string id)
                {
			return FindControl (id, 0);
                }

		Control LookForControlByName (string id)
		{
			if (!HasChildren)
				return null;

			foreach (Control c in _controls) {
				if (String.Compare (id, c._userId, true) == 0)
					return c;

				if (!c._isNamingContainer && c.HasChildren) {
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
			if (_controls == null)
				return null;

			Control namingContainer = null;
			if (!_isNamingContainer) {
				namingContainer = NamingContainer;
				if (namingContainer == null)
					return null;

				return namingContainer.FindControl (id, pathOffset);
			}

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
			if (savedState != null)
				ViewState.LoadViewState (savedState);
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

                protected virtual void RenderChildren(HtmlTextWriter writer) //DIT
                {
                        if (_renderMethodDelegate != null)
                                _renderMethodDelegate(writer, this);
                        else if (_controls != null)
                                foreach (Control c in _controls)
                                        c.RenderControl(writer);
                }
		
                protected virtual object SaveViewState ()
                {
			if (_viewState == null)
				return null;

			return _viewState.SaveViewState ();
                }

                protected virtual void TrackViewState()
                {
			if (_viewState != null)
				_viewState.TrackViewState ();
                        _trackViewState = true;
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

		internal bool HasChildren
		{
			get { return (_controls != null && _controls.Count > 0); }
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
                        OnDataBinding(EventArgs.Empty);
                        if (_controls != null)
                                foreach (Control c in _controls)
                                        c.DataBind();
                }

                public virtual bool HasControls() //DIT
                {
                        if (_controls != null && _controls.Count >0) return true;
                        return false;
                }

                public void RenderControl(HtmlTextWriter writer)
                {
                        if (_visible)
                        {
                                //TODO: Something about tracing here.
                                Render(writer);
                        }
                }
                
                public string ResolveUrl(string relativeUrl)
                {
			if (relativeUrl == null)
				throw new ArgumentNullException ("relativeUrl");

			if (relativeUrl == "")
				return "";

			string ts = TemplateSourceDirectory;
			if (UrlUtils.IsRelativeUrl (relativeUrl) == false || ts == "")
				return relativeUrl;
			
			HttpResponse resp = Context.Response;
			return resp.ApplyAppPathModifier (UrlUtils.Combine (ts, relativeUrl));
                }

		[EditorBrowsable (EditorBrowsableState.Advanced)]
                public void SetRenderMethodDelegate(RenderMethod renderMethod) //DIT
                {
			WebTrace.PushContext ("Control.AddParsedSubobject ()");
			WebTrace.WriteLine ("Start");
                        _renderMethodDelegate = renderMethod;
			WebTrace.WriteLine ("End");
			WebTrace.PopContext ();
                }

                internal void LoadRecursive()
                {
                        OnLoad (EventArgs.Empty);
                        if (_controls != null) {
				foreach (Control c in _controls)
					c.LoadRecursive ();
			}
			loaded = true;
                }

                internal void UnloadRecursive(Boolean dispose)
                {
                        if (_controls != null) {
				foreach (Control c in _controls)
					c.UnloadRecursive (dispose);
			}

                        OnUnload (EventArgs.Empty);
                        if (dispose)
				Dispose();
                }

                internal void PreRenderRecursiveInternal()
                {
			if (_visible) {
				EnsureChildControls ();
				OnPreRender (EventArgs.Empty);
				if (_controls == null)
					return;

				foreach (Control c in _controls)
					c.PreRenderRecursiveInternal ();
			}
			prerendered = true;
                }

                internal void InitRecursive(Control namingContainer)
                {
                        if (_controls != null) {
				if (_isNamingContainer)
					namingContainer = this;

				if (namingContainer != null && 
				    namingContainer._userId == null &&
				    namingContainer.autoID)
					namingContainer._userId = namingContainer.GetDefaultName () + "b";

				foreach (Control c in _controls) {
					c._page = Page;
					c._namingContainer = namingContainer;
					if (namingContainer != null && c._userId == null && c.autoID)
						c._userId = namingContainer.GetDefaultName () + "c";

					c.InitRecursive (namingContainer);
				}
			}

                        OnInit (EventArgs.Empty);
			TrackViewState ();
			inited = true;
                }
                
                internal object SaveViewStateRecursive ()
                {
			if (!EnableViewState)
				return null;

			ArrayList controlList = null;
			ArrayList controlStates = null;

			foreach (Control ctrl in Controls) {
				object ctrlState = ctrl.SaveViewStateRecursive ();
				if (ctrlState == null || ctrl.ID == null)
					continue;

				if (controlList == null) {
					controlList = new ArrayList ();
					controlStates = new ArrayList ();
				}

				controlList.Add (ctrl.ID);
				controlStates.Add (ctrlState);
			}
			
			object thisState = SaveViewState ();
			if (thisState == null && controlList == null && controlStates == null)
				return null;

			return new Triplet (thisState, controlList, controlStates);
                }
                
		internal void LoadViewStateRecursive (object savedState)
                {
			if (!EnableViewState || !Visible || savedState == null)
				return;

			Triplet savedInfo = (Triplet) savedState;
			LoadViewState (savedInfo.First);

			ArrayList controlList = savedInfo.Second as ArrayList;
			if (controlList == null)
				return;
			ArrayList controlStates = savedInfo.Third as ArrayList;
			int nControls = controlList.Count;
			for (int i = 0; i < nControls; i++) {
				Control c = FindControl ((string) controlList [i]);
				if (c != null && controlStates != null)
					c.LoadViewStateRecursive (controlStates [i]);
			}
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
			get { return autoID; }
			set { 
				if (value == false && _isNamingContainer)
					return;

				autoID = value;
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

                //TODO: I think there are some needed Interface implementations to do here.
        }
}
