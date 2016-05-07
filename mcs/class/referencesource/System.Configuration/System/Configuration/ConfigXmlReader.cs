//------------------------------------------------------------------------------
// <copyright file="ConfigXmlReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Net;

    internal sealed class ConfigXmlReader : XmlTextReader, IConfigErrorInfo {
        string  _rawXml;
        int     _lineOffset;
        string  _filename;

        // Used in a decrypted configuration section to locate
        // the line where the ecnrypted section begins.
        bool    _lineNumberIsConstant;

        internal ConfigXmlReader(string rawXml, string filename, int lineOffset) : 
                this(rawXml, filename, lineOffset, false) {
        }

        internal ConfigXmlReader(string rawXml, string filename, int lineOffset, bool lineNumberIsConstant) : 
                base(new StringReader(rawXml)) {

            _rawXml = rawXml;
            _filename = filename;
            _lineOffset = lineOffset;
            _lineNumberIsConstant = lineNumberIsConstant;

            Debug.Assert(!_lineNumberIsConstant || _lineOffset > 0, 
                        "!_lineNumberIsConstant || _lineOffset > 0");
        }

        internal ConfigXmlReader Clone() {
            return new ConfigXmlReader(_rawXml, _filename, _lineOffset, _lineNumberIsConstant);
        }

        int IConfigErrorInfo.LineNumber {
            get {
                if (_lineNumberIsConstant) {
                    return _lineOffset;
                }
                else if (_lineOffset > 0) {
                    return base.LineNumber + (_lineOffset - 1);
                }
                else {
                    return base.LineNumber;
                }
            }
        }
    
        string IConfigErrorInfo.Filename {
            get {
                return _filename;
            }
        }

        internal string RawXml {
            get {
                return _rawXml;
            }
        }
    }
}
