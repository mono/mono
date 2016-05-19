//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Model;

    public abstract class UndoUnit
    {
        EditingContext context;
        ModelItem designerRoot;

        public string Description { get; set; }
        public abstract void Redo();
        public abstract void Undo();

        protected UndoUnit(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            this.context = context;
        }
        protected void SaveGlobalState()
        {
            DesignerView designerView = context.Services.GetService<DesignerView>();
            if (designerView != null && designerView.RootDesigner != null)
            {
                designerRoot = ((WorkflowViewElement)designerView.RootDesigner).ModelItem;
            }
        }

        protected void ApplyGlobalState()
        {
            DesignerView designerView = context.Services.GetService<DesignerView>();
            if (designerView != null && designerView.RootDesigner != null)
            {
                ModelItem currentDesignerRoot = ((WorkflowViewElement)designerView.RootDesigner).ModelItem;
                if (currentDesignerRoot != designerRoot)
                {
                    designerView.MakeRootDesigner(designerRoot);
                }
            }
        }
    }
}
