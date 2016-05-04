//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Input;
    using System.Activities;


    partial class ReceiveReplyDesigner
    {
        const string CorrelationsCategoryLabelKey = "correlationsCategoryLabel";
        const string EndpointCategoryLabelKey = "endpointCategoryLabel";
        const string MiscellaneousCategoryLabelKey = "miscellaneousCategoryLabel";
        const string AdvancedCategoryLabelKey = "advancedCategoryLabel";
        static string Message;
        static string Action;
        static string DeclaredMessageType;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "PropertyValueEditors association needs to be done in the static constructor.")]
        static ReceiveReplyDesigner()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type receiveType = typeof(ReceiveReply);


            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CorrelationInitializers"), PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            var categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(CorrelationsCategoryLabelKey));

            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CorrelationInitializers"), categoryAttribute, BrowsableAttribute.Yes,
                PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));            

            categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(MiscellaneousCategoryLabelKey));

            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("DisplayName"), categoryAttribute);
            var descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingValueHint", "<Value to bind>"));
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("Content"), categoryAttribute, descriptionAttribute, PropertyValueEditor.CreateEditorAttribute(typeof(ReceiveContentPropertyEditor)));
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("Request"),
                categoryAttribute,
                PropertyValueEditor.CreateEditorAttribute(typeof(ActivityXRefPropertyEditor)));

            var advancedAttribute = new EditorBrowsableAttribute(EditorBrowsableState.Advanced);
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("Action"), advancedAttribute, categoryAttribute);

            Action = receiveType.GetProperty("Action").Name;

            Type receiveMessageContentType = typeof(ReceiveMessageContent);
            Message = receiveMessageContentType.GetProperty("Message").Name;
            DeclaredMessageType = receiveMessageContentType.GetProperty("DeclaredMessageType").Name;
            MetadataStore.AddAttributeTable(builder.CreateTable());

            Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorGenerator = (activity) => new ArgumentAccessor[]
            {
                new ArgumentAccessor
                {
                    Getter = (ownerActivity) =>
                    {
                        ReceiveReply receiveReply = (ReceiveReply)ownerActivity;
                        ReceiveMessageContent content = receiveReply.Content as ReceiveMessageContent;
                        return content != null ? content.Message : null;
                    },
                    Setter = (ownerActivity, arg) =>
                    {
                        ReceiveReply receiveReply = (ReceiveReply)ownerActivity;
                        ReceiveMessageContent content = receiveReply.Content as ReceiveMessageContent;
                        if (content != null)
                        {
                            content.Message = arg as OutArgument;
                        }
                    },
                },
            };
            ActivityArgumentHelper.RegisterAccessorsGenerator(receiveType, argumentAccessorGenerator);
        }

        public ReceiveReplyDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (null != this.ModelItem)
            {
                this.ModelItem.PropertyChanged += OnModelItemPropertyChanged;
            }
        }

        void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, Message))
            {
                ReceiveMessageContent messageContent = ((ReceiveReply)this.ModelItem.GetCurrentValue()).Content as ReceiveMessageContent;
                this.ModelItem.Properties[DeclaredMessageType].SetValue(null == messageContent ? null : messageContent.Message.ArgumentType);
            }
        }

        void OnDefineButtonClicked(object sender, RoutedEventArgs args)
        {
            using (EditingScope scope = this.Context.Services.GetRequiredService<ModelTreeManager>().CreateEditingScope(StringResourceDictionary.Instance.GetString("editReceiveContent"), true))
            {
                if (ReceiveContentDialog.ShowDialog(this.ModelItem, this.Context, this))
                {
                    scope.Complete();
                }
            }
        }
    }
}
