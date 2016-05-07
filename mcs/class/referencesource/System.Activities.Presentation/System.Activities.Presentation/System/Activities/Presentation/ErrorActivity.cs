//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


namespace System.Activities.Presentation
{
    using System;
    using System.Activities;
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.Runtime;
    using System.Xaml;
    using XamlDeferLoad = System.Windows.Markup.XamlDeferLoadAttribute;

    [Designer(typeof(ErrorActivity.ErrorActivityView))]
    internal class ErrorActivity : Activity
    {
        static readonly AttachableMemberIdentifier HasErrorActivitiesProperty =
            new AttachableMemberIdentifier(typeof(ErrorActivity), "HasErrorActivities");
        internal const string ErrorNodesProperty = "ErrorNodes";

        [Browsable(false)]
        [XamlDeferLoad(typeof(NodeListLoader), typeof(object))]
        public XamlNodeList ErrorNodes { get; set; }

        internal static bool GetHasErrorActivities(object target)
        {
            object result;
            if (AttachablePropertyServices.TryGetProperty(target, HasErrorActivitiesProperty, out result))
            {
                return (bool)result;
            }
            return false;
        }

        internal static void SetHasErrorActivities(object target, bool value)
        {
            AttachablePropertyServices.SetProperty(target, HasErrorActivitiesProperty, value);
        }

        internal static void WriteNodeList(XamlWriter writer, XamlNodeList nodeList)
        {
            // We need to pass the ErrorNodes contents through as a NodeList, because XOW doesn't
            // currently support unknown types, even inside a DeferLoad block.
            // But if a NodeList is written to XOW as a Value, XOW will unpack, forcing us to re-buffer
            // the nodes in our deferring loader. So we wrap the NodeList value inside a dummy StartObject.
            writer.WriteStartObject(XamlLanguage.Object);
            writer.WriteStartMember(XamlLanguage.Initialization);
            writer.WriteValue(nodeList);
            writer.WriteEndMember();
            writer.WriteEndObject();
        }

        internal class NodeListLoader : XamlDeferringLoader
        {
            public override object Load(XamlReader xamlReader, IServiceProvider serviceProvider)
            {
                // Expects a nodestream produced by WriteNodesList
                xamlReader.Read();
                xamlReader.Read();
                xamlReader.Read();
                Fx.Assert(xamlReader.NodeType == XamlNodeType.Value, "Expected Value node");
                return (XamlNodeList)xamlReader.Value;
            }

            public override XamlReader Save(object value, IServiceProvider serviceProvider)
            {
                return ((XamlNodeList)value).GetReader();
            }
        }

        internal class ErrorActivityView : WorkflowViewElement
        {
            public ErrorActivityView()
            {
                WorkflowViewService.ShowErrorInViewElement(this, SR.ActivityLoadError, null);
            }
        }
    }

    [Designer(typeof(ErrorActivity.ErrorActivityView))]
    internal class ErrorActivity<T> : Activity<T>
    {
        [Browsable(false)]
        [XamlDeferLoad(typeof(ErrorActivity.NodeListLoader), typeof(object))]
        public XamlNodeList ErrorNodes { get; set; }
    }
}
