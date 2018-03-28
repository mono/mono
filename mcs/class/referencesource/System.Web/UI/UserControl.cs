//------------------------------------------------------------------------------
// <copyright file="UserControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Page class definition
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Web.Caching;
    using System.Web.ModelBinding;
    using System.Web.SessionState;
    using System.Web.Util;


/// <devdoc>
///   <para>The ControlBuilder associated with a UserControl. If you want a custom ControlBuilder for your
///     derived UserControl, you should derive it from UserControlControlBuilder.
///   </para>
/// </devdoc>
public class UserControlControlBuilder : ControlBuilder {

    private string _innerText;


    /// <internalonly/>
    public override object BuildObject() {
        object o = base.BuildObject();

        if (InDesigner) {
            IUserControlDesignerAccessor designerAccessor = (IUserControlDesignerAccessor)o;
            
            designerAccessor.TagName = TagName;
            if (_innerText != null) {
                designerAccessor.InnerText = _innerText;
            }
        }
        return o;
    }


    /// <internalonly/>
    public override bool NeedsTagInnerText() {
        // in design-mode, we need to hang on to the inner text
        return InDesigner;
    }


    /// <internalonly/>
    public override void SetTagInnerText(string text) {
        Debug.Assert(InDesigner == true, "Should only be called in design-mode!");
        _innerText = text;
    }
}


/// <devdoc>
///    Default ControlBuilder used to parse user controls files.
/// </devdoc>
public class FileLevelUserControlBuilder: RootBuilder {
}


/// <devdoc>
///    <para>This class is not marked as abstract, because the VS designer
///          needs to instantiate it when opening .ascx files</para> 
/// </devdoc>
[
ControlBuilder(typeof(UserControlControlBuilder)),
DefaultEvent("Load"),
Designer("System.Web.UI.Design.UserControlDesigner, " + AssemblyRef.SystemDesign, typeof(IDesigner)),
Designer("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, " + AssemblyRef.MicrosoftVisualStudioWeb, typeof(IRootDesigner)),
DesignerCategory("ASPXCodeBehind"),
DesignerSerializer("Microsoft.VisualStudio.Web.WebForms.WebFormCodeDomSerializer, " + AssemblyRef.MicrosoftVisualStudioWeb, "System.ComponentModel.Design.Serialization.TypeCodeDomSerializer, " + AssemblyRef.SystemDesign),
ParseChildren(true),
ToolboxItem(false)
]
public class UserControl : TemplateControl, IAttributeAccessor, INonBindingContainer, IUserControlDesignerAccessor {

    private StateBag attributeStorage;
    private AttributeCollection attributes;

    private bool _fUserControlInitialized;


