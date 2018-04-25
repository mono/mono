//------------------------------------------------------------------------------
// <copyright file="WebPartTransformerAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WebPartTransformerAttribute : Attribute {

        // Cache provider and consumer types for each WebPartTransformer type. We store an array of
        // 2 Types (consumer, provider) indexed by transformer type.
        private static readonly Hashtable transformerCache = Hashtable.Synchronized(new Hashtable());

        private Type _consumerType;
        private Type _providerType;

        public WebPartTransformerAttribute(Type consumerType, Type providerType) {
            if (consumerType == null) {
                throw new ArgumentNullException("consumerType");
            }

            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }

            _consumerType = consumerType;
            _providerType = providerType;
        }

        public Type ConsumerType {
            get {
                return _consumerType;
            }
        }

        public Type ProviderType {
            get {
                return _providerType;
            }
        }

        public static Type GetConsumerType(Type transformerType) {
            return GetTransformerTypes(transformerType)[0];
        }

        public static Type GetProviderType(Type transformerType) {
            return GetTransformerTypes(transformerType)[1];
        }

        /// <devdoc>
        /// Returns the types a transformer can accept on its "connection points"
        /// </devdoc>
        private static Type[] GetTransformerTypes(Type transformerType) {
            if (transformerType == null) {
                throw new ArgumentNullException("transformerType");
            }

            if (!transformerType.IsSubclassOf(typeof(WebPartTransformer))) {
                throw new InvalidOperationException(
                    SR.GetString(SR.WebPartTransformerAttribute_NotTransformer, transformerType.FullName));
            }

            Type[] types = (Type[])transformerCache[transformerType];
            if (types == null) {
                types = GetTransformerTypesFromAttribute(transformerType);
                transformerCache[transformerType] = types;
            }

            return types;
        }

        private static Type[] GetTransformerTypesFromAttribute(Type transformerType) {
            Type[] types = new Type[2];

            object[] attributes = transformerType.GetCustomAttributes(typeof(WebPartTransformerAttribute), true);
            // WebPartTransformerAttribute.AllowMultiple is false
            Debug.Assert(attributes.Length == 0 || attributes.Length == 1);
            if (attributes.Length == 1) {
                WebPartTransformerAttribute attribute = (WebPartTransformerAttribute)attributes[0];
                if (attribute.ConsumerType == attribute.ProviderType) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartTransformerAttribute_SameTypes));
                }
                types[0] = attribute.ConsumerType;
                types[1] = attribute.ProviderType;
            }
            else {
                throw new InvalidOperationException(
                    SR.GetString(SR.WebPartTransformerAttribute_Missing, transformerType.FullName));
            }

            return types;
        }
    }
}
