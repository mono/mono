//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [TypeConverter(typeof(ChannelTokenTypeConverter))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ChannelToken : DependencyObject, IPropertyValueProvider
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty EndpointNameProperty =
            DependencyProperty.Register("EndpointName",
            typeof(string),
            typeof(ChannelToken),
            new PropertyMetadata(null));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string),
            typeof(ChannelToken),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata,
            new Attribute[] { new BrowsableAttribute(false) }));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty OwnerActivityNameProperty =
            DependencyProperty.Register("OwnerActivityName",
            typeof(string),
            typeof(ChannelToken),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata,
            new Attribute[] { new TypeConverterAttribute(typeof(PropertyValueProviderTypeConverter)) }));

        public ChannelToken()
        {
        }

        internal ChannelToken(string name)
        {
            this.Name = name;
        }

        [DefaultValue(null)]
        [SR2Description(SR2DescriptionAttribute.ChannelToken_EndpointName_Description)]
        public string EndpointName
        {
            get
            {
                return (string) GetValue(EndpointNameProperty);
            }

            set
            {
                SetValue(EndpointNameProperty, value);
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        [SR2Description(SR2DescriptionAttribute.ChannelToken_Name_Description)]
        public string Name
        {
            get
            {
                return (string) GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }

        [DefaultValue(null)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [SR2Description(SR2DescriptionAttribute.ChannelToken_OwnerActivityName_Description)]
        public string OwnerActivityName
        {
            get
            {
                return (string) GetValue(OwnerActivityNameProperty);
            }

            set
            {
                SetValue(OwnerActivityNameProperty, value);
            }
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection names = new StringCollection();

            if (string.Equals(context.PropertyDescriptor.Name, "OwnerActivityName", StringComparison.Ordinal))
            {
                ISelectionService selectionService = context.GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null && selectionService.SelectionCount == 1 && selectionService.PrimarySelection is Activity)
                {
                    // add empty string as an option
                    //
                    names.Add(string.Empty);

                    Activity currentActivity = selectionService.PrimarySelection as Activity;

                    foreach (Activity activity in GetEnclosingCompositeActivities(currentActivity))
                    {
                        string activityId = activity.QualifiedName;
                        if (!names.Contains(activityId))
                        {
                            names.Add(activityId);
                        }
                    }
                }
            }
            return names;
        }

        internal static LogicalChannel GetLogicalChannel(Activity activity,
            ChannelToken endpoint,
            Type contractType)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            return GetLogicalChannel(activity, endpoint.Name, endpoint.OwnerActivityName, contractType);
        }

        internal static LogicalChannel GetLogicalChannel(Activity activity,
            string name,
            string ownerActivityName,
            Type contractType)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            Activity contextActivity = activity.ContextActivity;
            Activity owner = null;

            if (contextActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            if (string.IsNullOrEmpty(ownerActivityName))
            {
                owner = contextActivity.RootActivity;
            }
            else
            {
                while (contextActivity != null)
                {
                    owner = contextActivity.GetActivityByName(ownerActivityName, true);
                    if (owner != null)
                    {
                        break;
                    }

                    contextActivity = contextActivity.Parent;
                    if (contextActivity != null)
                    {
                        contextActivity = contextActivity.ContextActivity;
                    }
                }
            }

            if (owner == null && !string.IsNullOrEmpty(ownerActivityName))
            {
                owner = Helpers.ParseActivityForBind(activity, ownerActivityName);
            }

            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            LogicalChannel logicalChannel = null;

            LogicalChannelCollection collection =
                owner.GetValue(LogicalChannelCollection.LogicalChannelCollectionProperty) as LogicalChannelCollection;
            if (collection == null)
            {
                collection = new LogicalChannelCollection();
                owner.SetValue(LogicalChannelCollection.LogicalChannelCollectionProperty, collection);

                logicalChannel = new LogicalChannel(name, contractType);
                collection.Add(logicalChannel);
            }
            else if (!collection.Contains(name))
            {
                logicalChannel = new LogicalChannel(name, contractType);
                collection.Add(logicalChannel);
            }
            else
            {
                logicalChannel = collection[name];
            }

            if (logicalChannel.ContractType != contractType)
            {
                logicalChannel = null;
            }

            return logicalChannel;
        }

        internal static LogicalChannel Register(Activity activity,
            ChannelToken endpoint,
            Type contractType)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            LogicalChannel logicalChannel = GetLogicalChannel(activity, endpoint, contractType);
            if (logicalChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_FailedToRegisterChannel, endpoint.Name)));
            }

            return logicalChannel;
        }

        private static IEnumerable GetEnclosingCompositeActivities(Activity startActivity)
        {
            Activity currentActivity = null;
            Stack<Activity> activityStack = new Stack<Activity>();
            activityStack.Push(startActivity);

            while ((currentActivity = activityStack.Pop()) != null)
            {
                if (currentActivity.Enabled)
                {
                    yield return currentActivity;
                }
                activityStack.Push(currentActivity.Parent);
            }
            yield break;
        }
    }
}
