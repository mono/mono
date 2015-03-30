
namespace System.Runtime.Serialization
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Runtime.Diagnostics;
	using System.ServiceModel.Diagnostics;
	using System.Security;
	using System.Xml;
	using System.Xml.Schema;
	using System.Xml.Serialization;
	using System.Runtime.Serialization.Diagnostics;

	class SchemaExporter
	{
        internal static void GetXmlTypeInfo(Type type, out XmlQualifiedName stableName, out XmlSchemaType xsdType, out bool hasRoot)
        {
            if (IsSpecialXmlType(type, out stableName, out xsdType, out hasRoot))
                return;
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.XmlResolver = null;
            InvokeSchemaProviderMethod(type, schemas, out stableName, out xsdType, out hasRoot);
            if (stableName.Name == null || stableName.Name.Length == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidXmlDataContractName, DataContract.GetClrTypeFullName(type))));
        }

        internal static bool IsSpecialXmlType(Type type, out XmlQualifiedName typeName, out XmlSchemaType xsdType, out bool hasRoot)
        {
            xsdType = null;
            hasRoot = true;
            if (type == Globals.TypeOfXmlElement || type == Globals.TypeOfXmlNodeArray)
            {
                string name = null;
                if (type == Globals.TypeOfXmlElement)
                {
                    xsdType = CreateAnyElementType();
                    name = "XmlElement";
                    hasRoot = false;
                }
                else
                {
                    xsdType = CreateAnyType();
                    name = "ArrayOfXmlNode";
                    hasRoot = true;
                }
                typeName = new XmlQualifiedName(name, DataContract.GetDefaultStableNamespace(type));
                return true;
            }
            typeName = null;
            return false;
        }

        static bool InvokeSchemaProviderMethod(Type clrType, XmlSchemaSet schemas, out XmlQualifiedName stableName, out XmlSchemaType xsdType, out bool hasRoot)
        {
            xsdType = null;
            hasRoot = true;
            object[] attrs = clrType.GetCustomAttributes(Globals.TypeOfXmlSchemaProviderAttribute, false);
            if (attrs == null || attrs.Length == 0)
            {
                stableName = DataContract.GetDefaultStableName(clrType);
                return false;
            }

            XmlSchemaProviderAttribute provider = (XmlSchemaProviderAttribute)attrs[0];
            if (provider.IsAny)
            {
                xsdType = CreateAnyElementType();
                hasRoot = false;
            }
            string methodName = provider.MethodName;
            if (methodName == null || methodName.Length == 0)
            {
                if (!provider.IsAny)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidGetSchemaMethod, DataContract.GetClrTypeFullName(clrType))));
                stableName = DataContract.GetDefaultStableName(clrType);
            }
            else
            {
                MethodInfo getMethod = clrType.GetMethod(methodName,  /*BindingFlags.DeclaredOnly |*/ BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(XmlSchemaSet) }, null);
                if (getMethod == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.MissingGetSchemaMethod, DataContract.GetClrTypeFullName(clrType), methodName)));

                if (!(Globals.TypeOfXmlQualifiedName.IsAssignableFrom(getMethod.ReturnType)) && !(Globals.TypeOfXmlSchemaType.IsAssignableFrom(getMethod.ReturnType)))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidReturnTypeOnGetSchemaMethod, DataContract.GetClrTypeFullName(clrType), methodName, DataContract.GetClrTypeFullName(getMethod.ReturnType), DataContract.GetClrTypeFullName(Globals.TypeOfXmlQualifiedName), typeof(XmlSchemaType))));

                object typeInfo = getMethod.Invoke(null, new object[] { schemas });

                if (provider.IsAny)
                {
                    if (typeInfo != null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidNonNullReturnValueByIsAny, DataContract.GetClrTypeFullName(clrType), methodName)));
                    stableName = DataContract.GetDefaultStableName(clrType);
                }
                else if (typeInfo == null)
                {
                    xsdType = CreateAnyElementType();
                    hasRoot = false;
                    stableName = DataContract.GetDefaultStableName(clrType);
                }
                else
                {
                    XmlSchemaType providerXsdType = typeInfo as XmlSchemaType;
                    if (providerXsdType != null)
                    {
                        string typeName = providerXsdType.Name;
                        string typeNs = null;
                        if (typeName == null || typeName.Length == 0)
                        {
                            DataContract.GetDefaultStableName(DataContract.GetClrTypeFullName(clrType), out typeName, out typeNs);
                            stableName = new XmlQualifiedName(typeName, typeNs);
                            providerXsdType.Annotation = GetSchemaAnnotation(ExportActualType(stableName, new XmlDocument()));
                            xsdType = providerXsdType;
                        }
                        else
                        {
                            foreach (XmlSchema schema in schemas.Schemas())
                            {
                                foreach (XmlSchemaObject schemaItem in schema.Items)
                                {
                                    if ((object)schemaItem == (object)providerXsdType)
                                    {
                                        typeNs = schema.TargetNamespace;
                                        if (typeNs == null)
                                            typeNs = String.Empty;
                                        break;
                                    }
                                }
                                if (typeNs != null)
                                    break;
                            }
                            if (typeNs == null)
                                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.MissingSchemaType, typeName, DataContract.GetClrTypeFullName(clrType))));
                            stableName = new XmlQualifiedName(typeName, typeNs);
                        }
                    }
                    else
                        stableName = (XmlQualifiedName)typeInfo;
                }
            }
            return true;
        }

        static XmlSchemaComplexType CreateAnyElementType()
        {
            XmlSchemaComplexType anyElementType = new XmlSchemaComplexType();
            anyElementType.IsMixed = false;
            anyElementType.Particle = new XmlSchemaSequence();
            XmlSchemaAny any = new XmlSchemaAny();
            any.MinOccurs = 0;
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            ((XmlSchemaSequence)anyElementType.Particle).Items.Add(any);
            return anyElementType;
        }

        static XmlSchemaAnnotation GetSchemaAnnotation(params XmlNode[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                return null;
            bool hasAnnotation = false;
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i] != null)
                {
                    hasAnnotation = true;
                    break;
                }
            if (!hasAnnotation)
                return null;

            XmlSchemaAnnotation annotation = new XmlSchemaAnnotation();
            XmlSchemaAppInfo appInfo = new XmlSchemaAppInfo();
            annotation.Items.Add(appInfo);
            appInfo.Markup = nodes;
            return annotation;
        }

        static XmlSchemaComplexType CreateAnyType()
        {
            XmlSchemaComplexType anyType = new XmlSchemaComplexType();
            anyType.IsMixed = true;
            anyType.Particle = new XmlSchemaSequence();
            XmlSchemaAny any = new XmlSchemaAny();
            any.MinOccurs = 0;
            any.MaxOccurs = Decimal.MaxValue;
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            ((XmlSchemaSequence)anyType.Particle).Items.Add(any);
            anyType.AnyAttribute = new XmlSchemaAnyAttribute();
            return anyType;
        }

        static XmlElement ExportActualType(XmlQualifiedName typeName, XmlDocument xmlDoc)
        {
            XmlElement actualTypeElement = xmlDoc.CreateElement(ActualTypeAnnotationName.Name, ActualTypeAnnotationName.Namespace);

            XmlAttribute nameAttribute = xmlDoc.CreateAttribute(Globals.ActualTypeNameAttribute);
            nameAttribute.Value = typeName.Name;
            actualTypeElement.Attributes.Append(nameAttribute);

            XmlAttribute nsAttribute = xmlDoc.CreateAttribute(Globals.ActualTypeNamespaceAttribute);
            nsAttribute.Value = typeName.Namespace;
            actualTypeElement.Attributes.Append(nsAttribute);

            return actualTypeElement;
        }

        static XmlQualifiedName actualTypeAnnotationName;
        internal static XmlQualifiedName ActualTypeAnnotationName
        {
            get
            {
                if (actualTypeAnnotationName == null)
                    actualTypeAnnotationName = new XmlQualifiedName(Globals.ActualTypeLocalName, Globals.SerializationNamespace);
                return actualTypeAnnotationName;
            }
        }
	}
}

