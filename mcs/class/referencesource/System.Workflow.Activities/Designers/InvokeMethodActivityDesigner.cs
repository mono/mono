namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #region Class CallExternalMethodActivityDesigner
    [ActivityDesignerTheme(typeof(InvokeMethodDesignerTheme))]
    internal class CallExternalMethodActivityDesigner : ActivityDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            CallExternalMethodActivity invokeMethod = Activity as CallExternalMethodActivity;

            object corrRefProperty = properties["CorrelationToken"];
            AddRemoveCorrelationToken(invokeMethod.InterfaceType, properties, corrRefProperty);

            Type type = invokeMethod.InterfaceType;
            if (type == null)
                return;

            AddRemoveCorrelationToken(type, properties, corrRefProperty);

            invokeMethod.GetParameterPropertyDescriptors(properties);
        }

        private void AddRemoveCorrelationToken(Type interfaceType, IDictionary properties, object corrRefProperty)
        {
            if (interfaceType != null)
            {
                object[] corrProvAttribs = interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), false);
                object[] corrParamAttribs = interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false);
                if (corrProvAttribs.Length != 0 || corrParamAttribs.Length != 0)
                {
                    if (!properties.Contains("CorrelationToken"))
                        properties.Add("CorrelationToken", corrRefProperty);
                    return;
                }
            }
            if (properties.Contains("CorrelationToken"))
                properties.Remove("CorrelationToken");
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null)
            {
                if (e.Member.Name == "InterfaceType")
                {
                    if (Activity.Site != null)
                    {
                        Type interfaceType = e.NewValue as Type;
                        if (interfaceType != null)
                            new ExternalDataExchangeInterfaceTypeFilterProvider(Activity.Site).CanFilterType(interfaceType, true);

                        CallExternalMethodActivity invokeActivity = e.Activity as CallExternalMethodActivity;
                        PropertyDescriptorUtils.SetPropertyValue(Activity.Site, TypeDescriptor.GetProperties(Activity)["MethodName"], Activity, String.Empty);

                        IExtendedUIService extUIService = (IExtendedUIService)Activity.Site.GetService(typeof(IExtendedUIService));
                        if (extUIService == null)
                            throw new Exception(SR.GetString(SR.General_MissingService, typeof(IExtendedUIService).FullName));
                    }
                }

                if ((e.Member.Name == "MethodName")
                    && e.Activity is CallExternalMethodActivity)
                    (e.Activity as CallExternalMethodActivity).ParameterBindings.Clear();

                if (e.Member.Name == "InterfaceType" || e.Member.Name == "MethodName" || e.Member.Name == "CorrelationToken")
                    TypeDescriptor.Refresh(e.Activity);
            }
        }

    }
    #endregion

    #region InvokeMethodDesignerTheme
    internal sealed class InvokeMethodDesignerTheme : ActivityDesignerTheme
    {
        public InvokeMethodDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x73, 0x79, 0xA5);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xDF, 0xE8, 0xFF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0x95, 0xB3, 0xFF);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
