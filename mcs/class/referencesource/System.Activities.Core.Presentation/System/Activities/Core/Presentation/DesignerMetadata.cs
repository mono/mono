//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Presentation;
    using System.ServiceModel.Activities.Presentation.Converters;
    using System.ServiceModel.Presentation;
    using System.Xml.Linq;

    public class DesignerMetadata : IRegisterMetadata
    {
        // Called by the designer to register any design-time metadata.
        //
        // Be aware of the accidential performance impact when adding things into this method.
        // In particular, pay attention to calls that will lead to loading extra assemblies.
        //
        public void Register()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();

            //shared component
            builder.AddCustomAttributes(typeof(Collection<Constraint>), new BrowsableAttribute(false));
            builder.AddCustomAttributes(typeof(string), new EditorReuseAttribute(false));
            builder.AddCustomAttributes(typeof(ActivityAction), new EditorReuseAttribute(false));
            builder.AddCustomAttributes(typeof(XName), new EditorReuseAttribute(false));

            //Flowchart activities
            FlowchartDesigner.RegisterMetadata(builder);
            FlowSwitchDesigner.RegisterMetadata(builder);
            FlowDecisionDesigner.RegisterMetadata(builder);

            // Messaging activities
            ServiceDesigner.RegisterMetadata(builder);

            // Registering inline for designers for InitializeCorrelation, Send, Receive, SendReply, ReceiveReply activities to avoid calling
            // their static constructors. This will avoid instantiating the ResourceDictionary for their PropertyValueEditors during designer load.
            builder.AddCustomAttributes(typeof(Send), new DesignerAttribute(typeof(SendDesigner)));
            builder.AddCustomAttributes(typeof(Send), new ActivityDesignerOptionsAttribute { AllowDrillIn = false });

            builder.AddCustomAttributes(typeof(Receive), new DesignerAttribute(typeof(ReceiveDesigner)));
            builder.AddCustomAttributes(typeof(Receive), new ActivityDesignerOptionsAttribute { AllowDrillIn = false });

            builder.AddCustomAttributes(typeof(SendReply), new FeatureAttribute(typeof(SendReplyValidationFeature)));
            builder.AddCustomAttributes(typeof(SendReply), new DesignerAttribute(typeof(SendReplyDesigner)));
            builder.AddCustomAttributes(typeof(SendReply), new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
            CutCopyPasteHelper.AddDisallowedTypeForCopy(typeof(SendReply));

            builder.AddCustomAttributes(typeof(ReceiveReply), new FeatureAttribute(typeof(ReceiveReplyValidationFeature)));
            builder.AddCustomAttributes(typeof(ReceiveReply), new DesignerAttribute(typeof(ReceiveReplyDesigner)));
            builder.AddCustomAttributes(typeof(ReceiveReply), new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
            CutCopyPasteHelper.AddDisallowedTypeForCopy(typeof(ReceiveReply));

            builder.AddCustomAttributes(typeof(InitializeCorrelation), new DesignerAttribute(typeof(InitializeCorrelationDesigner)));
            builder.AddCustomAttributes(typeof(InitializeCorrelation), new ActivityDesignerOptionsAttribute { AllowDrillIn = false });

            TransactedReceiveScopeDesigner.RegisterMetadata(builder);
            CorrelationScopeDesigner.RegisterMetadata(builder);

            //Procedural activities
            AssignDesigner.RegisterMetadata(builder);
            IfElseDesigner.RegisterMetadata(builder);
            InvokeMethodDesigner.RegisterMetadata(builder);
            DoWhileDesigner.RegisterMetadata(builder);
            WhileDesigner.RegisterMetadata(builder);
            ForEachDesigner.RegisterMetadata(builder);
            TryCatchDesigner.RegisterMetadata(builder);
            CatchDesigner.RegisterMetadata(builder);
            ParallelDesigner.RegisterMetadata(builder);
            SequenceDesigner.RegisterMetadata(builder);
            SwitchDesigner.RegisterMetadata(builder);
            CaseDesigner.RegisterMetadata(builder);

            //Compensation/Transaction
            CancellationScopeDesigner.RegisterMetadata(builder);
            CompensableActivityDesigner.RegisterMetadata(builder);
            TransactionScopeDesigner.RegisterMetadata(builder);

            //Misc activities            
            PickDesigner.RegisterMetadata(builder);
            PickBranchDesigner.RegisterMetadata(builder);
            WriteLineDesigner.RegisterMetadata(builder);
            NoPersistScopeDesigner.RegisterMetadata(builder);

            InvokeDelegateDesigner.RegisterMetadata(builder);

            // StateMachine
            StateMachineDesigner.RegisterMetadata(builder);
            StateDesigner.RegisterMetadata(builder);
            TransitionDesigner.RegisterMetadata(builder);

            builder.AddCustomAttributes(typeof(AddToCollection<>), new FeatureAttribute(typeof(UpdatableGenericArgumentsFeature)));
            builder.AddCustomAttributes(typeof(RemoveFromCollection<>), new FeatureAttribute(typeof(UpdatableGenericArgumentsFeature)));
            builder.AddCustomAttributes(typeof(ClearCollection<>), new FeatureAttribute(typeof(UpdatableGenericArgumentsFeature)));
            builder.AddCustomAttributes(typeof(ExistsInCollection<>), new FeatureAttribute(typeof(UpdatableGenericArgumentsFeature)));

            builder.AddCustomAttributes(typeof(AddToCollection<>), new DefaultTypeArgumentAttribute(typeof(int)));
            builder.AddCustomAttributes(typeof(RemoveFromCollection<>), new DefaultTypeArgumentAttribute(typeof(int)));
            builder.AddCustomAttributes(typeof(ClearCollection<>), new DefaultTypeArgumentAttribute(typeof(int)));
            builder.AddCustomAttributes(typeof(ExistsInCollection<>), new DefaultTypeArgumentAttribute(typeof(int)));

            MetadataStore.AddAttributeTable(builder.CreateTable());

            MorphHelper.AddPropertyValueMorphHelper(typeof(InArgument<>), MorphHelpers.ArgumentMorphHelper);
            MorphHelper.AddPropertyValueMorphHelper(typeof(OutArgument<>), MorphHelpers.ArgumentMorphHelper);
            MorphHelper.AddPropertyValueMorphHelper(typeof(InOutArgument<>), MorphHelpers.ArgumentMorphHelper);
            MorphHelper.AddPropertyValueMorphHelper(typeof(ActivityAction<>), MorphHelpers.ActivityActionMorphHelper);
            MorphHelper.AddPropertyValueMorphHelper(typeof(ActivityFunc<,>), MorphHelpers.ActivityFuncMorphHelper);

            // There is no need to keep an reference to this delayed worker since the AppDomain event handler will do it.
            RegisterMetadataDelayedWorker delayedWorker = new RegisterMetadataDelayedWorker();
            delayedWorker.RegisterMetadataDelayed("System.Workflow.Runtime", InteropDesigner.RegisterMetadata);
            delayedWorker.RegisterMetadataDelayed("System.ServiceModel", RegisterMetadataForMessagingActivitiesSearchMetadata);
            delayedWorker.RegisterMetadataDelayed("System.ServiceModel", RegisterMetadataForMessagingActivitiesPropertyEditors);
            delayedWorker.WorkNowIfApplicable();
        }

        private static void RegisterMetadataForMessagingActivitiesPropertyEditors(AttributeTableBuilder builder)
        {
            EndpointDesigner.RegisterMetadata(builder);

            builder.AddCustomAttributes(typeof(InArgument<CorrelationHandle>), new EditorReuseAttribute(false));
            builder.AddCustomAttributes(typeof(InArgument<Uri>), new EditorReuseAttribute(false));
            builder.AddCustomAttributes(typeof(MessageQuerySet), PropertyValueEditor.CreateEditorAttribute(typeof(CorrelatesOnValueEditor)));
        }

        private static void RegisterMetadataForMessagingActivitiesSearchMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(SendMessageContent),
                new SearchableStringConverterAttribute(typeof(SendMessageContentSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(SendParametersContent),
                new SearchableStringConverterAttribute(typeof(SendParametersContentSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(ReceiveMessageContent),
                new SearchableStringConverterAttribute(typeof(ReceiveMessageContentSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(ReceiveParametersContent),
                new SearchableStringConverterAttribute(typeof(ReceiveParametersContentSearchableStringConverter)));
            builder.AddCustomAttributes(typeof(XPathMessageQuery),
                new SearchableStringConverterAttribute(typeof(XPathMessageQuerySearchableStringConverter)));
        }
    }
}
