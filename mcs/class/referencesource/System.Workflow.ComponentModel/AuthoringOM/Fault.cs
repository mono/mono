namespace System.Workflow.ComponentModel
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [SRDescription(SR.FaultActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(ThrowDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(ThrowActivity), "Resources.Throw.png")]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ThrowActivity : Activity, ITypeFilterProvider, IDynamicPropertyTypeProvider
    {
        [Browsable(false)]
        public static readonly DependencyProperty FaultProperty = DependencyProperty.Register("Fault", typeof(Exception), typeof(ThrowActivity));
        [Browsable(false)]
        public static readonly DependencyProperty FaultTypeProperty = DependencyProperty.Register("FaultType", typeof(Type), typeof(ThrowActivity));

        #region Constructors

        public ThrowActivity()
        {
        }

        public ThrowActivity(string name)
            : base(name)
        {
        }

        #endregion

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected internal override sealed ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (this.Fault == null && this.FaultType == null)
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, SR.Error_PropertyNotSet, FaultProperty.Name));
            }

            if (this.FaultType != null && !typeof(Exception).IsAssignableFrom(this.FaultType))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, SR.Error_ExceptionTypeNotException, this.FaultType, FaultTypeProperty.Name));
            }

            if (this.Fault != null && this.FaultType != null && !this.FaultType.IsInstanceOfType(this.Fault))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, SR.Error_FaultIsNotOfFaultType));
            }

            if (this.Fault != null)
                throw this.Fault;

            ConstructorInfo cInfo = this.FaultType.GetConstructor(new Type[] { });

            if (cInfo != null)
                throw (Exception)cInfo.Invoke(null);

            throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, SR.Error_FaultTypeNoDefaultConstructor, this.FaultType));
        }

        [TypeConverter(typeof(FaultConverter))]
        [SRCategory(SR.Handlers)]
        [SRDescription(SR.FaultDescription)]
        [MergableProperty(false)]
        [DefaultValue(null)]
        public Exception Fault
        {
            get
            {
                return base.GetValue(FaultProperty) as Exception;
            }
            set
            {
                base.SetValue(FaultProperty, value);
            }
        }

        [Editor(typeof(TypeBrowserEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [SRCategory(SR.Handlers)]
        [SRDescription(SR.FaultTypeDescription)]
        [MergableProperty(false)]
        [DefaultValue(null)]
        [TypeConverter(typeof(FaultTypeConverter))]
        public Type FaultType
        {
            get
            {
                return base.GetValue(FaultTypeProperty) as Type;
            }
            set
            {
                base.SetValue(FaultTypeProperty, value);
            }
        }

        #region ITypeFilterProvider Members
        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            bool isAssignable = TypeProvider.IsAssignable(typeof(Exception), type);

            if (throwOnError && !isAssignable)
                throw new Exception(SR.GetString(SR.Error_ExceptionTypeNotException, type, "Type"));

            return isAssignable;
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString(SR.FilterDescription_FaultHandlerActivity);
            }
        }
        #endregion

        #region IDynamicPropertyTypeProvider Members
        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (!String.IsNullOrEmpty(propertyName) && propertyName.Equals("Fault", StringComparison.Ordinal))
                return FaultType;
            else
                return null;
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            return AccessTypes.Read;
        }
        #endregion

        #region Class FaultConverter
        private sealed class FaultConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return false;

                return base.CanConvertFrom(context, sourceType);
            }
        }
        #endregion

        #region Class FaultTypeConverter
        private sealed class FaultTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                object convertedValue = value;

                string stringValue = value as string;
                ITypeProvider typeProvider = context.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (context != null && typeProvider != null && !String.IsNullOrEmpty(stringValue))
                {
                    Type type = typeProvider.GetType(stringValue, false);
                    if (type != null)
                    {
                        ITypeFilterProvider typeFilterProvider = context.Instance as ITypeFilterProvider;
                        if (typeFilterProvider != null)
                            typeFilterProvider.CanFilterType(type, true);

                        convertedValue = type;
                    }
                }
                else if (stringValue != null && stringValue.Length == 0)
                {
                    convertedValue = null;
                }

                return convertedValue;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    Type type = value as Type;
                    if (type != null)
                        return type.FullName;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
        #endregion
    }
}
