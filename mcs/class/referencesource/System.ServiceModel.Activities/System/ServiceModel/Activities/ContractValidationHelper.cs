//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using SR2 = System.ServiceModel.Activities.SR;

    static class ContractValidationHelper
    {
        public static void ValidateReceiveWithReceive(Receive receive1, Receive receive2)
        {
            Fx.Assert(receive1 != null && receive2 != null, "Validation argument cannot be null!");
            Fx.Assert(receive1.OperationName != null, "OperationName cannot be null in Receive");
            string receiveOperationName = receive1.OperationName;

            if (receive1.Action != receive2.Action)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceivesWithSameNameButDifferentAction(receiveOperationName)));
            }

            if (receive1.InternalContent is ReceiveMessageContent && receive2.InternalContent is ReceiveMessageContent)
            {
                ReceiveMessageContent receiveMessage1 = receive1.InternalContent as ReceiveMessageContent;
                ReceiveMessageContent receiveMessage2 = receive2.InternalContent as ReceiveMessageContent;

                ValidateReceiveWithReceive(receiveMessage1, receiveMessage2, receiveOperationName);
            }
            else if (receive1.InternalContent is ReceiveParametersContent && receive2.InternalContent is ReceiveParametersContent)
            {
                ReceiveParametersContent receiveParameters1 = receive1.InternalContent as ReceiveParametersContent;
                ReceiveParametersContent receiveParameters2 = receive2.InternalContent as ReceiveParametersContent;

                ValidateReceiveParametersWithReceiveParameters(receiveParameters1, receiveParameters2, receiveOperationName);
            }
            else
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.ReceiveAndReceiveParametersHaveSameName(receiveOperationName)));
            }

            if (receive1.HasReply && receive2.HasReply)
            {
                ValidateSendReplyWithSendReply(receive1.FollowingReplies[0], receive2.FollowingReplies[0]);
            }
            else if ((receive1.HasReply || receive1.HasFault) != (receive2.HasReply || receive2.HasFault))
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceivesWithSameNameButDifferentIsOneWay(receiveOperationName)));
            }

            if ((receive1.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope != receive2.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope) ||
                (receive1.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree != receive2.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree))
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceivesWithSameNameButDifferentTxProperties(receiveOperationName)));
            }
        }

        static void ValidateReceiveWithReceive(ReceiveMessageContent receive1, ReceiveMessageContent receive2, string receiveOperationName)
        {
            Fx.Assert(receive1 != null && receive2 != null, "Validation argument cannot be null!");

            if (receive1.InternalDeclaredMessageType != receive2.InternalDeclaredMessageType)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceivesWithSameNameButDifferentValueType(receiveOperationName)));
            }
        }

        static void ValidateReceiveParametersWithReceiveParameters(ReceiveParametersContent receiveParameters1, ReceiveParametersContent receiveParameters2, string receiveOperationName)
        {
            Fx.Assert(receiveParameters1 != null && receiveParameters2 != null, "Validation argument cannot be null!");

            int count = receiveParameters1.ArgumentNames.Length;
            if (count != receiveParameters2.ArgumentNames.Length)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceiveParametersWithSameNameButDifferentParameterCount(receiveOperationName)));
            }
            for (int i = 0; i < count; i++)
            {
                if (receiveParameters1.ArgumentNames[i] != receiveParameters2.ArgumentNames[i])
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceiveParametersWithSameNameButDifferentParameterName(receiveOperationName)));
                }
                if (receiveParameters1.ArgumentTypes[i] != receiveParameters2.ArgumentTypes[i])
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoReceiveParametersWithSameNameButDifferentParameterType(receiveOperationName)));
                }
            }
        }

        public static void ValidateSendReplyWithSendReply(SendReply sendReply1, SendReply sendReply2)
        {
            Fx.Assert(sendReply1 != null && sendReply2 != null, "Validation argument cannot be null!");
            Fx.Assert(sendReply1.Request != null, "Request cannot be null in SendReply");
            string operationName = sendReply1.Request.OperationName;

            if (sendReply1.Action != sendReply2.Action)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoSendRepliesWithSameNameButDifferentAction(operationName)));
            }

            if (sendReply1.InternalContent is SendMessageContent && sendReply2.InternalContent is SendMessageContent)
            {
                SendMessageContent sendMessage1 = sendReply1.InternalContent as SendMessageContent;
                SendMessageContent sendMessage2 = sendReply2.InternalContent as SendMessageContent;

                if (sendMessage1.InternalDeclaredMessageType != sendMessage2.InternalDeclaredMessageType)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoSendRepliesWithSameNameButDifferentValueType(operationName)));
                }
            }
            else if (sendReply1.InternalContent is SendParametersContent && sendReply2.InternalContent is SendParametersContent)
            {
                SendParametersContent sendReplyParameters1 = sendReply1.InternalContent as SendParametersContent;
                SendParametersContent sendReplyParameters2 = sendReply2.InternalContent as SendParametersContent;

                int count = sendReplyParameters1.ArgumentNames.Length;
                if (count != sendReplyParameters2.ArgumentNames.Length)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoSendReplyParametersWithSameNameButDifferentParameterCount(operationName)));
                }
                for (int i = 0; i < count; i++)
                {
                    if (sendReplyParameters1.ArgumentNames[i] != sendReplyParameters2.ArgumentNames[i])
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoSendReplyParametersWithSameNameButDifferentParameterName(operationName)));
                    }
                    if (sendReplyParameters1.ArgumentTypes[i] != sendReplyParameters2.ArgumentTypes[i])
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR2.TwoSendReplyParametersWithSameNameButDifferentParameterType(operationName)));
                    }
                }
            }
            else
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.ReceivePairedWithSendReplyAndSendReplyParameters(operationName)));
            }
        }
        
        public static void ValidateFault(NativeActivityContext context, OperationDescription targetOperation, string overridingAction, Type faultType)
        {
            bool faultTypeExistOnContract = false;

            for (int index = 0; index < targetOperation.Faults.Count; index++)
            {
                FaultDescription targetFault = targetOperation.Faults[index];

                if (targetFault.DetailType == faultType)
                {
                    string name = NamingHelper.TypeName(faultType) + TypeLoader.FaultSuffix;
                    string action = overridingAction ?? NamingHelper.GetMessageAction(targetOperation, false) + name;

                    if (targetFault.Action != action)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(action, "Fault Action", targetFault.Action, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                    }
                    if (targetFault.Name != NamingHelper.XmlName(name))
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(NamingHelper.XmlName(name), "Fault Name", targetFault.Name, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                    }
                    if (targetFault.Namespace != targetOperation.DeclaringContract.Namespace)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(targetOperation.DeclaringContract.Namespace, "Fault Namespace", targetFault.Namespace, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                    }
                    if (targetFault.HasProtectionLevel)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.ProtectionLevelNotSupported(targetOperation.Name, targetOperation.DeclaringContract.Name)));
                    }

                    // TypeLoader guarantees that fault types are unique in the Faults collection.
                    faultTypeExistOnContract = true;
                    break;
                }
            }

            // It is OK to have fewer fault types than defined on the contract.
            // But we do not allow workflow to define more fault types than specified on the contract.
            if (!faultTypeExistOnContract)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.FaultTypeMismatch(faultType.FullName, targetOperation.Name, targetOperation.DeclaringContract.Name)));
            }
        }

        public static void ValidateAction(NativeActivityContext context, MessageDescription targetMessage, string overridingAction,
            OperationDescription targetOperation, bool isResponse)
        {
            if (overridingAction == null && targetMessage.Action != NamingHelper.GetMessageAction(targetOperation, isResponse)
                || overridingAction != null && overridingAction != targetMessage.Action)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(overridingAction, "Action", targetMessage.Action, targetOperation.Name, targetOperation.DeclaringContract.Name)));
            }
        }

        public static void ValidateMessageContent(NativeActivityContext context, MessageDescription targetMessage, Type declaredMessageType,
            SerializerOption serializerOption, OperationDescription operation, bool isResponse)
        {
            // MessageContract is allowed only if the WCF contract interface specifies the same message contract type.
            if (MessageBuilder.IsMessageContract(declaredMessageType))
            {
                // if it is a typed message contract, we just validate the type of the message matches
                if (targetMessage.MessageType != null )
                {
                    if (declaredMessageType != targetMessage.MessageType)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(declaredMessageType.ToString(), "type", targetMessage.MessageType.ToString(), operation.Name, operation.DeclaringContract.Name)));
                    }
                }
                else
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(declaredMessageType.ToString(), "type", "null", operation.Name, operation.DeclaringContract.Name))); 
                }
                return;
            }
            else if (declaredMessageType != null && declaredMessageType.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
            {
                //This is an untyped message contract
                if (targetMessage.Body == null)
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.BodyCannotBeNull));
                }
                else
                {
                    if (isResponse)
                    {
                        if (targetMessage.Body.ReturnValue == null)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.ExtraReturnValue));
                        }
                        else if (!targetMessage.Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.FirstParameterDoesnotMatchTheReturnValue(declaredMessageType.FullName, targetMessage.Body.ReturnValue.Type.Name, operation.Name, operation.DeclaringContract.Name)));
                        }
                    }
                    else
                    {
                        if (targetMessage.Body.Parts.Count == 0)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.ParameterNumberMismatch(declaredMessageType.FullName, operation.Name, operation.DeclaringContract.Name)));
                        }
                        else if (targetMessage.Body.Parts.Count > 1)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.MessageContentCannotHaveMoreThanOneParameter(operation.Name, operation.DeclaringContract.Name)));
                        }
                        else
                        {
                            if (!targetMessage.Body.Parts[0].Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                            {
                                Constraint.AddValidationError(context, new ValidationError(SR2.MessageTypeMismatch(targetMessage.Body.Parts[0].Type.FullName, operation.Name, operation.DeclaringContract.Name)));
                            }
                        }
                    }
                }

                return;
            }

            // In case the WCF contract is a typed message, and the Receive activity also uses ReceiveMessageContent to infer a typed message, the contract needs to be matched
            Fx.Assert(targetMessage.Body != null, "MessageDescription.Body is never null!");

            // MessageDescription: Headers, Properties, ProtectionLevel
            // MessageBodyDescription: ReturnValue, WrapperName, WrapperNamespace
            // MessagePartDescription: Name, Namespace, Type, ProtectionLevel, Multiple, Index
            if (targetMessage.Headers.Count > 0)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.MessageHeaderNotSupported(operation.Name, operation.DeclaringContract.Name)));
            }
            if (targetMessage.Properties.Count > 0)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.MessagePropertyIsNotSupported(operation.Name, operation.DeclaringContract.Name)));
            }
            if (targetMessage.HasProtectionLevel)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.ProtectionLevelIsNotSupported(operation.Name, operation.DeclaringContract.Name)));
            }
            
            if (declaredMessageType == null || declaredMessageType == TypeHelper.VoidType)
            {
                if (!targetMessage.IsVoid)
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.MessageCannotBeEmpty(operation.Name, operation.DeclaringContract.Name)));
                }
            }
            else
            {
                string partName;
                string partNamespace;

                if (serializerOption == SerializerOption.DataContractSerializer)
                {
                    XmlQualifiedName xmlQualifiedName = MessageBuilder.XsdDataContractExporter.GetRootElementName(declaredMessageType);
                    if (xmlQualifiedName == null)
                    {
                        xmlQualifiedName = MessageBuilder.XsdDataContractExporter.GetSchemaTypeName(declaredMessageType);
                    }

                    if (!xmlQualifiedName.IsEmpty)
                    {
                        partName = xmlQualifiedName.Name;
                        partNamespace = xmlQualifiedName.Namespace;
                    }
                    else
                    {
                        // For anonymous type, we assign CLR type name and contract namespace to MessagePartDescription
                        partName = declaredMessageType.Name;
                        partNamespace = operation.DeclaringContract.Namespace;
                    }
                }
                else
                {
                    XmlTypeMapping xmlTypeMapping = MessageBuilder.XmlReflectionImporter.ImportTypeMapping(declaredMessageType);
                    partName = xmlTypeMapping.ElementName;
                    partNamespace = xmlTypeMapping.Namespace;
                }

                MessagePartDescription targetPart = null;

                if (isResponse && targetMessage.Body.ReturnValue != null && targetMessage.Body.ReturnValue.Type != TypeHelper.VoidType)
                {
                    if (targetMessage.Body.Parts.Count > 0)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.NotSupportMoreThanOneParametersInMessageContract(operation.Name, operation.DeclaringContract.Name)));
                    }
                    targetPart = targetMessage.Body.ReturnValue;
                }
                else if (!isResponse)
                {
                    if (targetMessage.Body.WrapperName != null && targetMessage.Body.WrapperName != String.Empty)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.WrapperNotSupportedInMessageContract(operation.Name, operation.DeclaringContract.Name)));
                    }

                    if (targetMessage.Body.WrapperNamespace != null && targetMessage.Body.WrapperNamespace != String.Empty)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.WrapperNotSupportedInMessageContract(operation.Name, operation.DeclaringContract.Name)));
                    }

                    if (targetMessage.Body.Parts.Count == 0)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.ParameterNumberMismatch(declaredMessageType.FullName, operation.Name, operation.DeclaringContract.Name)));
                    }
                    else if (targetMessage.Body.Parts.Count > 1)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.MessageContentCannotHaveMoreThanOneParameter(operation.Name, operation.DeclaringContract.Name)));
                    }
                    else
                    {
                        targetPart = targetMessage.Body.Parts[0];
                    }
                }

                if (targetPart != null)
                {
                    if (partName != targetPart.Name)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(partName, "parameter name", targetPart.Name, operation.Name, operation.DeclaringContract.Name)));
                    }
                    if (partNamespace != targetPart.Namespace)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.PropertyMismatch(partNamespace, "parameter namespace", targetPart.Namespace, operation.Name, operation.DeclaringContract.Name)));
                    }
                    if (declaredMessageType != targetPart.Type)
                    {
                        if (declaredMessageType != null)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.ParameterTypeMismatch(declaredMessageType.FullName, targetPart.Type.FullName, operation.Name, operation.DeclaringContract.Name)));
                        }
                        else
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.ParameterTypeMismatch(TypeHelper.VoidType.FullName, targetPart.Type.FullName, operation.Name, operation.DeclaringContract.Name)));
                        }
                    }
                    if (targetPart.HasProtectionLevel)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.ProtectionLevelIsNotSupported(operation.Name, operation.DeclaringContract.Name)));
                    }

                    // Multiple and Index do not need to be validate because there is only one part in the message.
                }
            }
        }

        public static void ValidateParametersContent(NativeActivityContext context, MessageDescription targetMessage, IDictionary parameters,
            OperationDescription targetOperation, bool isResponse)
        {
            // The following properties can only be set via message contract. Therefore, we do not need to validate them here.
            // MessageDescription: Headers, Properties, ProtectionLevel
            // MessagePartDescription: Namespace, ProtectionLevel, Multiple, Index
            MessageBodyDescription targetMessageBody = targetMessage.Body;
            Fx.Assert(targetMessageBody != null, "MessageDescription.Body is never null!");

            if (targetMessageBody.WrapperName == null)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.UnwrappedMessageNotSupported(targetOperation.Name, targetOperation.DeclaringContract.Name)));
            }
            if (targetMessageBody.WrapperNamespace == null)
            {
                Constraint.AddValidationError(context, new ValidationError(SR2.UnwrappedMessageNotSupported(targetOperation.Name, targetOperation.DeclaringContract.Name)));
            }

            IDictionaryEnumerator iterator = parameters.GetEnumerator();
            int benchmarkIndex = 0;
            int hitCount = 0;

            // Return value needs to be treated specially since ReceiveParametersContent does not have return value on the OM.
            bool targetHasReturnValue = isResponse && targetMessageBody.ReturnValue != null && targetMessageBody.ReturnValue.Type != TypeHelper.VoidType;
            if (targetHasReturnValue)
            {
                if (iterator.MoveNext() && (string)iterator.Key == targetMessageBody.ReturnValue.Name)
                {
                    Argument argument = (Argument)iterator.Value;
                    if (argument != null && argument.ArgumentType != targetMessageBody.ReturnValue.Type)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.FirstParameterDoesnotMatchTheReturnValue(argument.ArgumentType.FullName, targetMessageBody.ReturnValue.Type.FullName, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                    }
                    hitCount++;
                }
                else if (parameters.Contains(targetMessageBody.ReturnValue.Name))
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.ParameterPositionMismatch(targetMessageBody.ReturnValue.Name, targetOperation.Name, targetOperation.DeclaringContract.Name, "0")));
                    hitCount++;
                }
                else
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.ReturnValueMissing(targetMessageBody.ReturnValue.Type.FullName, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                }

                benchmarkIndex++;
            }

            foreach (MessagePartDescription targetPart in targetMessageBody.Parts)
            {
                if (iterator.MoveNext() && (string)iterator.Key == targetPart.Name)
                {
                    Argument argument = (Argument)iterator.Value;
                    if (argument != null && argument.ArgumentType != targetPart.Type)
                    {
                        Constraint.AddValidationError(context, new ValidationError(SR2.ParameterTypeMismatch(targetPart.Name, targetPart.Type.FullName, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                    }
                    hitCount++;
                }
                else if (parameters.Contains(targetPart.Name))
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.ParameterPositionMismatch(targetPart.Name, targetOperation.Name, targetOperation.DeclaringContract.Name, benchmarkIndex)));
                    hitCount++;
                }
                else
                {
                    Constraint.AddValidationError(context, new ValidationError(SR2.MissingParameter(targetPart.Name, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                }

                benchmarkIndex++;
            }

            if (hitCount != parameters.Count)
            {
                foreach (string name in parameters.Keys)
                {
                    XmlQualifiedName qName = new XmlQualifiedName(name, targetOperation.DeclaringContract.Namespace);
                    if (!targetMessageBody.Parts.Contains(qName))
                    {
                        if (!targetHasReturnValue || targetHasReturnValue && name != targetMessageBody.ReturnValue.Name)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR2.ExtraParameter(name, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                        }
                    }
                }
            }
        }
                
        // This method trying to validate if the receive message from the operation description should be Parameter content or Message Content
        public static bool IsReceiveParameterContent(OperationDescription operation)
        {
            Fx.Assert(operation != null, "OperationDescription should not be null");
            MessageDescription message;
            bool contentIsParameter = false;
            bool noReceiveMessageContent = false;

            message = operation.Messages[0];

            // MessageType is null indicating it is not typed message contract
            if (message.MessageType == null)
            {
                if (message.Body.Parts != null)
                {
                    if (message.Body.Parts.Count != 0)
                    {
                        foreach (MessagePartDescription messagePart in message.Body.Parts)
                        {
                            if (messagePart.Index > 0)
                            {
                                contentIsParameter = true;
                                break;
                            }
                            // Indicating it is a untyped message contract
                            if (!messagePart.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                            {
                                contentIsParameter = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        noReceiveMessageContent = true;
                    }
                }
                else
                {
                    noReceiveMessageContent = true;
                }
            }

            if (noReceiveMessageContent)
            {
                if ((message.Body.ReturnValue != null && message.Body.ReturnValue.Type.IsDefined(typeof(MessageContractAttribute), false))
                    || (message.Body.ReturnValue != null && message.Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message))))
                {
                    contentIsParameter = false;
                }
                else if (operation.Messages.Count > 1)
                {
                    if (operation.Messages[1].MessageType != null || operation.Messages[1].Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                    {
                        contentIsParameter = false;
                    }
                    else
                    {
                        contentIsParameter = true;
                    }
                }
                else
                {
                    contentIsParameter = true;
                }
            }

            return contentIsParameter;
        }

        public static bool IsSendParameterContent(OperationDescription operation)
        {
            Fx.Assert(operation != null, "OperationDescription should not be null");
            if (operation.IsOneWay)
            {
                return false;
            }

            bool contentIsParameter = false;
            bool isSendContentEmpty = false;
            MessageDescription message;

            if (operation.Messages.Count > 1)
            {
                message = operation.Messages[1];
                contentIsParameter = false;

                if (message.MessageType == null)
                {
                    if (message.Body.ReturnValue != null && message.Body.ReturnValue.Type != typeof(void))
                    {
                        if (!message.Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                        {
                            contentIsParameter = true;
                        }

                        isSendContentEmpty = true;
                    }
                }

                if (message.MessageType == null)
                {
                    if (message.Body.Parts != null)
                    {
                        if (message.Body.Parts.Count > 0)
                        {
                            MessagePartDescriptionCollection parts = message.Body.Parts;
                            foreach (MessagePartDescription messagePart in parts)
                            {
                                if (messagePart.Index >= 0)
                                {
                                    contentIsParameter = true;
                                    break;
                                }
                                if (!messagePart.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                                {
                                    contentIsParameter = true;
                                }
                            }
                            isSendContentEmpty = true;
                        }
                    }
                }

                if (!isSendContentEmpty)
                {
                    if (message.MessageType != null && message.MessageType.IsDefined(typeof(MessageContractAttribute), false))
                    {
                        contentIsParameter = false;
                    }
                    else if (operation.Messages[0].MessageType != null)
                    {
                        contentIsParameter = false;
                    }
                    else if (operation.Messages[0].Body.Parts != null
                        && operation.Messages[0].Body.Parts.Count == 1
                        && operation.Messages[0].Body.Parts[0].Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                    {
                        contentIsParameter = false;
                    }
                    else
                    {
                        contentIsParameter = true;
                    }
                }
            }

            return contentIsParameter;
        }

        public static string GetErrorMessageEndpointName(string endpointName)
        {
            return !string.IsNullOrEmpty(endpointName) ? endpointName : SR.NotSpecified;
        }

        public static string GetErrorMessageEndpointServiceContractName(XName serviceContractName)
        {
            return serviceContractName != null ? serviceContractName.LocalName : SR.NotSpecified;
        }

        public static string GetErrorMessageOperationName(string operationName)
        {
            return !string.IsNullOrEmpty(operationName) ? operationName : SR.NotSpecified;
        }
    }
}
