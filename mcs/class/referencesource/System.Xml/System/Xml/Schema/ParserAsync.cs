
namespace System.Xml.Schema {

    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.IO;
    using System.Diagnostics;

        using System.Threading.Tasks;
    
    internal sealed partial class Parser {

        public async Task< SchemaType > ParseAsync(XmlReader reader, string targetNamespace) {
            await StartParsingAsync(reader, targetNamespace).ConfigureAwait(false);
            while(ParseReaderNode() && await reader.ReadAsync().ConfigureAwait(false)) {}
            return FinishParsing();
        }

        public async Task StartParsingAsync(XmlReader reader, string targetNamespace) {
            this.reader = reader;
            positionInfo = PositionInfo.GetPositionInfo(reader);
            namespaceManager = reader.NamespaceManager;
            if (namespaceManager == null) {
                namespaceManager = new XmlNamespaceManager(nameTable);
                isProcessNamespaces = true;
            } 
            else {
                isProcessNamespaces = false;
            }
            while (reader.NodeType != XmlNodeType.Element && await reader.ReadAsync().ConfigureAwait(false)) {}

            markupDepth = int.MaxValue;
            schemaXmlDepth = reader.Depth;
            SchemaType rootType = schemaNames.SchemaTypeFromRoot(reader.LocalName, reader.NamespaceURI);
            
            string code;
            if (!CheckSchemaRoot(rootType, out code)) {
                throw new XmlSchemaException(code, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition);
            }
            
            if (schemaType == SchemaType.XSD) {
                schema = new XmlSchema();
                schema.BaseUri = new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute);
                builder = new XsdBuilder(reader, namespaceManager, schema, nameTable, schemaNames, eventHandler);
            }
            else {  
                Debug.Assert(schemaType == SchemaType.XDR);
                xdrSchema = new SchemaInfo();
                xdrSchema.SchemaType = SchemaType.XDR;
                builder = new XdrBuilder(reader, namespaceManager, xdrSchema, targetNamespace, nameTable, schemaNames, eventHandler);
                ((XdrBuilder)builder).XmlResolver = xmlResolver;
            }
        }

    };

} // namespace System.Xml
