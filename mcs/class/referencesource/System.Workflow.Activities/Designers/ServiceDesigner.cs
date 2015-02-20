namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Reflection.Emit;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using Microsoft.CSharp;
    using System.Diagnostics;
    using System.Collections.Generic;

    #endregion

    #region Class SequentialWorkflowDesigner
    [ActivityDesignerTheme(typeof(SequentialWorkflowDesignerTheme))]
    internal class SequentialWorkflowDesigner : SequentialWorkflowRootDesigner
    {
        #region Members, Constructor and Destructor
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            HelpText = DR.GetString(DR.SequentialWorkflowHelpText);
            Header.Text = DR.GetString(DR.StartSequentialWorkflow);
        }

        #endregion

        //TBD: NOTE, there needs to be two way protocol, the roots must indicate 
        //what activities that they support and the activities must indicate
        //what root they support. The latter, is missing right now.
        //The following method just checks for the former.
        protected override bool IsSupportedActivityType(Type activityType)
        {
            //we specifically, do not support state machine related activities.
            if (typeof(SetStateActivity).IsAssignableFrom(activityType) ||
                typeof(StateActivity).IsAssignableFrom(activityType) ||
                typeof(StateInitializationActivity).IsAssignableFrom(activityType) ||
                typeof(StateFinalizationActivity).IsAssignableFrom(activityType))
                return false;

            return base.IsSupportedActivityType(activityType);
        }

        #region MultiViewDesigner Support
        protected override void OnViewChanged(DesignerView view)
        {
            base.OnViewChanged(view);

            ActivityDesigner hostedDesigner = (ActiveView != null) ? ActiveView.AssociatedDesigner : null;
            if (hostedDesigner.Activity is FaultHandlersActivity)
            {
                Header.Text = DR.GetString(DR.WorkflowExceptions);
                HelpText = String.Empty;
            }
            else if (hostedDesigner.Activity is EventHandlersActivity)
            {
                Header.Text = DR.GetString(DR.WorkflowEvents);
                HelpText = String.Empty;
            }
            else if (hostedDesigner.Activity is CompensationHandlerActivity)
            {
                Header.Text = DR.GetString(DR.WorkflowCompensation);
                HelpText = String.Empty;
            }
            else if (hostedDesigner.Activity is CancellationHandlerActivity)
            {
                Header.Text = DR.GetString(DR.WorkflowCancellation);
                HelpText = String.Empty;
            }
            else
            {
                Header.Text = DR.GetString(DR.StartSequentialWorkflow);
                HelpText = DR.GetString(DR.SequentialWorkflowHelpText);
            }
        }
        #endregion
    }
    #endregion

    #region SequentialWorkflowWithDataContextDesignerTheme
    internal sealed class SequentialWorkflowDesignerTheme : CompositeDesignerTheme
    {
        public SequentialWorkflowDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.WatermarkImagePath = "System.Workflow.Activities.ActivityDesignerResources.SequentialWorkflowDesigner";
            this.WatermarkAlignment = DesignerContentAlignment.BottomRight;
            this.ShowDropShadow = true;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x49, 0x77, 0xB4);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
