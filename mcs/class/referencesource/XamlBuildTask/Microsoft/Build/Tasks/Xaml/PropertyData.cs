//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xaml;
    using System.Collections.ObjectModel;
    using XamlBuildTask;

    public sealed class PropertyData
    {
        List<AttributeData> attributes;
        MemberVisibility visibility;

        public PropertyData()
        {
        }

        public IList<AttributeData> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new List<AttributeData>();
                }
                return this.attributes;
            }
        }

        [DefaultValue("")]
        public MemberVisibility Visibility
        {
            get
            {
                return this.visibility;
            }
            set
            {
                if (!Enum.IsDefined(typeof(MemberVisibility), value))
                {
                    throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("value"));
                }
                this.visibility = value;
            }
        }

        public string Name
        {
            get;
            set;
        }

        public XamlType Type
        {
            get;
            set;
        }
    }
}
