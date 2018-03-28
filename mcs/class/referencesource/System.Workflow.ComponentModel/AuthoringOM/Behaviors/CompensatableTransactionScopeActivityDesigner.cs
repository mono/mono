namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Design;


    #region Class CompensatableTransactionScopeActivityDesigner

    internal sealed class CompensatableTransactionScopeActivityDesigner : SequenceDesigner
    {
        public override ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                List<DesignerView> views = new List<DesignerView>();
                foreach (DesignerView view in base.Views)
                {
                    // disable the exceptions view and cancellation handler view
                    Type activityType = view.UserData[SecondaryView.UserDataKey_ActivityType] as Type;
                    if (activityType != null &&
                        !typeof(CancellationHandlerActivity).IsAssignableFrom(activityType) &&
                        !typeof(FaultHandlersActivity).IsAssignableFrom(activityType))
                    {
                        views.Add(view);
                    }
                }
                return new ReadOnlyCollection<DesignerView>(views);
            }
        }
    }
    #endregion
}
