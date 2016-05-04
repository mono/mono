//------------------------------------------------------------------------------
// <copyright file="ConfigXmlElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Configuration.Internal;
    using System.IO;
    using System.Xml;
    using System.Security.Permissions;

    internal sealed class ConfigXmlElement : XmlElement, IConfigErrorInfo {
        int _line;
        string _filename;

        public ConfigXmlElement( string filename, int line, string prefix, string localName, string namespaceUri, XmlDocument doc )
            : base( prefix, localName, namespaceUri, doc) {
            _line = line;
            _filename = filename;
        }
        int IConfigErrorInfo.LineNumber {
            get { return _line; }
        }
        string IConfigErrorInfo.Filename {
            get { return _filename; }
        }
        public override XmlNode CloneNode(bool deep) {
            XmlNode cloneNode = base.CloneNode(deep);
            ConfigXmlElement clone = cloneNode as ConfigXmlElement;
            if (clone != null) {
                clone._line = _line;
                clone._filename = _filename;
            }
            return cloneNode;
        }
    }
}
