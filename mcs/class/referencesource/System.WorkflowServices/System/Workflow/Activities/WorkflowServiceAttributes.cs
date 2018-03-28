//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [TypeConverter(typeof(WorkflowServiceAttributesTypeConverter))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowServiceAttributes : DependencyObject, IServiceDescriptionBuilder
    {
        private static readonly DependencyProperty AddressFilterModeProperty =
            DependencyProperty.Register("AddressFilterMode",
            typeof(AddressFilterMode), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(AddressFilterMode.Exact, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty ConfigurationNameProperty =
            DependencyProperty.Register("ConfigurationName",
            typeof(string), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty IgnoreExtensionDataObjectProperty =
            DependencyProperty.Register("IgnoreExtensionDataObject",
            typeof(bool), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(false, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty IncludeExceptionDetailInFaultsProperty =
            DependencyProperty.Register("IncludeExceptionDetailInFaults",
            typeof(bool), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(false, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty MaxItemsInObjectGraphProperty =
            DependencyProperty.Register("MaxItemsInObjectGraph",
            typeof(int), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(65536, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty NamespaceProperty =
            DependencyProperty.Register("Namespace",
            typeof(string), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty UseSynchronizationContextProperty =
            DependencyProperty.Register("UseSynchronizationContext",
            typeof(bool), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(true, DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty ValidateMustUnderstandProperty =
            DependencyProperty.Register("ValidateMustUnderstand",
            typeof(bool), typeof(WorkflowServiceAttributes),
            new PropertyMetadata(true, DependencyPropertyOptions.Metadata));


        public WorkflowServiceAttributes()
        {
        }

        [DefaultValue(AddressFilterMode.Exact)]
        public AddressFilterMode AddressFilterMode
        {
            get { return (AddressFilterMode) this.GetValue(WorkflowServiceAttributes.AddressFilterModeProperty); }
            set { this.SetValue(WorkflowServiceAttributes.AddressFilterModeProperty, value); }
        }

        [DefaultValue(null)]
        public string ConfigurationName
        {
            get { return (string) this.GetValue(WorkflowServiceAttributes.ConfigurationNameProperty); }
            set { this.SetValue(WorkflowServiceAttributes.ConfigurationNameProperty, value); }
        }

        [DefaultValue(false)]
        public bool IgnoreExtensionDataObject
        {
            get { return (bool) this.GetValue(WorkflowServiceAttributes.IgnoreExtensionDataObjectProperty); }
            set { this.SetValue(WorkflowServiceAttributes.IgnoreExtensionDataObjectProperty, value); }
        }

        [DefaultValue(false)]
        public bool IncludeExceptionDetailInFaults
        {
            get { return (bool) this.GetValue(WorkflowServiceAttributes.IncludeExceptionDetailInFaultsProperty); }
            set { this.SetValue(WorkflowServiceAttributes.IncludeExceptionDetailInFaultsProperty, value); }
        }

        [DefaultValue(65536)]
        public int MaxItemsInObjectGraph
        {
            get { return (int) this.GetValue(WorkflowServiceAttributes.MaxItemsInObjectGraphProperty); }
            set { this.SetValue(WorkflowServiceAttributes.MaxItemsInObjectGraphProperty, value); }
        }

        [DefaultValue(null)]
        public string Name
        {
            get { return (string) this.GetValue(WorkflowServiceAttributes.NameProperty); }
            set { this.SetValue(WorkflowServiceAttributes.NameProperty, value); }
        }

        [DefaultValue(null)]
        public string Namespace
        {
            get { return (string) this.GetValue(WorkflowServiceAttributes.NamespaceProperty); }
            set { this.SetValue(WorkflowServiceAttributes.NamespaceProperty, value); }
        }

        [DefaultValue(true)]
        public bool UseSynchronizationContext
        {
            get { return (bool) this.GetValue(WorkflowServiceAttributes.UseSynchronizationContextProperty); }
            set { this.SetValue(WorkflowServiceAttributes.UseSynchronizationContextProperty, value); }
        }

        [DefaultValue(true)]
        public bool ValidateMustUnderstand
        {
            get { return (bool) this.GetValue(WorkflowServiceAttributes.ValidateMustUnderstandProperty); }
            set { this.SetValue(WorkflowServiceAttributes.ValidateMustUnderstandProperty, value); }
        }

        void IServiceDescriptionBuilder.BuildServiceDescription(ServiceDescriptionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            WorkflowServiceBehavior workflowServiceBehavior = context.ServiceDescription.Behaviors.Find<WorkflowServiceBehavior>();
            if (workflowServiceBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.NoWorkflowServiceBehavior)));
            }

            workflowServiceBehavior.AddressFilterMode = this.AddressFilterMode;
            workflowServiceBehavior.IgnoreExtensionDataObject = this.IgnoreExtensionDataObject;
            workflowServiceBehavior.IncludeExceptionDetailInFaults = this.IncludeExceptionDetailInFaults;
            workflowServiceBehavior.MaxItemsInObjectGraph = this.MaxItemsInObjectGraph;
            workflowServiceBehavior.UseSynchronizationContext = this.UseSynchronizationContext;
            workflowServiceBehavior.ValidateMustUnderstand = this.ValidateMustUnderstand;

            if (!string.IsNullOrEmpty(this.ConfigurationName))
            {
                workflowServiceBehavior.ConfigurationName = this.ConfigurationName;
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                workflowServiceBehavior.Name = this.Name;
            }
            if (!string.IsNullOrEmpty(this.Namespace))
            {
                workflowServiceBehavior.Namespace = this.Namespace;
            }
        }
    }
}
