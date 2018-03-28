//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Input;
    using System.Activities;

    partial class SendReplyDesigner
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
        static SendReplyDesigner()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type sendType = typeof(SendReply);

            builder.AddCustomAttributes(sendType, sendType.GetProperty("CorrelationInitializers"), PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            var categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(CorrelationsCategoryLabelKey));

            builder.AddCustomAttributes(sendType, sendType.GetProperty("CorrelationInitializers"), categoryAttribute, BrowsableAttribute.Yes,
                PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(MiscellaneousCategoryLabelKey));

            builder.AddCustomAttributes(sendType, sendType.GetProperty("DisplayName"), categoryAttribute);
            var descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingValueHint", "<Value to bind>"));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("Content"), categoryAttribute, descriptionAttribute, PropertyValueEditor.CreateEditorAttribute(typeof(SendContentPropertyEditor)));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("Request"),
                categoryAttribute,
                PropertyValueEditor.CreateEditorAttribute(typeof(ActivityXRefPropertyEditor)));

            var advancedAttribute = new EditorBrowsableAttribute(EditorBrowsableState.Advanced);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("Action"), categoryAttribute, advancedAttribute);

            Action = sendType.GetProperty("Action").Name;

            Type sendMessageContentType = typeof(SendMessageContent);
            Message = sendMessageContentType.GetProperty("Message").Name;
            DeclaredMessageType = sendMessageContentType.GetProperty("DeclaredMessageType").Name;

            MetadataStore.AddAttributeTable(builder.CreateTable());

            Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorGenerator = (activity) => new ArgumentAccessor[]
            {
                new ArgumentAccessor
                {
                    Getter = (ownerActivity) =>
                    {
                        SendReply sendReply = (SendReply)ownerActivity;
                        SendMessageContent content = sendReply.Content as SendMessageContent;
                        return content != null ? content.Message : null;
                    },
                    Setter = (ownerActivity, arg) =>
                    {
                        SendReply sendReply = (SendReply)ownerActivity;
                        SendMessageContent content = sendReply.Content as SendMessageContent;
                        if (content != null)
                        {
                            content.Message = arg as InArgument;
                        }
                    },
                },
            };
            ActivityArgumentHelper.RegisterAccessorsGenerator(sendType, argumentAccessorGenerator);
        }

        public SendReplyDesigner()
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
                SendMessageContent messageContent = ((SendReply)this.ModelItem.GetCurrentValue()).Content as SendMessageContent;
                this.ModelItem.Properties[DeclaredMessageType].SetValue(null == messageContent ? null : messageContent.Message.ArgumentType);
            }
        }

        void OnDefineButtonClicked(object sender, RoutedEventArgs args)
        {
            using (EditingScope scope = this.Context.Services.GetRequiredService<ModelTreeManager>().CreateEditingScope(StringResourceDictionary.Instance.GetString("editSendContent"), true))
            {
                if (SendContentDialog.ShowDialog(this.ModelItem, this.Context, this))
                {
                    scope.Complete();
                }
            }
        }
    }
}
