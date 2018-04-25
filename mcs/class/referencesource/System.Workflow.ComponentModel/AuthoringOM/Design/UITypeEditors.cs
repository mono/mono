namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;
    using System.CodeDom;
    using System.Drawing;
    using System.Reflection;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;


    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TypeBrowserEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public TypeBrowserEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object value)
        {
            if (typeDescriptorContext == null)
                throw new ArgumentNullException("typeDescriptorContext");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            object returnVal = value;
            this.editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                ITypeFilterProvider typeFilterProvider = null;
                TypeFilterProviderAttribute typeFilterProvAttr = null;

                if (typeDescriptorContext.PropertyDescriptor != null && typeDescriptorContext.PropertyDescriptor.Attributes != null)
                    typeFilterProvAttr = typeDescriptorContext.PropertyDescriptor.Attributes[typeof(TypeFilterProviderAttribute)] as TypeFilterProviderAttribute;

                if (typeFilterProvAttr != null)
                {
                    ITypeProvider typeProvider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (typeProvider == null)
                        throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                    Type typeFilterProviderType = Type.GetType(typeFilterProvAttr.TypeFilterProviderTypeName);
                    //typeProvider.GetType(typeFilterProvAttr.TypeFilterProviderTypeName);
                    if (typeFilterProviderType != null)
                        typeFilterProvider = Activator.CreateInstance(typeFilterProviderType, new object[] { serviceProvider }) as ITypeFilterProvider;
                }

                if (typeFilterProvider == null)
                    typeFilterProvider = ((typeDescriptorContext.Instance is object[]) ? ((object[])typeDescriptorContext.Instance)[0] : typeDescriptorContext.Instance) as ITypeFilterProvider;

                if (typeFilterProvider == null)
                    typeFilterProvider = value as ITypeFilterProvider;

                if (typeFilterProvider == null)
                {
                    IReferenceService rs = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (rs != null)
                    {
                        IComponent baseComponent = rs.GetComponent(typeDescriptorContext.Instance);
                        if (baseComponent is ITypeFilterProvider)
                            typeFilterProvider = baseComponent as ITypeFilterProvider;
                    }
                }

                if (typeFilterProvider == null)
                {
                    typeFilterProvider = typeDescriptorContext.PropertyDescriptor as ITypeFilterProvider;
                }

                string oldTypeName = value as string;
                if (value != null && typeDescriptorContext.PropertyDescriptor.PropertyType != typeof(string) && typeDescriptorContext.PropertyDescriptor.Converter != null && typeDescriptorContext.PropertyDescriptor.Converter.CanConvertTo(typeof(string)))
                    oldTypeName = typeDescriptorContext.PropertyDescriptor.Converter.ConvertTo(typeDescriptorContext, CultureInfo.CurrentCulture, value, typeof(string)) as string;

                using (TypeBrowserDialog dlg = new TypeBrowserDialog(serviceProvider, typeFilterProvider as ITypeFilterProvider, oldTypeName))
                {
                    if (DialogResult.OK == editorService.ShowDialog(dlg))
                    {
                        if (typeDescriptorContext.PropertyDescriptor.PropertyType == typeof(Type))
                            returnVal = dlg.SelectedType;
                        else if (typeDescriptorContext.PropertyDescriptor.PropertyType == typeof(string))
                            returnVal = dlg.SelectedType.FullName;
                        else if (typeDescriptorContext.PropertyDescriptor.Converter != null && typeDescriptorContext.PropertyDescriptor.Converter.CanConvertFrom(typeDescriptorContext, typeof(string)))
                            returnVal = typeDescriptorContext.PropertyDescriptor.Converter.ConvertFrom(typeDescriptorContext, CultureInfo.CurrentCulture, dlg.SelectedType.FullName);
                    }
                }
            }
            return returnVal;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class BindUITypeEditor : UITypeEditor
    {
        private const int MaxItems = 10;
        private IServiceProvider serviceProvider;

        public BindUITypeEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider serviceProvider, object value)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;

            object returnValue = value;
            if (context != null && context.PropertyDescriptor is DynamicPropertyDescriptor)
            {
                try
                {
                    using (ActivityBindForm bindDialog = new ActivityBindForm(this.serviceProvider, context))
                    {
                        if (DialogResult.OK == bindDialog.ShowDialog())
                        {
                            //Now that OK has been pressed in the dialog we need to create members if necessary
                            if (bindDialog.CreateNew)
                            {
                                //Emit the field / property as required
                                if (bindDialog.CreateNewProperty)
                                {
                                    List<CustomProperty> properties = CustomActivityDesignerHelper.GetCustomProperties(context);
                                    if (properties != null)
                                    {
                                        properties.Add(CustomProperty.CreateCustomProperty(this.serviceProvider, bindDialog.NewMemberName, context.PropertyDescriptor, context.Instance));
                                        CustomActivityDesignerHelper.SetCustomProperties(properties, context);
                                    }
                                }
                                else
                                {
                                    ActivityBindPropertyDescriptor.CreateField(context, bindDialog.Binding, true);
                                }
                            }
                            returnValue = bindDialog.Binding;
                        }
                    }
                }
                catch (Exception e)
                {
                    string message = SR.GetString(SR.Error_CanNotBindProperty, context.PropertyDescriptor.Name);
                    if (!String.IsNullOrEmpty(e.Message))
                        message += "\n\n" + e.Message;
                    DesignerHelpers.ShowError(context, message);
                }
            }
            else
            {
                DesignerHelpers.ShowError(this.serviceProvider, SR.GetString(SR.Error_MultipleSelectNotSupportedForBindAndPromote));
            }

            return returnValue;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }

        internal static object EditValue(ITypeDescriptorContext context)
        {
            object value = null;
            if (context != null && context.PropertyDescriptor != null && context.Instance != null)
            {
                BindUITypeEditor bindTypeEditor = new BindUITypeEditor();
                value = context.PropertyDescriptor.GetValue(context.Instance);

                value = bindTypeEditor.EditValue(context, context, value);

                try
                {
                    context.PropertyDescriptor.SetValue(context.Instance, value);
                }
                catch (Exception e)
                {
                    string message = SR.GetString(SR.Error_CanNotBindProperty, context.PropertyDescriptor.Name);
                    if (!String.IsNullOrEmpty(e.Message))
                        message += "\n\n" + e.Message;
                    DesignerHelpers.ShowError(context, message);
                }
            }
            return value;
        }
    }
}
