//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Serialization;

    static class MessageBuilder
    {
        static Type messageContractAttributeType;
        static XsdDataContractExporter xsdDataContractExporter;
        static XmlReflectionImporter xmlReflectionImporter;

        public static Type MessageContractAttributeType
        {
            get
            {
                if (messageContractAttributeType == null)
                {
                    messageContractAttributeType = typeof(MessageContractAttribute);
                }
                return messageContractAttributeType;
            }
        }

        public static XsdDataContractExporter XsdDataContractExporter
        {
            get
            {
                if (xsdDataContractExporter == null)
                {
                    xsdDataContractExporter = new XsdDataContractExporter();
                }
                return xsdDataContractExporter;
            }
        }

        public static XmlReflectionImporter XmlReflectionImporter
        {
            get
            {
                if (xmlReflectionImporter == null)
                {
                    xmlReflectionImporter = new XmlReflectionImporter();
                }
                return xmlReflectionImporter;
            }
        }

        public static MessageDescription CreateMessageDescription(OperationDescription operation, bool isResponse,
            MessageDirection direction, string overridingAction, Type type, SerializerOption serializerOption)
        {
            MessageDescription result;
            if (type != null && IsMessageContract(type))
            {
                result = CreateFromMessageContract(operation, isResponse, direction, overridingAction, type);
            }
            else
            {
                // For Send/Receive, we do not wrap message
                result = CreateEmptyMessageDescription(operation, isResponse, direction, overridingAction);
                AddMessagePartDescription(operation, isResponse, result, type, serializerOption);
            }

            return result;
        }

        public static MessageDescription CreateMessageDescription(OperationDescription operation, bool isResponse,
            MessageDirection direction, string overridingAction, string[] argumentNames, Type[] argumentTypes)
        {
            MessageDescription result;
            if (argumentTypes.Length == 1 && argumentTypes[0] == MessageDescription.TypeOfUntypedMessage)
            {
                result = CreateEmptyMessageDescription(operation, isResponse, direction, overridingAction);
                AddMessagePartDescription(operation, isResponse, result, argumentNames, argumentTypes);
            }
            else if (argumentTypes.Length == 1 && IsMessageContract(argumentTypes[0]))
            {
                result = CreateFromMessageContract(operation, isResponse, direction, overridingAction, argumentTypes[0]);
            }
            else
            {
                // For SendParameters/ReceiveParameters, we wrap for non-Message cases
                result = CreateEmptyMessageDescription(operation, isResponse, direction, overridingAction);
                AddMessagePartDescription(operation, isResponse, result, argumentNames, argumentTypes);
                SetWrapperName(operation, isResponse, result);
            }

            return result;
        }

        public static bool IsMessageContract(Type type)
        {
            if (type == null)
            {
                return false;
            }
            else
            {
                return type.IsDefined(MessageContractAttributeType, false);
            }
        }

        public static MessageDescription CreateFromMessageContract(OperationDescription operation, bool isResponse,
            MessageDirection direction, string overridingAction, Type messageContractType)
        {
            string action = overridingAction ?? NamingHelper.GetMessageAction(operation, isResponse);

            // 

            TypeLoader typeLoader = new TypeLoader();
            return typeLoader.CreateTypedMessageDescription(messageContractType, null, null,
                operation.DeclaringContract.Namespace, action, direction);
        }

        public static MessageDescription CreateEmptyMessageDescription(OperationDescription operation, bool isResponse,
            MessageDirection direction, string overridingAction)
        {
            string action = overridingAction ?? NamingHelper.GetMessageAction(operation, isResponse);
            MessageDescription result = new MessageDescription(action, direction);

            // Clear message wrapper
            result.Body.WrapperName = null;
            result.Body.WrapperNamespace = null;

            return result;
        }

        public static void AddMessagePartDescription(OperationDescription operation, bool isResponse,
            MessageDescription message, Type type, SerializerOption serializerOption)
        {
            if (type != null)
            {
                string partName;
                string partNamespace;

                if (serializerOption == SerializerOption.DataContractSerializer)
                {
                    XmlQualifiedName xmlQualifiedName = XsdDataContractExporter.GetRootElementName(type);
                    if (xmlQualifiedName == null)
                    {
                        xmlQualifiedName = XsdDataContractExporter.GetSchemaTypeName(type);
                    }

                    if (!xmlQualifiedName.IsEmpty)
                    {
                        partName = xmlQualifiedName.Name;
                        partNamespace = xmlQualifiedName.Namespace;
                    }
                    else
                    {
                        // For anonymous type, we assign CLR type name and contract namespace to MessagePartDescription
                        partName = type.Name;
                        partNamespace = operation.DeclaringContract.Namespace;
                    }
                }
                else
                {
                    XmlTypeMapping xmlTypeMapping = XmlReflectionImporter.ImportTypeMapping(type);
                    partName = xmlTypeMapping.ElementName;
                    partNamespace = xmlTypeMapping.Namespace;
                }

                MessagePartDescription messagePart = new MessagePartDescription(NamingHelper.XmlName(partName), partNamespace)
                {
                    Index = 0,
                    Type = type

                    // We do not infer MessagePartDescription.ProtectionLevel
                };

                message.Body.Parts.Add(messagePart);
            }
            
            if (isResponse)
            {
                SetReturnValue(message, operation);
            }
        }

        public static void AddMessagePartDescription(OperationDescription operation, bool isResponse,
            MessageDescription message, string[] argumentNames, Type[] argumentTypes)
        {
            Fx.Assert(argumentNames != null && argumentTypes != null, "Argument cannot be null!");
            Fx.Assert(argumentNames.Length == argumentTypes.Length, "Name and Type do not match!");

            // Infer MessagePartDescription.Namespace from contract namespace
            string partNamespace = operation.DeclaringContract.Namespace;

            for (int index = 0; index < argumentNames.Length; index++)
            {
                // Infer MessagePartDescription.Name from parameter name
                string partName = argumentNames[index];

                MessagePartDescription messagePart = new MessagePartDescription(NamingHelper.XmlName(partName), partNamespace)
                {
                    Index = index,
                    Type = argumentTypes[index]

                    // We do not infer MessagePartDescription.ProtectionLevel
                };

                message.Body.Parts.Add(messagePart);
            }

            if (isResponse)
            {
                SetReturnValue(message, operation);
            }
        }

        static void SetReturnValue(MessageDescription message, OperationDescription operation)
        {
            if (message.IsUntypedMessage)
            {
                message.Body.ReturnValue = message.Body.Parts[0];
                message.Body.Parts.RemoveAt(0);
            }
            else if (!message.IsTypedMessage)
            {
                message.Body.ReturnValue = new MessagePartDescription(operation.Name + TypeLoader.ReturnSuffix,
                    operation.DeclaringContract.Namespace);
                message.Body.ReturnValue.Type = TypeHelper.VoidType;
            }
        }

        public static void SetWrapperName(OperationDescription operation, bool isResponse, MessageDescription message)
        {
            message.Body.WrapperName = operation.Name + (isResponse ? TypeLoader.ResponseSuffix : string.Empty);
            message.Body.WrapperNamespace = operation.DeclaringContract.Namespace;
        }

        public static void ClearWrapperNames(OperationDescription operation)
        {
            // Reproduce logic from TypeLoader.CreateOperationDescription
            if (!operation.IsOneWay)
            {
                MessageDescription requestMessage = operation.Messages[0];
                MessageDescription responseMessage = operation.Messages[1];
                if (responseMessage.IsVoid &&
                    (requestMessage.IsUntypedMessage || requestMessage.IsTypedMessage))
                {
                    responseMessage.Body.WrapperName = null;
                    responseMessage.Body.WrapperNamespace = null;
                }
                else if (requestMessage.IsVoid &&
                    (responseMessage.IsUntypedMessage || responseMessage.IsTypedMessage))
                {
                    requestMessage.Body.WrapperName = null;
                    requestMessage.Body.WrapperNamespace = null;
                }
            }
        }

        public static FaultDescription CreateFaultDescription(OperationDescription operation, Type faultType, string overridingAction)
        {
            string name = NamingHelper.TypeName(faultType) + TypeLoader.FaultSuffix;
            string action = overridingAction ?? NamingHelper.GetMessageAction(operation, false) + name;
            FaultDescription result = new FaultDescription(action)
            {
                Namespace = operation.DeclaringContract.Namespace,
                DetailType = faultType
            };
            result.SetNameOnly(new XmlName(name));
            return result;
        }
    }
}
