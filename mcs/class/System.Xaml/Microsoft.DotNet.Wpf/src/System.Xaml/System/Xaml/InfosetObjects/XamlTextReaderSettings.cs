using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml
#else
namespace System.Xaml
#endif    
{
#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XamlTextReaderSettings: XamlReaderSettings
    {
        public string XmlLang { get; set; }
        public bool XmlSpacePreserve { get; set; }
        public bool SkipXmlCompatibilityProcessing { get; set; }

        private Dictionary<string, string> _xmlnsDictionary;

        public XamlTextReaderSettings()
        {
            _xmlnsDictionary = new Dictionary<string, string>();
        }

        public XamlTextReaderSettings(XamlTextReaderSettings settings) : base(settings)
        {
            if (settings == null)
            {
                _xmlnsDictionary = new Dictionary<string, string>();
            }
            else
            {
                _xmlnsDictionary = settings._xmlnsDictionary;
                XmlLang = settings.XmlLang;
                XmlSpacePreserve = settings.XmlSpacePreserve;
                SkipXmlCompatibilityProcessing = settings.SkipXmlCompatibilityProcessing;
            }
        }

        public void AddNamespace(string prefix, string XamlNamespaceUri)
        {
            _xmlnsDictionary.Add(prefix, XamlNamespaceUri);
        }

        public Dictionary<string, string> XmlnsDictionary
        {
            get { return _xmlnsDictionary; }
        }
    }
}
