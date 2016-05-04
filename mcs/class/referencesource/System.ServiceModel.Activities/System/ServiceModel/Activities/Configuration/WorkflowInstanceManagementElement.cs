namespace System.ServiceModel.Activities.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowInstanceManagementElement : BehaviorExtensionElement
    {
        
        const string authorizedWindowsGroup = "authorizedWindowsGroup";
        ConfigurationPropertyCollection properties;

        public WorkflowInstanceManagementElement()
        {
        }

        [SuppressMessage(
            FxCop.Category.Configuration,
            FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property only overrides the base property.")]
        public override Type BehaviorType
        {
            get { return typeof(WorkflowInstanceManagementBehavior); }
        }        

        [ConfigurationProperty(
            authorizedWindowsGroup,
            IsRequired = false)]
        [StringValidator(MinLength = 0)]
        public string AuthorizedWindowsGroup
        {
            get { return (string)base[authorizedWindowsGroup]; }
            set { base[authorizedWindowsGroup] = value; }
        }
        
        protected internal override object CreateBehavior()
        {
            string authorizedWindowsGroup;
            if (!string.IsNullOrEmpty(this.AuthorizedWindowsGroup))
            {
                authorizedWindowsGroup = this.AuthorizedWindowsGroup;
            }
            else
            {
                authorizedWindowsGroup = WorkflowInstanceManagementBehavior.GetDefaultBuiltinAdministratorsGroup();
            }

            WorkflowInstanceManagementBehavior workflowInstanceManagementBehavior = new WorkflowInstanceManagementBehavior
            {
                WindowsGroup = authorizedWindowsGroup
            };
            return workflowInstanceManagementBehavior;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(authorizedWindowsGroup, typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, int.MaxValue, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}
