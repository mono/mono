//------------------------------------------------------------------------------
// <copyright file="MultiView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;


    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    [
    ControlBuilder(typeof(MultiViewControlBuilder)),
    Designer("System.Web.UI.Design.WebControls.MultiViewDesigner, " + AssemblyRef.SystemDesign),
    DefaultEvent("ActiveViewChanged"),
    ParseChildren(typeof(View))
    ]
    [ToolboxData("<{0}:MultiView runat=\"server\"></{0}:MultiView>")]
    public class MultiView : Control {

        private static readonly object _eventActiveViewChanged = new object();
        private int _activeViewIndex = -1;
        private int _cachedActiveViewIndex = -1;
        private bool _ignoreBubbleEvents = false;
        private bool _controlStateApplied;


        /// <devdoc>
        /// <para> Specifies the NextView command. This field is constant.</para>
        /// </devdoc>
        public static readonly string NextViewCommandName = "NextView";

        /// <devdoc>
        /// <para> Specifies the PrevView command. This field is constant.</para>
        /// </devdoc>
        public static readonly string PreviousViewCommandName = "PrevView";

        /// <devdoc>
        /// <para> Specifies the SwitchViewById command. This field is constant.</para>
        /// </devdoc>
        public static readonly string SwitchViewByIDCommandName = "SwitchViewByID";

        /// <devdoc>
        /// <para> Specifies the SwitchViewByIndex command. This field is constant.</para>
        /// </devdoc>
        public static readonly string SwitchViewByIndexCommandName = "SwitchViewByIndex";


        /// <devdoc>
        /// <para>Indicates the active <see cref='System.Web.UI.WebControls.View'/> inside the <see cref='System.Web.UI.WebControls.MultiView'/> control.</para>
        /// </devdoc>
        [
        DefaultValue(-1),
        WebCategory("Behavior"),
        WebSysDescription(SR.MultiView_ActiveView)
        ]
        public virtual int ActiveViewIndex {
            get {
                if (_cachedActiveViewIndex > -1) {
                    return _cachedActiveViewIndex;
                }
                return _activeViewIndex;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.MultiView_ActiveViewIndex_less_than_minus_one, value));
                }
                if (Views.Count == 0 && ControlState < ControlState.FrameworkInitialized /* Whidbey 113333 */) {
                    _cachedActiveViewIndex = value;
                    return;
                }
                if (value >= Views.Count) {
                    throw new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.MultiView_ActiveViewIndex_equal_or_greater_than_count, value, Views.Count));
                }

                // VSWhidbey 472054: If the cached index is not -1, that means we
                // haven't activate any view, hence original index is set to -1.
                int originalIndex = (_cachedActiveViewIndex != -1) ? -1 : _activeViewIndex;
                _activeViewIndex = value;
                _cachedActiveViewIndex = -1;
                // Need to compare originalIndex to Views.Count in case originalIndex was set declaratively.
                if (originalIndex != value && originalIndex != -1  && originalIndex < Views.Count) {
                    Views[originalIndex].Active = false;
                    if (ShouldTriggerViewEvent) {
                        Views[originalIndex].OnDeactivate(EventArgs.Empty);
                    }
                }
                if (originalIndex != value && Views.Count != 0 && value != -1) {
                    Views[value].Active = true;
                    if (ShouldTriggerViewEvent) {
                        Views[value].OnActivate(EventArgs.Empty);
                        OnActiveViewChanged(EventArgs.Empty);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }

        private bool ShouldTriggerViewEvent {
            get {
                // We want to fire the View's event on the first request
                return (_controlStateApplied || (Page != null && !Page.IsPostBack));
            }
        }

        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.MultiView_Views),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual ViewCollection Views {
            get {
                return (ViewCollection)Controls;
            }
        }



        /// <devdoc>
        ///    <para>Occurs when the active view changed.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.MultiView_ActiveViewChanged)
        ]
        public event EventHandler ActiveViewChanged {
            add {
                Events.AddHandler(_eventActiveViewChanged, value);
            }
            remove {
                Events.RemoveHandler(_eventActiveViewChanged, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Overridden to only allow View controls to be added.
        /// </devdoc>
        protected override void AddParsedSubObject(object obj) {
            if (obj is View) {
                Controls.Add((Control)obj);
            }
            else if (!(obj is LiteralControl))
                throw new HttpException(SR.GetString(SR.MultiView_cannot_have_children_of_type, obj.GetType().Name));
        }


        /// <devdoc>
        /// [To be supplied.]
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new ViewCollection(this);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public View GetActiveView() {
            int activeViewIndex = ActiveViewIndex;
            if (activeViewIndex >= Views.Count) {
                throw new Exception(SR.GetString(SR.MultiView_ActiveViewIndex_out_of_range));
            }
            if (activeViewIndex < 0) {
                return null;
            }
            View view = Views[activeViewIndex];
            if (!view.Active) {
                UpdateActiveView(activeViewIndex);
            }
            return view;
        }

        // Used to wizard to disable bubble events on MultiView.
        internal void IgnoreBubbleEvents() {
            _ignoreBubbleEvents = true;
        }

        private void UpdateActiveView(int activeViewIndex) {
            for (int i=0; i<Views.Count; ++i) {
                View view = Views[i];
                if (i == activeViewIndex) {
                    view.Active = true;
                    if (ShouldTriggerViewEvent) {
                        view.OnActivate(EventArgs.Empty);
                    }
                    continue;
                }

                if (view.Active) {
                    // There will be at most one other active view
                    view.Active = false;
                    if (ShouldTriggerViewEvent) {
                        view.OnDeactivate(EventArgs.Empty);
                    }
                }
            }
        }


        /// <devdoc>
        /// <para>Loads the control state.</para>
        /// </devdoc>
        protected internal override void LoadControlState(object state) {
            Pair p = state as Pair;
            if (p != null) {
                base.LoadControlState(p.First);
                ActiveViewIndex = (int) p.Second;
            }

            _controlStateApplied = true;
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ActiveViewChanged '/>event.</para>
        /// </devdoc>
        protected virtual void OnActiveViewChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[_eventActiveViewChanged];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Overridden to handle navigation between child views.
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (_ignoreBubbleEvents) {
                return false;
            }

            if (e is CommandEventArgs) {
                CommandEventArgs ce = (CommandEventArgs) e;
                string cn = ce.CommandName;
                if (cn ==  NextViewCommandName) {
                    if (ActiveViewIndex < Views.Count - 1) {
                       ActiveViewIndex = ActiveViewIndex + 1;
                    }
                    else {
                        ActiveViewIndex = - 1;
                    }
                    return true;
                }
                else if (cn == PreviousViewCommandName) {
                    if (ActiveViewIndex > -1 ) {
                        ActiveViewIndex = ActiveViewIndex - 1;
                    }
                    return true;
                }
                else if (cn == SwitchViewByIDCommandName) {
                    View view = FindControl((string)ce.CommandArgument) as View;
                    if (view != null && view.Parent == this) {
                        SetActiveView(view);
                        return true;
                    }
                    else {
                        throw new HttpException(SR.GetString(SR.MultiView_invalid_view_id, ID,
                            (string)ce.CommandArgument, SwitchViewByIDCommandName));
                    }
                }
                else if (cn == SwitchViewByIndexCommandName) {
                    int index;
                    try {
                        index = Int32.Parse((string)ce.CommandArgument, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        throw new FormatException(SR.GetString(SR.MultiView_invalid_view_index_format,
                            (string)ce.CommandArgument, SwitchViewByIndexCommandName));
                    }
                    ActiveViewIndex = index;
                    return true;
                }
            }
            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Overridden to handle navigation between child views.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);

            if (_cachedActiveViewIndex > -1) {
                ActiveViewIndex = _cachedActiveViewIndex;
                _cachedActiveViewIndex = -1;
                GetActiveView();
            }
        }


        protected internal override void RemovedControl(Control ctl) {
            if (((View)ctl).Active && ActiveViewIndex < Views.Count) {
                GetActiveView();
            }
            base.RemovedControl(ctl);
        }


        protected internal override void Render(HtmlTextWriter writer) {
            View activeView = GetActiveView();
            if (activeView != null) {
                activeView.RenderControl(writer);
            }
        }


        /// <devdoc>
        /// <para>Saves the control state.</para>
        /// </devdoc>
        protected internal override object SaveControlState() {
            int avi = ActiveViewIndex;
            object obj = base.SaveControlState();
            if (obj != null || avi != -1) {
                return new Pair(obj, avi);
            }
            return null;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public void SetActiveView(View view) {
            int index = Views.IndexOf (view);
            if (index < 0) {
                throw new HttpException(SR.GetString(SR.MultiView_view_not_found, (view == null ? "null" : view.ID), this.ID));
            }
            ActiveViewIndex = index;
        }
    }


    /// <devdoc> [To be supplied.] </devdoc>
    public class MultiViewControlBuilder : ControlBuilder {

        public override void AppendSubBuilder(ControlBuilder subBuilder) {
            if (subBuilder is CodeBlockBuilder) {
                throw new Exception(SR.GetString(SR.Multiview_rendering_block_not_allowed));
            }
            base.AppendSubBuilder(subBuilder);
        }
    }



    /// <devdoc> [To be supplied.] </devdoc>
    public class ViewCollection : ControlCollection {


        /// <devdoc> [To be supplied.] </devdoc>
        public ViewCollection (Control owner) : base (owner) {}


        /// <devdoc> [To be supplied.] </devdoc>
        public override void Add(Control v) {
            if (!(v is View)) {
                throw new ArgumentException(SR.GetString(SR.ViewCollection_must_contain_view));
            }
            base.Add(v);
        }


        /// <devdoc> [To be supplied.] </devdoc>
        public override void AddAt(int index, Control v) {
            if (!(v is View)) {
                throw new ArgumentException(SR.GetString(SR.ViewCollection_must_contain_view));
            }
            base.AddAt(index, v);
        }


        /// <devdoc> [To be supplied.] </devdoc>
        public new View this[int i] {
            get {
                return (View)base[i];
            }
        }
    }
}
