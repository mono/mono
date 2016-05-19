//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xaml;
    using System.Xaml.Schema;
    using XamlBuildTask;

    public class NamedObject
    {
        MemberVisibility visibility;

        public String Name
        { get; set; }

        public XamlType Type
        { get; set; }

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
    }
}
