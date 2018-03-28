namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;

    #region Class ParallelToolboxItem
    [Serializable]
    internal sealed class ParallelToolboxItem : ActivityToolboxItem
    {
        public ParallelToolboxItem(Type type)
            : base(type)
        {
        }
        private ParallelToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }
        protected override IComponent[] CreateComponentsCore(IDesignerHost designerHost)
        {
            CompositeActivity parallelActivity = new ParallelActivity();
            parallelActivity.Activities.Add(new SequenceActivity());
            parallelActivity.Activities.Add(new SequenceActivity());
            return (IComponent[])new IComponent[] { parallelActivity };
        }
    }
    #endregion

    #region Class ParallelDesigner
    [ActivityDesignerTheme(typeof(ParallelDesignerTheme))]
    internal sealed class ParallelDesigner : ParallelActivityDesigner
    {
        #region Properties and Methods
        protected override CompositeActivity OnCreateNewBranch()
        {
            return new SequenceActivity();
        }

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (activity.GetType() != typeof(SequenceActivity))
                    return false;
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }
        #endregion
    }
    #endregion

    #region ParallelDesignerTheme
    internal sealed class ParallelDesignerTheme : CompositeDesignerTheme
    {
        public ParallelDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xFF, 0x80, 0x00, 0x80);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
