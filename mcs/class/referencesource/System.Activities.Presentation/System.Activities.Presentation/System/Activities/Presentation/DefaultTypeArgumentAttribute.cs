//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultTypeArgumentAttribute : Attribute
    {
        Type type;

        public DefaultTypeArgumentAttribute(Type type)
        {
            this.type = type;
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "By design.")]
        public Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}
