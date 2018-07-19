namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Resources;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;

    #region Class ActivityCodeDomSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityCodeDomSerializer : DependencyObjectCodeDomSerializer
    {
        public static readonly DependencyProperty MarkupFileNameProperty = DependencyProperty.RegisterAttached("MarkupFileName", typeof(string), typeof(ActivityCodeDomSerializer), new PropertyMetadata(null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        public ActivityCodeDomSerializer()
        {
        }

        #region CodeDomSerializer overrides
        public override object Serialize(IDesignerSerializationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            Activity activity = obj as Activity;
            if (activity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            if (Helpers.IsActivityLocked(activity))
                return null;

            CodeStatementCollection retVal = base.Serialize(manager, activity) as CodeStatementCollection;
            if (retVal != null)
            {
                Activity rootActivity = Helpers.GetRootActivity(activity);
                if (rootActivity != null && rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty) != null &&
                    (int)activity.GetValue(ActivityMarkupSerializer.StartLineProperty) != -1)
                {
                    foreach (CodeStatement statement in retVal)
                    {
                        if (!(statement is CodeCommentStatement))
                            statement.LinePragma = new CodeLinePragma((string)rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int)activity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                    }
                }
            }
            return retVal;
        }

        #endregion
    }
    #endregion

}
