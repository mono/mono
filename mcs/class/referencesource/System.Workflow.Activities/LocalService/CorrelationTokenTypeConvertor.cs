using System;
using System.Xml;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Globalization;

namespace System.Workflow.Activities
{
    internal sealed class CorrelationTokenTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            object convertedValue = null;
            string correlatorName = value as String;
            if (!String.IsNullOrEmpty(correlatorName))
            {
                foreach (object obj in GetStandardValues(context))
                {
                    CorrelationToken correlator = obj as CorrelationToken;
                    if (correlator != null && correlator.Name == correlatorName)
                    {
                        convertedValue = correlator;
                        break;
                    }
                }

                if (convertedValue == null)
                    convertedValue = new CorrelationToken(correlatorName);
            }

            return convertedValue;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            object convertedValue = null;
            CorrelationToken correlator = value as CorrelationToken;
            if (destinationType == typeof(string) && correlator != null)
                convertedValue = correlator.Name;
            return convertedValue;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            Activity activity = context.Instance as Activity;
            if (activity != null)
            {
                foreach (Activity preceedingActivity in GetPreceedingActivities(activity))
                {
                    PropertyDescriptor correlatorProperty = TypeDescriptor.GetProperties(preceedingActivity)["CorrelationToken"] as PropertyDescriptor;
                    if (correlatorProperty != null)
                    {
                        CorrelationToken correlator = correlatorProperty.GetValue(preceedingActivity) as CorrelationToken;
                        if (correlator != null && !values.Contains(correlator))
                            values.Add(correlator);
                    }
                }
            }
            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = base.GetProperties(context, value, attributes);
            ArrayList props = new ArrayList(properties);
            return new PropertyDescriptorCollection((PropertyDescriptor[])props.ToArray(typeof(PropertyDescriptor)));
        }

        // 
        private IEnumerable GetPreceedingActivities(Activity startActivity)
        {
            Activity currentActivity = null;
            Stack<Activity> activityStack = new Stack<Activity>();
            activityStack.Push(startActivity);

            while ((currentActivity = activityStack.Pop()) != null)
            {
                if (currentActivity.Parent != null)
                {
                    foreach (Activity siblingActivity in currentActivity.Parent.Activities)
                    {
                        if (siblingActivity == currentActivity)
                            break;

                        if (siblingActivity.Enabled)
                        {
                            if (siblingActivity is CompositeActivity)
                            {
                                foreach (Activity containedActivity in GetContainedActivities((CompositeActivity)siblingActivity))
                                    yield return containedActivity;
                            }
                            else
                            {
                                yield return siblingActivity;
                            }
                        }
                    }
                }
                activityStack.Push(currentActivity.Parent);
            }
            yield break;
        }

        private IEnumerable GetContainedActivities(CompositeActivity activity)
        {
            if (!activity.Enabled)
                yield break;

            foreach (Activity containedActivity in activity.Activities)
            {
                if (containedActivity is CompositeActivity)
                {
                    foreach (Activity nestedActivity in GetContainedActivities((CompositeActivity)containedActivity))
                    {
                        if (nestedActivity.Enabled)
                            yield return nestedActivity;
                    }
                }
                else
                {
                    if (containedActivity.Enabled)
                        yield return containedActivity;
                }
            }
            yield break;
        }
    }
}
