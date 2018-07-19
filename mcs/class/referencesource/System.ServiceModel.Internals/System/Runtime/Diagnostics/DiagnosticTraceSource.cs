//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System;
    using System.Diagnostics;

    class DiagnosticTraceSource : TraceSource
    {
        const string PropagateActivityValue = "propagateActivity";
        internal DiagnosticTraceSource(string name)
            : base(name)
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { DiagnosticTraceSource.PropagateActivityValue };
        }

        internal bool PropagateActivity
        {
            get
            {
                bool retval = false;
                string attributeValue = this.Attributes[DiagnosticTraceSource.PropagateActivityValue];
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    if (!bool.TryParse(attributeValue, out retval))
                    {
                        retval = false;
                    }
                }
                return retval;
            }
            set
            {
                this.Attributes[DiagnosticTraceSource.PropagateActivityValue] = value.ToString();
            }
        }
    }
}
