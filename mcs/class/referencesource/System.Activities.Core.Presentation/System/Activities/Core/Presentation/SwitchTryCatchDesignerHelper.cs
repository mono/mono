//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;

    static class SwitchTryCatchDesignerHelper
    {
        public static void MakeRootDesigner(WorkflowViewElement wve)
        {
            DesignerView designerView = wve.Context.Services.GetService<DesignerView>();
            if (!wve.Equals(designerView.RootDesigner))
            {
                designerView.MakeRootDesigner(wve.ModelItem);
            }
        }

        public static void MakeParentRootDesigner<TParentType>(WorkflowViewElement wve)
            where TParentType : WorkflowViewElement
        {
            WorkflowViewElement view = FindParentDesigner<TParentType>(wve);
            if (view != null)
            {
                MakeRootDesigner(view);
            }
        }

        static TParentType FindParentDesigner<TParentType>(WorkflowViewElement wve) 
            where TParentType : WorkflowViewElement
        {
            ModelItem parent = wve.ModelItem.Parent;
            while (parent != null)
            {
                if (parent.View != null && parent.View is TParentType)
                {
                    return (TParentType)parent.View;
                }
                parent = parent.Parent;
            }
            return null;
        }
    }
}
