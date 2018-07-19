//------------------------------------------------------------------------------
// <copyright file="TransformerTypeCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;

    public sealed class TransformerTypeCollection : ReadOnlyCollectionBase {

        public static readonly TransformerTypeCollection Empty = new TransformerTypeCollection();

        public TransformerTypeCollection() {
        }

        public TransformerTypeCollection(ICollection transformerTypes) {
            Initialize(null, transformerTypes);
        }

        public TransformerTypeCollection(TransformerTypeCollection existingTransformerTypes, ICollection transformerTypes) {
            Initialize(existingTransformerTypes, transformerTypes);
        }

        internal int Add(Type value) {
            if (!value.IsSubclassOf(typeof(WebPartTransformer))) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartTransformerAttribute_NotTransformer, value.Name));
            }
            return InnerList.Add(value);
        }

        private void Initialize(TransformerTypeCollection existingTransformerTypes, ICollection transformerTypes) {
            if (existingTransformerTypes != null) {
                foreach (Type existingTransformerType in existingTransformerTypes) {
                    // Don't need to check arg, since we know it is valid since it came
                    // from a TransformerTypeCollection.
                    InnerList.Add(existingTransformerType);
                }
            }

            if (transformerTypes != null) {
                foreach (object obj in transformerTypes) {
                    if (obj == null) {
                        throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), "transformerTypes");
                    }
                    if (!(obj is Type)) {
                        throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "Type"), "transformerTypes");
                    }
                    if (!((Type)obj).IsSubclassOf(typeof(WebPartTransformer))) {
                        throw new ArgumentException(SR.GetString(SR.WebPartTransformerAttribute_NotTransformer, ((Type)obj).Name),
                                                    "transformerTypes");
                    }
                    InnerList.Add(obj);
                }
            }
        }

        public bool Contains(Type value) {
            return InnerList.Contains(value);
        }

        public int IndexOf(Type value) {
            return InnerList.IndexOf(value);
        }

        public Type this[int index] {
            get {
                return (Type)InnerList[index];
            }
        }

        public void CopyTo(Type[] array, int index) {
            InnerList.CopyTo(array, index);
        }
    }
}

