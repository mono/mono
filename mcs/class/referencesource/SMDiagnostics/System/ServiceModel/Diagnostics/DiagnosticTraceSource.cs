//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;

    class DiagnosticTraceSource : PiiTraceSource
    {
        const string PropagateActivityValue = "propagateActivity";
        internal DiagnosticTraceSource(string name, string eventSourceName)
            : base(name, eventSourceName)
        {
        }

        internal DiagnosticTraceSource(string name, string eventSourceName, SourceLevels level)
            : base(name, eventSourceName, level)
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            string[] baseAttributes = base.GetSupportedAttributes();
            string[] supportedAttributes = new string[baseAttributes.Length + 1];
            for (int i = 0; i < baseAttributes.Length; i++)
            {
                supportedAttributes[i] = baseAttributes[i];
            }
            supportedAttributes[baseAttributes.Length] = DiagnosticTraceSource.PropagateActivityValue;

            return supportedAttributes;
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
