// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System;

namespace System.Workflow.Runtime.Tracking
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class PreviousTrackingServiceAttribute : Attribute
    {
        string assemblyQualifiedName;

        public string AssemblyQualifiedName
        {
            get
            {
                return this.assemblyQualifiedName;
            }
        }

        // The parameter must be the exact TypeOfPreviousTrackingService.AssemblyQualifiedTypeName.
        public PreviousTrackingServiceAttribute(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                throw new ArgumentNullException(assemblyQualifiedName);

            this.assemblyQualifiedName = assemblyQualifiedName;
        }
    }
    
}
