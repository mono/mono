namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.ComponentModel.Design.Serialization;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Web.Services.Protocols;
    using System.Windows.Forms.Design;
    using System.Security.Permissions;
    using System.Workflow.Activities.Common;

    #region Class InvokeWebServiceToolboxItem
    [Serializable]
    internal sealed class InvokeWebServiceToolboxItem : ActivityToolboxItem
    {
        public InvokeWebServiceToolboxItem(Type type)
            : base(type)
        {

        }

        private InvokeWebServiceToolboxItem(SerializationInfo info, StreamingContext context)
        {
            base.Deserialize(info, context);
        }

        public override IComponent[] CreateComponentsWithUI(IDesignerHost host)
        {
            Uri url = null;
            Type proxyClass = null;
            IExtendedUIService extUIService = host.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (extUIService != null)
                extUIService.AddWebReference(out url, out proxyClass);

            IComponent[] components = base.CreateComponentsWithUI(host);
            if (components.GetLength(0) > 0)
            {
                InvokeWebServiceActivity webService = components[0] as InvokeWebServiceActivity;
                if (webService != null)
                    webService.ProxyClass = proxyClass;
            }

            return components;
        }
    }
    #endregion

    #region Class InvokeWebServiceDesigner
    [ActivityDesignerTheme(typeof(InvokeWebServiceDesignerTheme))]
    internal sealed class InvokeWebServiceDesigner : ActivityDesigner
    {
        #region Members, Constructor and Destructor
        private string url = null;

        #endregion

        #region Properties and Methods
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (properties["URL"] == null)
                properties["URL"] = new WebServiceUrlPropertyDescriptor(Activity.Site, TypeDescriptor.CreateProperty(this.GetType(), "URL", typeof(string), DesignOnlyAttribute.Yes, MergablePropertyAttribute.No));

            //

            ITypeProvider typeProvider = (ITypeProvider)GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            InvokeWebServiceActivity invokeWebService = Activity as InvokeWebServiceActivity;
            invokeWebService.GetParameterPropertyDescriptors(properties);
        }

        [SRCategory(SR.Activity)]
        [SRDescription(SR.URLDescr)]
        [Editor(typeof(WebServicePickerEditor), typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.All)]
        public string URL
        {
            get
            {
                if (this.url == null)
                {
                    InvokeWebServiceActivity invokeWebServiceDecl = Activity as InvokeWebServiceActivity;
                    IExtendedUIService extUIService = (IExtendedUIService)Activity.Site.GetService(typeof(IExtendedUIService));
                    if (extUIService != null && invokeWebServiceDecl.ProxyClass != null)
                    {
                        Uri uri = extUIService.GetUrlForProxyClass(invokeWebServiceDecl.ProxyClass);
                        this.url = (uri != null) ? uri.ToString() : string.Empty;
                    }
                }

                return this.url;
            }
            set
            {
                if (this.url != value)
                {
                    this.url = value;

                    IExtendedUIService extUIService = (IExtendedUIService)Activity.Site.GetService(typeof(IExtendedUIService));
                    if (extUIService == null)
                        throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IExtendedUIService).FullName));

                    //Create the designer transaction
                    DesignerTransaction trans = null;
                    IDesignerHost host = Activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (host != null)
                        trans = host.CreateTransaction(SR.GetString(SR.ChangingVariable));
                    try
                    {
                        PropertyDescriptorUtils.SetPropertyValue(Activity.Site, TypeDescriptor.GetProperties(Activity)["ProxyClass"], Activity, string.IsNullOrEmpty(this.url) ? null : extUIService.GetProxyClassForUrl(new Uri(this.url)));
                        if (trans != null)
                            trans.Commit();
                    }
                    finally
                    {
                        if (trans != null)
                            ((IDisposable)trans).Dispose();
                    }
                }
            }
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null)
            {
                if (e.Member.Name == "ProxyClass")
                {
                    if (Activity.Site != null)
                    {
                        InvokeWebServiceActivity invokeWebServiceDecl = e.Activity as InvokeWebServiceActivity;
                        PropertyDescriptorUtils.SetPropertyValue(Activity.Site, TypeDescriptor.GetProperties(Activity)["MethodName"], Activity, String.Empty);

                        IExtendedUIService extUIService = (IExtendedUIService)Activity.Site.GetService(typeof(IExtendedUIService));
                        if (extUIService == null)
                            throw new Exception(SR.GetString(SR.General_MissingService, typeof(IExtendedUIService).FullName));

                        if (invokeWebServiceDecl.ProxyClass == null)
                        {
                            this.url = null;
                        }
                        else
                        {
                            Uri uri = extUIService.GetUrlForProxyClass(invokeWebServiceDecl.ProxyClass);
                            this.url = (uri != null) ? uri.ToString() : string.Empty;
                        }
                    }
                }

                if ((e.Member.Name == "MethodName" || e.Member.Name == "TargetWorkflow")
                    && e.Activity is InvokeWebServiceActivity)
                    (e.Activity as InvokeWebServiceActivity).ParameterBindings.Clear();

                if (e.Member.Name == "ProxyClass" || e.Member.Name == "MethodName")
                    TypeDescriptor.Refresh(e.Activity);
            }
        }
        #endregion
    }
    #endregion

    #region InvokeWebServiceDesignerTheme
    internal sealed class InvokeWebServiceDesignerTheme : ActivityDesignerTheme
    {
        public InvokeWebServiceDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x94, 0xB6, 0xF7);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xA5, 0xC3, 0xF7);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion

    #region Class WebServiceUrlPropertyDescriptor
    internal sealed class WebServiceUrlPropertyDescriptor : DynamicPropertyDescriptor
    {
        internal WebServiceUrlPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor pd)
            : base(serviceProvider, pd)
        {
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
    #endregion

    internal sealed class WebServicePickerEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;
        public WebServicePickerEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            object returnVal = o;
            this.editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            IExtendedUIService extUIService = (IExtendedUIService)serviceProvider.GetService(typeof(IExtendedUIService));
            if (editorService != null && extUIService != null)
            {
                Uri url = null;
                Type proxyClass = null;

                if (DialogResult.OK == extUIService.AddWebReference(out url, out proxyClass))
                {
                    returnVal = (url != null) ? url.ToString() : string.Empty;
                    typeDescriptorContext.PropertyDescriptor.SetValue(typeDescriptorContext.Instance, returnVal as string);
                }
            }
            return returnVal;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
