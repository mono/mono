//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnumMemberAttribute : Attribute
    {
        string value;
        bool isValueSetExplicit;

        public EnumMemberAttribute()
        {
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; this.isValueSetExplicit = true; }
        }

        internal bool IsValueSetExplicit
        {
            get { return this.isValueSetExplicit; }
        }
    }
}
