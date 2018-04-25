using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Windows.Forms.Design;
using System.Security.Permissions;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel.Design.Serialization;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities
{
    [ActivityDesignerTheme(typeof(WebServiceDesignerTheme))]
    internal sealed class WebServiceReceiveDesigner : ActivityDesigner
    {
        #region Properties and Methods
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            WebServiceInputActivity webServiceReceive = this.Activity as WebServiceInputActivity;
            webServiceReceive.GetParameterPropertyDescriptors(properties);

            if (properties.Contains("InterfaceType"))
                properties["InterfaceType"] = new WebServiceInterfacePropertyDescriptor(Activity.Site, properties["InterfaceType"] as PropertyDescriptor);
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null)
            {
                // If the interface name changed, clear out method name.
                if (e.Member.Name == "InterfaceType")
                {
                    if (this.Activity.Site != null)
                    {
                        Type interfaceType = e.NewValue as Type;
                        if (interfaceType != null)
                            new InterfaceTypeFilterProvider(Activity.Site).CanFilterType(interfaceType, true);

                        WebServiceInputActivity webServiceReceive = e.Activity as WebServiceInputActivity;
                        PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(Activity)["MethodName"];
                        if (propertyDescriptor != null)
                            propertyDescriptor.SetValue(Activity, String.Empty);
                    }
                }
                else if (e.Member.Name == "MethodName")
                {
                    // If method name changed, clear out parameters.
                    (e.Activity as WebServiceInputActivity).ParameterBindings.Clear();
                }

                // Refresh all other properties as well
                if (e.Member.Name == "InterfaceType" || e.Member.Name == "MethodName")
                    TypeDescriptor.Refresh(e.Activity);

                foreach (Activity succeedingActivity in WebServiceActivityHelpers.GetSucceedingActivities(this.Activity))
                {
                    if (succeedingActivity is WebServiceOutputActivity && ((WebServiceOutputActivity)(succeedingActivity)).InputActivityName == this.Activity.QualifiedName)
                        TypeDescriptor.Refresh(succeedingActivity);
                }
            }
        }
        #endregion
    }

    #region WebServiceDesignerTheme
    internal sealed class WebServiceDesignerTheme : ActivityDesignerTheme
    {
        public WebServiceDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x94, 0xB6, 0xF7);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xDF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xA5, 0xC3, 0xF7);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion

    internal sealed class WebServiceInterfacePropertyDescriptor : DynamicPropertyDescriptor
    {
        internal WebServiceInterfacePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor pd)
            : base(serviceProvider, pd)
        {
        }

        public override void SetValue(object component, object value)
        {
            string typeName = value as String;
            if (typeName != null && typeName.Length > 0)
            {
                ITypeProvider typeProvider = (ITypeProvider)this.ServiceProvider.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                Type type = typeProvider.GetType(value as string);
                if (type == null)
                    throw new Exception(SR.GetString(SR.Error_TypeNotResolved, value));

                TypeFilterProviderAttribute filterProviderAttribute = this.Attributes[typeof(TypeFilterProviderAttribute)] as TypeFilterProviderAttribute;
                if (filterProviderAttribute != null)
                {
                    ITypeFilterProvider typeFilterProvider = null;
                    Type typeFilterProviderType = Type.GetType(filterProviderAttribute.TypeFilterProviderTypeName);
                    if (typeFilterProviderType != null)
                        typeFilterProvider = Activator.CreateInstance(typeFilterProviderType, new object[] { this.ServiceProvider }) as ITypeFilterProvider;
                    if (typeFilterProvider != null)
                        typeFilterProvider.CanFilterType(type, true);
                }
                // we always store assembly qualified name of the type
                value = type.AssemblyQualifiedName;
            }

            RealPropertyDescriptor.SetValue(component, value);
        }
    }
}
