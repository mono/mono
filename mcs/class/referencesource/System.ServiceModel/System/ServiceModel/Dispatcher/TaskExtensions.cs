// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for Task and its generic counterpart(Task of T)
    /// </summary>
    internal static class TaskExtensions
    {
        private const string TaskAsAsyncResultMethodName = "AsAsyncResult";
        private static MethodInfo taskAsAsyncResultMethodInfo;

        public static MethodInfo TaskAsAsyncResultMethodInfo
        {
            get
            {
                if (taskAsAsyncResultMethodInfo == null)
                {
                    taskAsAsyncResultMethodInfo = typeof(System.Runtime.TaskExtensions).GetMethods().Where(m =>
                                                   m.IsGenericMethod && m.Name == TaskAsAsyncResultMethodName).First();
                    Fx.Assert(taskAsAsyncResultMethodInfo != null, "taskAsAsyncResultMethodInfo should not be null.");
                }

                return taskAsAsyncResultMethodInfo;
            }
        }

        public static MethodInfo MakeGenericMethod(Type genericArgument)
        {
            return TaskExtensions.TaskAsAsyncResultMethodInfo.MakeGenericMethod(genericArgument);
        }
    }
}
