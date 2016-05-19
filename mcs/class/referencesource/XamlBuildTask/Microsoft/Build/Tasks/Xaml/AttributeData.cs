//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System.Collections.Generic;
    using System.Xaml;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Globalization;
    using XamlBuildTask;

    public sealed class AttributeData 
    {
        static CultureInfo invariantEnglishUS = CultureInfo.ReadOnly(new CultureInfo("en-us", false));
        List<AttributeParameterData> parameters;
        Dictionary<string, AttributeParameterData> properties;        

        public XamlType Type
        {
            get;
            set;
        }

        public IList<AttributeParameterData> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new List<AttributeParameterData>();
                }
                return parameters;
            }
        }

        public IDictionary<string, AttributeParameterData> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, AttributeParameterData>();
                }
                return properties;
            }
        }

        // We get here when we are inside x:ClassAttributes or x:Property.Attributes. We expect the first element to be the Attribute SO.
        internal static AttributeData LoadAttributeData(XamlReader reader, NamespaceTable namespaceTable, string rootNamespace)
        {
            AttributeData attributeData = null;
            reader.Read();
            if (reader.NodeType == XamlNodeType.StartObject)
            {
                attributeData = new AttributeData { Type = reader.Type };

                bool readNext = false;
                while (readNext || reader.Read())
                {
                    namespaceTable.ManageNamespace(reader);
                    readNext = false;
                    if (reader.NodeType == XamlNodeType.StartMember)
                    {
                        if (reader.Member == XamlLanguage.Arguments)
                        {
                            foreach (AttributeParameterData parameterData in ReadParameters(reader.ReadSubtree(), namespaceTable, rootNamespace))
                            {
                                attributeData.Parameters.Add(parameterData);
                            }
                            readNext = true;
                        }
                        else if (!reader.Member.IsDirective)
                        {
                            KeyValuePair<string, AttributeParameterData> propertyInfo = ReadAttributeProperty(reader.ReadSubtree(), namespaceTable, rootNamespace);
                            attributeData.Properties.Add(propertyInfo.Key, propertyInfo.Value);
                            readNext = true;
                        }
                    }
                }
            }
            return attributeData;
        }

        // Read the Property on the attribute.
        private static KeyValuePair<string, AttributeParameterData> ReadAttributeProperty(XamlReader reader, NamespaceTable namespaceTable, string rootNamespace)
        {
            reader.Read();
            Fx.Assert(reader.Member != null, "Member element should not be null");
            XamlMember member = reader.Member;

            AttributeParameterData propertyInfo = new AttributeParameterData();

            if (member.Type != null && !member.Type.IsUnknown)
            {
                propertyInfo.Type = member.Type;
            } 
            
            ReadParamInfo(reader, member.Type, namespaceTable, rootNamespace, propertyInfo);
            return new KeyValuePair<string, AttributeParameterData>(member.Name, propertyInfo);
        }

        // Read the parameters on the Attribute. We expect the parameters to be in the order in which they are supposed to appear in the output code.
        // Here we are inside x:Arguments and we expect a list of parameters.
        private static IList<AttributeParameterData> ReadParameters(XamlReader reader, NamespaceTable namespaceTable, string rootNamespace)
        {
            IList<AttributeParameterData> parameters = new List<AttributeParameterData>();
            bool readNext = false;
            while (readNext || reader.Read())
            {
                readNext = false;
                if (reader.NodeType == XamlNodeType.StartObject)
                {
                    AttributeParameterData paramInfo = new AttributeParameterData();
                    ReadParamInfo(reader.ReadSubtree(), null, namespaceTable, rootNamespace, paramInfo);
                    parameters.Add(paramInfo);
                    readNext = true;
                }
            }
            return parameters;
        }

        // Read the actual parameter info, i.e. the type of the paramter and its value.
        // The first element could be a V or an SO.
        private static void ReadParamInfo(XamlReader reader, XamlType type, NamespaceTable namespaceTable, string rootNamespace, AttributeParameterData paramInfo)
        {
            reader.Read();
            
            bool readNext = false;
            do
            {
                readNext = false;
                if (reader.NodeType == XamlNodeType.StartObject && reader.Type == XamlLanguage.Array)
                {
                    paramInfo.IsArray = true;
                    XamlReader xamlArrayReader = reader.ReadSubtree();
                    xamlArrayReader.Read();
                    while (readNext || xamlArrayReader.Read())
                    {
                        readNext = false;
                        if (xamlArrayReader.NodeType == XamlNodeType.StartMember && xamlArrayReader.Member.Name == "Type")
                        {
                            xamlArrayReader.Read();
                            if (xamlArrayReader.NodeType == XamlNodeType.Value)
                            {
                                XamlType arrayType = XamlBuildTaskServices.GetXamlTypeFromString(xamlArrayReader.Value as string, namespaceTable, xamlArrayReader.SchemaContext);
                                if (arrayType.UnderlyingType != null)
                                {
                                    paramInfo.Type = xamlArrayReader.SchemaContext.GetXamlType(arrayType.UnderlyingType.MakeArrayType());
                                }
                                else
                                {
                                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.AttributeParameterTypeUnknown(arrayType)));
                                }
                            }
                        }
                        else if (xamlArrayReader.NodeType == XamlNodeType.StartObject)
                        {
                            AttributeParameterData arrayEntry = new AttributeParameterData();
                            ReadParamInfo(xamlArrayReader.ReadSubtree(), null, namespaceTable, rootNamespace, arrayEntry);
                            paramInfo.AddArrayContentsEntry(arrayEntry);
                            readNext = true;
                        }
                    }
                }                    
                else if (reader.NodeType == XamlNodeType.StartObject || reader.NodeType == XamlNodeType.Value)
                {
                    paramInfo.IsArray = false;
                    string paramVal;
                    object paramObj = null;
                    XamlType paramType;
                    GetParamValueType(reader.ReadSubtree(), type, namespaceTable, rootNamespace, out paramVal, out paramType, out paramObj);
                    paramInfo.TextValue = paramVal;
                    paramInfo.Type = paramType;
                    paramInfo.Value = paramObj;
                }
            } while (readNext || reader.Read());
        }

        // Get the paramter value. If the value is enclosed inside nodes of the type, then get the parameter type as well. 
        // Else infer the type from the type of the property.
        private static void GetParamValueType(XamlReader reader, XamlType type, NamespaceTable namespaceTable, string rootNamespace, out string paramValue, out XamlType paramType, out Object paramObj)
        {
            paramValue = String.Empty;
            paramType = type;
            paramObj = null;
            while (reader.Read())
            {                
                if (reader.NodeType == XamlNodeType.Value)
                {
                    if (paramType != null && paramType.UnderlyingType != null)
                    {
                        if (!IsSupportedParameterType(paramType.UnderlyingType) || paramType.UnderlyingType.IsArray)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.AttributeParamTypeNotSupported(paramType.UnderlyingType.FullName)));
                        }

                        paramValue = reader.Value as string;
                        if (typeof(Type).IsAssignableFrom(paramType.UnderlyingType))
                        {
                            Tuple<string, Type> result = ParseParameterValueTypeName(paramValue, rootNamespace, reader.SchemaContext, namespaceTable);
                            paramValue = result.Item1;
                            paramObj = result.Item2;
                        }
                        else
                        {
                            paramObj = ParseParameterValue(ref paramValue, paramType);
                        }
                    }                    
                    else
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.AttributeParameterTypeUnknown(reader.Value as string)));
                    }
                }
                else if (reader.NodeType == XamlNodeType.StartObject)
                {
                    if (reader.Type == XamlLanguage.Null)
                    {
                        paramValue = null;
                        paramType = null;
                    }
                    else if (reader.Type == XamlLanguage.Type)
                    {
                        paramType = reader.SchemaContext.GetXamlType(typeof(Type));
                    }
                    else
                    {
                        paramType = reader.Type;
                    }
                }
            }
        }

        internal static bool IsSupportedParameterType(Type type)
        {
            if (type.IsArray)
            {
                return IsSupportedParameterType(type.GetElementType());
            }
            return type.IsEnum || 
                type.IsPrimitive ||
                typeof(string) == type ||
                typeof(Type).IsAssignableFrom(type);
        }

        // Given a live value for an attribute parameter, returns the text that needs to be
        // code-gened for the value.
        internal static string GetParameterText(object value, XamlType paramType)
        {
            Type type = paramType.UnderlyingType;
            if (type.IsEnum)
            {
                // Note: this doesn't support flags enums with multiple flags set, but neither
                // does the existing Dev10 code
                return type.FullName + "." + value.ToString();
            }
            else if (typeof(Type).IsAssignableFrom(type))
            {
                return ((Type)value).FullName;
            }
            else if (type == typeof(String))
            {
                return (string)value;
            }
            else if (type.IsPrimitive)
            {
                TypeConverter typeConverter = paramType.TypeConverter.ConverterInstance;
                Fx.Assert(typeConverter != null, "All primitives have TypeConverters");
                return (string)typeConverter.ConvertTo(null, invariantEnglishUS, value, typeof(string));
            }
            else
            {
                throw Fx.AssertAndThrow("Unexpected attribute parameter type");
            }
        }

        // Given the text for an attribute parameter, parses it to a live value (if possible).
        internal static object GetParameterValue(ref string paramValue, XamlType paramType)
        {
            if (typeof(Type).IsAssignableFrom(paramType.UnderlyingType))
            {
                // We can't convert a CLR type name to a Type because we don't know what assembly it's from
                return null;
            }
            return ParseParameterValue(ref paramValue, paramType);
        }

        // Parses a XAML QName to a CLR Type Name (and the corresponding ROL type, if available)
        private static Tuple<string, Type> ParseParameterValueTypeName(string paramValue, string rootNamespace, XamlSchemaContext schemaContext, NamespaceTable namespaceTable)
        {
            XamlType xamlType = XamlBuildTaskServices.GetXamlTypeFromString(paramValue, namespaceTable, schemaContext);

            string clrTypeName;
            if (!XamlBuildTaskServices.TryGetClrTypeName(xamlType, rootNamespace, out clrTypeName))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TypeNameUnknown(XamlBuildTaskServices.GetFullTypeName(xamlType))));
            }
            return Tuple.Create(clrTypeName, xamlType.UnderlyingType);
        }

        // Given a text value for an attribute parameter, attempts to convert it to a live value.
        // Inverse of GetParameterText.
        private static object ParseParameterValue(ref string paramValue, XamlType paramType)
        {
            object valueObj = null;
            paramValue = NormalizeParameterText(paramValue, paramType);
            if (!paramType.UnderlyingType.Assembly.ReflectionOnly)
            {
                TypeConverter typeConverter = paramType.TypeConverter.ConverterInstance;
                if (typeConverter != null && typeConverter.CanConvertFrom(paramValue.GetType()))
                {
                    try
                    {
                        valueObj = typeConverter.ConvertFrom(null, invariantEnglishUS, paramValue);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        // ----ing exceptions here to avoid throwing on 
                        // a format that we don't recognize, but the compiler
                        // might be able to interpret.
                    }
                }
            }

            return valueObj;
        }

        // Get the parameter value that is to be put in the generated code as is.
        private static string NormalizeParameterText(string value, XamlType xamlType)
        {
            string paramValue;
            Type type = xamlType.UnderlyingType;
            Fx.Assert(!typeof(Type).IsAssignableFrom(type), "This method should not be called for Types");

            if (type.IsEnum)
            {
                paramValue = type.FullName + "." + value;
            }
            else if (type == typeof(String))
            {
                paramValue = value;
            }
            else if (type.IsPrimitive)
            {
                value = value.TrimStart('"');
                value = value.TrimEnd('"');

                if (type == typeof(bool))
                {
                    if (string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        paramValue = "true";
                    }
                    else if (string.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        paramValue = "false";
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnknownBooleanValue(value)));
                    }
                }
                else
                {
                    paramValue = value;
                }
            }
            else
            {
                throw Fx.AssertAndThrow("Unexpected attribute parameter type");
            }
            return paramValue;
        }
    }
}

