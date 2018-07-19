namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.CodeDom;
    using System.Reflection;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    #endregion

    // Interface for objects that support mining for workflow changes.
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IWorkflowChangeDiff
    {
        IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition);
    }
}
