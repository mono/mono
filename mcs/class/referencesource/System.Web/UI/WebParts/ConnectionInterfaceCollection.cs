//------------------------------------------------------------------------------
// <copyright file="ConnectionInterfaceCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;

    public sealed class ConnectionInterfaceCollection : ReadOnlyCollectionBase {

        public static readonly ConnectionInterfaceCollection Empty = new ConnectionInterfaceCollection();

        public ConnectionInterfaceCollection() {
        }

        public ConnectionInterfaceCollection(ICollection connectionInterfaces) {
            Initialize(null, connectionInterfaces);
        }

        public ConnectionInterfaceCollection(ConnectionInterfaceCollection existingConnectionInterfaces,
                                             ICollection connectionInterfaces) {
            Initialize(existingConnectionInterfaces, connectionInterfaces);
        }

        private void Initialize(ConnectionInterfaceCollection existingConnectionInterfaces, ICollection connectionInterfaces) {
            if (existingConnectionInterfaces != null) {
                foreach (Type existingConnectionInterface in existingConnectionInterfaces) {
                    // Don't need to check arg, since we know it is valid since it came
                    // from a ConnectionInterfaceCollection.
                    InnerList.Add(existingConnectionInterface);
                }
            }

            if (connectionInterfaces != null) {
                foreach (object obj in connectionInterfaces) {
                    if (obj == null) {
                        throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), "connectionInterfaces");
                    }
                    if (!(obj is Type)) {
                        throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "Type"), "connectionInterfaces");
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


