//
// System.Web.UI.Control.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

//notes: view state only tracks changes after OnInit method is executed for the page request. You can read from it at any time, but cant write to it during rendering.
//more notes: look at the private on* methods for initialization order. they will help.
//even more notes: view state info in trackviewstate method description. read later.
//Ok, enough notes: what the heck is different between enable view state, and track view state.

//cycle:
//init is called when control is first created.
//load view state ic called right after init to populate the view state.
//loadpostdata is called if ipostbackdatahandler is implemented.
//load is called when control is loaded into a page
//raisepostdatachangedevent if ipostbackdatahandler is implemented.
//raisepostbackevent if ipostbackeventhandler is implemented.
//prerender is called when the server is about to render its page object
//SaveViewState is called.
//Dispose disposed/unload not sure but is last.

//read this later. http://gotdotnet.com/quickstart/aspplus/

using System;
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
                private string _clientId; //default to "ctrl#" where # is a static count of ctrls per page.
                private string _userId = null;
                private ControlCollection _controls = null;
                private bool _enableViewState = true;
                private bool _isNamingContainer = false;
                private Control _namingContainer = null;
                private Page _page = null;
                private Control _parent; //TODO: set default.
                private ISite _site; //TODO: what default?
                private bool _visible; //TODO: what default?
                private HttpContext _context = null;
                private bool _childControlsCreated = false;
                private StateBag _viewState = null;
                private bool _trackViewState = false; //TODO: I think this is right. Verify. Also modify other methods to use this.
                private EventHandlerList _events = new EventHandlerList();
                public Control() {}
                public virtual string ClientID
                {
                        get
                        {
                                return _clientId;
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
                        get
                        {
                                if (_userId == null)
                                        return _clientId;
                                else
                                        return _userId;
                        }
                        set
                        {
                                _userId = value;
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
                                return Context.Request.ApplicationPath;
                        }
                }
                public virtual string UniqueID
                {
                        get
                        {
                                if (_namingContainer == null)
                                        if (_userId == null)
                                                return _clientId;
                                        else
                                                return _clientId + ":" + _userId;
                                else if (_userId == null)
                                        return _namingContainer.UniqueID + ":" + _clientId;
                                return _namingContainer.UniqueID + ":" + _clientId + ":" + _userId;
                        }
                }
                public virtual bool Visible
                {
                        get
                        {
                                return _visible;
                        }
                        set
                        {
                                _visible = value;
                        }
                }
                protected bool ChildControlsCreated
                {
                        get
                        {
                                return _childControlsCreated;
                        }
                        set
                        {
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
                                if (_events != null) return _events;
                                _events = new EventHandlerList();
                        }
                }
                protected bool HasChildViewState
                {
                        get
                        {       //FIXME: Relook over this. not sure!
                                foreach (control c in _controls)
                                        if (c.IsTrackingViewState) return true;
                                return false;
                        }
                }
                protected bool IsTrackingViewState //DIT
                {
                        get
                        {
                                return _trackingViewState;
                        }
                }
                protected virtual StateBag ViewState
                {
                        get
                        {
                                if (_viewState == null) _viewState = new StateBag(ViewStateIgnoreCase);
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
                public virtual Control FindControl(string id)
                {
                        int i;
                        for (i = 0; i < _controls.Count; i++)
                                if (_controls[i].ID == id) return _controls[i].ID;
                        return null;
                }
                protected virtual Control FindControl(string id, int pathOffset)
                {
                        int i;
                        for (i = pathOffset; i < _controls.Count; i++)
                                if (_controls[i].ID == id) return _controls[i].ID;
                        return null;
                }
                protected virtual void LoadViewState(object savedState)
                {
                        //TODO: What should I do by default?
                }
                protected string MapPathSecure(string virtualPath)
                {
                        //TODO: Need to read up on security+web.
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
                        return false;
                }
                protected virtual void Render(HtmlTextWriter writer) //DIT
                {
                        RenderChildren(writer);
                }
                protected virtual void RenderChildren(HtmlTextWriter writer)
                {
                        //if render method delegate is set, call it here. otherwise,
                        //render any child controls. just a for loop?
                }
                protected virtual object SaveViewState()
                {
                        return ViewState;
                }
                protected virtual void TrackViewState()
                {
                        _trackViewState = true;
                }
                public virtual void DataBind()
                {
//TODO: I think this recursively calls this method on its children.
                }
                public virtual void Dispose()
                {
                        //TODO: nuke stuff.
                        if (_events != null)
                        {
                                EventHandler eh = (EventHandler)(_events[DisposedEvent]);
                                if (eh != null) eh(this, e);
                        }
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

        }
}
