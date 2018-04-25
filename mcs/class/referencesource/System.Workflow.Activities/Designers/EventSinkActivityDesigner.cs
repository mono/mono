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

    #region Class HandleExternalEventActivityDesigner
    [ActivityDesignerTheme(typeof(EventSinkDesignerTheme))]
    internal class HandleExternalEventActivityDesigner : ActivityDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            object corrRefProperty = properties["CorrelationToken"];

            HandleExternalEventActivity eventSink = Activity as HandleExternalEventActivity;

            AddRemoveCorrelationToken(eventSink.InterfaceType, properties, corrRefProperty);

            Type type = eventSink.InterfaceType;
            if (type == null)
                return;

            AddRemoveCorrelationToken(type, properties, corrRefProperty);

            eventSink.GetParameterPropertyDescriptors(properties);
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

                        HandleExternalEventActivity eventSinkActivity = e.Activity as HandleExternalEventActivity;
                        PropertyDescriptorUtils.SetPropertyValue(Activity.Site, TypeDescriptor.GetProperties(Activity)["EventName"], Activity, String.Empty);

                        IExtendedUIService extUIService = (IExtendedUIService)Activity.Site.GetService(typeof(IExtendedUIService));
                        if (extUIService == null)
                            throw new Exception(SR.GetString(SR.General_MissingService, typeof(IExtendedUIService).FullName));
                    }
                }
                else if ((e.Member.Name == "EventName")
                    && e.Activity is HandleExternalEventActivity)
                {
                    (e.Activity as HandleExternalEventActivity).ParameterBindings.Clear();
                }

                if (e.Member.Name == "InterfaceType" || e.Member.Name == "EventName" || e.Member.Name == "CorrelationToken")
                    TypeDescriptor.Refresh(e.Activity);
            }
        }
    }
    #endregion

    #region EventSinkDesignerTheme
    internal sealed class EventSinkDesignerTheme : ActivityDesignerTheme
    {
        public EventSinkDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x9C, 0xAE, 0x73);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xF5, 0xFB, 0xE1);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xD6, 0xEB, 0x84);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
