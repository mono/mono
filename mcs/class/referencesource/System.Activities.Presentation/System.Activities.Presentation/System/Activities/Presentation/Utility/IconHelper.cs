//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Utility
{
    using System;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Windows.Media;

    internal static class IconHelper
    {
        private static Dictionary<string, string> iconNameMapping;

        internal static Dictionary<string, string> IconNameMapping
        {
            get
            {
                if (iconNameMapping == null)
                {
                    iconNameMapping = new Dictionary<string, string>()
                    {
                        { "System.Activities.Statements.AddToCollection`1", "AddToCollectionIcon" },
                        { "System.Activities.Statements.Assign", "AssignIcon" },
                        { "System.Activities.Statements.CancellationScope", "CancellationScopeIcon" },
                        { "System.Activities.Statements.ClearCollection`1", "ClearCollectionIcon" },
                        { "System.Activities.Statements.CompensableActivity", "CompensableActivityIcon" },
                        { "System.Activities.Statements.Compensate", "CompensateIcon" },
                        { "System.Activities.Statements.Confirm", "ConfirmIcon" },
                        { "System.Activities.Statements.Delay", "DelayIcon" },
                        { "System.Activities.Statements.DoWhile", "DoWhileIcon" },
                        { "System.Activities.Statements.ExistsInCollection`1", "ExistsInCollectionIcon" },
                        { "System.Activities.Statements.Flowchart", "FlowchartIcon" },
                        { "System.Activities.Statements.FlowDecision", "FlowDecisionIcon" },
                        { "System.Activities.Statements.FlowSwitch`1", "FlowSwitchIcon" },
                        { "System.Activities.Statements.If", "IfIcon" },
                        { "System.Activities.Statements.Interop", "InteropIcon" },
                        { "System.Activities.Statements.InvokeDelegate", "InvokeDelegateIcon" },
                        { "System.Activities.Statements.InvokeMethod", "InvokeMethodIcon" },
                        { "System.Activities.Statements.NoPersistScope", "NoPersistScopeIcon" },
                        { "System.Activities.Statements.Parallel", "ParallelIcon" },
                        { "System.Activities.Statements.Persist", "PersistIcon" },
                        { "System.Activities.Statements.Pick", "PickIcon" },
                        { "System.Activities.Statements.PickBranch", "PickBranchIcon" },
                        { "System.Activities.Statements.RemoveFromCollection`1", "RemoveFromCollectionIcon" },
                        { "System.Activities.Statements.Rethrow", "RethrowIcon" },
                        { "System.Activities.Statements.Sequence", "SequenceIcon" },
                        { "System.Activities.Statements.State", "StateIcon" },
                        { "System.Activities.Statements.StateMachine", "StateMachineIcon" },
                        { "System.Activities.Statements.Switch`1", "SwitchIcon" },
                        { "System.Activities.Statements.TerminateWorkflow", "TerminateWorkflowIcon" },
                        { "System.Activities.Statements.Throw", "ThrowIcon" },
                        { "System.Activities.Statements.TransactionScope", "TransactionScopeIcon" },
                        { "System.Activities.Statements.TryCatch", "TryCatchIcon" },
                        { "System.Activities.Statements.While", "WhileIcon" },
                        { "System.Activities.Statements.WriteLine", "WriteLineIcon" },
                        { "System.Activities.Core.Presentation.FinalState", "FinalStateIcon" },
                        { "System.Activities.Core.Presentation.Factories.StateMachineWithInitialStateFactory", "StateMachineIcon" },
                        { "System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory`1", "ForEachIcon" },
                        { "System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory`1", "ParallelForEachIcon" },
                        { "System.Activities.Core.Presentation.Factories.PickWithTwoBranchesFactory", "PickIcon" },
                        { "System.ServiceModel.Activities.CorrelationScope", "CorrelationScopeIcon" },
                        { "System.ServiceModel.Activities.InitializeCorrelation", "InitializeCorrelationIcon" },
                        { "System.ServiceModel.Activities.Receive", "ReceiveIcon" },
                        { "System.ServiceModel.Activities.ReceiveReply", "ReceiveReplyIcon" },
                        { "System.ServiceModel.Activities.Send", "SendIcon" },
                        { "System.ServiceModel.Activities.SendReply", "SendReplyIcon" },
                        { "System.ServiceModel.Activities.TransactedReceiveScope", "TransactedReceiveScopeIcon" },
                        { "System.ServiceModel.Activities.Presentation.Factories.ReceiveAndSendReplyFactory", "ReceiveAndSendReplyIcon" },
                        { "System.ServiceModel.Activities.Presentation.Factories.SendAndReceiveReplyFactory", "SendAndReceiveReplyIcon" },
                    };
                }

                return iconNameMapping;
            }
        }

        internal static string GetIconResourceKey(string activityFullName)
        {
            if (!string.IsNullOrWhiteSpace(activityFullName) && IconNameMapping.ContainsKey(activityFullName))
            {
                return IconNameMapping[activityFullName];
            }

            return null;
        }

        internal static DrawingBrush GetBrushFromResource(string activityFullName)
        {
            string resourceKey = GetIconResourceKey(activityFullName);
            if (resourceKey == null)
            {
                return null;
            }

            var resourceDictionary = EditorResources.GetIcons();
            if (resourceDictionary != null && resourceDictionary.Contains(resourceKey))
            {
                DrawingBrush drawingBrush = resourceDictionary[resourceKey] as DrawingBrush;
                return drawingBrush;
            }

            return null;
        }
    }
}
