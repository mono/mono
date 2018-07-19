//------------------------------------------------------------------------------
// <copyright file="ConfigXmlSignificantWhitespace.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Configuration.Internal;
    using System.IO;
    using System.Xml;
    using System.Security.Permissions;

    internal sealed class ConfigXmlSignificantWhitespace : XmlSignificantWhitespace, IConfigErrorInfo {
        public ConfigXmlSignificantWhitespace(string filename, int line, string strData, XmlDocument doc)
            : base(strData, doc) {
            _line = line;
            _filename = filename;
        }
        int _line;
        string _filename;

        int IConfigErrorInfo.LineNumber {
            get { return _line; }
        }
        string IConfigErrorInfo.Filename {
            get { return _filename; }
        }
        public override XmlNode CloneNode(bool deep) {
            XmlNode cloneNode = base.CloneNode(deep);
            ConfigXmlSignificantWhitespace clone = cloneNode as ConfigXmlSignificantWhitespace;
            if (clone != null) {
                clone._line = _line;
                clone._filename = _filename;
            }
            return cloneNode;
        }
    }
}
