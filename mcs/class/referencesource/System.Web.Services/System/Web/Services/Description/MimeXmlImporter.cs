//------------------------------------------------------------------------------
// <copyright file="MimeXmlImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.Services.Description {
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Serialization.Advanced;
    using System.Xml.Schema;
    using System.Xml;
    using System.Data;
    using System.CodeDom;

    internal class MimeXmlReturn : MimeReturn {
        XmlTypeMapping mapping;

        internal XmlTypeMapping TypeMapping {
            get { return mapping; }
            set { mapping = value; }
        }
    }

    internal class MimeXmlImporter : MimeImporter {
        XmlSchemaImporter importer;
        XmlCodeExporter exporter;

        internal override MimeParameterCollection ImportParameters() {
            return null;
        }

        internal override MimeReturn ImportReturn() {
            MimeContentBinding mimeContentBinding = (MimeContentBinding)ImportContext.OperationBinding.Output.Extensions.Find(typeof(MimeContentBinding));
            if (mimeContentBinding != null) {
                if (!ContentType.MatchesBase(mimeContentBinding.Type, ContentType.TextXml)) {
                     return null;
                }
                MimeReturn importedReturn = new MimeReturn();
                importedReturn.TypeName = typeof(XmlElement).FullName;
                importedReturn.ReaderType = typeof(XmlReturnReader);
                return importedReturn;
            }
            
            MimeXmlBinding mimeXmlBinding = (MimeXmlBinding)ImportContext.OperationBinding.Output.Extensions.Find(typeof(MimeXmlBinding));
            if (mimeXmlBinding != null) {
                MimeXmlReturn importedReturn = new MimeXmlReturn();
                MessagePart part;
                switch (ImportContext.OutputMessage.Parts.Count) {
                    case 0: 
                        throw new InvalidOperationException(Res.GetString(Res.MessageHasNoParts1, ImportContext.InputMessage.Name));
                    case 1: 
                        if (mimeXmlBinding.Part == null || mimeXmlBinding.Part.Length == 0) {
                            part = ImportContext.OutputMessage.Parts[0];
                        }
                        else {
                            part = ImportContext.OutputMessage.FindPartByName(mimeXmlBinding.Part);
                        }
                        break;
                    default:
                        part = ImportContext.OutputMessage.FindPartByName(mimeXmlBinding.Part);
                        break;
                }
                importedReturn.TypeMapping = Importer.ImportTypeMapping(part.Element);
                importedReturn.TypeName = importedReturn.TypeMapping.TypeFullName;
                importedReturn.ReaderType = typeof(XmlReturnReader);
                Exporter.AddMappingMetadata(importedReturn.Attributes, importedReturn.TypeMapping, string.Empty);
                return importedReturn;
            }
            return null;
        }

        XmlSchemaImporter Importer {
            get {
                if (importer == null) {
                    importer = new XmlSchemaImporter(ImportContext.ConcreteSchemas, ImportContext.ServiceImporter.CodeGenerationOptions, ImportContext.ServiceImporter.CodeGenerator, ImportContext.ImportContext);
                    foreach (Type extensionType in ImportContext.ServiceImporter.Extensions) {
                        importer.Extensions.Add(extensionType.FullName, extensionType);
                    }
                    importer.Extensions.Add(new System.Data.Design.TypedDataSetSchemaImporterExtension());
                    importer.Extensions.Add(new DataSetSchemaImporterExtension());
                }
                return importer;
            }
        }

        XmlCodeExporter Exporter {
            get {
                if (exporter == null)
                    exporter = new XmlCodeExporter(ImportContext.CodeNamespace, ImportContext.ServiceImporter.CodeCompileUnit, 
                        ImportContext.ServiceImporter.CodeGenerator, ImportContext.ServiceImporter.CodeGenerationOptions, ImportContext.ExportContext);
                return exporter;
            }
        }

        internal override void GenerateCode(MimeReturn[] importedReturns, MimeParameterCollection[] importedParameters) { 
            for (int i = 0; i < importedReturns.Length; i++) {
                if (importedReturns[i] is MimeXmlReturn) {
                    GenerateCode((MimeXmlReturn)importedReturns[i]);
                }
            }
        }

        void GenerateCode(MimeXmlReturn importedReturn) {
            Exporter.ExportTypeMapping(importedReturn.TypeMapping);
        }

        internal override void AddClassMetadata(CodeTypeDeclaration codeClass) {
            foreach (CodeAttributeDeclaration attribute in Exporter.IncludeMetadata) {
                codeClass.CustomAttributes.Add(attribute);
            }
        }
    }
}
