//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    internal partial class ConnectorWithoutStartDot : Connector
    {
        public ConnectorWithoutStartDot()
        {
            this.InitializeComponent();
        }

        public override void SetLabelToolTip(object toolTip)
        {
            this.labelTextBlock.ToolTip = toolTip;
        }
    }
}
