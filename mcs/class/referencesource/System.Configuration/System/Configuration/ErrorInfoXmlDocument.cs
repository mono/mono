//------------------------------------------------------------------------------
// <copyright file="ErrorInfoXmlDocument.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Configuration.Internal;
    using System.IO;
    using System.Xml;
    using System.Security.Permissions;

    // ErrorInfoXmlDocument - the default Xml Document doesn't track line numbers, and line
    // numbers are necessary to display source on config errors.
    // These classes wrap corresponding System.Xml types and also carry 
    // the necessary information for reporting filename / line numbers.
    // Note: these classes will go away if webdata ever decides to incorporate line numbers
    // into the default XML classes.  This class could also go away if webdata brings back
    // the UserData property to hang any info off of any node.
    internal sealed class ErrorInfoXmlDocument : XmlDocument, IConfigErrorInfo {
        XmlTextReader   _reader;
        int             _lineOffset;
        string          _filename;

        int IConfigErrorInfo.LineNumber {
            get { 
                if (_reader == null) {
                    return 0;
                }
                
                if (_lineOffset > 0) {
                    return _reader.LineNumber + _lineOffset - 1;
                }
                
                return _reader.LineNumber;
            }
        }

        internal int LineNumber { get { return ((IConfigErrorInfo)this).LineNumber; } }

        string IConfigErrorInfo.Filename { 
            get { return _filename; } 
        }

        public override void Load(string filename) {
            _filename = filename;
            try {
                _reader = new XmlTextReader(filename);
                _reader.XmlResolver = null;
                base.Load(_reader);
            }
            finally {
                if (_reader != null) {
                    _reader.Close();
                    _reader = null;
                }
            }
        }

        private void LoadFromConfigXmlReader(ConfigXmlReader reader) {
            IConfigErrorInfo err = (IConfigErrorInfo) reader;
            _filename = err.Filename;
            _lineOffset = err.LineNumber + 1;

            try {
                _reader = reader;
                base.Load(_reader);
            }
            finally {
                if (_reader != null) {
                    _reader.Close();
                    _reader = null;
                }
            }
        }

        static internal XmlNode CreateSectionXmlNode(ConfigXmlReader reader) {
            ErrorInfoXmlDocument doc = new ErrorInfoXmlDocument();
            doc.LoadFromConfigXmlReader(reader);
            XmlNode xmlNode = doc.DocumentElement;

            return xmlNode;
        }


        public override XmlAttribute CreateAttribute( string prefix, string localName, string namespaceUri ) {
            return new ConfigXmlAttribute( _filename, LineNumber, prefix, localName, namespaceUri, this );
        }
        public override XmlElement CreateElement( string prefix, string localName, string namespaceUri) {
            return new ConfigXmlElement( _filename, LineNumber, prefix, localName, namespaceUri, this );
        }
        public override XmlText CreateTextNode(String text) {
            return new ConfigXmlText( _filename, LineNumber, text, this );
        }
        public override XmlCDataSection CreateCDataSection(String data) {
            return new ConfigXmlCDataSection( _filename, LineNumber, data, this );
        }
        public override XmlComment CreateComment(String data) {
            return new ConfigXmlComment( _filename, LineNumber, data, this );
        }
        public override XmlSignificantWhitespace CreateSignificantWhitespace(String data) {
            return new ConfigXmlSignificantWhitespace( _filename, LineNumber, data, this );
        }
        public override XmlWhitespace CreateWhitespace(String data) {
            return new ConfigXmlWhitespace( _filename, LineNumber, data, this );
        }
    }
}
