//------------------------------------------------------------------------------
// <copyright file="SchemaCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    
    internal class SchemaCompiler {
        static StringCollection warnings;

        internal static StringCollection Warnings {
            get {
                if (warnings == null)
                    warnings = new StringCollection();
                return warnings;
            }
        }
        internal static StringCollection Compile(XmlSchemas schemas) {
            AddImports(schemas);
            Warnings.Clear();
            schemas.Compile(new ValidationEventHandler(ValidationCallbackWithErrorCode), true);
            return Warnings;
        }

        static void AddImport(XmlSchema schema, string ns) {
            if (schema.TargetNamespace == ns)
                return;
            foreach (XmlSchemaExternal ex in schema.Includes) {
                XmlSchemaImport import = ex as XmlSchemaImport;
                if (import != null && import.Namespace == ns) {
                    return;
                }
            }
            XmlSchemaImport newImport = new XmlSchemaImport();
            newImport.Namespace = ns;
            schema.Includes.Add(newImport);
        }

        static void AddImports(XmlSchemas schemas) {
            foreach (XmlSchema schema in schemas) {
                AddImport(schema, Soap.Encoding);
                AddImport(schema, ServiceDescription.Namespace);
            }
        }

        internal static string WarningDetails(XmlSchemaException exception, string message) {
            string details;
            XmlSchemaObject source = exception.SourceSchemaObject;
            if (exception.LineNumber == 0 && exception.LinePosition == 0) {
                details = GetSchemaItem(source, null, message);
            }
            else {
                string ns = null;
                if (source != null) {
                    while (source.Parent != null) {
                        source = source.Parent;
                    }
                    if (source is XmlSchema) {
                        ns = ((XmlSchema)source).TargetNamespace;
                    }
                }
                details = Res.GetString(Res.SchemaSyntaxErrorDetails, ns, message, exception.LineNumber, exception.LinePosition);
            }
            return details;
        }

        static string GetSchemaItem(XmlSchemaObject o, string ns, string details) {
            if (o == null) {
                return null;
            }
            while (o.Parent != null && !(o.Parent is XmlSchema)) {
                o = o.Parent;
            }
            if (ns == null || ns.Length == 0) {
                XmlSchemaObject tmp = o;
                while (tmp.Parent != null) {
                    tmp = tmp.Parent;
                }
                if (tmp is XmlSchema) {
                    ns = ((XmlSchema)tmp).TargetNamespace;
                }
            }
            string item = null;
            if (o is XmlSchemaNotation) {
                item = Res.GetString(Res.XmlSchemaNamedItem, ns, "notation", ((XmlSchemaNotation)o).Name, details);
            }
            else if (o is XmlSchemaGroup) {
                item = Res.GetString(Res.XmlSchemaNamedItem, ns, "group", ((XmlSchemaGroup)o).Name, details);
            }
            else if (o is XmlSchemaElement) {
                XmlSchemaElement e = ((XmlSchemaElement)o);
                if (e.Name == null || e.Name.Length == 0) {
                    XmlQualifiedName parentName = GetParentName(o);
                    // Element reference '{0}' declared in schema type '{1}' from namespace '{2}'
                    item = Res.GetString(Res.XmlSchemaElementReference, e.RefName.ToString(), parentName.Name, parentName.Namespace);
                }
                else {
                    item = Res.GetString(Res.XmlSchemaNamedItem, ns, "element", e.Name, details);
                }
            }
            else if (o is XmlSchemaType) {
                item = Res.GetString(Res.XmlSchemaNamedItem, ns, o.GetType() == typeof(XmlSchemaSimpleType) ? "simpleType" : "complexType", ((XmlSchemaType)o).Name, details);
            }
            else if (o is XmlSchemaAttributeGroup) {
                item = Res.GetString(Res.XmlSchemaNamedItem, ns, "attributeGroup", ((XmlSchemaAttributeGroup)o).Name, details);
            }
            else if (o is XmlSchemaAttribute) {
                XmlSchemaAttribute a = ((XmlSchemaAttribute)o);
                if (a.Name == null || a.Name.Length == 0) {
                    XmlQualifiedName parentName = GetParentName(o);
                    // Attribure reference '{0}' declared in schema type '{1}' from namespace '{2}'
                    return Res.GetString(Res.XmlSchemaAttributeReference, a.RefName.ToString(), parentName.Name, parentName.Namespace);
                }
                else {
                    item = Res.GetString(Res.XmlSchemaNamedItem, ns, "attribute", a.Name, details);
                }

            }
            else if (o is XmlSchemaContent) {
                XmlQualifiedName parentName = GetParentName(o);
                // Check content definition of schema type '{0}' from namespace '{1}'. {2}
                item = Res.GetString(Res.XmlSchemaContentDef, parentName.Name, parentName.Namespace, details);
            }
            else if (o is XmlSchemaExternal) {
                string itemType = o is XmlSchemaImport ? "import" : o is XmlSchemaInclude ? "include" : o is XmlSchemaRedefine ? "redefine" : o.GetType().Name;
                item = Res.GetString(Res.XmlSchemaItem, ns, itemType, details);
            }
            else if (o is XmlSchema) {
                item = Res.GetString(Res.XmlSchema, ns, details);
            }
            else {
                item = Res.GetString(Res.XmlSchemaNamedItem, ns, o.GetType().Name, null, details);
            }

            return item;
        }

        internal static XmlQualifiedName GetParentName(XmlSchemaObject item) {
            while (item.Parent != null) {
                if (item.Parent is XmlSchemaType) {
                    XmlSchemaType type = (XmlSchemaType)item.Parent;
                    if (type.Name != null && type.Name.Length != 0) {
                        return type.QualifiedName;
                    }
                }
                item = item.Parent;
            }
            return XmlQualifiedName.Empty;
        }

        private static void ValidationCallbackWithErrorCode (object sender, ValidationEventArgs args) {
            Warnings.Add(Res.GetString(args.Severity == XmlSeverityType.Error ? Res.SchemaValidationError : Res.SchemaValidationWarning, WarningDetails(args.Exception, args.Message)));
        }
    }
}
