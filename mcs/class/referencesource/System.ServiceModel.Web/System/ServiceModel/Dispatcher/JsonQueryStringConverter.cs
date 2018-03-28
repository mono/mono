//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.IO;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    public class JsonQueryStringConverter : QueryStringConverter
    {
        DataContractSerializerOperationBehavior dataContractSerializerOperationBehavior = null;
        OperationDescription operationDescription = null;


        public JsonQueryStringConverter() : base()
        {
        }

        internal JsonQueryStringConverter(OperationDescription operationDescription)
            : base()
        {
            if (operationDescription == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationDescription");
            }
            this.operationDescription = operationDescription;
            this.dataContractSerializerOperationBehavior = this.operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
        }

        public override bool CanConvert(Type type)
        {
            XsdDataContractExporter exporter = new XsdDataContractExporter();
            return exporter.CanExport(type);
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (parameterType == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterType");
            }
            switch (Type.GetTypeCode(parameterType))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Boolean:
                    // base.ConvertStringToValue handles parameter == null case. 
                    return base.ConvertStringToValue(parameter, parameterType);
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.DateTime:
                    // base.ConvertStringToValue handles parameter == null case. 
                    // IsFirstCharacterReservedCharacter returns false for null strings.
                    if (IsFirstCharacterReservedCharacter(parameter, '"'))
                    {
                        return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                    }
                    return base.ConvertStringToValue(parameter, parameterType);
                default:
                    {
                        if (parameterType == typeof(Guid))
                        {
                            if (parameter == null)
                            {
                                return default(Guid);
                            }
                            if (IsFirstCharacterReservedCharacter(parameter, '"'))
                            {
                                return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                            }
                            return base.ConvertStringToValue(parameter, parameterType);
                        }
                        else if (parameterType == typeof(Uri))
                        {
                            if (parameter == null)
                            {
                                return default(Uri);
                            }
                            if (IsFirstCharacterReservedCharacter(parameter, '"'))
                            {
                                return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                            }
                            return base.ConvertStringToValue(parameter, parameterType);
                        }
                        else if (parameterType == typeof(TimeSpan))
                        {
                            if (parameter == null)
                            {
                                return default(TimeSpan);
                            }
                            if (IsFirstCharacterReservedCharacter(parameter, '"'))
                            {
                                return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                            }
                            return base.ConvertStringToValue(parameter, parameterType);
                        }
                        else if (parameterType == typeof(byte[]))
                        {
                            if (parameter == null)
                            {
                                return default(byte[]);
                            }
                            if (IsFirstCharacterReservedCharacter(parameter, '['))
                            {
                                return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                            }
                            return base.ConvertStringToValue(parameter, parameterType);
                        }
                        else if (parameterType == typeof(DateTimeOffset))
                        {
                            if (parameter == null)
                            {
                                return default(DateTimeOffset);
                            }
                            if (IsFirstCharacterReservedCharacter(parameter, '{'))
                            {
                                return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                            }
                            return base.ConvertStringToValue(parameter, parameterType);
                        }
                        else if (parameterType == typeof(object))
                        {
                            if (parameter == null)
                            {
                                return default(object);
                            }
                            if (IsFirstCharacterReservedCharacter(parameter, '{'))
                            {
                                return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                            }
                            return base.ConvertStringToValue(parameter, parameterType);
                        }

                        if (parameter == null)
                        {
                            return null;
                        }
                        return CreateJsonDeserializedObject(parameter.Trim(), parameterType);
                    }
            }
        }

        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            if (parameter == null)
            {
                return null;
            }
            MemoryStream memoryStream = new MemoryStream();
            XmlDictionaryWriter jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, Encoding.UTF8);
            GetDataContractJsonSerializer(parameterType).WriteObject(jsonWriter, parameter);
            jsonWriter.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return Encoding.UTF8.GetString(memoryStream.GetBuffer(), (int) memoryStream.Position, (int) memoryStream.Length);
        }

        object CreateJsonDeserializedObject(string parameter, Type parameterType)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(parameter);
            XmlDictionaryReader jsonReader = JsonReaderWriterFactory.CreateJsonReader
                (byteArray, 0, byteArray.Length, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null);
            return GetDataContractJsonSerializer(parameterType).ReadObject(jsonReader);
        }

        DataContractJsonSerializer GetDataContractJsonSerializer(Type parameterType)
        {
            if (this.operationDescription == null)
            {
                return new DataContractJsonSerializer(parameterType);
            }
            else if (this.dataContractSerializerOperationBehavior == null)
            {
                return new DataContractJsonSerializer(parameterType, operationDescription.KnownTypes);
            }
            else
            {
                return new DataContractJsonSerializer(parameterType, this.operationDescription.KnownTypes, this.dataContractSerializerOperationBehavior.maxItemsInObjectGraph,
                    this.dataContractSerializerOperationBehavior.IgnoreExtensionDataObject, this.dataContractSerializerOperationBehavior.DataContractSurrogate, false); //alwaysEmitTypeInformation
            }
        }

        bool IsFirstCharacterReservedCharacter(string parameter, char reservedCharacter)
        {
            if (parameter == null)
            {
                return false;
            }
            string localParameter = parameter.Trim();
            if (localParameter == string.Empty)
            {
                return false;
            }
            if (localParameter[0] == reservedCharacter)
            {
                return true;
            }
            return false;
        }
    }
}
