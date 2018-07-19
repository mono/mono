#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //

    #region Class CompositeDesignerAccessibleObject
    /// <summary>
    /// Represents accessibility object associated with CompositeActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeDesignerAccessibleObject : ActivityDesignerAccessibleObject
    {
        /// <summary>
        /// Constructor for accessibility object
        /// </summary>
        /// <param name="activityDesigner">Designer which is associated with accessibility object</param>
        public CompositeDesignerAccessibleObject(CompositeActivityDesigner activityDesigner)
            : base(activityDesigner)
        {
        }

        public override AccessibleStates State
        {
            get
            {
                AccessibleStates state = base.State;
                CompositeActivityDesigner compositeDesigner = base.ActivityDesigner as CompositeActivityDesigner;
                state |= (compositeDesigner.Expanded) ? AccessibleStates.Expanded : AccessibleStates.Collapsed;
                return state;
            }
        }

        public override AccessibleObject GetChild(int index)
        {
            CompositeActivityDesigner compositeDesigner = base.ActivityDesigner as CompositeActivityDesigner;
            if (index >= 0 && index < compositeDesigner.ContainedDesigners.Count)
                return compositeDesigner.ContainedDesigners[index].AccessibilityObject;
            else
                return base.GetChild(index);
        }

        public override int GetChildCount()
        {
            CompositeActivityDesigner compositeDesigner = base.ActivityDesigner as CompositeActivityDesigner;
            return compositeDesigner.ContainedDesigners.Count;
        }
    }
    #endregion

}
