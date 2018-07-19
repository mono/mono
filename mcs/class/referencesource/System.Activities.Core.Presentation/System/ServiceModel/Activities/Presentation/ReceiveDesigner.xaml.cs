//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Core.Presentation;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.PropertyEditing;
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

    partial class ReceiveDesigner
    {
        const string CorrelationsCategoryLabelKey = "correlationsCategoryLabel";
        const string MiscellaneousCategoryLabelKey = "miscellaneousCategoryLabel";
        const string AdvancedCategoryLabelKey = "advancedCategoryLabel";
        static string CorrelationHandleTypeNamespace = typeof(CorrelationHandle).Namespace;
        static string Message;
        static string Action;
        static string DeclaredMessageType;

        public static readonly RoutedCommand CreateSendReplyCommand = new RoutedCommand("CreateSendReply", typeof(ReceiveDesigner));

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "PropertyValueEditors association needs to be done in the static constructor.")]
        static ReceiveDesigner()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type receiveType = typeof(Receive);

            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CorrelationInitializers"), PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            var categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(CorrelationsCategoryLabelKey));
            var descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingCorrelatesWithHint", "<Correlation handle>"));
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CorrelatesWith"), categoryAttribute, descriptionAttribute);

            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CorrelatesOn"), categoryAttribute, BrowsableAttribute.Yes,
                PropertyValueEditor.CreateEditorAttribute(typeof(CorrelatesOnValueEditor)));
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CorrelationInitializers"), categoryAttribute, BrowsableAttribute.Yes,
                PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationInitializerValueEditor)));

            categoryAttribute = new CategoryAttribute(EditorCategoryTemplateDictionary.Instance.GetCategoryTitle(MiscellaneousCategoryLabelKey));
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("DisplayName"), categoryAttribute);
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("OperationName"), categoryAttribute);
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("ServiceContractName"), categoryAttribute, new TypeConverterAttribute(typeof(XNameConverter)));
            descriptionAttribute = new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingValueHint", "<Value to bind>"));
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("Content"), categoryAttribute, descriptionAttribute, PropertyValueEditor.CreateEditorAttribute(typeof(ReceiveContentPropertyEditor)));

            var advancedAttribute = new EditorBrowsableAttribute(EditorBrowsableState.Advanced);
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("Action"), advancedAttribute, categoryAttribute);
            builder.AddCustomAttributes(
                receiveType,
                "KnownTypes",
                advancedAttribute,
                categoryAttribute,
                PropertyValueEditor.CreateEditorAttribute(typeof(TypeCollectionPropertyEditor)),
                new EditorOptionAttribute { Name = TypeCollectionPropertyEditor.AllowDuplicate, Value = false });

            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("ProtectionLevel"), advancedAttribute, categoryAttribute);
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("SerializerOption"), advancedAttribute, categoryAttribute);
            builder.AddCustomAttributes(receiveType, receiveType.GetProperty("CanCreateInstance"), advancedAttribute, categoryAttribute);

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
                        Receive receive = (Receive)ownerActivity;
                        ReceiveMessageContent content = receive.Content as ReceiveMessageContent;
                        return content != null ? content.Message : null;
                    },
                    Setter = (ownerActivity, arg) =>
                    {
                        Receive receive = (Receive)ownerActivity;
                        ReceiveMessageContent content = receive.Content as ReceiveMessageContent;
                        if (content != null)
                        {
                            content.Message = arg as OutArgument;
                        }
                    },
                },
            };
            ActivityArgumentHelper.RegisterAccessorsGenerator(receiveType, argumentAccessorGenerator);
        }

        public ReceiveDesigner()
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
                ReceiveMessageContent messageContent = ((Receive)this.ModelItem.GetCurrentValue()).Content as ReceiveMessageContent;
                this.ModelItem.Properties[DeclaredMessageType].SetValue(null == messageContent ? null : messageContent.Message.ArgumentType);
            }
        }

        protected override void OnReadOnlyChanged(bool isReadOnly)
        {
            this.txtOperationName.IsReadOnly = isReadOnly;
        }

        void OnCreateSendReplyExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ModelItem container;
            ModelItem flowStepContainer;

            using (ModelEditingScope scope = this.ModelItem.BeginEdit((string)this.FindResource("createSendReplyDescription")))
            {                    
                //special case handling for Sequence
                if (this.ModelItem.IsItemInSequence(out container))
                {
                    //get activities collection
                    ModelItemCollection activities = container.Properties["Activities"].Collection;
                    //get index of Send within collection and increment by one
                    int index = activities.IndexOf(this.ModelItem) + 1;
                    //insert created reply just after the Receive
                    activities.Insert(index, ReceiveDesigner.CreateSendReply(container, this.ModelItem));                 
                }
                //special case handling for Flowchart
                else if (this.ModelItem.IsItemInFlowchart(out container, out flowStepContainer))
                {
                    Activity replyActivity = ReceiveDesigner.CreateSendReply(container, this.ModelItem);
                    FlowchartDesigner.DropActivityBelow(this.ViewStateService, this.ModelItem, replyActivity, 30);
                }
                else
                {
                    ErrorReporting.ShowAlertMessage(string.Format(CultureInfo.CurrentUICulture, System.Activities.Core.Presentation.SR.CannotPasteSendReplyOrReceiveReply, typeof(SendReply).Name));
                }
                scope.Complete();
            }
            //always copy reply to clipboard
            Func<ModelItem, object, object> callback = CreateSendReply;
            CutCopyPasteHelper.PutCallbackOnClipBoard(callback, typeof(SendReply), this.ModelItem);
            e.Handled = true;
        }

        static SendReply CreateSendReply(ModelItem target, object context)
        {
            SendReply reply = null;
            ModelItem receive = (ModelItem)context;
            if (null != receive)
            {
                Receive receiveInstance = (Receive)receive.GetCurrentValue();
                string name = null;
                //if no correlation is set - create one
                if (null == receiveInstance.CorrelatesWith)
                {
                    Variable handleVariable = null;
                    //first, look for nearest variable scope 
                    ModelItemCollection variableScope = VariableHelper.FindRootVariableScope(receive).GetVariableCollection();
                    if (null != variableScope)
                    {
                        ModelItemCollection correlations = receive.Properties["CorrelationInitializers"].Collection;
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
                            ImportDesigner.AddImport(CorrelationHandleTypeNamespace, receive.GetEditingContext());
                            VariableValue<CorrelationHandle> expression = new VariableValue<CorrelationHandle> { Variable = handleVariable };
                            InArgument<CorrelationHandle> handle = new InArgument<CorrelationHandle>(expression);
                            correlations.Add(new RequestReplyCorrelationInitializer { CorrelationHandle = handle });
                        }
                    }
                }

                reply = new SendReply()
                {
                    DisplayName = string.Format(CultureInfo.CurrentUICulture, "SendReplyTo{0}", receive.Properties["DisplayName"].ComputedValue),
                    Request = (Receive)receive.GetCurrentValue(),
                };
            }
            else
            {
                MessageBox.Show(
                    (string)StringResourceDictionary.Instance["receiveActivityCreateReplyErrorLabel"] ?? "Source 'Reply' element not found!",
                    (string)StringResourceDictionary.Instance["MessagingActivityTitle"] ?? "Send",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return reply;
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
