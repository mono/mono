//------------------------------------------------------------------------------
// <copyright file="ConfigurationLocationCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Text;

    public class ConfigurationLocationCollection : ReadOnlyCollectionBase {
        
        internal ConfigurationLocationCollection(ICollection col) {
            InnerList.AddRange(col);
        }

        public ConfigurationLocation this[int index] {
            get {
                return (ConfigurationLocation) InnerList[index];
            }
        }
    }
}
