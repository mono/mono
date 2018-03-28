namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Windows.Forms.Design;

    #region Class ReplicatorDesigner

    internal sealed class ReplicatorDesigner : System.Workflow.Activities.SequenceDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity != null && compositeActivity.EnabledActivities.Count > 0)
                return false;

            if (activitiesToInsert.Count > 1)
                return false;

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }
    }

    #endregion
}
