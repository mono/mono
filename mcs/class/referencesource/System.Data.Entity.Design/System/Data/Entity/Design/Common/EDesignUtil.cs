//------------------------------------------------------------------------------
// <copyright file="EDesignUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

namespace System.Data.Entity.Design.Common {

    using System;
    using System.Data;
    using System.Data.Metadata.Edm;

    internal static class EDesignUtil {

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        //
        // Helper Functions
        //
        internal static string GetMessagesFromEntireExceptionChain(Exception e)
        {
            // get the full error message list from the inner exceptions
            string message = e.Message;
            int count = 0;
            for (Exception inner = e.InnerException; inner != null; inner = inner.InnerException)
            {
                count++;
                string indent = string.Empty.PadLeft(count, '\t');
                message += Environment.NewLine + indent;
                message += inner.Message;
            }
            return message;
        }

        static internal T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw ArgumentNull(parameterName);
            }
            return value;
        }

        static internal void CheckStringArgument(string value, string parameterName)
        {
            // Throw ArgumentNullException when string is null
            CheckArgumentNull(value, parameterName);

            // Throw ArgumentException when string is empty
            if (value.Length == 0)
            {
                throw InvalidStringArgument(parameterName);
            }
        }

        static internal LanguageOption CheckLanguageOptionArgument(LanguageOption value, string paramName)
        {
            if (value == LanguageOption.GenerateCSharpCode ||
                value == LanguageOption.GenerateVBCode)
            {
                return value;
            }
            throw ArgumentOutOfRange(paramName);
        }

        static internal ArgumentException SingleStoreEntityContainerExpected(string parameterName)
        {
            ArgumentException e = new ArgumentException(Strings.SingleStoreEntityContainerExpected, parameterName);
            return e;
        }
        static internal ArgumentException InvalidStoreEntityContainer(string entityContainerName, string parameterName)
        {
            ArgumentException e = new ArgumentException(Strings.InvalidNonStoreEntityContainer(entityContainerName), parameterName);
            return e;
        }
        static internal ArgumentException InvalidStringArgument(string parameterName) {
            ArgumentException e = new ArgumentException(Strings.InvalidStringArgument(parameterName));
            return e;
        }

        static internal ArgumentException EdmReservedNamespace(string namespaceName) {
            ArgumentException e = new ArgumentException(Strings.ReservedNamespace(namespaceName));
            return e;
        }

        static internal ArgumentNullException ArgumentNull(string parameter) {
            ArgumentNullException e = new ArgumentNullException(parameter);
            return e;
        }

        static internal ArgumentException Argument(string parameter)
        {
            ArgumentException e = new ArgumentException(parameter);
            return e;
        }

        static internal ArgumentException Argument(string message, Exception inner)
        {
            ArgumentException e = new ArgumentException(message, inner);
            return e;
        }

        static internal InvalidOperationException InvalidOperation(string error)
        {
            InvalidOperationException e = new InvalidOperationException(error);
            return e;
        }

        // SSDL Generator
        static internal StrongTypingException StrongTyping(string error, Exception innerException) {
            StrongTypingException e = new StrongTypingException(error, innerException);
            return e;
        }

        static internal StrongTypingException StonglyTypedAccessToNullValue(string columnName, string tableName, Exception innerException) {
            return StrongTyping(Strings.StonglyTypedAccessToNullValue(columnName, tableName), innerException);
        }
        
        static internal InvalidOperationException EntityStoreGeneratorSchemaNotLoaded() {
            return InvalidOperation(Strings.EntityStoreGeneratorSchemaNotLoaded);
        }

        static internal InvalidOperationException EntityModelGeneratorSchemaNotLoaded() {
            return InvalidOperation(Strings.EntityModelGeneratorSchemaNotLoaded);
        }

        static internal InvalidOperationException NonSerializableType(BuiltInTypeKind kind)
        {
            return InvalidOperation(Strings.Serialization_UnknownGlobalItem(kind));
        }
        
        static internal InvalidOperationException MissingGenerationPatternForType(BuiltInTypeKind kind) 
        {
            return InvalidOperation(Strings.ModelGeneration_UnGeneratableType(kind));
        }

        static internal ArgumentException InvalidNamespaceNameArgument(string namespaceName)
        {
            return new ArgumentException(Strings.InvalidNamespaceNameArgument(namespaceName));
        }

        static internal ArgumentException InvalidEntityContainerNameArgument(string entityContainerName)
        {
            return new ArgumentException(Strings.InvalidEntityContainerNameArgument(entityContainerName));
        }

        static internal ArgumentException DuplicateEntityContainerName(string newModelEntityContainerName, string storeEntityContainer) 
        {
            return new ArgumentException(Strings.DuplicateEntityContainerName(newModelEntityContainerName, storeEntityContainer));
        }

        static internal ProviderIncompatibleException ProviderIncompatible(string message)
        {
            return new ProviderIncompatibleException(message);
        }

        static internal ProviderIncompatibleException ProviderIncompatible(string message, Exception inner)
        {
            return new ProviderIncompatibleException(message, inner);
        }

        static internal ArgumentOutOfRangeException ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static void CheckTargetEntityFrameworkVersionArgument(Version targetEntityFrameworkVersion, string parameterName)
        {
            EDesignUtil.CheckArgumentNull(targetEntityFrameworkVersion, parameterName);
            if (!EntityFrameworkVersions.IsValidVersion(targetEntityFrameworkVersion))
            {
                throw EDesignUtil.Argument(parameterName);
            }
        }
    }
}
