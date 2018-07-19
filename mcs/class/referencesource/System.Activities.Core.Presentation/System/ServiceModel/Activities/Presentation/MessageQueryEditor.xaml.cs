//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Activities.Core.Presentation;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Xml;
    using System.Xml.Linq;
    using System.ServiceModel.Description;
    using System.Runtime;

    partial class MessageQueryEditor
    {
        public static readonly DependencyProperty TypeCollectionProperty = DependencyProperty.Register(
            "TypeCollection",
            typeof(IList<KeyValuePair<string, Type>>), 
            typeof(MessageQueryEditor), 
            new UIPropertyMetadata(null, OnTypeCollectionChanged));

        static readonly DependencyPropertyKey QueryPropertyKey = DependencyProperty.RegisterReadOnly(
            "Query",
            typeof(XPathMessageQuery),
            typeof(MessageQueryEditor),
            new UIPropertyMetadata(null));

        static readonly DependencyProperty ActivityProperty = DependencyProperty.Register(
            "Activity",
            typeof(ModelItem),
            typeof(MessageQueryEditor),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty QueryProperty = QueryPropertyKey.DependencyProperty;

        public static readonly RoutedEvent XPathCreatedEvent = EventManager.RegisterRoutedEvent(
            "XPathCreated", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(MessageQueryEditor));
      
        public MessageQueryEditor()
        {
            InitializeComponent();
        }

        //a collection of name and type (argument name + argument type) to expand
        public IList<KeyValuePair<string, Type>> TypeCollection
        {
            get { return (IList<KeyValuePair<string, Type>>)GetValue(TypeCollectionProperty); }
            set { SetValue(TypeCollectionProperty, value); }
        }

        public XPathMessageQuery Query
        {
            get { return (XPathMessageQuery)GetValue(QueryProperty); }
            private set { SetValue(QueryPropertyKey, value); }
        }

        internal ModelItem Activity
        {
            get { return (ModelItem)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        //event raised whenever user creates a xpath
        public event RoutedEventHandler XPathCreated
        {
            add { this.AddHandler(XPathCreatedEvent, value); }
            remove { this.RemoveHandler(XPathCreatedEvent, value); }
        }

        //override default combo box item with my own implementation and style
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MessageQueryComboBoxItem() { Style = (Style)this.FindResource("comboBoxStyle") };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                if (!this.IsDropDownOpen && !string.IsNullOrEmpty(this.Text))
                {
                    this.Query = new XPathMessageQuery(this.Text);
                    this.RaiseEvent(new RoutedEventArgs(XPathCreatedEvent, this));
                }
            }
            base.OnKeyDown(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!LocalAppContextSwitches.UseLegacyAccessibilityFeatures)
            {
                this.SetValue(AutomationProperties.NameProperty, this.Resources["MessageQueryEditorAutomationName"]);
                if (this.IsEditable && this.Template != null)
                {
                    var textBox = this.Template.FindName("PART_EditableTextBox", this) as TextBox;
                    if (textBox != null)
                    {
                        textBox.SetValue(AutomationProperties.NameProperty, this.GetValue(AutomationProperties.NameProperty));
                    }
                }
            }
        }

        //user double clicked on the expanded type, create a xpath
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Propagating exceptions might lead to VS crash.")]
        void OnTypeSelectionChanged(object sender, RoutedEventArgs e)
        {
            var contentCorrelationDesigner = (ContentCorrelationTypeExpander)sender;
            //is selection valid (valid type or property)
            if (contentCorrelationDesigner.IsSelectionValid)
            {
                var path = contentCorrelationDesigner.GetMemberPath();
                var type = contentCorrelationDesigner.GetSelectedType();
                try
                {
                    XmlNamespaceManager namespaceManager = null;
                    string xpathQuery = string.Empty;
                    var content = this.Activity.Properties["Content"].Value;
                    if (content.IsAssignableFrom<ReceiveMessageContent>() || content.IsAssignableFrom<SendMessageContent>())
                    {
                        //generating xpath for message content
                        xpathQuery = XPathQueryGenerator.CreateFromDataContractSerializer(type, path, out namespaceManager);
                    }
                    else
                    {
                        //generating xpath for parameter content
                        XName serviceContractName = null;
                        string operationName = null;
                        string parameterName = contentCorrelationDesigner.SelectedTypeEntry.Name;
                        bool isReply = this.Activity.IsAssignableFrom<SendReply>() || this.Activity.IsAssignableFrom<ReceiveReply>();
                        if (isReply)
                        {
                            operationName = (string)this.Activity.Properties["Request"].Value.Properties["OperationName"].ComputedValue;
                            serviceContractName = (XName)this.Activity.Properties["Request"].Value.Properties["ServiceContractName"].ComputedValue;

                            if (string.IsNullOrEmpty(operationName) || null == serviceContractName)
                            {
                                ModelItem requestDisplayName;
                                this.Activity.TryGetPropertyValue(out requestDisplayName, "Request", "DisplayName");
                                throw FxTrace.Exception.AsError(new InvalidOperationException(
                                        string.Format(CultureInfo.CurrentUICulture, (string)this.FindResource("parametersRequiredText"), requestDisplayName.GetCurrentValue())));
                            }
                        }
                        else
                        {
                            operationName = (string)this.Activity.Properties["OperationName"].ComputedValue;
                            serviceContractName = (XName)this.Activity.Properties["ServiceContractName"].ComputedValue;

                            if (string.IsNullOrEmpty(operationName) || null == serviceContractName)
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(
                                        string.Format(CultureInfo.CurrentUICulture, (string)this.FindResource("parametersRequiredText"), this.Activity.Properties["DisplayName"].ComputedValue)));
                            }
                        }
                        xpathQuery = ParameterXPathQueryGenerator.CreateFromDataContractSerializer(serviceContractName, operationName, parameterName, isReply, type, path, out namespaceManager);
                    }
                    //use CDF api to build a xpath out of type and its properties
                    string xpath = string.Format(CultureInfo.InvariantCulture, "sm:body(){0}", xpathQuery);

                    //get the context
                    //We need to copy over the namespaces from the manager's table 1 by 1. According to MSDN:
                    //If you specify an existing name table, any namespaces in the name table are not automatically added to XmlNamespaceManager.
                    //You must use AddNamespace and RemoveNamespace to add or remove namespaces.
                    XPathMessageContext messageContext = new XPathMessageContext();
                    foreach (string prefix in namespaceManager)
                    {
                        if (!string.IsNullOrEmpty(prefix) && !messageContext.HasNamespace(prefix) && prefix != "xmlns")
                        {
                            messageContext.AddNamespace(prefix, namespaceManager.LookupNamespace(prefix));
                        }
                    }

                    var typeEntry = (ExpanderTypeEntry)contentCorrelationDesigner.Tag;
                    //construct xpath 
                    XPathMessageQuery query = new XPathMessageQuery(xpath, messageContext);
                    //store the xpath in the Tag property; this combo's selectedValue is bound to i
                    typeEntry.Tag = query;
                    this.SelectedIndex = 0;
                    this.IsDropDownOpen = false;
                    this.Query = query;
                    this.RaiseEvent(new RoutedEventArgs(XPathCreatedEvent, this));
                }
                catch (Exception err)
                {
                    MessageBox.Show(
                        err.Message,
                        (string)this.Resources["controlTitle"],
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void OnTypeCollectionChanged()
        {
            //user provided a list of types to expand
            var items = new List<ExpanderTypeEntry>();
            if (null != this.TypeCollection)
            {
                StringBuilder text = new StringBuilder((string)this.FindResource("selectedDisplayText"));
                bool addComma = false;
                //copy all of then into ExpanderTypeEntry
                foreach (var entry in this.TypeCollection)
                {
                    if (addComma)
                    {
                        text.Append(", ");
                    }
                    items.Add(new ExpanderTypeEntry() { TypeToExpand = entry.Value, Name = entry.Key });
                    text.Append(entry.Key);
                    addComma = true;
                }
                //requirement of combo box is that data source must be enumerable, so provide one elemnt array 
                this.ItemsSource = new object[] { new TypeEntryContainer() { Items = items, DisplayText = text.ToString() } };
            }
            else
            {
                this.ItemsSource = null;
            }
        }

        static void OnTypeCollectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((MessageQueryEditor)sender).OnTypeCollectionChanged();
        }

        //ComboBox item subclass 
        sealed class MessageQueryComboBoxItem : ComboBoxItem
        {
            public MessageQueryComboBoxItem()
            {
                //i don't want it to be focusable - its conent is
                this.Focusable = false;
            }

            //do not notify parent ComboBox about mouse down & up events - i don't want to close popup too early
            //i handle closing myself
            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
            {                
            }
            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
            }
        }

    }

}
