//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Hosting
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Globalization;
    using System.Xml.Linq;

    [DataContract]
    [Fx.Tag.XamlVisible(false)]
    public sealed class LocationInfo
    {
        string name;
        string ownerDisplayName;
        object value;

        internal LocationInfo(string name, string ownerDisplayName, object value)
        {
            this.Name = name;
            this.OwnerDisplayName = ownerDisplayName;
            this.Value = value;
        }        

        public string Name
        {
            get
            {
                return this.name;
            }
            private set
            {
                this.name = value;
            }
        }
        
        public string OwnerDisplayName
        {
            get
            {
                return this.ownerDisplayName;
            }
            private set
            {
                this.ownerDisplayName = value;
            }
        }
        
        public object Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
            }
        }

        [DataMember(Name = "Name")]
        internal string SerializedName
        {
            get { return this.Name; }
            set { this.Name = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "OwnerDisplayName")]
        internal string SerializedOwnerDisplayName
        {
            get { return this.OwnerDisplayName; }
            set { this.OwnerDisplayName = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "Value")]
        internal object SerializedValue
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
    }
}
