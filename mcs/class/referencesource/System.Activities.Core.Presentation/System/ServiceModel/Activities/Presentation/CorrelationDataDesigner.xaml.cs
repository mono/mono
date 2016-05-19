//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Windows;
    using System.Activities.Presentation.View;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Activities.Presentation.Hosting;
    using System.Windows.Controls;
    using System.Globalization;
    using System.Linq;

    internal partial class CorrelationDataDesigner 
    {
        const string KeyPrefix = "key";
                
        public static readonly DependencyProperty ActivityProperty = DependencyProperty.Register(
            "Activity", 
            typeof(ModelItem), 
            typeof(CorrelationDataDesigner), 
            new UIPropertyMetadata(OnActivityChanged));

        public static readonly DependencyProperty CorrelationInitializeDataProperty = DependencyProperty.Register(
            "CorrelationInitializeData", 
            typeof(ObservableCollection<CorrelationDataWrapper>), 
            typeof(CorrelationDataDesigner),
            new UIPropertyMetadata(OnCorrelationDataChanged));

        public static readonly DependencyProperty CorrelationHandleProperty = DependencyProperty.Register(
            "CorrelationHandle", 
            typeof(ModelItem), 
            typeof(CorrelationDataDesigner));

        public static readonly RoutedCommand AddNewDataCommand = new RoutedCommand("AddNewDataCommand", typeof(CorrelationDataDesigner));

        DataGridHelper correlationDataDGHelper;

        public ModelItem Activity
        {
            get { return (ModelItem)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        public ObservableCollection<CorrelationDataWrapper> CorrelationInitializeData
        {
            get { return (ObservableCollection<CorrelationDataWrapper>)GetValue(CorrelationInitializeDataProperty); }
            set { SetValue(CorrelationInitializeDataProperty, value); }
        }

        public ModelItem CorrelationHandle
        {
            get { return (ModelItem)GetValue(CorrelationHandleProperty); }
            set { SetValue(CorrelationHandleProperty, value); }
        }

        public CorrelationDataDesigner()
        {
            this.InitializeComponent();

            //create data grid helper
            this.correlationDataDGHelper = new DataGridHelper(this.correlationInitializers, this);            
            //add binding to handle Add new entry clicks
            this.CommandBindings.Add(new CommandBinding(AddNewDataCommand, OnAddNewDataExecuted));
            //provide callback to add new row functionality
            this.correlationDataDGHelper.AddNewRowCommand = AddNewDataCommand;
            //add title for "add new row" button
            this.correlationDataDGHelper.AddNewRowContent = (string)this.FindResource("addNewEntry");                       

            CorrelationDataWrapper.Editor = this;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += this.OnCorrelationDataDesignerLoaded;
        }

        void OnCorrelationDataDesignerLoaded(object sender, RoutedEventArgs e)
        {
            bool isReadOnly = this.Activity != null ? this.Activity.GetEditingContext().Items.GetValue<ReadOnlyState>().IsReadOnly : false;
            this.correlationInitializers.IsReadOnly = isReadOnly;
            this.correlationHandleETB.IsReadOnly = isReadOnly;
        }

        void OnExpressionTextBoxLoaded(object sender, RoutedEventArgs args)
        {
            ((ExpressionTextBox)sender).IsIndependentExpression = true;
        }

        //user clicked Add new data
        void OnAddNewDataExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //generate unique dictionary key
            string keyName = this.CorrelationInitializeData.GetUniqueName<CorrelationDataWrapper>(KeyPrefix, item => item.Key);
            //create new key value pair and add it to the dictionary
            CorrelationDataWrapper wrapper = new CorrelationDataWrapper(keyName, null);
            this.CorrelationInitializeData.Add(wrapper);
            //begin row edit after adding new entry
            this.correlationDataDGHelper.BeginRowEdit(wrapper, this.correlationInitializers.Columns[1]);
        }
       
        static void OnActivityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as ModelItem;
            if (null != item && !item.IsAssignableFrom<InitializeCorrelation>())
            {
                Fx.Assert("CorrelationDataDesigner can only used to edit CorrelationData property of InitializeCorrelation activity");                
            }            
        }

        static void OnCorrelationDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CorrelationDataDesigner).correlationInitializers.ItemsSource = e.NewValue as ObservableCollection<CorrelationDataWrapper>;
        }

        internal void CommitEdit()
        {
            if ((this.Activity != null) && (this.Activity.ItemType == typeof(InitializeCorrelation)))
            {
                using (ModelEditingScope scope = this.Activity.BeginEdit((string)this.FindResource("editCorrelationDataDescription")))
                {
                    this.Activity.Properties[InitializeCorrelationDesigner.CorrelationPropertyName].SetValue(this.CorrelationHandle);
                    ModelItemCollection correlationDataCollection = this.Activity.Properties[InitializeCorrelationDesigner.CorrelationDataPropertyName].Dictionary.Properties["ItemsCollection"].Collection;
                    correlationDataCollection.Clear();
                    foreach (CorrelationDataWrapper wrapper in this.CorrelationInitializeData)
                    {
                        correlationDataCollection.Add(new ModelItemKeyValuePair<string, InArgument<string>>
                            {
                                Key = wrapper.Key,
                                Value = wrapper.Value != null ? wrapper.Value.GetCurrentValue() as InArgument<string> : null
                            });
                    }
                    scope.Complete();
                }
            }
        }

        internal void ValidateKey(CorrelationDataWrapper wrapper, string oldKey)
        {
            string newKey = wrapper.Key;            
            if (string.IsNullOrEmpty(newKey))
            {
                ErrorReporting.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, System.Activities.Core.Presentation.SR.NullOrEmptyKeyName));
                wrapper.Key = oldKey;
            }
            else 
            {
                // At this point, the key of the entry has already been changed. If there are 
                // entries with duplicate keys, the number of those entries is greater than 1.
                // Thus, we only need to check the entry count.
                int entryCount = this.CorrelationInitializeData.Count(entry => entry.Key == newKey);
                if (entryCount > 1)
                {
                    ErrorReporting.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, System.Activities.Core.Presentation.SR.DuplicateKeyName, newKey));
                    wrapper.Key = oldKey;
                }
            }
        }

        void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }
    }

    internal sealed class CorrelationDataWrapper : DependencyObject
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(CorrelationDataWrapper), new UIPropertyMetadata(string.Empty, OnKeyChanged));


        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(ModelItem), typeof(CorrelationDataWrapper));

        bool isValidating;
        public ModelItem Value
        {
            get { return (ModelItem)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
       
        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static CorrelationDataDesigner Editor
        {
            get;
            set;
        }

        public CorrelationDataWrapper()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        internal CorrelationDataWrapper(string key, ModelItem value)            
        {
            //Skip validation when first populate the collection
            this.isValidating = true;
            this.Key = key;
            this.Value = value;
            this.isValidating = false;
        }

        static void OnKeyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CorrelationDataWrapper wrapper = sender as CorrelationDataWrapper;
            if ((wrapper != null) && (!wrapper.isValidating))
            {                
                wrapper.isValidating = true;
                CorrelationDataWrapper.Editor.ValidateKey(wrapper, (string)e.OldValue);
                wrapper.isValidating = false;
            }
        }
    }
}
