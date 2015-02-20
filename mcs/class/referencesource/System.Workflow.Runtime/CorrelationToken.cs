using System;
using System.Xml;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Globalization;

namespace System.Workflow.Runtime
{
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CorrelationToken : DependencyObject, IPropertyValueProvider
    {
        internal static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(CorrelationToken), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false) }));
        internal static readonly DependencyProperty OwnerActivityNameProperty = DependencyProperty.Register("OwnerActivityName", typeof(string), typeof(CorrelationToken), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new TypeConverterAttribute(typeof(PropertyValueProviderTypeConverter)) }));

        // instance properties
        internal static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(ICollection<CorrelationProperty>), typeof(CorrelationToken), new PropertyMetadata(new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        internal static readonly DependencyProperty SubscriptionsProperty = DependencyProperty.Register("Subscriptions", typeof(IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>), typeof(CorrelationToken));
        internal static readonly DependencyProperty InitializedProperty = DependencyProperty.Register("Initialized", typeof(bool), typeof(CorrelationToken), new PropertyMetadata(false, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        public CorrelationToken()
        {
        }

        public CorrelationToken(string name)
        {
            this.Name = name;
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }

        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        public string OwnerActivityName
        {
            get
            {
                return (string)GetValue(OwnerActivityNameProperty);
            }

            set
            {
                SetValue(OwnerActivityNameProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<CorrelationProperty> Properties
        {
            get
            {
                return GetValue(PropertiesProperty) as ICollection<CorrelationProperty>;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Initialized
        {
            get
            {
                return (bool)GetValue(InitializedProperty);
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
                    Activity currentActivity = selectionService.PrimarySelection as Activity;
                    foreach (Activity activity in GetEnclosingCompositeActivities(currentActivity))
                    {
                        string activityId = activity.QualifiedName;
                        if (!names.Contains(activityId))
                            names.Add(activityId);
                    }
                }
            }
            return names;
        }

        public void Initialize(Activity activity, ICollection<CorrelationProperty> propertyValues)
        {
            if (this.Initialized)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CorrelationAlreadyInitialized, this.Name));

            SetValue(PropertiesProperty, propertyValues);

            // fire correlation initialized events
            CorrelationTokenEventArgs eventArgs = new CorrelationTokenEventArgs(this, true);
            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> subscribers = GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (subscribers != null)
            {
                foreach (ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> subscriber in subscribers)
                {
                    subscriber.InvokeDelegate(ContextActivityUtils.ContextActivity(activity), eventArgs, true, false);
                }
            }
            SetValue(InitializedProperty, true);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "CorrelationToken initialized for {0} owner activity {1} ", this.Name, this.OwnerActivityName);
        }

        internal void Uninitialize(Activity activity)
        {
            SetValue(PropertiesProperty, null);

            // fire correlation uninitialized events
            CorrelationTokenEventArgs eventArgs = new CorrelationTokenEventArgs(this, false);
            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> subscribers = GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (subscribers != null)
            {
                ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>[] clonedSubscribers = new ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>[subscribers.Count];
                subscribers.CopyTo(clonedSubscribers, 0);
                foreach (ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> subscriber in clonedSubscribers)
                {
                    subscriber.InvokeDelegate(ContextActivityUtils.ContextActivity(activity), eventArgs, true, false);
                }
            }
            //SetValue(InitializedProperty, false);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "CorrelationToken Uninitialized for {0} owner activity {1}", this.Name, this.OwnerActivityName);
        }

        public void SubscribeForCorrelationTokenInitializedEvent(Activity activity, IActivityEventListener<CorrelationTokenEventArgs> dataChangeListener)
        {
            if (dataChangeListener == null)
                throw new ArgumentNullException("dataChangeListener");
            if (activity == null)
                throw new ArgumentNullException("activity");

            ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> subscriber =
                new ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>(dataChangeListener,
                ContextActivityUtils.ContextActivity(activity), true);

            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> subscriptions = GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (subscriptions == null)
            {
                subscriptions = new List<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>();
                SetValue(SubscriptionsProperty, subscriptions);
            }

            subscriptions.Add(subscriber);
        }

        public void UnsubscribeFromCorrelationTokenInitializedEvent(Activity activity, IActivityEventListener<CorrelationTokenEventArgs> dataChangeListener)
        {
            if (dataChangeListener == null)
                throw new ArgumentNullException("dataChangeListener");
            if (activity == null)
                throw new ArgumentNullException("activity");

            ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> subscriber =
                new ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>(dataChangeListener,
                ContextActivityUtils.ContextActivity(activity), true);

            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> subscriptions = GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (subscriptions != null)
            {
                subscriptions.Remove(subscriber);
            }
        }

        private static IEnumerable GetEnclosingCompositeActivities(Activity startActivity)
        {
            Activity currentActivity = null;
            Stack<Activity> activityStack = new Stack<Activity>();
            activityStack.Push(startActivity);

            while ((currentActivity = activityStack.Pop()) != null)
            {
                if ((typeof(CompositeActivity).IsAssignableFrom(currentActivity.GetType())) && currentActivity.Enabled)
                {
                    yield return currentActivity;
                }
                activityStack.Push(currentActivity.Parent);
            }
            yield break;
        }
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CorrelationTokenCollection : KeyedCollection<string, CorrelationToken>
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "Design has been approved.  This is a false positive. DependencyProperty is an immutable type.")]
        public static readonly DependencyProperty CorrelationTokenCollectionProperty = DependencyProperty.RegisterAttached("CorrelationTokenCollection", typeof(CorrelationTokenCollection), typeof(CorrelationTokenCollection));

        public CorrelationTokenCollection()
        {
        }

        public CorrelationToken GetItem(string key)
        {
            return this[key];
        }

        protected override string GetKeyForItem(CorrelationToken item)
        {
            return item.Name;
        }
        protected override void ClearItems()
        {
            base.ClearItems();
        }
        protected override void InsertItem(int index, CorrelationToken item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }
        protected override void SetItem(int index, CorrelationToken item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            base.SetItem(index, item);
        }

        internal static void UninitializeCorrelationTokens(Activity activity)
        {
            CorrelationTokenCollection collection = activity.GetValue(CorrelationTokenCollectionProperty) as CorrelationTokenCollection;
            if (collection != null)
            {
                foreach (CorrelationToken correlator in collection)
                {
                    correlator.Uninitialize(activity);
                }
            }
        }

        public static CorrelationToken GetCorrelationToken(Activity activity, string correlationTokenName, string ownerActivityName)
        {
            if (null == correlationTokenName)
                throw new ArgumentNullException("correlationTokenName");
            if (null == ownerActivityName)
                throw new ArgumentNullException("ownerActivityName");
            if (null == activity)
                throw new ArgumentNullException("activity");

            Activity contextActivity = ContextActivityUtils.ContextActivity(activity);
            Activity owner = null;

            if (!String.IsNullOrEmpty(ownerActivityName))
            {
                while (contextActivity != null)
                {
                    owner = contextActivity.GetActivityByName(ownerActivityName, true);
                    if (owner != null)
                        break;
                    contextActivity = ContextActivityUtils.ParentContextActivity(contextActivity);
                }

                if (owner == null)
                    owner = Helpers.ParseActivityForBind(activity, ownerActivityName);
            }

            if (owner == null)
                throw new InvalidOperationException(ExecutionStringManager.OwnerActivityMissing);

            CorrelationTokenCollection collection = owner.GetValue(CorrelationTokenCollectionProperty) as CorrelationTokenCollection;
            if (collection == null)
            {
                collection = new CorrelationTokenCollection();
                owner.SetValue(CorrelationTokenCollection.CorrelationTokenCollectionProperty, collection);
            }

            if (!collection.Contains(correlationTokenName))
            {
                collection.Add(new CorrelationToken(correlationTokenName));
            }
            return collection[correlationTokenName];
        }
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CorrelationProperty
    {
        private object value = null;
        private string name;

        public CorrelationProperty(string name, object value)
        {
            if (null == name)
            {
                throw new ArgumentNullException("name");
            }

            this.name = name;
            this.value = value;
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CorrelationTokenEventArgs : EventArgs
    {
        CorrelationToken correlator;
        bool initialized;

        internal CorrelationTokenEventArgs(CorrelationToken correlator, bool initialized)
        {
            this.correlator = correlator;
            this.initialized = initialized;
        }

        public bool IsInitializing
        {
            get
            {
                return this.initialized;
            }
        }

        public CorrelationToken CorrelationToken
        {
            get
            {
                return this.correlator;
            }
        }
    }
}
