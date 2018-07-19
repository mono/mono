//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Schema;

    static class SchemaHelper
    {
        static internal void AddElementForm(XmlSchemaElement element, XmlSchema schema)
        {
            if (schema.ElementFormDefault != XmlSchemaForm.Qualified)
                element.Form = XmlSchemaForm.Qualified;
        }

        static internal void AddElementToSchema(XmlSchemaElement element, XmlSchema schema, XmlSchemaSet schemaSet)
        {
            XmlSchemaElement existingElement = (XmlSchemaElement)schema.Elements[new XmlQualifiedName(element.Name, schema.TargetNamespace)];
            if (existingElement != null)
            {
                if (element.SchemaType == existingElement.SchemaType && element.SchemaTypeName == existingElement.SchemaTypeName)
                    return;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxConflictingGlobalElement, element.Name, schema.TargetNamespace, GetTypeName(element), GetTypeName(existingElement))));
            }

            schema.Items.Add(element);
            if (!element.SchemaTypeName.IsEmpty)
                AddImportToSchema(element.SchemaTypeName.Namespace, schema);

            schemaSet.Reprocess(schema);
        }

        static internal void AddImportToSchema(string ns, XmlSchema schema)
        {
            if (NamespacesEqual(ns, schema.TargetNamespace)
                || NamespacesEqual(ns, XmlSchema.Namespace)
                || NamespacesEqual(ns, XmlSchema.InstanceNamespace))
                return;

            foreach (object item in schema.Includes)
            {
                if (item is XmlSchemaImport && NamespacesEqual(ns, ((XmlSchemaImport)item).Namespace))
                    return;
            }

            XmlSchemaImport import = new XmlSchemaImport();
            if (ns != null && ns.Length > 0)
                import.Namespace = ns;
            schema.Includes.Add(import);
        }

        static internal void AddTypeToSchema(XmlSchemaType type, XmlSchema schema, XmlSchemaSet schemaSet)
        {
            XmlSchemaType existingType = (XmlSchemaType)schema.SchemaTypes[new XmlQualifiedName(type.Name, schema.TargetNamespace)];
            if (existingType != null)
            {
                if (existingType == type)
                    return;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxConflictingGlobalType, type.Name, schema.TargetNamespace)));
            }

            schema.Items.Add(type);

            schemaSet.Reprocess(schema);
        }

        static internal XmlSchema GetSchema(string ns, XmlSchemaSet schemaSet)
        {
            if (ns == null) { ns = String.Empty; }

            ICollection schemas = schemaSet.Schemas();
            foreach (XmlSchema existingSchema in schemas)
            {
                if ((existingSchema.TargetNamespace == null && ns.Length == 0) || ns.Equals(existingSchema.TargetNamespace))
                    return existingSchema;
            }

            XmlSchema schema = new XmlSchema();
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            if (ns.Length > 0)
                schema.TargetNamespace = ns;
            schemaSet.Add(schema);
            return schema;
        }

        static string GetTypeName(XmlSchemaElement element)
        {
            if (element.SchemaType != null)
                return "anonymous";
            if (!element.SchemaTypeName.IsEmpty)
                return element.SchemaTypeName.ToString();
            return String.Empty;
        }

        // Compare XmlSchemaElement properties set by WsdlExporter: do not check element QName, 
        // this code only called for elements with matching name and namespace
        internal static bool IsMatch(XmlSchemaElement e1, XmlSchemaElement e2)
        {
            // this code only called for elements with matching name and namespace
            Fx.Assert(e1.Name == e2.Name, "");
            // Anonymous types never match
            if (e1.SchemaType != null || e2.SchemaType != null)
                return false;
            if (e1.SchemaTypeName != e2.SchemaTypeName)
                return false;
            // do not need to check parent Schema.ElementFormDefault: see AddElementForm in this class.
            if (e1.Form != e2.Form)
                return false;
            if (e1.IsNillable != e2.IsNillable)
                return false;
            return true;
        }

        static internal bool NamespacesEqual(string ns1, string ns2)
        {
            if (ns1 == null || ns1.Length == 0)
                return (ns2 == null || ns2.Length == 0);
            else
                return ns1 == ns2;
        }

        static internal void Compile(XmlSchemaSet schemaSet, Collection<MetadataConversionError> errors)
        {
            ValidationEventHandler validationEventHandler = new ValidationEventHandler(delegate(object sender, ValidationEventArgs args)
            {
                HandleSchemaValidationError(sender, args, errors);
            }
            );
            schemaSet.ValidationEventHandler += validationEventHandler;
            schemaSet.Compile();
            schemaSet.ValidationEventHandler -= validationEventHandler;
        }

        static internal void HandleSchemaValidationError(object sender, ValidationEventArgs args, Collection<MetadataConversionError> errors)
        {
            MetadataConversionError warning = null;

            if (args.Exception != null && args.Exception.SourceUri != null)
            {
                XmlSchemaException ex = args.Exception;
                warning = new MetadataConversionError(SR.GetString(SR.SchemaValidationError, ex.SourceUri, ex.LineNumber, ex.LinePosition, ex.Message));
            }
            else
            {
                warning = new MetadataConversionError(SR.GetString(SR.GeneralSchemaValidationError, args.Message));
            }

            if (!errors.Contains(warning))
                errors.Add(warning);
        }

        static IList<string> xsdValueTypePrimitives = new string[]
        {
            "boolean", "float", "double", "decimal", "long", "unsignedLong", "int", "unsignedInt", "short", "unsignedShort", "byte", "unsignedByte", 
            "duration", "dateTime", "integer", "positiveInteger", "negativeInteger", "nonPositiveInteger"
        };

        static IList<string> dataContractPrimitives = new string[]
        {
            "char", "guid"
        };

        static IList<string> xmlSerializerPrimitives = new string[]
        {
            "char", "guid"
        };

        static string dataContractSerializerNamespace = "http://schemas.microsoft.com/2003/10/Serialization/";
        static string xmlSerializerNamespace = "http://microsoft.com/wsdl/types/";

        internal static bool IsElementValueType(XmlSchemaElement element)
        {
            XmlQualifiedName typeName = element.SchemaTypeName;
            if (typeName == null || typeName.IsEmpty)
                return false;
            if (typeName.Namespace == XmlSchema.Namespace)
                return xsdValueTypePrimitives.Contains(typeName.Name);
            if (typeName.Namespace == dataContractSerializerNamespace)
                return dataContractPrimitives.Contains(typeName.Name);
            if (typeName.Namespace == xmlSerializerNamespace)
                return dataContractPrimitives.Contains(typeName.Name);
            return false;
        }
    }
}
