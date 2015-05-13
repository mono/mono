namespace System.Diagnostics
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class UnescapedXmlDiagnosticData
    {
        private string _xmlString;

        public UnescapedXmlDiagnosticData(string xmlPayload)
        {
            this._xmlString = xmlPayload;
            if (this._xmlString == null)
            {
                this._xmlString = string.Empty;
            }
        }

        public override string ToString()
        {
            return this._xmlString;
        }

        public string UnescapedXml
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._xmlString;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._xmlString = value;
            }
        }
    }
}

