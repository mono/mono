//
// System.Web.UI.Control.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
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

//Naming Container MUST have some methods. What are they? No clue. Help?

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
using System.Web;
using System.ComponentModel;

namespace System.Web.UI
{
        public class Control : IComponent, IDisposable, IParserAccessor, IDataBindingsAccessor
        {
                private static readonly object DataBindingEvent = new object();
                private static readonly object DisposedEvent = new object();
                private static readonly object InitEvent = new object();
                private static readonly object LoadEvent = new object();
                private static readonly object PreRenderEvent = new object();
                private static readonly object UnloadEvent = new object();
                private string _userId = null;
                private string _cachedUserId = null;
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
        	
        	    private DataBindingCollection dataBindings = null;

                public Control()
                {
                        if (this is INamingContainer) _isNamingContainer = true;
                }
                public virtual string ClientID //DIT
                {
                        get
                        {
                                if (_cachedUserId != null && _cachedClientId != null)
                                        return _cachedClientId;
                                _cachedUserId = UniqueID.Replace(':', '_');
                                return _cachedUserId;
                        }
                }
                public virtual ControlCollection Controls //DIT
                {
                        get
                        {
                                if (_controls == null) _controls = CreateControlCollection();
                                return _controls;
                        }
                }
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
                public virtual string ID
                {
                        get //DIT
                        {
                                return _userId;
                        }
                        set
                        {
                                if (value == null || value == "") return;
                                _userId = value;
                                _cachedUserId = null;
                                //TODO: Some Naming Container stuff here I think.
                        }
                }
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
                public virtual Control Parent //DIT
                {
                        get
                        {
                                return _parent;
                        }
                }
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
                public virtual string TemplateSourceDirectory
                {
                        get
                        {
                                return Context.Request.ApplicationPath; //TODO: Dont think this is right.
                        }
                }

				[MonoTODO]
                public virtual string UniqueID
                {
                        get
                        {
                                //TODO: Some Naming container methods here. What are they? Why arnt they declared?
                                //Note: Nuked the old stuff here. Was total crap. :)
                                return ID;
                        }
                }
                public virtual bool Visible
                { //TODO: Are children visible when parents are not?
                        get
                        {
                                return _visible;
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
                        	{
	                        	_viewState = new StateBag(ViewStateIgnoresCase);
    	                    	if(IsTrackingViewState)
        	                		_viewState.TrackViewState();
                        	}
                        	return _viewState;
                        }
                }
                protected virtual bool ViewStateIgnoresCase //DIT
                {
                        get
                        {
                                return true;
                        }
                }
                protected virtual void AddParsedSubObject(object obj) //DIT
                {
                        Control c = (Control)obj;
                        if (c != null) Controls.Add(c);
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
                protected virtual void EnsureChildControls() //DIT
                {
                        if (_childControlsCreated == false)
                        {
                                CreateChildControls();
                                ChildControlsCreated = true;
                        }
                }
                protected virtual Control FindControl(string id, int pathOffset)
                {
                        //TODO: I think there is Naming Container stuff here. Redo.
                        int i;
                        for (i = pathOffset; i < _controls.Count; i++)
                                if (_controls[i].ID == id) return _controls[i];
                        return null;
                }
                protected virtual void LoadViewState(object savedState)
                {
                        //TODO: What should I do by default?
                }
                
				[MonoTODO]
                protected string MapPathSecure(string virtualPath)
                {
                        //TODO: Need to read up on security+web.
			//Return the same path. So AdRotator can read its config file.
			return virtualPath;
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
                
				[MonoTODO]
                protected void RaiseBubbleEvent(object source, EventArgs args)
                {
                        throw new NotImplementedException();
                        //return false;
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
                protected virtual object SaveViewState()
                {
                        return ViewState;
                }
                protected virtual void TrackViewState()
                {
                        _trackViewState = true;
                }
                
				[MonoTODO]
                public virtual void Dispose()
                {
                        //TODO: nuke stuff.
                        throw new NotImplementedException();
                	/*
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[DisposedEvent]);
                                if (eh != null) eh(this, e);
                        }
                    */
                }

		public bool HasChildren
		{
			get { return (_controls != null && _controls.Count > 0); }
		}

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
                public virtual Control FindControl(string id) //DIT
                {
                        return FindControl(id, 0);
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
				// By now, PreRender is fired here.
				OnPreRender (EventArgs.Empty); //FIXME
                                //TODO: Something about tracing here.
                                Render(writer);
                        }
                }
                
                [MonoTODO]
                public string ResolveUrl(string relativeUrl)
                {
                	return relativeUrl;
                }
                public void SetRenderMethodDelegate(RenderMethod renderMethod) //DIT
                {
                        _renderMethodDelegate = renderMethod;
                }
                protected void LoadRecursive()
                {
                        OnLoad(EventArgs.Empty);
                        if (_controls != null) foreach (Control c in _controls) c.LoadRecursive();
                }
                protected void UnloadRecursive(Boolean dispose)
                {
                        OnUnload(EventArgs.Empty);
                        if (_controls != null) foreach (Control c in _controls) c.UnloadRecursive(dispose);
                        if (dispose) Dispose();
                }
                protected void PreRenderRecursiveInternal()
                {
                        OnPreRender(EventArgs.Empty);
                        if (_controls != null) foreach (Control c in _controls) c.PreRenderRecursiveInternal();
                }
                protected void InitRecursive(Control namingContainer)
                {
                        if (_controls != null) foreach (Control c in _controls) c.InitRecursive(namingContainer);
                        OnInit(EventArgs.Empty);
                }
                
                [MonoTODO]
                protected object SaveViewStateRecursive()
                {
                        throw new NotImplementedException();
                }
                
                [MonoTODO]
                protected void LoadViewStateRecursive(Object savedState)
                {
                        throw new NotImplementedException();
                }
                
                void IParserAccessor.AddParsedSubObject(object obj)
                {
                	this.AddParsedSubObject(obj);
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
			set { autoID = value; }
		}

                internal void PreventAutoID()
                {
			AutoID = false;
                }
                
                //TODO: I think there are some needed Interface implementations to do here.
                //TODO: Find api for INamingContainer.
        }
}
