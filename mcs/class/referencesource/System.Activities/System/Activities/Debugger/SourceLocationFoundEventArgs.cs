// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    public sealed class SourceLocationFoundEventArgs : EventArgs
    {
        private object target;
        private SourceLocation sourceLocation;
        private bool isValueNode;

        public SourceLocationFoundEventArgs(object target, SourceLocation sourceLocation)
        {
            UnitTestUtility.Assert(target != null, "Target cannot be null and is ensured by caller");
            UnitTestUtility.Assert(sourceLocation != null, "Target cannot be null and is ensured by caller");
            this.target = target;
            this.sourceLocation = sourceLocation;
        }

        internal SourceLocationFoundEventArgs(object target, SourceLocation sourceLocation, bool isValueNode)
            : this(target, sourceLocation)
        {
            this.isValueNode = isValueNode;
        }

        public object Target
        {
            get { return this.target; }
        }

        public SourceLocation SourceLocation
        {
            get { return this.sourceLocation; }
        }

        internal bool IsValueNode
        {
            get { return this.isValueNode; }
        }
    }
}
