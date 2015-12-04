//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Presentation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Windows;
    using System.Windows.Controls;
    using System.Activities.Presentation.Model;
    using System.Configuration;
    using System.ServiceModel.Configuration;
    using System.Activities.Presentation;
    
    partial class BindingEditor
    {
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.Register("Binding",
            typeof(object),
            typeof(BindingEditor),
            new PropertyMetadata(OnBindingChanged));

        List<BindingDescriptor> bindingElements = new List<BindingDescriptor>();

        bool isInitializing;

        public BindingEditor()
        {
            InitializeComponent();
        }

        public object Binding
        {
            get { return GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        void LoadBindings()
        {
            try
            {
                this.bindingElements.Add(new BindingDescriptor { BindingName = (string)(this.TryFindResource("bindingEditorEmptyBindingLabel") ?? "none"), Value = null });
                Configuration machineConfig = ConfigurationManager.OpenMachineConfiguration();              
                ServiceModelSectionGroup section = ServiceModelSectionGroup.GetSectionGroup(machineConfig);
                if (null != section && null != section.Bindings)
                {
                    this.bindingElements.AddRange(section.Bindings.BindingCollections
                        .OrderBy(p => p.BindingName)
                        .Select<BindingCollectionElement, BindingDescriptor>(p => new BindingDescriptor() { BindingName = p.BindingName, Value = p }));
                }
            }
            catch (ConfigurationErrorsException err)
            {
                ErrorReporting.ShowErrorMessage(err.Message);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.isInitializing = true;
            LoadBindings();
            this.ItemsSource = this.bindingElements;
            this.SelectedIndex = 0;
            this.isInitializing = false;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (!this.isInitializing)
            {

                BindingDescriptor entry = (BindingDescriptor)e.AddedItems[0];
                if (null == entry.Value)
                {
                    Binding = null;
                }
                // try to avoid blowing away any binding that has been custom-tweaked in XAML.
                else if (Binding == null || !(Binding is ModelItem) || !((ModelItem)Binding).ItemType.Equals(entry.Value.BindingType))
                {
                    Binding instance = (Binding)Activator.CreateInstance(entry.Value.BindingType);
                    instance.Name = entry.BindingName;
                    Binding = instance;
                }
            }
        }

        static void OnBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            BindingEditor editor = (BindingEditor)sender;
            object newValue = args.NewValue;

            Type bindingType = null;
            ModelItem item = newValue as ModelItem;
            string bindingName = null;
            if (item != null)
            {
                bindingType = item.ItemType;
                bindingName = (string)item.Properties["Name"].ComputedValue;
            }
            else if (newValue != null)
            {
                bindingType = newValue.GetType();
                if (typeof(Binding).IsAssignableFrom(bindingType))
                {
                    bindingName = ((Binding)newValue).Name;
                }
            }

            // Make combo appear empty if the binding is not one of the ones known to us, e.g., has been custom-tweaked in XAML.
            BindingDescriptor toSelect = null;
            Func<BindingDescriptor, bool> where = p => null != p.Value && p.Value.BindingType == bindingType;
            if (editor.bindingElements.Count(where) > 1)
            {
                toSelect = editor.bindingElements.Where(where).Where(p => string.Equals(p.BindingName, bindingName)).FirstOrDefault();
            }
            else
            {
                toSelect = editor.bindingElements.Where(where).FirstOrDefault();
            }
            //prevent OnSelectionChanged now - the binding is set directly to the object, no need to set again through event handler
            editor.isInitializing = true;
            if (null != toSelect)
            {
                editor.SelectedItem = toSelect;
            }
            else
            {
                editor.SelectedIndex = 0;
            }
            //allow selection changed events to be consumed again
            editor.isInitializing = false;
        }

        sealed class BindingDescriptor 
        {
            public string BindingName
            {
                get;
                internal set;
            }

            public BindingCollectionElement Value
            {
                get;
                internal set;
            }

            public override string ToString()
            {
                return BindingName;
            }
        }
    }
}
