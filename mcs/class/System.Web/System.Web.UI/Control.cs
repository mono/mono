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

using System;
using System.Web;
using System.ComponentModel;

namespace System.Web.UI
{
        public class Control : IComponent, IDisposable, IParserAccessor, IDataBindingsAccessor
        {
                private string _clientId; //default to "ctrl#" where # is a static count of ctrls per page.
                private string _userId = null;
                private ControlCollection _controls;
                private bool _enableViewState = true;
                private Control _namingContainer;
                private Page _page;
                private Control _parent; //TODO: set default.
                private ISite _site; //TODO: what default?
                private bool _visible; //TODO: what default?
                private HttpContext _context = null;
                private bool _childControlsCreated = false;
                private StateBag _viewState; //TODO: help me.
                private bool _trackViewState = false; //TODO: I think this is right. Verify. Also modify other methods to use this.
                private bool _viewStateIgnoreCase = true;
                private EventHandlerList _events;
                public Control()
                {
                        _namingContainer = _parent;
                        _viewState = new StateBag(_viewStateIgnoreCase);
                        _events = new EventHandlerList();
                        _controls = this.CreateControlCollection(); //FIXME: this goes here?
                }
                public virtual string ClientID
                {
                        get
                        {
                                return _clientId;
                        }
                }
                public virtual ControlCollection Controls
                {
                        get
                        {
                                return _controls;
                        }
                }
                public virtual bool EnableViewState
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
                public virtual Control NamingContainer
                {
                        get
                        {
                                return _namingContainer;
                        }
                }
                public virtual Page Page
                {
                        get
                        {
                                return _page;
                        }
                        set
                        {
                                _page = value;
                        }
                }
                public virtual Control Parent
                {
                        get
                        {
                                return _parent;
                        }
                }
                public ISite Site
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
                protected virtual HttpContext Context
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
                protected EventHandlerList Events
                {
                        get
                        {
                                return _events;
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
                protected bool IsTrackingViewState
                {
                        get
                        {       //FIXME: ME TO!
                                return _enableViewState;
                        }
                }
                protected virtual StateBag ViewState
                {
                        get
                        {
                                return _viewState;
                        }
                }
                protected virtual bool ViewStateIgnoresCase
                {
                        get
                        {
                                return _viewStateIgnoreCase;
                        }
                }
                protected virtual void AddParsedSubObject(object obj)
                {
                        _controls.Add(obj);
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
                protected virtual void CreateChildControls() {}
                protected virtual ControlCollection CreateControlCollection()
                {
                        _controls = new ControlCollection(this);
                }
                protected virtual void EnsureChildControls() {} //FIXME: I think this should be empty.
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
                protected virtual bool OnBubbleEvent(object source, EventArgs args)
                {
                        return false; //FIXME: It might throw "ItemCommand". not sure.
                }
                protected virtual void OnDataBinding(EventArgs e) {} //FIXME: I think this should be empty.
                protected virtual void OnInit(EventArgs e) {} //FIXME: This one too.controls 
                protected virtual void OnPreRender(EventArgs e) {} //FIXME: Me to.
                protected virtual void OnUnload(EventArgs e) {} //TODO: Ok, I'm missing something. Read up on the event system.
                protected void RaiseBubbleEvent(object source, EventArgs args)
                {
                        _parent.OnBubbleEvent(source, args); //FIXME: I think this is right. Check though.
                }
                protected virtual void Render(HtmlTextWriter writer) {} //FIXME: Default?
                protected virtual void RenderChildren(HtmlTextWriter writer)
                {
                        //if render method delegate is set, call it here. otherwise,
                        //render any child controls. just a for loop?
                }
                protected virtual void TrackViewState()
                {
                        _trackViewState = true;
                }


        }
}
