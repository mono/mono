//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Data;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors;

    // <summary>
    // Blend's ChoiceEditor that we augment with our an AutomationPeer
    // </summary>
    internal class AutomatedChoiceEditor : ChoiceEditor 
    {

        // <summary>
        // AutomationValueConverter is used to expose an IValueConverter that the
        // ChoiceEditorAutomationPeer uses to convert the selected item into a
        // string exposed through the automation APIs.  If no converver is specified
        // (default), we call ToString() on the item.
        // </summary>
        public static DependencyProperty AutomationValueConverterProperty = DependencyProperty.Register(
            "AutomationValueConverter",
            typeof(IValueConverter),
            typeof(AutomatedChoiceEditor),
            new PropertyMetadata(null));

        internal event DependencyPropertyChangedEventHandler DependencyPropertyChanged;

        // <summary>
        // Gets or set AutomationValueConverter
        // </summary>
        public IValueConverter AutomationValueConverter 
        {
            get { return (IValueConverter)this.GetValue(AutomationValueConverterProperty); }
            set { this.SetValue(AutomationValueConverterProperty, value); }
        }

        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new HiddenUIElementAutomationPeer(this);
        }

        // Expose DependencyPropertyChanged event to which the AutomationPeer listens, broadcasting
        // its own set of automation events as needed.
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) 
        {
            if (DependencyPropertyChanged != null)
            {
                DependencyPropertyChanged(this, e);
            }

            base.OnPropertyChanged(e);
        }
    }
}
