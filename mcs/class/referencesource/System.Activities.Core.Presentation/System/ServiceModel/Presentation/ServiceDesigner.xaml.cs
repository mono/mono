//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Presentation
{
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.ComponentModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Presentation;

    partial class ServiceDesigner : IExpandChild
    {
        const string MiscellaneousCategoryLabelKey = "miscellaneousCategoryLabel";

        public ServiceDesigner()
        {
            InitializeComponent();
        }

        public ModelItem ExpandedChild
        {
            get 
            {
                ModelItem modelItemToSelect = null;
                if (this.ModelItem != null)
                {
                    modelItemToSelect = this.ModelItem.Properties["Body"].Value;
                }
                return modelItemToSelect;
            }
        }

        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            var serviceType = typeof(WorkflowService);
            var advancedAttribute = new EditorBrowsableAttribute(EditorBrowsableState.Advanced);
            var categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(MiscellaneousCategoryLabelKey));

            builder.AddCustomAttributes(serviceType, new DesignerAttribute(typeof(ServiceDesigner)));
            builder.AddCustomAttributes(serviceType, "Name", new TypeConverterAttribute(typeof(XNameConverter)));
            builder.AddCustomAttributes(serviceType, serviceType.GetProperty("Endpoints"), BrowsableAttribute.No);
            builder.AddCustomAttributes(
                serviceType,
                "ImplementedContracts", 
                advancedAttribute, 
                categoryAttribute, 
                PropertyValueEditor.CreateEditorAttribute(typeof(TypeCollectionPropertyEditor)), 
                new EditorOptionAttribute { Name = TypeCollectionPropertyEditor.AllowDuplicate, Value = false },
                new EditorOptionAttribute { Name = TypeCollectionPropertyEditor.Filter, Value = ServiceContractImporter.FilterFunction },
                new EditorOptionAttribute { Name = TypeCollectionPropertyEditor.DefaultType, Value = null });
            builder.AddCustomAttributes(serviceType, serviceType.GetProperty("Body"), BrowsableAttribute.No);
        }
    }
}
