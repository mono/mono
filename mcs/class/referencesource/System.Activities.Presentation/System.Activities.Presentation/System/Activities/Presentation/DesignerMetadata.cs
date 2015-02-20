//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Expressions;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Expressions;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.ServiceModel.Activities;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.VisualBasic.Activities;

    class DesignerMetadata : IRegisterMetadata
    {
        // Called by the designer to register  design-time metadata.
        public void Register()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();

            // Register Designers.
            builder.AddCustomAttributes(typeof(Activity), new DesignerAttribute(typeof(ActivityDesigner)));
            builder.AddCustomAttributes(typeof(ActivityBuilder), new DesignerAttribute(typeof(ActivityTypeDesigner)));
            builder.AddCustomAttributes(typeof(ActivityBuilder<>), new DesignerAttribute(typeof(GenericActivityTypeDesigner)));

            // Register PropertyValueEditors
            builder.AddCustomAttributes(typeof(Argument), new EditorAttribute(typeof(ExpressionValueEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(Type), PropertyValueEditor.CreateEditorAttribute(typeof(TypePropertyEditor)));

            builder.AddCustomAttributes(typeof(Activity<>), new EditorAttribute(typeof(ExpressionValueEditor), typeof(PropertyValueEditor)));

            // Disable reuse of propertyvalueeditors for Arguments
            builder.AddCustomAttributes(typeof(Argument), new EditorReuseAttribute(false));
            builder.AddCustomAttributes(typeof(Activity<>), new EditorReuseAttribute(false));

            //Removing all the properties except "Name" from property grid for the type SchemaType.            
            foreach (MemberInfo mi in typeof(ActivityBuilder).GetMembers())
            {
                if (mi.MemberType == MemberTypes.Property && !mi.Name.Equals("Name") && !mi.Name.Equals("ImplementationVersion"))
                {
                    builder.AddCustomAttributes(typeof(ActivityBuilder), mi, new BrowsableAttribute(false));
                }
            }

            // Removing all the properties property grid for the type SchemaType.            
            foreach (MemberInfo mi in typeof(ActivityBuilder<>).GetMembers())
            {
                builder.AddCustomAttributes(typeof(ActivityBuilder<>), mi, new BrowsableAttribute(false));
            }

            builder.AddCustomAttributes(typeof(Argument), new SearchableStringConverterAttribute(typeof(ArgumentSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(VisualBasicValue<>), new SearchableStringConverterAttribute(typeof(VisualBasicValueSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(Type), new SearchableStringConverterAttribute(typeof(TypeSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(ActivityAction<>),
                new SearchableStringConverterAttribute(typeof(ActivityActionSearchableStringConverter<>)));
            builder.AddCustomAttributes(typeof(XName), new SearchableStringConverterAttribute(typeof(XNameSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(Encoding), new SearchableStringConverterAttribute(typeof(EncodingSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(ErrorActivity), new SearchableStringConverterAttribute(typeof(EmptySearchableStringConverter)));

            builder.AddCustomAttributes(typeof(XName), new TypeConverterAttribute(typeof(XNameConverter)));

            builder.AddCustomAttributes(typeof(VBIdentifierName), new EditorAttribute(typeof(VBIdentifierNameEditor), typeof(PropertyValueEditor)));            
            builder.AddCustomAttributes(typeof(VBIdentifierName), new EditorReuseAttribute(false));

            ExpressionTextBox.RegisterExpressionActivityEditor(VisualBasicEditor.ExpressionLanguageName, typeof(VisualBasicEditor), VisualBasicEditor.CreateExpressionFromString);
            builder.AddCustomAttributes(typeof(VisualBasicValue<>), new ExpressionMorphHelperAttribute(typeof(VisualBasicExpressionMorphHelper)));
            builder.AddCustomAttributes(typeof(VisualBasicReference<>), new ExpressionMorphHelperAttribute(typeof(VisualBasicExpressionMorphHelper)));
            builder.AddCustomAttributes(typeof(VisualBasicValue<>), new FeatureAttribute(typeof(VisualBasicValueValidationFeature)));
            builder.AddCustomAttributes(typeof(VisualBasicReference<>), new FeatureAttribute(typeof(VisualBasicReferenceValidationFeature)));

            builder.AddCustomAttributes(typeof(Literal<>), new ExpressionMorphHelperAttribute(typeof(NonTextualExpressionMorphHelper)));
            builder.AddCustomAttributes(typeof(VariableValue<>), new ExpressionMorphHelperAttribute(typeof(NonTextualExpressionMorphHelper)));
            builder.AddCustomAttributes(typeof(VariableReference<>), new ExpressionMorphHelperAttribute(typeof(NonTextualExpressionMorphHelper)));

            builder.AddCustomAttributes(typeof(Activity), new ShowInOutlineViewAttribute());
            builder.AddCustomAttributes(typeof(Collection<Activity>), new ShowInOutlineViewAttribute());



            Type type = typeof(ActivityDelegate);
            builder.AddCustomAttributes(type, new ShowInOutlineViewAttribute() { PromotedProperty = "Handler" });

            type = typeof(ActivityBuilder);
            builder.AddCustomAttributes(type, new ShowInOutlineViewAttribute());
            builder.AddCustomAttributes(type, type.GetProperty("Implementation"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });

            type = typeof(WorkflowService);
            builder.AddCustomAttributes(type, type.GetProperty("Body"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });

            builder.AddCustomAttributes(typeof(WorkflowIdentity), new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
            builder.AddCustomAttributes(typeof(Version), new EditorAttribute(typeof(VersionPropertyValueEditor), typeof(PropertyValueEditor)));

            // Apply the metadata
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
