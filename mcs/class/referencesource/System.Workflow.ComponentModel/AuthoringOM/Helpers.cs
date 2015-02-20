namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.Workflow.ComponentModel.Design;
    using System.Diagnostics;
    using System.Reflection;

    internal static class InternalHelpers
    {
        // Should only be called for BuiltIn activities
        internal static string GenerateQualifiedNameForLockedActivity(Activity activity, string id)
        {
            System.Text.StringBuilder sbQId = new System.Text.StringBuilder();

            // Walk up the parent chain to find the custom activity that contains this built-in activity
            // and prepend the ID of the custom activity to the front of the qualified ID of this activity.
            Debug.Assert(activity.Parent != null, "If this is a built-in activity, its parent should never be null.");
            string newID = (string.IsNullOrEmpty(id)) ? activity.Name : id;
            CompositeActivity customActivity = Helpers.GetDeclaringActivity(activity);
            if (customActivity != null)
                // 
                sbQId.Append(customActivity.QualifiedName).Append(".").Append(newID);
            else
                sbQId.Append(newID);

            return sbQId.ToString();
        }
    }
}
