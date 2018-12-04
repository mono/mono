// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Xml;
using MS.Internal.Xaml;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Parser;
using System.Xaml.MS.Impl;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Diagnostics;
using System.IO;

namespace System.Xaml
{
    public class XamlXmlReader : XamlReader, IXamlLineInfo
    {
        XamlParserContext _context;
        IEnumerator<XamlNode> _nodeStream;

        XamlNode _current;
        LineInfo _currentLineInfo;
        XamlNode _endOfStreamNode;

        XamlXmlReaderSettings _mergedSettings;

        public XamlXmlReader(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }

            Initialize(xmlReader, null, null);
        }

        public XamlXmlReader(XmlReader xmlReader, XamlXmlReaderSettings settings)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }

            Initialize(xmlReader, null, settings);
        }

        public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }

            Initialize(xmlReader, schemaContext, null);
        }

        public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }

            Initialize(xmlReader, schemaContext, settings);
        }

        public XamlXmlReader(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            Initialize(CreateXmlReader(fileName, null), null, null);
        }

        public XamlXmlReader(string fileName, XamlXmlReaderSettings settings)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            Initialize(CreateXmlReader(fileName, settings), null, settings);
        }

        public XamlXmlReader(string fileName, XamlSchemaContext schemaContext)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }

            Initialize(CreateXmlReader(fileName, null), schemaContext, null);
        }

        public XamlXmlReader(string fileName, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }

            Initialize(CreateXmlReader(fileName, settings), schemaContext, settings);
        }

        private XmlReader CreateXmlReader(string fileName, XamlXmlReaderSettings settings)
        {
            bool closeInput = (settings == null) ? true : settings.CloseInput;
            return XmlReader.Create(fileName, new XmlReaderSettings { CloseInput = closeInput, DtdProcessing = DtdProcessing.Prohibit });
        }

        public XamlXmlReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            Initialize(CreateXmlReader(stream, null), null, null);
        }

        public XamlXmlReader(Stream stream, XamlXmlReaderSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            Initialize(CreateXmlReader(stream, settings), null, settings);
        }

        public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }

            Initialize(CreateXmlReader(stream, null), schemaContext, null);
        }

        public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }

            Initialize(CreateXmlReader(stream, settings), schemaContext, settings);
        }

        private XmlReader CreateXmlReader(Stream stream, XamlXmlReaderSettings settings)
        {
            bool closeInput = (settings != null) && settings.CloseInput;
            return XmlReader.Create(stream, new XmlReaderSettings { CloseInput = closeInput, DtdProcessing = DtdProcessing.Prohibit });
        }

        public XamlXmlReader(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            Initialize(CreateXmlReader(textReader, null), null, null);
        }

        public XamlXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            Initialize(CreateXmlReader(textReader, settings), null, settings);
        }

        public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }

            Initialize(CreateXmlReader(textReader, null), schemaContext, null);
        }

        public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }

            Initialize(CreateXmlReader(textReader, settings), schemaContext, settings);
        }

        private XmlReader CreateXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
        {
            bool closeInput = (settings != null) && settings.CloseInput;
            return XmlReader.Create(textReader, new XmlReaderSettings { CloseInput = closeInput, DtdProcessing = DtdProcessing.Prohibit });
        }

        private void Initialize(XmlReader givenXmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            XmlReader myXmlReader;

            _mergedSettings = (settings == null) ? new XamlXmlReaderSettings() : new XamlXmlReaderSettings(settings);
            //Wrap the xmlreader with a XmlCompatReader instance to apply MarkupCompat rules.
            if (!_mergedSettings.SkipXmlCompatibilityProcessing)
            {
                XmlCompatibilityReader mcReader =
                        new XmlCompatibilityReader(givenXmlReader,
                                new IsXmlNamespaceSupportedCallback(IsXmlNamespaceSupported)
                        );
                mcReader.Normalization = true;
                myXmlReader = mcReader;
            }
            else
            {   //Don't wrap the xmlreader with XmlCompatReader.
                // Useful for uses where users want to keep mc: content in the XamlNode stream.
                // Or have already processed the markup compat and want that extra perf.
                myXmlReader = givenXmlReader;
            }
            // Pick up the XmlReader settings to override the "settings" defaults.
            if (!String.IsNullOrEmpty(myXmlReader.BaseURI))
            {
                _mergedSettings.BaseUri = new Uri(myXmlReader.BaseURI);
            }
            if (myXmlReader.XmlSpace == XmlSpace.Preserve)
            {
                _mergedSettings.XmlSpacePreserve = true;
            }
            if (!String.IsNullOrEmpty(myXmlReader.XmlLang))
            {
                _mergedSettings.XmlLang = myXmlReader.XmlLang;
            }
            IXmlNamespaceResolver myXmlReaderNS = myXmlReader as IXmlNamespaceResolver;
            Dictionary<string, string> xmlnsDictionary = null;
            if (myXmlReaderNS != null)
            {
                IDictionary<string, string> rootNamespaces = myXmlReaderNS.GetNamespacesInScope(XmlNamespaceScope.Local);
                if (rootNamespaces != null)
                {
                    foreach (KeyValuePair<string, string> ns in rootNamespaces)
                    {
                        if (xmlnsDictionary == null)
                        {
                            xmlnsDictionary = new Dictionary<string, string>();
                        }
                        xmlnsDictionary[ns.Key] = ns.Value;
                    }
                }
            }

            if (schemaContext == null)
            {
                schemaContext = new XamlSchemaContext();
            }

            _endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);

            _context = new XamlParserContext(schemaContext, _mergedSettings.LocalAssembly);
            _context.AllowProtectedMembersOnRoot = _mergedSettings.AllowProtectedMembersOnRoot;
            _context.AddNamespacePrefix(KnownStrings.XmlPrefix, XamlLanguage.Xml1998Namespace);

            Func<string, string> namespaceResolver = myXmlReader.LookupNamespace;
            _context.XmlNamespaceResolver = namespaceResolver;

            XamlScanner xamlScanner = new XamlScanner(_context, myXmlReader, _mergedSettings);
            XamlPullParser parser = new XamlPullParser(_context, xamlScanner, _mergedSettings);
            _nodeStream = new NodeStreamSorter(_context, parser, _mergedSettings, xmlnsDictionary);
            _current = new XamlNode(XamlNode.InternalNodeType.StartOfStream);  // user must call Read() before using properties.
            _currentLineInfo = new LineInfo(0, 0);
        }

        #region XamlReader Members

        public override bool Read()
        {
            ThrowIfDisposed();
            do
            {
                if (_nodeStream.MoveNext())
                {
                    _current = _nodeStream.Current;
                    if (_current.NodeType == XamlNodeType.None)
                    {
                        if (_current.LineInfo != null)
                        {
                            _currentLineInfo = _current.LineInfo;
                        }
                        else if (_current.IsEof)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Assert(_current.IsEof, "Xaml Parser returned an illegal internal XamlNode");
                        }
                    }
                }
                else
                {
                    _current = _endOfStreamNode;
                    break;
                }
            } while (_current.NodeType == XamlNodeType.None);
            return !IsEof;
        }

        public override XamlNodeType NodeType
        {
            get { return _current.NodeType; }
        }

        public override bool IsEof
        {
            get { return _current.IsEof; }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return _current.NamespaceDeclaration; }
        }

        public override XamlType Type
        {
            get { return _current.XamlType; }
        }

        public override object Value
        {
            get { return _current.Value; }
        }

        public override XamlMember Member
        {
            get { return _current.Member; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return _context.SchemaContext; }
        }

        #endregion

        #region IXamlLineInfo Members

        public bool HasLineInfo
        {
            get { return _mergedSettings.ProvideLineInfo; }
        }

        public int LineNumber
        {
            get { return _currentLineInfo.LineNumber; }
        }

        public int LinePosition
        {
            get { return _currentLineInfo.LinePosition; }
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("XamlXmlReader");
            }
        }

        // Return true if the passed namespace is known, meaning that it maps
        // to a set of assemblies and clr namespaces
        internal bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
        {
            // 4 cases: refer to Framework\XamlParser.IsXmlNamespaceSupported
            // startins with clr-namespace:
            // Namespace is known
            // http://schemas.microsoft.com/winfx/2006/xaml/presentation/options
            // We're inside of a XmlDataIsland
            
            // First, substitute in the LocalAssembly if needed
            if (_mergedSettings.LocalAssembly != null)
            {
                string clrNs, assemblyName;
                if (ClrNamespaceUriParser.TryParseUri(xmlNamespace, out clrNs, out assemblyName) &&
                    String.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = _mergedSettings.LocalAssembly.FullName;
                    newXmlNamespace = ClrNamespaceUriParser.GetUri(clrNs, assemblyName);
                    return true;
                }
            }

            bool result = _context.SchemaContext.TryGetCompatibleXamlNamespace(xmlNamespace, out newXmlNamespace);
            if (newXmlNamespace == null)
            {
                newXmlNamespace = string.Empty;
            }

            
            // we need to treat all namespaces inside of XmlDataIslands as Supported.
            // we need to tree Freeze as known, if it is around... don't hardcode.
            //else if (xmlNamespace == XamlReaderHelper.PresentationOptionsNamespaceURI)
            //{
            //    // PresentationOptions is expected to be marked as 'ignorable' in most Xaml
            //    // so that other Xaml parsers don't have to interpret it, but this parser
            //    // does handle it to support it's Freeze attribute.
            //    return true;
            //}
            return result;
        }
    }
}
