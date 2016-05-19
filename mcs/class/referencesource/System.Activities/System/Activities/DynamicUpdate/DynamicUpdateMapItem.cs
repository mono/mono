// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.XamlIntegration;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    
    [TypeConverter(typeof(DynamicUpdateMapItemConverter))]
    [DataContract]
    public class DynamicUpdateMapItem
    {
        internal DynamicUpdateMapItem(int originalId)
        {
            this.OriginalId = originalId;
        }

        internal DynamicUpdateMapItem(int originalVariableOwnerId, int originalVariableId)
        {
            this.OriginalVariableOwnerId = originalVariableOwnerId;
            this.OriginalId = originalVariableId;
        }

        [DataMember]
        internal int OriginalId
        {
            get;
            set;
        }

        [DataMember]
        internal int OriginalVariableOwnerId
        {
            get;
            set;
        }

        internal bool IsVariableMapItem
        {
            get
            {
                return this.OriginalVariableOwnerId > 0;
            }
        }
    }
}

