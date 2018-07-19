//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.ComponentModel;
    using System.Activities.Statements;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Core.Presentation.Themes;
    using System.Linq;
    using System.Activities.Core.Presentation;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Activities;
    using System.Windows;
    using System.Runtime;
    using System.Globalization;



    static class ActivityDesignerHelper
    {
        public const string ChannelBasedCorrelationKey = "ChannelBasedCorrelation";

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
            Justification = "This is a TryGet pattern that requires out parameters")]
        public static bool IsItemInSequence(this ModelItem item, out ModelItem sequence)
        {
            if (null == item)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }

            bool result = false;
            int level = 0;

            Func<ModelItem, bool> isInSequencePredicate = (p) =>
            {
                switch (level)
                {
                    case 0:
                        ++level;
                        return (p is ModelItemCollection);

                    case 1:
                        ++level;
                        result = typeof(Sequence).IsAssignableFrom(p.ItemType);
                        return result;

                    default:
                        return false;
                };
            };

            ModelItem container = item.GetParentEnumerator(isInSequencePredicate).LastOrDefault();
            sequence = result ? container : null;
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
            Justification = "This is a TryGet pattern that requires out parameters")]
        public static bool IsItemInFlowchart(this ModelItem item, out ModelItem flowchart, out ModelItem flowStep)
        {
            if (null == item)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }

            bool result = false;
            int level = 0;
            ModelItem flowStepContainer = null;

            flowchart = null;
            flowStep = null;

            Func<ModelItem, bool> isInFlowchartPredicate = (p) =>
            {
                switch (level)
                {
                    case 0:
                        ++level;
                        flowStepContainer = typeof(FlowStep).IsAssignableFrom(p.ItemType) ? p : null;
                        return null != flowStepContainer;

                    case 1:
                        ++level;
                        return (p is ModelItemCollection);

                    case 2:
                        ++level;
                        result = (typeof(Flowchart).IsAssignableFrom(p.ItemType));
                        return result;

                    default:
                        return false;
                }
            };

            ModelItem container = item.GetParentEnumerator(isInFlowchartPredicate).LastOrDefault();

            if (result)
            {
                flowchart = container;
                flowStep = flowStepContainer;
            }

            return result;
        }

        public static bool IsMessagingActivity(this ModelItem item)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }

            bool result =
                item.IsAssignableFrom<Receive>() ||
                item.IsAssignableFrom<Send>() ||
                item.IsAssignableFrom<ReceiveReply>() ||
                item.IsAssignableFrom<SendReply>();

            return result;
        }

        public static string GenerateUniqueVariableNameForContext(DependencyObject context, string prefix)
        {
            if (null == context)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            var viewElement = context as WorkflowViewElement;
            if (null == viewElement)
            {
                var msg = StringResourceDictionary.Instance.GetString("activityFactoryWrongTarget");
                throw FxTrace.Exception.Argument("target", msg);
            }

            string name;
            var scope = VariableHelper.FindCommonVariableScope(viewElement.ModelItem, viewElement.ModelItem);
            if (null == scope)
            {
                name = string.Format(CultureInfo.CurrentUICulture, "{0}{1}", prefix, 1);
            }
            else
            {
                name = scope.GetVariableCollection().CreateUniqueVariableName(prefix, 1);
            }
            return name;

        }
    }
}
