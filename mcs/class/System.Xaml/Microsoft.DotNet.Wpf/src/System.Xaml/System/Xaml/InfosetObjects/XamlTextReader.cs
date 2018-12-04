using System;
using System.Collections.Generic;
using System.Xml;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.Parser;
using MS.Internal.Xaml.MS.Impl;
using MS.Internal.Xaml.Context;
#else
using System.Xaml.Parser;
using System.Xaml.MS.Impl;
using System.Windows.Markup;
#endif

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
    class XamlTextReader : XamlReader, IXamlLineInfo
    {
        XamlParserContext _context;
        IEnumerator<XamlNode> _nodeStream;

        XamlNode _current;
        XamlNode _endOfStreamNode;

        XamlTextReaderSettings _mergedSettings;

        public XamlTextReader(XmlReader xmlReader)
        {
            Initialize(xmlReader, null, null);
        }

        public XamlTextReader(XmlReader xmlReader, XamlTextReaderSettings settings)
        {
            Initialize(xmlReader, null, settings);
        }

        public XamlTextReader(XmlReader xmlReader, XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
                throw new ArgumentNullException("schemaContext");

            Initialize(xmlReader, schemaContext, null);
        }

        public XamlTextReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlTextReaderSettings settings)
        {
            if (schemaContext == null)
                throw new ArgumentNullException("schemaContext");

            Initialize(xmlReader, schemaContext, settings);
        }

        private void Initialize(XmlReader givenXmlReader, XamlSchemaContext schemaContext, XamlTextReaderSettings settings)
        {
            XmlReader myXmlReader;

            if (givenXmlReader == null)
            {
                throw new ArgumentNullException("XmlReader is null");
            }

            _mergedSettings = (settings == null) ? new XamlTextReaderSettings() : new XamlTextReaderSettings(settings);

            //Wrap the xmlreader with a XmlCompatReader instance to apply MarkupCompat rules.
            if (!_mergedSettings.SkipXmlCompatibilityProcessing)
            {
                XmlCompatibilityReader mcReader =
                        new XmlCompatibilityReader(givenXmlReader,
                                new IsXmlNamespaceSupportedCallback(IsXmlNamespaceSupported)
                        );
                myXmlReader = mcReader;
            }
            else
            {   // Don't wrap the xmlreader with XmlCompatReader.
                // Useful for uses where users want to keep mc: content in the XamlNode stream.
                // Or have already processed the markup compat and want that extra perf.
                // We need to go make sure the parser thinks it knows mc: uri,
                // in case SkipXmlCompatibilityProcessing is true... likely won't work yet.
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

            if (schemaContext == null)
            {
                schemaContext = new XamlSchemaContext();
            }

            _endOfStreamNode = new InternalNode(InternalNodeType.EndOfStream);

            _context = (XamlParserContext)XamlContext.CreateContext(UsageMode.Parser, schemaContext,
                                                            _mergedSettings.LocalAssembly, false /*ignoreCanConvert*/);

            XamlScanner xamlScanner = new XamlScanner(_context, myXmlReader, _mergedSettings);
            XamlPullParser parser = new XamlPullParser(_context, xamlScanner, _mergedSettings);
            _nodeStream = new NodeStreamSorter(_context, parser, _mergedSettings);
            _current = _endOfStreamNode;  // user must call Read() before using properties.
        }

        #region XamlReader Members

        public override bool Read()
        {
            if (_nodeStream.MoveNext())
            {
                _current = _nodeStream.Current;
            }
            else
            {
                _current = _endOfStreamNode;
            }
            return !IsEof;
        }

        public override XamlNodeType NodeType
        {
            get { return _current.NodeType; }
        }

        public override bool IsEof
        {
            get { return XamlNode.IsEof(_current); }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return XamlNode.GetNamespaceDeclaration(_current); }
        }

        public override XamlType Type
        {
            get { return XamlNode.GetXamlType(_current); }
        }

        public override object Value
        {
            get { return XamlNode.GetValue(_current); }
        }

        public override XamlProperty Member
        {
            get { return XamlNode.GetMember(_current); }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return _context.SchemaContext; }
        }

        public override bool IsObjectFromMember
        {
            get { return XamlNode.GetIsObjectFromMember(_current); }
        }

        #endregion

        #region IXamlLineInfo Members

        public bool HasLineInfo
        {
            get { return _mergedSettings.ProvideLineInfo; }
        }

        public int LineNumber
        {
            get { return XamlNode.GetLineNumber(_current); }
        }

        public int LinePosition
        {
            get { return XamlNode.GetLinePosition(_current); }
        }

        #endregion

        // Return true if the passed namespace is known, meaning that it maps
        // to a set of assemblies and clr namespaces
        internal bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
        {
            newXmlNamespace = null;

            bool isSupported = _context.SchemaContext.XamlNamespaceExists(xmlNamespace, _mergedSettings.LocalAssembly);
            newXmlNamespace = xmlNamespace; // should probalby be upgraded via CompatibleWith

            // we should treat all namespaces inside of XmlDataIslands as Supported.
            return isSupported;

            // we need to tree Freeze as known, if it is around... don't hardcode.
            //else if (xmlNamespace == XamlReaderHelper.PresentationOptionsNamespaceURI)
            //{
            //    // PresentationOptions is expected to be marked as 'ignorable' in most Xaml
            //    // so that other Xaml parsers don't have to interpret it, but this parser
            //    // does handle it to support it's Freeze attribute.
            //    return true;
            //}
        }
        
    }
}
