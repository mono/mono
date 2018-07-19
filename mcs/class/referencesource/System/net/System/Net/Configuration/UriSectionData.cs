using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Permissions;
using System.IO;
using System.Xml;
using System.Security;
using System.Diagnostics;

namespace System.Configuration
{
    internal sealed class UriSectionData
    {
        private UriIdnScope? idnScope;
        private bool? iriParsing;
        private Dictionary<string, SchemeSettingInternal> schemeSettings;

        public UriSectionData()
        {
            schemeSettings = new Dictionary<string, SchemeSettingInternal>();            
        }

        public UriIdnScope? IdnScope
        {
            get { return idnScope; }
            set { idnScope = value; }
        }

        public bool? IriParsing
        {
            get { return iriParsing; }
            set { iriParsing = value; }
        }

        public Dictionary<string, SchemeSettingInternal> SchemeSettings
        {
            get { return schemeSettings; }
        }
    }
}
