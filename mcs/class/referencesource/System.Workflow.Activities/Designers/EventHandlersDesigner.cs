namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    #region Class EventHandlersDesigner
    [ActivityDesignerTheme(typeof(EventHandlersDesignerTheme))]
    internal sealed class EventHandlersDesigner : ActivityPreviewDesigner
    {
        #region Members, Constructor and Destructor
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            HelpText = DR.GetString(DR.DropEventsHere);
            ShowPreview = false;
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

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivity");

            if (parentActivityDesigner.Activity != null)
            {
                if (!(parentActivityDesigner.Activity is EventHandlingScopeActivity))
                    return false;
            }
            return base.CanBeParentedTo(parentActivityDesigner);
        }

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is EventDrivenActivity))
                    return false;
            }

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (Expanded && IsVisible)
                {
                    if (PreviewedDesigner != null || ContainedDesigners.Count > 0)
                        return base.FirstSelectableObject;
                    else if (ContainedDesigners.Count == 0)
                        return new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0).SelectableObject;
                }

                return null;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (Expanded && IsVisible)
                {
                    if (PreviewedDesigner != null || ContainedDesigners.Count > 0)
                        return base.LastSelectableObject;
                    else if (ContainedDesigners.Count == 0)
                        return new ConnectorHitTestInfo(this, HitTestLocations.Designer, GetConnectors().GetLength(0) - 1).SelectableObject;
                }

                return null;
            }
        }
        #endregion
    }
    #endregion

    #region EventHandlersDesignerTheme
    internal sealed class EventHandlersDesignerTheme : ActivityPreviewDesignerTheme
    {
        public EventHandlersDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x35, 0xFF, 0xFF, 0xB0);
            this.BackColorEnd = Color.FromArgb(0x35, 0xFF, 0xFF, 0xB0);
            this.PreviewForeColor = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
            this.PreviewBorderColor = Color.FromArgb(0xFF, 0x6B, 0x6D, 0x6B);
            this.PreviewBackColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        }
    }
    #endregion
}
