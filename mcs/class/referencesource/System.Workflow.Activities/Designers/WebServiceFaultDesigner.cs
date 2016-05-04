using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Windows.Forms.Design;
using System.Security.Permissions;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System.Workflow.Activities
{
    [ActivityDesignerTheme(typeof(WebServiceFaultDesignerTheme))]
    internal sealed class WebServiceFaultDesigner : ActivityDesigner
    {
        #region Properties and Methods
        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null)
            {
                // If the receive activity id changed, clear out parameters.
                if (e.Member.Name == "InputActivityName")
                {
                    TypeDescriptor.Refresh(e.Activity);
                }
            }
        }
        #endregion
    }
    #region WebServiceFaultDesignerTheme
    internal sealed class WebServiceFaultDesignerTheme : ActivityDesignerTheme
    {
        public WebServiceFaultDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x94, 0xB6, 0xF7);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xDF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xA5, 0xC3, 0xF7);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
