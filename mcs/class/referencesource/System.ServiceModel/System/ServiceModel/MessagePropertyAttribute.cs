//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    [AttributeUsage(ServiceModelAttributeTargets.MessageMember, Inherited = false)]
    public sealed class MessagePropertyAttribute : Attribute
    {
        string name;
        bool isNameSetExplicit;

        public MessagePropertyAttribute()
        {
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                isNameSetExplicit = true;
                this.name = value;
            }
        }
        internal bool IsNameSetExplicit
        {
            get
            {
                return isNameSetExplicit;
            }
        }
    }
}

