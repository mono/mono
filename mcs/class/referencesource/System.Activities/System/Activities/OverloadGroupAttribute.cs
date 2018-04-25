//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefineAccessorsForAttributeArguments,
    Justification = "The setter is needed to enable XAML serialization of the attribute object.")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class OverloadGroupAttribute : Attribute
    {
        string groupName;

        public OverloadGroupAttribute()
        {
        }

        public OverloadGroupAttribute(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("groupName");
            }

            this.groupName = groupName;
        }

        public string GroupName
        {
            get 
            { 
                return this.groupName; 
            }

            set 
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("value");
                }
                this.groupName = value;
            }
        }

        public override object TypeId
        {
            get
            {
                return this;
            }
        }
    }
}
