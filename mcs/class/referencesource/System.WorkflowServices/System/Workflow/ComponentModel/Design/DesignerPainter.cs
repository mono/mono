//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Workflow.ComponentModel.Design;
    using System.Windows.Forms;
    using System.Reflection;
    using System.Drawing;
    using System.Diagnostics;
    using System.ServiceModel;


    // <summary>
    // This is a helper class with static methods that dont fit anywhere but are useful in general
    // </summary>
    internal static class DesignerPainter
    {

        public static CompositeActivityDesigner GetRootDesigner(ActivityDesigner designer)
        {
            if (designer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("designer");
            }

            CompositeActivityDesigner rootDesigner = designer.ParentDesigner;
            while (!rootDesigner.IsRootDesigner && rootDesigner.ParentDesigner != null)
            {
                rootDesigner = rootDesigner.ParentDesigner;
            }
            return rootDesigner;
        }
        public static void PaintDesigner(ActivityDesigner activityDesigner, ActivityDesignerPaintEventArgs eventArgs)
        {
            if (activityDesigner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activityDesigner");
            }
            if (eventArgs == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("eventArgs");
            }
            ActivityDesigner parentDesigner = activityDesigner.ParentDesigner;
            if (parentDesigner == null)
            {
                // This designer is no more on the design surface , dont paint this.
                return;
            }
            if (!IsBranchVisible(activityDesigner))
            {
                return;
            }
            // special case designers contained inside activity preview designers ( only one of the contained designers is shown)
            bool visible = false;
            if (IsInsidePreviewDesignerBranch(activityDesigner, out visible))
            {
                if (visible)
                {
                    PaintDesignerInternal(activityDesigner, eventArgs);
                }
            }
            else
            {
                PaintDesignerInternal(activityDesigner, eventArgs);
            }
        }

        private static bool IsBranchVisible(ActivityDesigner activityDesigner)
        {
            ActivityDesigner currentDesigner = activityDesigner;
            ActivityDesigner parentDesigner = activityDesigner.ParentDesigner;
            while (!currentDesigner.IsRootDesigner)
            {
                if (!((CompositeActivityDesigner) parentDesigner).ContainedDesigners.Contains(currentDesigner))
                {
                    return false;
                }
                else
                {
                    currentDesigner = parentDesigner;
                    parentDesigner = parentDesigner.ParentDesigner;
                }
            }
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        // The above suppression is required because, the parentDesigner object is changeing inside the loop and it is 
        // not possible to cache the result of the cast as suggested by FxCop
        private static bool IsInsidePreviewDesignerBranch(ActivityDesigner activityDesigner, out bool visible)
        {
            visible = false;
            ActivityDesigner currentDesigner = activityDesigner;
            ActivityDesigner parentDesigner = activityDesigner.ParentDesigner;
            while (!currentDesigner.IsRootDesigner)
            {
                if (parentDesigner is ActivityPreviewDesigner)
                {
                    break;
                }
                else
                {
                    currentDesigner = parentDesigner;
                    parentDesigner = parentDesigner.ParentDesigner;
                }
            }
            if (parentDesigner is ActivityPreviewDesigner)
            {
                if (((ActivityPreviewDesigner) parentDesigner).IsContainedDesignerVisible(currentDesigner))
                {
                    visible = true;
                }
                return true;
            }
            return false;
        }

        private static void PaintDesignerInternal(ActivityDesigner activityDesigner, ActivityDesignerPaintEventArgs eventArgs)
        {
            IWorkflowDesignerMessageSink sink = (IWorkflowDesignerMessageSink) activityDesigner;
            sink.OnPaint(new PaintEventArgs(eventArgs.Graphics, eventArgs.ClipRectangle), eventArgs.ClipRectangle);
        }
    }

}
