//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Sqm
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    enum ActivityTypeId
    {
        // Unknown represents all custom activities
        Unknown = 0,
        // Out of box activities for .NET 4.0
        DoWhile = 1,
        ForEach = 2,
        If = 3,
        Parallel = 4,
        ParallelForEach = 5,
        Pick = 6,
        PickBranch = 7,
        Sequence = 8,
        Switch = 9,
        While = 10,
        Flowchart = 11,
        FlowDecision = 12,
        FlowSwitch = 13,
        CorrelationScope = 14,
        InitializeCorrelation = 15,
        Receive = 16,
        ReceiveAndSendReply = 17,
        Send = 18,
        SendAndReceiveReply = 19,
        TransactedReceiveScope = 20,
        Persist = 21,
        TerminateWorkflow = 22,
        Assign = 23,
        Delay = 24,
        InvokeMethod = 25,
        WriteLine = 26,
        CancellationScope = 27,
        CompensableActivity = 28,
        Compensate = 29,
        Confirm = 30,
        TransactionScope = 31,
        AddToCollection = 32,
        ClearCollection = 33,
        ExistsInCollection = 34,
        RemoveFromCollection = 35,
        Throw = 36,
        TryCatch = 37,
        Rethrow = 38,
        Interop = 39,

        // To be added: Out of box activities introduced in .NET 4.5
        StateMachine = 40,
        State = 41,
        FinalState = 42,
        NoPersistScope = 44,
        InvokeDelegate = 49,
    };

    static class ActivityUsageCounter
    {
        static Dictionary<string, ActivityTypeId> mapping = new Dictionary<string, ActivityTypeId>();

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline, Justification = "Dictionary items Cannot be initialized inline.")]
        static ActivityUsageCounter()
        {
            // For non-generic activity, use FullName as the key.
            // For generic activity, use Name only since the combination of FullName is open ended.
            mapping["System.Activities.Statements.DoWhile"] = ActivityTypeId.DoWhile;
            mapping["ForEachWithBodyFactory`1"] = ActivityTypeId.ForEach;
            mapping["System.Activities.Statements.If"] = ActivityTypeId.If;
            mapping["System.Activities.Statements.Parallel"] = ActivityTypeId.Parallel;
            mapping["ParallelForEachWithBodyFactory`1"] = ActivityTypeId.ParallelForEach;
            mapping["System.Activities.Core.Presentation.Factories.PickWithTwoBranchesFactory"] = ActivityTypeId.Pick;
            mapping["System.Activities.Statements.PickBranch"] = ActivityTypeId.PickBranch;
            mapping["System.Activities.Statements.Sequence"] = ActivityTypeId.Sequence;
            mapping["Switch`1"] = ActivityTypeId.Switch;
            mapping["System.Activities.Statements.While"] = ActivityTypeId.While;
            mapping["System.Activities.Statements.Flowchart"] = ActivityTypeId.Flowchart;
            mapping["System.Activities.Statements.FlowDecision"] = ActivityTypeId.FlowDecision;
            mapping["FlowSwitch`1"] = ActivityTypeId.FlowSwitch;
            mapping["System.ServiceModel.Activities.CorrelationScope"] = ActivityTypeId.CorrelationScope;
            mapping["System.ServiceModel.Activities.InitializeCorrelation"] = ActivityTypeId.InitializeCorrelation;
            mapping["System.ServiceModel.Activities.Receive"] = ActivityTypeId.Receive;
            mapping["System.ServiceModel.Activities.Presentation.Factories.ReceiveAndSendReplyFactory"] = ActivityTypeId.ReceiveAndSendReply;
            mapping["System.ServiceModel.Activities.Send"] = ActivityTypeId.Send;
            mapping["System.ServiceModel.Activities.Presentation.Factories.SendAndReceiveReplyFactory"] = ActivityTypeId.SendAndReceiveReply;
            mapping["System.ServiceModel.Activities.TransactedReceiveScope"] = ActivityTypeId.TransactedReceiveScope;
            mapping["System.Activities.Statements.Persist"] = ActivityTypeId.Persist;
            mapping["System.Activities.Statements.TerminateWorkflow"] = ActivityTypeId.TerminateWorkflow;
            mapping["System.Activities.Statements.Assign"] = ActivityTypeId.Assign;
            mapping["System.Activities.Statements.Delay"] = ActivityTypeId.Delay;
            mapping["System.Activities.Statements.InvokeMethod"] = ActivityTypeId.InvokeMethod;
            mapping["System.Activities.Statements.WriteLine"] = ActivityTypeId.WriteLine;
            mapping["System.Activities.Statements.CancellationScope"] = ActivityTypeId.CancellationScope;
            mapping["System.Activities.Statements.CompensableActivity"] = ActivityTypeId.CompensableActivity;
            mapping["System.Activities.Statements.Compensate"] = ActivityTypeId.Compensate;
            mapping["System.Activities.Statements.Confirm"] = ActivityTypeId.Confirm;
            mapping["System.Activities.Statements.TransactionScope"] = ActivityTypeId.TransactionScope;
            mapping["AddToCollection`1"] = ActivityTypeId.AddToCollection;
            mapping["ClearCollection`1"] = ActivityTypeId.ClearCollection;
            mapping["ExistsInCollection`1"] = ActivityTypeId.ExistsInCollection;
            mapping["RemoveFromCollection`1"] = ActivityTypeId.RemoveFromCollection;
            mapping["System.Activities.Statements.Rethrow"] = ActivityTypeId.Rethrow;
            mapping["System.Activities.Statements.Throw"] = ActivityTypeId.Throw;
            mapping["System.Activities.Statements.TryCatch"] = ActivityTypeId.TryCatch;
            mapping["System.Activities.Statements.Interop"] = ActivityTypeId.Interop;
            mapping["System.Activities.Core.Presentation.Factories.StateMachineWithInitialStateFactory"] = ActivityTypeId.StateMachine;
            mapping["System.Activities.Statements.State"] = ActivityTypeId.State;
            mapping["System.Activities.Core.Presentation.FinalState"] = ActivityTypeId.FinalState;
            mapping["System.Activities.Statements.NoPersistScope"] = ActivityTypeId.NoPersistScope;
            mapping["System.Activities.Statements.InvokeDelegate"] = ActivityTypeId.InvokeDelegate;
        }

        static internal ActivityTypeId MapTypeToId(Type activityType)
        {
            ActivityTypeId typeId = ActivityTypeId.Unknown;
            if (activityType != null)
            {
                string typeName = activityType.IsGenericType ? activityType.Name : activityType.FullName;
                if (mapping.ContainsKey(typeName))
                {
                    typeId = mapping[typeName];
                }
            }
            return typeId;
        }

        static internal void ReportUsage(IVSSqmService sqmService, Type activityType)
        {
            if (sqmService != null)
            {
                uint[] data = new uint[1];
                data[0] = (uint)MapTypeToId(activityType);
                sqmService.AddArrayToStream((int)DataPointIds.ActivityUsageCount, data, data.Length);
            }
        }
    }
}
