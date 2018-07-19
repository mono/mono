//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Activities;
    using System.Activities.Core.Presentation;
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Runtime;
    using System.Activities.Presentation.Services;
    using System.Activities.Expressions;

    partial class SendDesigner
    {
        const string CorrelationsCategoryLabelKey = "correlationsCategoryLabel";
        const string EndpointCategoryLabelKey = "endpointCategoryLabel";
        const string MiscellaneousCategoryLabelKey = "miscellaneousCategoryLabel";
        const string AdvancedCategoryLabelKey = "advancedCategoryLabel";
        static string CorrelationHandleTypeNamespace = typeof(CorrelationHandle).Namespace;
        static string Message;
        static string Action;
        static string DeclaredMessageType;

        public static readonly RoutedCommand CreateReceiveReplyCommand = new RoutedCommand("CreateReceiveReply", typeof(SendDesigner));

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "PropertyValueEditors association needs to be done in the static constructor.")]
        static SendDesigner()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type sendType = typeof(Send);

            builder.AddCustomAttributes(sendType, sendType.GetProperty("CorrelationInitializers"), PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            var categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(CorrelationsCategoryLabelKey));
            var descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingCorrelatesWithHint", "Correlation handle"));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("CorrelatesWith"), categoryAttribute, descriptionAttribute);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("CorrelationInitializers"), categoryAttribute, BrowsableAttribute.Yes,
                PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(EndpointCategoryLabelKey));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("Endpoint"), categoryAttribute, new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
            descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingEndpointAddressHint", "<Address>"));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("EndpointAddress"), categoryAttribute, descriptionAttribute);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("EndpointConfigurationName"), categoryAttribute);

            categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(MiscellaneousCategoryLabelKey));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("DisplayName"), categoryAttribute);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("OperationName"), categoryAttribute);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("ServiceContractName"), categoryAttribute, new TypeConverterAttribute(typeof(XNameConverter)));
            descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingValueHint", "<Value to bind>"));
            builder.AddCustomAttributes(sendType, sendType.GetProperty("Content"), categoryAttribute, descriptionAttribute, PropertyValueEditor.CreateEditorAttribute(typeof(SendContentPropertyEditor)));

            var advancedAttribute = new EditorBrowsableAttribute(EditorBrowsableState.Advanced);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("Action"), advancedAttribute, categoryAttribute);
            builder.AddCustomAttributes(
                sendType,
                "KnownTypes",
                advancedAttribute,
                categoryAttribute,
                PropertyValueEditor.CreateEditorAttribute(typeof(TypeCollectionPropertyEditor)),
                new EditorOptionAttribute { Name = TypeCollectionPropertyEditor.AllowDuplicate, Value = false });
            builder.AddCustomAttributes(sendType, sendType.GetProperty("ProtectionLevel"), advancedAttribute, categoryAttribute);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("SerializerOption"), advancedAttribute, categoryAttribute);
            builder.AddCustomAttributes(sendType, sendType.GetProperty("TokenImpersonationLevel"), advancedAttribute, categoryAttribute);

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
                        Send send = (Send)ownerActivity;
                        SendMessageContent content = send.Content as SendMessageContent;
                        return content != null ? content.Message : null;
                    },
                    Setter = (ownerActivity, arg) =>
                    {
                        Send send = (Send)ownerActivity;
                        SendMessageContent content = send.Content as SendMessageContent;
                        if (content != null)
                        {
                            content.Message = arg as InArgument;
                        }
                    },
                },
            };
            ActivityArgumentHelper.RegisterAccessorsGenerator(sendType, argumentAccessorGenerator);
        }

        public SendDesigner()
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
                SendMessageContent messageContent = ((Send)this.ModelItem.GetCurrentValue()).Content as SendMessageContent;
                this.ModelItem.Properties[DeclaredMessageType].SetValue(null == messageContent ? null : messageContent.Message.ArgumentType);
            }
        }

        protected override void OnReadOnlyChanged(bool isReadOnly)
        {
            this.txtOperationName.IsReadOnly = isReadOnly;
        }

        void OnCreateReceiveReplyExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ModelItem container;
            ModelItem flowStepContainer;

            using (ModelEditingScope scope = this.ModelItem.BeginEdit((string)this.FindResource("createReceiveReplyDescription")))
            {
                //special case handling for Sequence
                if (this.ModelItem.IsItemInSequence(out container))
                {            
                    //get activities collection
                    ModelItemCollection activities = container.Properties["Activities"].Collection;
                    //get index of Send within collection and increment by one
                    int index = activities.IndexOf(this.ModelItem) + 1;
                    //insert created reply just after the Send
                    activities.Insert(index, SendDesigner.CreateReceiveReply(container, this.ModelItem));                 
                }
                //special case handling for Flowchart
                else if (this.ModelItem.IsItemInFlowchart(out container, out flowStepContainer))
                {
                    Activity replyActivity = SendDesigner.CreateReceiveReply(container, this.ModelItem);
                    FlowchartDesigner.DropActivityBelow(this.ViewStateService, this.ModelItem, replyActivity, 30);
                }
                else
                {
                    ErrorReporting.ShowAlertMessage(string.Format(CultureInfo.CurrentUICulture, System.Activities.Core.Presentation.SR.CannotPasteSendReplyOrReceiveReply, typeof(ReceiveReply).Name));
                }
                scope.Complete();
            }            
            //always copy reply to clipboard            
            Func<ModelItem, object, object> callback = CreateReceiveReply;
            CutCopyPasteHelper.PutCallbackOnClipBoard(callback, typeof(ReceiveReply), this.ModelItem);
            e.Handled = true;
        }

        static ReceiveReply CreateReceiveReply(ModelItem target, object context)
        {
            ReceiveReply reply = null;
            ModelItem send = (ModelItem)context;
            if (null != send)
            {
                Send sendInstance = (Send)send.GetCurrentValue();
                string name = null;
                //if no correlation is set - create one
                if (null == sendInstance.CorrelatesWith)
                {
                    Variable handleVariable = null;
                    //first, look for root variable scope
                    ModelItemCollection variableScope = VariableHelper.FindRootVariableScope(send).GetVariableCollection();
                    if (null != variableScope)
                    {
                        ModelItemCollection correlations = send.Properties["CorrelationInitializers"].Collection;
                        bool hasRequestReplyHandle = false;
                        foreach (ModelItem item in correlations)
                        {
                            if (item.ItemType.IsAssignableFrom(typeof(RequestReplyCorrelationInitializer)))
                            {
                                hasRequestReplyHandle = true;
                                break;
                            }
                        }

                        if (!hasRequestReplyHandle)
                        {
                            //create unique variable name
                            name = variableScope.CreateUniqueVariableName("__handle", 1);
                            //create variable
                            handleVariable = Variable.Create(name, typeof(CorrelationHandle), VariableModifiers.None);
                            //add it to the scope
                            variableScope.Add(handleVariable);
                            //setup correlation
                            ImportDesigner.AddImport(CorrelationHandleTypeNamespace, send.GetEditingContext());                            
                            VariableValue<CorrelationHandle> expression = new VariableValue<CorrelationHandle> { Variable = handleVariable };
                            InArgument<CorrelationHandle> handle = new InArgument<CorrelationHandle>(expression);
                            correlations.Add(new RequestReplyCorrelationInitializer { CorrelationHandle = handle });
                        }
                    }
                }
                //create receive reply
                reply = new ReceiveReply()
                {
                    DisplayName = string.Format(CultureInfo.CurrentUICulture, "ReceiveReplyFor{0}", send.Properties["DisplayName"].ComputedValue),
                    Request = (Send)send.GetCurrentValue(),
                };
            }
            else
            {
                MessageBox.Show(
                    (string)StringResourceDictionary.Instance["sendActivityCreateReplyErrorLabel"] ?? "Source 'Send' element not found!",
                    (string)StringResourceDictionary.Instance["MessagingActivityTitle"] ?? "Send",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return reply;
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