    /// <devdoc>
    ///    <para>Gets the collection of attribute name/value pairs expressed on a UserControl but
    ///       not supported by the control's strongly typed properties.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public AttributeCollection Attributes {
        get {
            if (attributes == null) {
                if (attributeStorage == null) {
                    attributeStorage = new StateBag(true);
                    if (IsTrackingViewState) {
                        attributeStorage.TrackViewState();
                    }
                }
                attributes = new AttributeCollection(attributeStorage);
            }
            return attributes;
        }
    }

    // Delegate most things to the Page


    /// <devdoc>
    /// <para>Gets the <see langword='Application'/> object provided by 
    ///    the HTTP Runtime.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpApplicationState Application { get { return Page.Application;} }

    /*
     * Trace context for output of useful information to page during development
     */

    /// <devdoc>
    /// <para>Indicates the <see cref='System.Web.TraceContext'/> object for the current Web 
    ///    request. Tracing tracks and presents the execution details about a Web request.
    ///    For trace data to be visible in a rendered page, you must turn tracing on for
    ///    that page. This property is read-only.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public TraceContext Trace { get { return Page.Trace; } }


    /// <devdoc>
    ///    <para>
    ///       Gets the <see langword='Request'/> object provided by the HTTP Runtime, which
    ///       allows developers to access data from incoming HTTP requests.
    ///    </para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpRequest Request { get { return Page.Request; } }


    /// <devdoc>
    /// <para>Gets the <see langword='Response '/>object provided by the HTTP Runtime, which
    ///    allows developers to send HTTP response data to a client browser.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpResponse Response { get { return Page.Response; } }


    /// <devdoc>
    /// <para>Gets the ASP-compatible <see langword='Server'/> object.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpServerUtility Server { get { return Page.Server; } }

    /*
     * Cache intrinsic
     */

    /// <devdoc>
    /// <para>Retrieves a <see langword='Cache'/> 
    /// object in which to store the user control's data for
    /// subsequent requests. This property is read-only.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public Cache Cache { get { return Page.Cache; } }


    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public ControlCachePolicy CachePolicy {
        get {
            // Check if we're inside a PartialCachingControl
            BasePartialCachingControl pcc = Parent as BasePartialCachingControl;

            // If so, return its CachePolicy
            if (pcc != null)
                return pcc.CachePolicy;

            // Otherwise, return a stub, which returns SupportsCaching==false and throws
            // on everything else.
            return ControlCachePolicy.GetCachePolicyStub();
        }
    }


    /// <devdoc>
    ///    <para>Gets a value indicating whether the user control is being loaded in response to a
    ///       client postback, or if it is being loaded and accessed for the first time.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public bool IsPostBack { get { return Page.IsPostBack; } }


    /// <devdoc>
    /// <para>Gets the <see langword='Session '/> object provided by the HTTP Runtime.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpSessionState Session { get { return Page.Session; } }

    /*
     * Performs intialization of the control required by the designer.
     */

    /// <devdoc>
    ///    <para>Performs any initialization of the control that is required by RAD designers.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void DesignerInitialize() {
        InitRecursive(null);
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    protected internal override void OnInit(EventArgs e) {

        // We want to avoid calling this when the user control is being used in the designer,
        // regardless of whether it is a top-level control (DesignMode == true),
        // or if its inside another control in design-mode (Page.Site.DesignMode == true)

        bool designTime = DesignMode;
        if (designTime == false) {
            if ((Page != null) && (Page.Site != null)) {
                designTime = Page.Site.DesignMode;
            }
        }

        if (designTime == false) {
            InitializeAsUserControlInternal();
        }

        base.OnInit(e);
    }

    /*
     * Called on declarative controls to initialize them correctly
     */

    /// <devdoc>
    /// <para>Initializes the <see langword='UserControl'/> object. Since there are some 
    ///    differences between pages and user controls, this method makes sure that the
    ///    user control is initialized properly.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void InitializeAsUserControl(Page page) {

        _page = page;

        InitializeAsUserControlInternal();
    }

    internal void InitializeAsUserControlInternal() {

        // Make sure we only do this once
        if (_fUserControlInitialized)
            return;
        _fUserControlInitialized = true;

        // Hook up any automatic handler we may find (e.g. Page_Load)
        HookUpAutomaticHandlers();

        // Initialize the object and instantiate all the controls defined in the ascx file
        FrameworkInitialize();
    }


    protected override void LoadViewState(object savedState) {
        if (savedState != null) {
            Pair myState = (Pair)savedState;
            base.LoadViewState(myState.First);

            if (myState.Second != null) {
                if (attributeStorage == null) {
                    attributeStorage = new StateBag(true);
                    attributeStorage.TrackViewState();
                }
                attributeStorage.LoadViewState(myState.Second);
            }
        }
    }


    protected override object SaveViewState() {
        Pair myState = null;

        object baseState = base.SaveViewState();
        object attrState = null;
        if (attributeStorage != null) {
            attrState = attributeStorage.SaveViewState();
        }

        if (baseState != null || attrState != null) {
            myState = new Pair(baseState, attrState);
        }
        return myState;
    }



    /// <internalonly/>
    /// <devdoc>
    /// Returns the attribute value of the UserControl having
    /// the specified attribute name.
    /// </devdoc>
    string IAttributeAccessor.GetAttribute(string name) {
        return ((attributeStorage != null) ? (string)attributeStorage[name] : null);
    }


    /// <internalonly/>
    /// <devdoc>
    /// <para>Sets an attribute of the UserControl with the specified
    /// name and value.</para>
    /// </devdoc>
    void IAttributeAccessor.SetAttribute(string name, string value) {
        Attributes[name] = value;
    }

    /*
     * Map virtual path (absolute or relative) to physical path
     */

    /// <devdoc>
    ///    <para>Assigns a virtual path, either absolute or relative, to a physical path.</para>
    /// </devdoc>
    public string MapPath(string virtualPath) {
        return Request.MapPath(VirtualPath.CreateAllowNull(virtualPath), TemplateControlVirtualDirectory,
            true/*allowCrossAppMapping*/);
    }


    /// <internalonly/>
    string IUserControlDesignerAccessor.TagName {
        get {
            string text = (string)ViewState["!DesignTimeTagName"];
            if (text == null) {
                return String.Empty;
            }
            return text;
        }
        set {
            ViewState["!DesignTimeTagName"] = value;
        }
    }


    /// <internalonly/>
    string IUserControlDesignerAccessor.InnerText {
        get {
            string text = (string)ViewState["!DesignTimeInnerText"];
            if (text == null) {
                return String.Empty;
            }
            return text;
        }
        set {
            ViewState["!DesignTimeInnerText"] = value;
        }
    }

    /// <summary>
    /// Updates the model object from the values within a databound control. This must be invoked 
    /// within the Select/Update/Delete/InsertMethods used for data binding.
    /// Throws an exception if the update fails.
    /// </summary>
    public virtual void UpdateModel<TModel>(TModel model) where TModel : class {
        Page.UpdateModel<TModel>(model);
    }

    /// <summary>
    /// Updates the model object from the values provided by given valueProvider.
    /// Throws an exception if the update fails.
    /// </summary>
    public virtual void UpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class {
        Page.UpdateModel<TModel>(model, valueProvider);
    }

    /// <summary>
    /// Attempts to update the model object from the values provided by given valueProvider.
    /// </summary>
    /// <returns>True if the model object is updated succesfully with valid values. False otherwise.</returns>
    public virtual bool TryUpdateModel<TModel>(TModel model) where TModel : class {
        return Page.TryUpdateModel<TModel>(model);
    }

    /// <summary>
    /// Attempts to update the model object from the values within a databound control. This
    /// must be invoked within the Select/Update/Delete/InsertMethods used for data binding. 
    /// </summary>
    /// <returns>True if the model object is updated succesfully with valid values. False otherwise.</returns>
    public virtual bool TryUpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class {
        return Page.TryUpdateModel<TModel>(model, valueProvider);
    }
}

}
