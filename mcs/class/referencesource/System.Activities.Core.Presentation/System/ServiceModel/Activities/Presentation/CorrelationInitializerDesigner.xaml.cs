//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Input;
    using System.Globalization;
    using System.Collections.Generic;

    internal partial class CorrelationInitializerDesigner 
    {
        DataGridHelper correlationInitializerDGHelper;

        const string CorrelationInitializersKey = "CorrelationInitializers";

        public static readonly DependencyProperty ActivityProperty = DependencyProperty.Register(
            "Activity", 
            typeof(ModelItem), 
            typeof(CorrelationInitializerDesigner), 
            new UIPropertyMetadata(OnActivityChanged));

        static readonly ICommand AddNewInitializerCommand = new RoutedCommand();

        public CorrelationInitializerDesigner()
        {
            this.InitializeComponent();
        }

        public ModelItem Activity
        {
            get { return (ModelItem)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        ModelItemCollection CorrelationInitializers
        {
            get { return this.Activity.Properties[CorrelationInitializersKey].Collection; }
        }

        protected override void OnInitialized(EventArgs args)
        {
            base.OnInitialized(args);

            this.CommandBindings.Add(new CommandBinding(AddNewInitializerCommand, this.OnAddNewInitializerExecuted));

            //create data grid helper
            this.correlationInitializerDGHelper = new DataGridHelper(this.correlationInitializers, this);
            this.correlationInitializerDGHelper.ShowValidationErrorAsToolTip = true;
            this.correlationInitializerDGHelper.AddNewRowContent = (string)this.FindResource("addNewInitializer");
            this.correlationInitializerDGHelper.AddNewRowCommand = CorrelationInitializerDesigner.AddNewInitializerCommand;            
        }

        void OnAddNewInitializerExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var initializer = (CanUseQueryCorrelationInitializer(this.Activity) ? 
                (CorrelationInitializer)new QueryCorrelationInitializer() : (CorrelationInitializer)new ContextCorrelationInitializer());
            var result = this.CorrelationInitializers.Add(initializer);
            var wrapper = new CorrelationInitializerEntry(result);
            this.correlationInitializerDGHelper.Source<IList>().Add(wrapper);
            this.correlationInitializerDGHelper.BeginRowEdit(wrapper);
        }

        static bool CanUseQueryCorrelationInitializer(ModelItem activity)
        {
            bool result = true;
            if (null != activity)
            {
                if (activity.IsAssignableFrom<Receive>() || activity.IsAssignableFrom<Send>())
                {
                    ModelItem serializationOption;
                    activity.TryGetPropertyValue(out serializationOption, "SerializerOption");
                    result = SerializerOption.XmlSerializer != (SerializerOption)serializationOption.GetCurrentValue();
                }
                else if (activity.IsAssignableFrom<SendReply>() || activity.IsAssignableFrom<ReceiveReply>())
                {
                    ModelItem request;
                    activity.TryGetPropertyValue(out request, "Request");
                    result = CanUseQueryCorrelationInitializer(request);
                }
            }
            return result;
        }

        void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (CorrelationInitializerEntry entry in e.OldItems)
                    {
                        this.CorrelationInitializers.Remove(entry.ReflectedObject);
                        entry.Dispose();
                    }
                    break;
            }
        }


        internal void CleanupObjectMap()
        {
        }

        void OnActivityChanged()
        {
            if (null != this.Activity)
            {
                var source = new ObservableCollection<CorrelationInitializerEntry>();

                foreach (var entry in this.CorrelationInitializers)
                {
                    var wrapper = new CorrelationInitializerEntry(entry);
                    source.Add(wrapper);
                }

                this.correlationInitializers.ItemsSource = source;
                source.CollectionChanged += this.OnDataCollectionChanged;
            }
        }

        static void OnActivityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var activity = e.NewValue as ModelItem;
            if (null != activity && !activity.IsMessagingActivity())
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(activity.ItemType.FullName));
            }
            ((CorrelationInitializerDesigner)sender).OnActivityChanged();
        }

        void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }

        internal sealed class CorrelationInitializerEntry : DesignObjectWrapper
        {
            internal static readonly string HandleProperty = "CorrelationHandle";
            internal static readonly string CorrelationTypeProperty = "CorrelationType";
            internal static readonly string MessageQuerySetModelPropertyProperty = "MessageQuerySet";

            static readonly string[] Properties = new string[] { HandleProperty, CorrelationTypeProperty, MessageQuerySetModelPropertyProperty };

            #region Initialize type properties code
            public static PropertyDescriptorData[] InitializeTypeProperties()
            {
                return new PropertyDescriptorData[]
                {
                    new PropertyDescriptorData()
                    {
                        PropertyName = HandleProperty,
                        PropertyType = typeof(InArgument),
                        PropertySetter = (instance, newValue) =>
                            {
                                ((CorrelationInitializerEntry)instance).SetHandle(newValue);
                            },
                        PropertyGetter = (instance) => (((CorrelationInitializerEntry)instance).GetHandle()),
                    },
                    new PropertyDescriptorData()
                    {
                        PropertyName = CorrelationTypeProperty,
                        PropertyType = typeof(Type),
                        PropertyValidator = (instance, newValue, errors) => (((CorrelationInitializerEntry)instance).ValidateCorrelationType(newValue, errors)),
                        PropertySetter = (instance, newValue) =>
                            {
                                ((CorrelationInitializerEntry)instance).SetCorrelationType(newValue);                                
                            },
                        PropertyGetter = (instance) => (((CorrelationInitializerEntry)instance).GetCorrelationType()),
                    },
                    new PropertyDescriptorData()
                    {
                        PropertyName = MessageQuerySetModelPropertyProperty,
                        PropertyType = typeof(ModelProperty),
                        PropertyGetter = (instance) => (((CorrelationInitializerEntry)instance).GetMessageQuerySetModelProperty()),
                    },
                };
            }
            #endregion


            public CorrelationInitializerEntry()
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            public CorrelationInitializerEntry(ModelItem initializer) : base(initializer)
            {
            }

            protected override string AutomationId
            {
                get { return ((ModelItemCollection)this.ReflectedObject.Parent).IndexOf(this.ReflectedObject).ToString(CultureInfo.InvariantCulture); }
            }

            internal object GetHandle()
            {
                return this.ReflectedObject.Properties[HandleProperty].ComputedValue;
            }

            void SetHandle(object value)
            {
                InArgument handle = (InArgument)(value is ModelItem ? ((ModelItem)value).GetCurrentValue() : value);
                this.ReflectedObject.Properties[HandleProperty].SetValue(handle);
            }

            internal Type GetCorrelationType()
            {
                return this.ReflectedObject.ItemType;
            }

            void SetCorrelationType(object value)
            {
                Type type = (Type)(value is ModelItem ? ((ModelItem)value).GetCurrentValue() : value);
                var source = (ModelItemCollection)this.ReflectedObject.Parent;
                int index = source.IndexOf(this.ReflectedObject);
                var oldInitalizer = (CorrelationInitializer)this.ReflectedObject.GetCurrentValue();
                var newInitializer = (CorrelationInitializer)Activator.CreateInstance(type);
                newInitializer.CorrelationHandle = oldInitalizer.CorrelationHandle;
                this.Dispose();
                source.RemoveAt(index);
                this.Initialize(source.Insert(index, newInitializer));
                this.RaisePropertyChangedEvent(MessageQuerySetModelPropertyProperty);
            }

            bool ValidateCorrelationType(object value, List<string> errors)
            {
                Type type = (Type)(value is ModelItem ? ((ModelItem)value).GetCurrentValue() : value);
                var activity = this.ReflectedObject.Parent.Parent;
                if (typeof(QueryCorrelationInitializer).IsAssignableFrom(type) && !CorrelationInitializerDesigner.CanUseQueryCorrelationInitializer(activity))
                {
                    errors.Add(System.Activities.Core.Presentation.SR.CorrelationInitializerNotSupported);
                }
                return 0 == errors.Count;
            }

            internal ModelProperty GetMessageQuerySetModelProperty()
            {
                return this.ReflectedObject.Properties[MessageQuerySetModelPropertyProperty];
            }
        }
    }
}
