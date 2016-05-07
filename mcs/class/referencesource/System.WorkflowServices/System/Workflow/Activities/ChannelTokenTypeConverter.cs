//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Workflow.Activities
{
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

    internal sealed class ChannelTokenTypeConverter : ExpandableObjectConverter
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
            string endpointName = value as String;
            if (!String.IsNullOrEmpty(endpointName))
            {
                foreach (object obj in GetStandardValues(context))
                {
                    ChannelToken endpoint = obj as ChannelToken;
                    if (endpoint != null && endpoint.Name == endpointName)
                    {
                        convertedValue = endpoint;
                        break;
                    }
                }

                if (convertedValue == null)
                {
                    convertedValue = new ChannelToken(endpointName);
                }
            }

            return convertedValue;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            object convertedValue = null;
            ChannelToken endpoint = value as ChannelToken;
            if (destinationType == typeof(string) && endpoint != null)
            {
                convertedValue = endpoint.Name;
            }
            return convertedValue;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = base.GetProperties(context, value, attributes);
            ArrayList props = new ArrayList(properties);
            return new PropertyDescriptorCollection((PropertyDescriptor[]) props.ToArray(typeof(PropertyDescriptor)));
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            Activity activity = context.Instance as Activity;
            if (activity != null)
            {
                foreach (Activity preceedingActivity in GetPreceedingActivities(activity))
                {
                    PropertyDescriptor endpointProperty = TypeDescriptor.GetProperties(preceedingActivity)["ChannelToken"] as PropertyDescriptor;
                    if (endpointProperty != null)
                    {
                        ChannelToken endpoint = endpointProperty.GetValue(preceedingActivity) as ChannelToken;
                        if (endpoint != null && !values.Contains(endpoint))
                        {
                            values.Add(endpoint);
                        }
                    }
                }
            }
            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private IEnumerable GetContainedActivities(CompositeActivity activity)
        {
            if (!activity.Enabled)
            {
                yield break;
            }

            foreach (Activity containedActivity in activity.Activities)
            {
                if (containedActivity.Enabled)
                {
                    yield return containedActivity;

                    if (containedActivity is CompositeActivity)
                    {
                        foreach (Activity nestedActivity in GetContainedActivities((CompositeActivity) containedActivity))
                        {
                            if (nestedActivity.Enabled)
                            {
                                yield return nestedActivity;
                            }
                        }
                    }
                }
            }
            yield break;
        }

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
                        {
                            continue;
                        }

                        if (siblingActivity.Enabled)
                        {
                            yield return siblingActivity;

                            if (siblingActivity is CompositeActivity)
                            {
                                foreach (Activity containedActivity in GetContainedActivities((CompositeActivity) siblingActivity))
                                {
                                    yield return containedActivity;
                                }
                            }
                        }
                    }
                }
                activityStack.Push(currentActivity.Parent);
            }
            yield break;
        }
    }
}
