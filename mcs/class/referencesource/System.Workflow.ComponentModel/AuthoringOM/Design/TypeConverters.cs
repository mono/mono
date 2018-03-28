namespace System.Workflow.ComponentModel.Design
{
    #region Imports

    using System;
    using System.Drawing.Design;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.CodeDom;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Text;

    #endregion

    #region Class ConditionDeclTypeConverter
    internal sealed class ConditionTypeConverter : TypeConverter
    {
        internal static readonly Type RuleConditionReferenceType = null;
        internal static readonly Type RuleDefinitionsType = null;
        internal static readonly Type CodeConditionType = null;
        internal static DependencyProperty DeclarativeConditionDynamicProp = null;
        private Hashtable conditionDecls = new Hashtable();

        static ConditionTypeConverter()
        {
            RuleConditionReferenceType = Type.GetType("System.Workflow.Activities.Rules.RuleDefinitions, " + AssemblyRef.ActivitiesAssemblyRef);
            RuleDefinitionsType = Type.GetType("System.Workflow.Activities.Rules.RuleConditionReference, " + AssemblyRef.ActivitiesAssemblyRef);
            CodeConditionType = Type.GetType("System.Workflow.Activities.CodeCondition, " + AssemblyRef.ActivitiesAssemblyRef);
        
            DeclarativeConditionDynamicProp = (DependencyProperty)RuleConditionReferenceType.GetField("RuleDefinitionsProperty").GetValue(null);
        }

        public ConditionTypeConverter()
        {
            string key = CodeConditionType.FullName;
            object[] attributes = CodeConditionType.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (attributes != null && attributes.Length > 0 && attributes[0] is DisplayNameAttribute)
                key = ((DisplayNameAttribute)attributes[0]).DisplayName;
            this.conditionDecls.Add(key, CodeConditionType);

            key = RuleDefinitionsType.FullName;
            attributes = RuleDefinitionsType.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (attributes != null && attributes.Length > 0 && attributes[0] is DisplayNameAttribute)
                key = ((DisplayNameAttribute)attributes[0]).DisplayName;
            this.conditionDecls.Add(key, RuleDefinitionsType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                if (((string)value).Length == 0 || ((string)value) == SR.GetString(SR.NullConditionExpression))
                    return null;
                else
                    return Activator.CreateInstance(this.conditionDecls[value] as Type);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                return SR.GetString(SR.NullConditionExpression);

            object convertedValue = null;
            if (destinationType == typeof(string) && value is ActivityCondition)
            {
                foreach (DictionaryEntry conditionTypeEntry in this.conditionDecls)
                {
                    if ((object)value.GetType() == conditionTypeEntry.Value)
                    {
                        convertedValue = conditionTypeEntry.Key;
                        break;
                    }
                }
            }

            if (convertedValue == null)
                convertedValue = base.ConvertTo(context, culture, value, destinationType);

            return convertedValue;
        }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList conditionDeclList = new ArrayList();

            conditionDeclList.Add(null);
            foreach (object key in this.conditionDecls.Keys)
            {
                Type declType = this.conditionDecls[key] as Type;
                conditionDeclList.Add(Activator.CreateInstance(declType));
            }
            return new StandardValuesCollection((ActivityCondition[])conditionDeclList.ToArray(typeof(ActivityCondition)));
        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection props = new PropertyDescriptorCollection(new PropertyDescriptor[] { });

            TypeConverter typeConverter = TypeDescriptor.GetConverter(value.GetType());
            if (typeConverter != null && typeConverter.GetType() != GetType() && typeConverter.GetPropertiesSupported())
            {
                return typeConverter.GetProperties(context, value, attributes);
            }
            else
            {
                IComponent component = PropertyDescriptorUtils.GetComponent(context);
                if (component != null)
                    props = PropertyDescriptorFilter.FilterProperties(component.Site, value, TypeDescriptor.GetProperties(value, new Attribute[] { BrowsableAttribute.Yes }));
            }

            return props;
        }
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    #endregion

    #region ActivityBindTypeConverter
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityBindTypeConverter : TypeConverter
    {
        public ActivityBindTypeConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
            GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);
            if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                return actualConverter.CanConvertFrom(actualContext, sourceType);
            else if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string) && context != null && context.PropertyDescriptor != null)
            {
                ActivityBind activityBind = context.PropertyDescriptor.GetValue(context.Instance) as ActivityBind;
                if (activityBind != null)
                    return true;
            }

            ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
            GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);
            if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                return actualConverter.CanConvertTo(actualContext, destinationType);
            else if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
        {
            string value = valueToConvert as string;
            if (value == null)
                return base.ConvertFrom(context, culture, valueToConvert);

            //Trim the value
            value = value.Trim();

            //Check if format is "Activity=, Path="
            string[] splitParts = Parse(value);
            object convertedValue = (splitParts.Length == 2) ? new ActivityBind(splitParts[0], splitParts[1]) : null;

            if (convertedValue == null && (context == null || context.PropertyDescriptor == null))
                return base.ConvertFrom(context, culture, valueToConvert);

            //For string's only if they begin and end with " we will use the type converter
            if (convertedValue == null)
            {
                ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
                GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);
                if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter) && actualConverter.CanConvertFrom(actualContext, typeof(string)))
                    convertedValue = actualConverter.ConvertFrom(actualContext, culture, value);
                else
                    convertedValue = valueToConvert;
            }

            return convertedValue;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            string convertedValue = null;

            ActivityBind activityBind = value as ActivityBind;
            if (activityBind != null)
            {
                Activity activity = PropertyDescriptorUtils.GetComponent(context) as Activity;
                activity = (activity != null) ? Helpers.ParseActivityForBind(activity, activityBind.Name) : null;
                convertedValue = String.Format(CultureInfo.InvariantCulture, ("Activity={0}, Path={1}"), (activity != null) ? activity.QualifiedName : activityBind.Name, activityBind.Path);
            }
            else
            {
                ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
                GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);
                if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter) && actualConverter.CanConvertTo(actualContext, destinationType))
                    convertedValue = actualConverter.ConvertTo(actualContext, culture, value, destinationType) as string;
                else
                    convertedValue = base.ConvertTo(context, culture, value, destinationType) as string;
            }

            return convertedValue;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            bool propertiesSupported = false;
            if (context != null && context.PropertyDescriptor != null)
            {
                ActivityBind activityBind = context.PropertyDescriptor.GetValue(context.Instance) as ActivityBind;
                if (activityBind != null)
                {
                    propertiesSupported = true;
                }
                else
                {
                    ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
                    GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);
                    if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                        propertiesSupported = actualConverter.GetPropertiesSupported(actualContext);
                }
            }

            return propertiesSupported;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            ActivityBind activityBind = value as ActivityBind;
            if (activityBind != null && context != null)
            {
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(value, new Attribute[] { BrowsableAttribute.Yes });

                PropertyDescriptor activityDescriptor = props["Name"];
                if (activityDescriptor != null)
                    properties.Add(new ActivityBindNamePropertyDescriptor(context, activityDescriptor));

                PropertyDescriptor pathDescriptor = props["Path"];
                if (pathDescriptor != null)
                    properties.Add(new ActivityBindPathPropertyDescriptor(context, pathDescriptor));
            }
            else if (context != null && context.PropertyDescriptor != null)
            {
                ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
                GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);
                if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                    properties.AddRange(actualConverter.GetProperties(actualContext, value, attributes));
            }

            return new PropertyDescriptorCollection((PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor)));
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList valuesList = new ArrayList();
            if (context == null || context.PropertyDescriptor == null)
                return new StandardValuesCollection(new ArrayList());

            //If the property type supports exclusive values then we need to add them to list
            ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
            GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);

            if (actualConverter != null && actualConverter.GetStandardValuesSupported(actualContext) && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                valuesList.AddRange(actualConverter.GetStandardValues(actualContext));

            return new StandardValuesCollection(valuesList.ToArray()); 
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //We do not support standard values through the 
            bool standardValuesSupported = false;
            if (context != null && context.PropertyDescriptor != null)
            {
                object existingPropertyValue = (context.Instance != null) ? context.PropertyDescriptor.GetValue(context.Instance) : null;
                if (!(existingPropertyValue is ActivityBind))
                {
                    ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
                    GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);

                    if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                        standardValuesSupported = actualConverter.GetStandardValuesSupported(actualContext);
                }
            }

            return standardValuesSupported;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            bool exclusiveValuesSupported = false;
            if (context != null && context.PropertyDescriptor != null)
            {
                object existingPropertyValue = (context.Instance != null) ? context.PropertyDescriptor.GetValue(context.Instance) : null;
                if (!(existingPropertyValue is ActivityBind))
                {
                    ITypeDescriptorContext actualContext = null; TypeConverter actualConverter = null;
                    GetActualTypeConverterAndContext(context, out actualConverter, out actualContext);

                    if (actualConverter != null && actualConverter.GetType() != typeof(ActivityBindTypeConverter))
                        exclusiveValuesSupported = actualConverter.GetStandardValuesExclusive(actualContext);
                }
            }

            return exclusiveValuesSupported;
        }

        #region Helper Methods
        private void GetActualTypeConverterAndContext(ITypeDescriptorContext currentContext, out TypeConverter realTypeConverter, out ITypeDescriptorContext realContext)
        {
            //The following case covers the scenario where we have users writting custom property descriptors in which they have returned custom type converters
            //In such cases we should honor the type converter returned by property descriptor only if it is not a ActivityBindTypeConverter
            //If it is ActivityBindTypeConveter then we should lookup the converter based on Property type
            //Please be care ful when you change this code as it will break ParameterInfoBasedPropertyDescriptor
            realContext = currentContext;
            realTypeConverter = null;

            if (currentContext != null && currentContext.PropertyDescriptor != null)
            {
                realTypeConverter = TypeDescriptor.GetConverter(currentContext.PropertyDescriptor.PropertyType);

                ActivityBindPropertyDescriptor activityBindPropertyDescriptor = currentContext.PropertyDescriptor as ActivityBindPropertyDescriptor;
                if (activityBindPropertyDescriptor != null &&
                    activityBindPropertyDescriptor.RealPropertyDescriptor != null &&
                    activityBindPropertyDescriptor.RealPropertyDescriptor.Converter != null &&
                    activityBindPropertyDescriptor.RealPropertyDescriptor.Converter.GetType() != typeof(ActivityBindTypeConverter))
                {
                    realTypeConverter = activityBindPropertyDescriptor.RealPropertyDescriptor.Converter;
                    realContext = new TypeDescriptorContext(currentContext, activityBindPropertyDescriptor.RealPropertyDescriptor, currentContext.Instance);
                }
            }
        }

        private string[] Parse(string value)
        {
            string[] splitValues = value.Split(new char[] { ',' }, 2); //array could be multi-dimentional
            if (splitValues.Length == 2)
            {
                string activityIDMatch = "Activity=";
                string pathMatch = "Path=";

                string activityID = splitValues[0].Trim();
                string path = splitValues[1].Trim();
                if (activityID.StartsWith(activityIDMatch, StringComparison.OrdinalIgnoreCase) &&
                    path.StartsWith(pathMatch, StringComparison.OrdinalIgnoreCase))
                {
                    activityID = activityID.Substring(activityIDMatch.Length);
                    path = path.Substring(pathMatch.Length);
                    return new String[] { activityID, path };
                }
            }

            return new string[] { };
        }
        #endregion
    }
    #endregion

    #region ActivityBindPathTypeConverter
    internal sealed class ActivityBindPathTypeConverter : PropertyValueProviderTypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return new StringConverter().CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return new StringConverter().CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new StringConverter().ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return new StringConverter().ConvertTo(context, culture, value, destinationType);
        }
    }
    #endregion
}
