#region Using directives

using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Win32;
#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    #region Interface IExpressionEvaluationFrame

    public delegate void DebugEngineCallback();

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IExpressionEvaluationFrame
    {
        void CreateEvaluationFrame(IInstanceTable instanceTable, DebugEngineCallback callback);
    }

    #endregion
}
