namespace System.Workflow.ComponentModel
{
    using System;
    using System.Xml;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    using System.ComponentModel;
    using System.Transactions;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.CodeDom;
    using System.Collections.Specialized;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [Browsable(true)]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowTransactionOptions : DependencyObject
    {
        public static readonly DependencyProperty TimeoutDurationProperty = DependencyProperty.Register("TimeoutDuration", typeof(TimeSpan), typeof(WorkflowTransactionOptions), new PropertyMetadata(new TimeSpan(0, 0, 30), DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty IsolationLevelProperty = DependencyProperty.Register("IsolationLevel", typeof(IsolationLevel), typeof(WorkflowTransactionOptions), new PropertyMetadata(IsolationLevel.Serializable, DependencyPropertyOptions.Metadata));

        [SRDescription(SR.TimeoutDescr)]
        [SRCategory(SR.Activity)]
        [MergableProperty(false)]
        [DefaultValue(typeof(TimeSpan), "0:0:30")]
        [TypeConverter(typeof(TimeoutDurationConverter))]
        public TimeSpan TimeoutDuration
        {
            get
            {
                return (TimeSpan)base.GetValue(TimeoutDurationProperty);
            }
            set
            {
                base.SetValue(TimeoutDurationProperty, value);
            }
        }

        [SRDescription(SR.IsolationLevelDescr)]
        [SRCategory(SR.Activity)]
        [MergableProperty(false)]
        public IsolationLevel IsolationLevel
        {
            get
            {
                return (IsolationLevel)base.GetValue(IsolationLevelProperty);
            }
            set
            {
                base.SetValue(IsolationLevelProperty, value);
            }
        }
    }

    internal sealed class TimeoutDurationConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is TimeSpan)
            {
                TimeSpan timespan = (TimeSpan)value;
                return timespan.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            TimeSpan parsedTimespan = TimeSpan.Zero;
            string timeSpan = value as string;
            if (!String.IsNullOrEmpty(timeSpan))
            {
                //If this fails then an exception is thrown and the property set would fail
                try
                {
                    parsedTimespan = TimeSpan.Parse(timeSpan, CultureInfo.InvariantCulture);
                }
                catch
                {
                }

                if (parsedTimespan.Ticks < 0)
                {
                    throw new Exception(string.Format(System.Globalization.CultureInfo.CurrentCulture, SR.GetString(SR.Error_NegativeValue), value.ToString(), "TimeoutDuration"));
                }
            }

            return parsedTimespan;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList standardValuesCollection = new ArrayList();
            standardValuesCollection.Add(new TimeSpan(0, 0, 0));
            standardValuesCollection.Add(new TimeSpan(0, 0, 15));
            standardValuesCollection.Add(new TimeSpan(0, 1, 0));
            return new StandardValuesCollection(standardValuesCollection);
        }
    }
}
