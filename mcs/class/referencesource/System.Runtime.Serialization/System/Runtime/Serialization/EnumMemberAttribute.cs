//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnumMemberAttribute : Attribute
    {
        string value;
        bool isValueSetExplicitly;

        public EnumMemberAttribute()
        {
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; this.isValueSetExplicitly = true; }
        }

        public bool IsValueSetExplicitly
        {
            get { return this.isValueSetExplicitly; }
        }
    }
}
