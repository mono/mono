//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.Collections;
    using System.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.Activities.Design;
    using System.ServiceModel;
    using System.Workflow.ComponentModel.Compiler;


    internal class SendActivityDesignerTheme : ActivityDesignerTheme
    {
        public SendActivityDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            base.BackColorStart = Color.FromArgb(255, 255, 255);
            base.BackColorEnd = Color.FromArgb(200, 200, 200);
            base.BorderColor = Color.FromArgb(200, 200, 200);
            base.ForeColor = Color.FromArgb(80, 80, 80);
            base.BackgroundStyle = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
        }

    }

}
