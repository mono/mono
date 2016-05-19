namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Xml;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;

    [ActivityDesignerTheme(typeof(ConditionedActivityGroupDesignerTheme))]
    internal sealed class ConditionedActivityGroupDesigner : ActivityPreviewDesigner
    {
        #region Members, Constructor and Destructor

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            IExtenderListService extenderListService = (IExtenderListService)GetService(typeof(IExtenderListService));
            if (extenderListService != null)
            {
                bool foundCAGExtender = false;
                foreach (IExtenderProvider extenderProvider in extenderListService.GetExtenderProviders())
                {
                    if (extenderProvider.GetType() == typeof(ConditionPropertyProviderExtender))
                        foundCAGExtender = true;
                }

                if (!foundCAGExtender)
                {
                    IExtenderProviderService extenderProviderService = (IExtenderProviderService)GetService(typeof(IExtenderProviderService));
                    if (extenderProviderService != null)
                    {
                        extenderProviderService.AddExtenderProvider(new ConditionPropertyProviderExtender());
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion
    }

    #region ConditionedActivityGroupDesignerTheme
    internal sealed class ConditionedActivityGroupDesignerTheme : ActivityPreviewDesignerTheme
    {
        public ConditionedActivityGroupDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x6B, 0x6D, 0x6B);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF);
            this.PreviewForeColor = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
            this.PreviewBorderColor = Color.FromArgb(0xFF, 0x6B, 0x6D, 0x6B);
            this.PreviewBackColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        }
    }
    #endregion

    #region Class ConditionPropertyProviderExtender
    [ProvideProperty("WhenCondition", typeof(Activity))]
    [ProvideProperty("UnlessCondition", typeof(Activity))]
    internal sealed class ConditionPropertyProviderExtender : IExtenderProvider
    {
        internal ConditionPropertyProviderExtender()
        {
        }

        [SRCategory(SR.ConditionedActivityConditions)]
        [SRDescription(SR.WhenConditionDescr)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivityCondition GetWhenCondition(Activity activity)
        {
            if (activity.Parent is ConditionedActivityGroup)
                return activity.GetValue(ConditionedActivityGroup.WhenConditionProperty) as ActivityCondition;
            else
                return null;
        }

        [SRCategory(SR.ConditionedActivityConditions)]
        [SRDescription(SR.WhenConditionDescr)]
        public void SetWhenCondition(Activity activity, ActivityCondition handler)
        {
            if (activity.Parent is ConditionedActivityGroup)
                activity.SetValue(ConditionedActivityGroup.WhenConditionProperty, handler);
        }

        #region IExtenderProvider Members
        public bool CanExtend(object extendee)
        {
            return ((extendee != this) && (extendee is Activity) && (((Activity)extendee).Parent is ConditionedActivityGroup));
        }
        #endregion
    }
    #endregion
}
