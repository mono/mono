//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // Class to describe the name and type for an early bound local
    // that will show up in Locals window.
    [DebuggerNonUserCode]
    [Fx.Tag.XamlVisible(false)]
    public class LocalsItemDescription
    {
        public LocalsItemDescription(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name
        {
            get;
            private set;
        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.PropertyNamesShouldNotMatchGetMethods,
            Justification = "Workflow normalizes on Type for Type properties")]
        public Type Type
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return this.Name + ":" + this.Type.ToString();
        }
    }

}
