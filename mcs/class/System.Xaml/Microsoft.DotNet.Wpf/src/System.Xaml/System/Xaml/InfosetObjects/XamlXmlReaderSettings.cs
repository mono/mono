// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace System.Xaml
{
    public class XamlXmlReaderSettings : XamlReaderSettings
    {
        public string XmlLang { get; set; }
        public bool XmlSpacePreserve { get; set; }
        public bool SkipXmlCompatibilityProcessing { get; set; }
        public bool CloseInput { get; set; }

        internal Dictionary<string, string> _xmlnsDictionary;

        public XamlXmlReaderSettings()
        {
        }

        public XamlXmlReaderSettings(XamlXmlReaderSettings settings)
            : base(settings)
        {
            if (settings != null)
            {
                if (settings._xmlnsDictionary != null)
                {
                    _xmlnsDictionary = new Dictionary<string, string>(settings._xmlnsDictionary);
                }
                XmlLang = settings.XmlLang;
                XmlSpacePreserve = settings.XmlSpacePreserve;
                SkipXmlCompatibilityProcessing = settings.SkipXmlCompatibilityProcessing;
                CloseInput = settings.CloseInput;
            }
        }
    }
}
