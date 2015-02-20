namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Design;


    #region CompensationDesigner
    [ActivityDesignerTheme(typeof(CompensationDesignerTheme))]
    internal sealed class CompensationHandlerActivityDesigner : SequentialActivityDesigner
    {
        #region Members, Constructor and Destructor
        public override ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                List<DesignerView> views = new List<DesignerView>();
                foreach (DesignerView view in base.Views)
                {
                    // disable the fault handlers, cancellation handler and compensation handler
                    if ((view.ViewId != 2) &&
                            (view.ViewId != 3) &&
                            (view.ViewId != 4)
                        )
                        views.Add(view);
                }
                return new ReadOnlyCollection<DesignerView>(views);
            }
        }

        #endregion

        #region Properties and Methods
        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (Helpers.IsFrameworkActivity(activity))
                    return false;
            }

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        #endregion
    }
    #endregion

    #region CompensationDesignerTheme
    internal sealed class CompensationDesignerTheme : CompositeDesignerTheme
    {
        public CompensationDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x35, 0xB0, 0xE0, 0xFF);
            this.BackColorEnd = Color.FromArgb(0x35, 0xB0, 0xE0, 0xFF);
        }
    }
    #endregion
}
