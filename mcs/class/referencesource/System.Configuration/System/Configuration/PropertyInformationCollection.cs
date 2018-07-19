//------------------------------------------------------------------------------
// <copyright file="PropertyInformationCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Configuration {

    // PropertyInformationCollection
    //
    // Collection of PropertyInformation objects
    //

    [Serializable()]
    public sealed class PropertyInformationCollection : NameObjectCollectionBase {
        ConfigurationElement ThisElement = null;
        internal PropertyInformationCollection(ConfigurationElement thisElement) : base(StringComparer.Ordinal) {
            ThisElement = thisElement;
            foreach (ConfigurationProperty prop in ThisElement.Properties) {
                if (prop.Name != ThisElement.ElementTagName) {                    
                    BaseAdd(prop.Name, new PropertyInformation(thisElement, prop.Name));
                }
            }
            IsReadOnly = true;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
        }

        // Item
        //
        // Indexor for retrieving a Property by name
        //
        public PropertyInformation this[string propertyName] {
            get {
                PropertyInformation result = (PropertyInformation) BaseGet (propertyName);

                // check for default collection name
                if (result == null) {
                    PropertyInformation defaultColl = 
                        (PropertyInformation) BaseGet (ConfigurationProperty.DefaultCollectionPropertyName);

                    if ((defaultColl != null) && (defaultColl.ProvidedName == propertyName)) {
                        result = defaultColl;
                    }
                }
                return result;   
            }
        }

        internal PropertyInformation this[int index] {
            get {
                return (PropertyInformation)BaseGet(BaseGetKey(index));
            }
        }

        public void CopyTo(PropertyInformation[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            if (array.Length < Count + index) {
                throw new ArgumentOutOfRangeException("index");
            }

            foreach (PropertyInformation pi in this) {
                array[index++] = pi;
            }
        }


        public override IEnumerator GetEnumerator() {
            int c = Count;
            for (int i = 0; i < c; i++) {
                yield return this[i];
            }
        }
    }
}
