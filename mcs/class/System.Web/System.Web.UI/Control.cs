//
// System.Web.UI.Control.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

//notes: view state only tracks changes after OnInit method is executed for the page request. You can read from it at any time, but cant write to it during rendering.

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
                private bool _viewStateIgnoreCase = true;
                private EventHandlerList _events;
                public Control()
                {
                        _namingContainer = _parent;
                        _viewState = new StateBag(_viewStateIgnoreCase);
                        _events = new EventHandlerList();
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
                        {
                                //TODO: look through children for any that have saved view state info.
                        }
                }
                protected bool IsTrackingViewState
                {
                        get
                        {
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
        }

}
