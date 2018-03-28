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
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ContextToken : DependencyObject, IPropertyValueProvider
    {
        public const string RootContextName = "(RootContext)";

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string),
            typeof(ContextToken),
            new PropertyMetadata(null,
            DependencyPropertyOptions.Metadata,
            new Attribute[] { new BrowsableAttribute(false) }));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty OwnerActivityNameProperty =
            DependencyProperty.Register("OwnerActivityName",
            typeof(string),
            typeof(ContextToken),
            new PropertyMetadata(null,
            DependencyPropertyOptions.Metadata,
            new Attribute[] { new TypeConverterAttribute(typeof(PropertyValueProviderTypeConverter)) }));

        public ContextToken()
        {
            this.Name = ContextToken.RootContextName;
        }

        public ContextToken(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }
            this.Name = name;
        }

        [Browsable(false)]
        [DefaultValue(null)]
        [SR2Description(SR2DescriptionAttribute.ContextToken_Name_Description)]
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
        [SR2Description(SR2DescriptionAttribute.ContextToken_OwnerActivityName_Description)]
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

        internal bool IsRootContext
        {
            get
            {
                if (!string.IsNullOrEmpty(this.OwnerActivityName))
                {
                    return false;
                }
                if (string.Compare(this.Name, ContextToken.RootContextName, StringComparison.Ordinal) != 0)
                {
                    return false;
                }
                return true;
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

        internal static ReceiveContext GetReceiveContext(Activity activity,
            string contextName,
            string ownerActivityName)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(contextName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("contextToken",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
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

            if (owner == null)
            {
                owner = Helpers.ParseActivityForBind(activity, ownerActivityName);
            }

            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            ReceiveContextCollection collection =
                owner.GetValue(ReceiveContextCollection.ReceiveContextCollectionProperty) as ReceiveContextCollection;
            if (collection == null)
            {
                return null;
            }

            if (!collection.Contains(contextName))
            {
                return null;
            }

            ReceiveContext receiveContext = collection[contextName];
            receiveContext.EnsureInitialized(owner.ContextGuid);

            return receiveContext;
        }

        internal static ReceiveContext GetRootReceiveContext(Activity activity)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }

            Activity contextActivity = activity.ContextActivity;
            if (contextActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            Activity owner = contextActivity.RootActivity;
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            ReceiveContextCollection collection =
                owner.GetValue(ReceiveContextCollection.ReceiveContextCollectionProperty) as ReceiveContextCollection;
            if (collection == null)
            {
                return null;
            }

            if (!collection.Contains(ContextToken.RootContextName))
            {
                return null;
            }

            ReceiveContext receiveContext = collection[ContextToken.RootContextName];
            receiveContext.EnsureInitialized(owner.ContextGuid);

            return receiveContext;
        }

        internal static void Register(ReceiveActivity activity, Guid workflowId)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }

            ContextToken contextToken = activity.ContextToken;

            if (contextToken == null)
            {
                RegisterRootReceiveContext(activity, workflowId);
            }
            else if (contextToken.IsRootContext)
            {
                RegisterRootReceiveContext(activity, workflowId);
            }
            else
            {
                RegisterReceiveContext(activity, workflowId, contextToken.Name, contextToken.OwnerActivityName);
            }
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

        static void RegisterReceiveContext(ReceiveActivity activity,
            Guid workflowId,
            string contextName,
            string ownerActivityName)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(contextName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("contextName",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
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

            if (owner == null)
            {
                owner = Helpers.ParseActivityForBind(activity, ownerActivityName);
            }

            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            ReceiveContextCollection collection =
                owner.GetValue(ReceiveContextCollection.ReceiveContextCollectionProperty) as ReceiveContextCollection;
            if (collection == null)
            {
                collection = new ReceiveContextCollection();
                owner.SetValue(ReceiveContextCollection.ReceiveContextCollectionProperty, collection);
            }

            if (!collection.Contains(contextName))
            {
                collection.Add(new ReceiveContext(contextName, workflowId, false));
            }
        }

        static void RegisterRootReceiveContext(Activity activity, Guid workflowId)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }

            Activity contextActivity = activity.ContextActivity;
            if (contextActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            Activity owner = contextActivity.RootActivity;
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ContextOwnerActivityMissing)));
            }

            ReceiveContextCollection collection =
                owner.GetValue(ReceiveContextCollection.ReceiveContextCollectionProperty) as ReceiveContextCollection;
            if (collection == null)
            {
                collection = new ReceiveContextCollection();
                owner.SetValue(ReceiveContextCollection.ReceiveContextCollectionProperty, collection);
            }

            if (!collection.Contains(ContextToken.RootContextName))
            {
                collection.Add(new ReceiveContext(ContextToken.RootContextName, workflowId, true));
            }
        }
    }
}
