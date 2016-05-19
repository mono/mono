//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics.Eventing;

    class DesignerPerfEventProvider
    {
        EventProvider provider = null;

        public DesignerPerfEventProvider()
        {
            try
            {
                this.provider = new EventProvider(new Guid("{B5697126-CBAF-4281-A983-7851DAF56454}"));
            }
            catch (PlatformNotSupportedException)
            {
                this.provider = null;
            }
        }

        public void WorkflowDesignerApplicationIdleAfterLoad()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerApplicationIdleAfterLoad);
            }
        }

        public void WorkflowDesignerDeserializeEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerDeserializeEnd);
            }
        }

        public void WorkflowDesignerDeserializeStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerDeserializeStart);
            }
        }

        public void WorkflowDesignerDrop()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerDrop);
            }
        }

        public void WorkflowDesignerIdleAfterDrop()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerIdleAfterDrop);
            }
        }

        public void WorkflowDesignerLoadComplete()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerLoadComplete);
            }
        }

        public void WorkflowDesignerLoadStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerLoadStart);
            }
        }

        public void WorkflowDesignerSerializeEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerSerializeEnd);
            }
        }

        public void WorkflowDesignerSerializeStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerSerializeStart);
            }
        }

        public void WorkflowDesignerExpressionEditorCompilationEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerExpressionEditorCompilationEnd);
            }
        }

        public void WorkflowDesignerExpressionEditorCompilationStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerExpressionEditorCompilationStart);
            }
        }

        public void WorkflowDesignerExpressionEditorLoaded()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerExpressionEditorLoaded);
            }
        }

        public void WorkflowDesignerExpressionEditorLoadStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerExpressionEditorLoadStart);
            }
        }

        public void WorkflowDesignerValidationEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerValidationEnd);
            }
        }

        public void WorkflowDesignerValidationStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerValidationStart);
            }
        }

        public void FlowchartDesignerLoadEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.FlowchartDesignerLoadEnd);
            }
        }

        public void FlowchartDesignerLoadStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.FlowchartDesignerLoadStart);
            }
        }

        public void FreeFormPanelMeasureEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.FreeFormPanelMeasureEnd);
            }
        }

        public void FreeFormPanelMeasureStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.FreeFormPanelMeasureStart);
            }
        }

        public void WorkflowDesignerCopyStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerCopyStart);
            }
        }

        public void WorkflowDesignerCopyEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerCopyEnd);
            }
        }

        public void WorkflowDesignerPasteStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerPasteStart);
            }
        }

        public void WorkflowDesignerPasteEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.WorkflowDesignerPasteEnd);
            }
        }

        public void DesignerTreeViewLoadChildrenStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.DesignerTreeViewLoadChildrenStart);
            }
        }

        public void DesignerTreeViewLoadChildrenEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.DesignerTreeViewLoadChildrenEnd);
            }
        }

        public void DesignerTreeViewUpdateStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.DesignerTreeViewUpdateStart);
            }
        }

        public void DesignerTreeViewUpdateEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.DesignerTreeViewUpdateEnd);
            }
        }

        public void DesignerTreeViewExpandStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.DesignerTreeViewExpandStart);
            }
        }

        public void DesignerTreeViewExpandEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.DesignerTreeViewExpandEnd);
            }
        }

        public void TypeBrowserApplicationIdleAfterShowDialog()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.TypeBrowserApplicationIdleAfterShowDialog);
            }
        }

        public void TypeBrowserOkPressed()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.TypeBrowserOkPressed);
            }
        }

        public void SelectionChangedStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.SelectionChangedStart);
            }
        }

        public void PropertyInspectorUpdatePropertyListStart()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.PropertyInspectorUpdatePropertyListStart);
            }
        }

        public void PropertyInspectorUpdatePropertyListEnd()
        {
            if (this.IsEnabled())
            {
                WriteEventHelper((int)DesignerPerfEvents.PropertyInspectorUpdatePropertyListEnd);
            }
        }

        private bool IsEnabled()
        {
            bool isEnabled = false;
            if (this.provider != null)
            {
                isEnabled = this.provider.IsEnabled();
            }
            return isEnabled;
        }

        private void WriteEventHelper(int eventId)
        {
            if (this.provider != null)
            {
                EventDescriptor descriptor = new EventDescriptor(eventId, 0, 0, 0, 0, 0, 0);
                this.provider.WriteEvent(ref descriptor);
            }
        }
    }
}
