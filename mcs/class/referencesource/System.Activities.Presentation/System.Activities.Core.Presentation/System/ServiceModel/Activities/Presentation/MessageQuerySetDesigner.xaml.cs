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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.ServiceModel;
    using System.Windows;

    partial class MessageQuerySetDesigner : INotifyPropertyChanged
    {
        static readonly string key = "key";
        DataGridHelper messageQueriesDGHelper;
        string querySetPropertyName = string.Empty;
        ObservableCollection<MessageQueryEntry> dataSource = new ObservableCollection<MessageQueryEntry>();
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty ActivityProperty = DependencyProperty.Register(
            "Activity",
            typeof(ModelItem),
            typeof(MessageQuerySetDesigner),
            new UIPropertyMetadata(OnActivityChanged));

        public static readonly DependencyProperty MessageQuerySetContainerProperty = DependencyProperty.Register(
            "MessageQuerySetContainer",
            typeof(ModelItem),
            typeof(MessageQuerySetDesigner),
            new UIPropertyMetadata(null, OnMessageQuerySetContainerChanged));

        static readonly DependencyPropertyKey IsTypeExpanderEnabledPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsTypeExpanderEnabled",
            typeof(bool),
            typeof(MessageQuerySetDesigner),
            new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsTypeExpanderEnabledProperty =
            IsTypeExpanderEnabledPropertyKey.DependencyProperty;

        public MessageQuerySetDesigner()
        {
            InitializeComponent();
        }

        //reference to messaging activity containing given message query set
        public ModelItem Activity
        {
            get { return (ModelItem)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        //reference to model item which contains edited message query set (may be the same as Activty)
        public ModelItem MessageQuerySetContainer
        {
            get { return (ModelItem)GetValue(MessageQuerySetContainerProperty); }
            set { SetValue(MessageQuerySetContainerProperty, value); }
        }

        public bool IsTypeExpanderEnabled
        {
            get { return (bool)GetValue(IsTypeExpanderEnabledProperty); }
            private set { SetValue(IsTypeExpanderEnabledPropertyKey, value); }
        }

        public IList<KeyValuePair<string, Type>> ActivityParameters
        {
            get
            {
                return null != this.Activity ? this.GetActivityParameters() : null;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //create data grid helper and provide drop down with type expander as new row template
            this.messageQueriesDGHelper = new DataGridHelper(this.messageQueries, this);
            this.messageQueriesDGHelper.AddNewRowContent = this.FindResource("newRowTemplate");
            this.messageQueriesDGHelper.ShowValidationErrorAsToolTip = true;
            this.messageQueries.ItemsSource = this.dataSource;
            this.dataSource.CollectionChanged += this.OnDataCollectionChanged;
        }

        void OnXpathCreated(object sender, RoutedEventArgs e)
        {
            //user created a xpath
            var editor = (MessageQueryEditor)sender;
            //get its value
            var query = editor.Query;
            if (null != query)
            {
                //get reference to message query set dictionary
                var messageQuerySet = this.MessageQuerySetContainer.Properties[this.querySetPropertyName].Dictionary;
                //create unique key name
                var name = messageQuerySet.GetUniqueName(key, p => (string)p.GetCurrentValue());
                //add new entry with created key and query
                messageQuerySet.Add(name, query);
                //look for created key value pair
                var entry = messageQuerySet.First(p => string.Equals(p.Key.GetCurrentValue(), name));
                //wrap it 
                var wrapper = new MessageQueryEntry(entry);
                //and add it to the ui
                this.messageQueriesDGHelper.Source<IList>().Add(wrapper);
            }
            //reset editor, so new query can be created
            editor.SelectedItem = null;
        }

        void OnActivityChanged()
        {
            if (null != this.PropertyChanged)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("ActivityParameters"));
            }
            this.OnInitialzeView();
        }

        //method called whenever Activity or MessageQuerySetContainer changes
        void OnInitialzeView()
        {
            //cleanup previous ui binding
            this.dataSource.Clear();

            //if we have activity and valid message query set container
            if (null != this.Activity && null != this.MessageQuerySetContainer && !string.IsNullOrEmpty(this.querySetPropertyName))
            {
                //check if message query set is initialized
                if (!this.MessageQuerySetContainer.Properties[this.querySetPropertyName].IsSet)
                {
                    //initialize if required
                    this.MessageQuerySetContainer.Properties[this.querySetPropertyName].SetValue(new MessageQuerySet());
                }
                //get reference to message query set
                var input = this.MessageQuerySetContainer.Properties[this.querySetPropertyName].Dictionary;

                if (null != input)
                {
                    //create all model objects into ux collection
                    foreach (var entry in input)
                    {
                        var wrapper = new MessageQueryEntry(entry);
                        this.dataSource.Add(wrapper);
                    }
                    this.IsTypeExpanderEnabled = true;
                }
            }
        }

        void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //whenever user removes an entry from ux, reflect that change into underlying model
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    var messageQuerySet = this.MessageQuerySetContainer.Properties[this.querySetPropertyName].Dictionary;
                    foreach (MessageQueryEntry entry in e.OldItems)
                    {
                        messageQuerySet.Remove(entry.GetKey());
                        entry.Dispose();
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.IsTypeExpanderEnabled = false;
                    break;
            }
        }

        //helper method - used to pull activity's parameters 
        IList<KeyValuePair<string, Type>> GetActivityParameters()
        {
            var result = new List<KeyValuePair<string, Type>>();
            //get activity's content
            var content = this.Activity.Properties["Content"].Value;
            if (null != content)
            {
                //simple scenario - content is just one argument - get its type
                if ((content.IsAssignableFrom<ReceiveMessageContent>() || content.IsAssignableFrom<SendMessageContent>()) &&
                    content.Properties["Message"].IsSet)
                {
                    ModelItem type = null;
                    content.TryGetPropertyValue(out type, "Message", "ArgumentType");
                    result.Add(new KeyValuePair<string, Type>((string)this.FindResource("defaultParameterName"), (Type)type.GetCurrentValue()));
                }
                //complex scenario - content is a collection of parameters, for each one get its name and type
                else if (content.IsAssignableFrom<ReceiveParametersContent>() && content.Properties["Parameters"].IsSet)
                {
                    var source = (IDictionary<string, OutArgument>)content.Properties["Parameters"].ComputedValue;
                    foreach (var entry in source)
                    {
                        result.Add(new KeyValuePair<string, Type>(entry.Key, entry.Value.ArgumentType));
                    }
                }
                else if (content.IsAssignableFrom<SendParametersContent>() && content.Properties["Parameters"].IsSet)
                {
                    var source = (IDictionary<string, InArgument>)content.Properties["Parameters"].ComputedValue;
                    foreach (var entry in source)
                    {
                        result.Add(new KeyValuePair<string, Type>(entry.Key, entry.Value.ArgumentType));
                    }
                }
            }
            return result;
        }

        static void OnActivityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var activity = e.NewValue as ModelItem;
            //throw if activity is not valid messaging activity type
            if (null != activity && !activity.IsMessagingActivity())
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(activity.ItemType.FullName));
            }
            ((MessageQuerySetDesigner)sender).OnActivityChanged();
        }

        static void OnMessageQuerySetContainerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageQuerySetDesigner)sender;
            var container = e.NewValue as ModelItem;
            //throw if query set container is not derived from correlation initializer or doesn't have required property
            if (null != container)
            {
                var property = container.Properties.FirstOrDefault(p => typeof(MessageQuerySet).IsAssignableFrom(p.PropertyType));
                if (!container.IsAssignableFrom<CorrelationInitializer>() && null == property)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(container.ItemType.FullName));
                }
                control.querySetPropertyName = null != property ? property.Name : string.Empty;
            }
            control.OnInitialzeView();
        }

        void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }

        internal sealed class MessageQueryEntry : DesignObjectWrapper
        {
            internal static string KeyProperty = "Key";
            internal static string ValueProperty = "Expression";

            string keyValue;

            public MessageQueryEntry()
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            public MessageQueryEntry(KeyValuePair<ModelItem, ModelItem> entry)
                : base(entry.Value)
            {
                this.keyValue = (string)entry.Key.GetCurrentValue();
            }

            #region Initialize type properties code
            public static PropertyDescriptorData[] InitializeTypeProperties()
            {
                return new PropertyDescriptorData[]
                {
                    new PropertyDescriptorData()
                    {
                         PropertyType = typeof(string),
                         PropertyName = KeyProperty,
                         PropertyGetter = (instance) => ( ((MessageQueryEntry)instance).GetKey() ),
                         PropertyValidator = (instance, value, errors) => (((MessageQueryEntry)instance).ValidateKey(value, errors)),
                         PropertySetter = (instance, value) => { ((MessageQueryEntry)instance).SetKey(value); },
                    },
                    new PropertyDescriptorData()
                    {
                        PropertyType = typeof(string),
                        PropertyName = ValueProperty,
                        PropertyGetter = (instance) => (((MessageQueryEntry)instance).ReflectedObject.Properties[ValueProperty].ComputedValue),
                        PropertySetter = (instance, value) => { ((MessageQueryEntry)instance).ReflectedObject.Properties[ValueProperty].SetValue( value ); },
                    }
                };
            }
            #endregion

            internal string GetKey()
            {
                return this.keyValue;
            }

            void SetKey(object value)
            {
                string name = (string)(value is ModelItem ? ((ModelItem)value).GetCurrentValue() : value);
                name = name.Trim();

                var source = (ModelItemDictionary)this.ReflectedObject.Parent;
                ModelItem newKeyMI = null;
                source.SwitchKeys(this.keyValue, name, out newKeyMI);
                this.keyValue = name;
            }

            bool ValidateKey(object newValue, List<string> errors)
            {
                string name = (string)(newValue is ModelItem ? ((ModelItem)newValue).GetCurrentValue() : newValue);
                if (null != name)
                {
                    name = name.Trim();
                }

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add(System.Activities.Core.Presentation.SR.NullOrEmptyKeyName);
                }
                else
                {
                    var source = (ModelItemDictionary)this.ReflectedObject.Parent;
                    if (source.Keys.Any(p => string.Equals(p.GetCurrentValue(), name)))
                    {
                        errors.Add(string.Format(CultureInfo.CurrentUICulture, System.Activities.Core.Presentation.SR.DuplicateKeyName, name));
                    }
                }
                return errors.Count == 0;
            }

            protected override string AutomationId
            {
                get { return this.keyValue; }
            }
        }
    }
}
