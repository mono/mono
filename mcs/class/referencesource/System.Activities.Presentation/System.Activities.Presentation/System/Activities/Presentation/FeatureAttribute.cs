//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    sealed class FeatureAttribute : Attribute
    {
        Type type;

        public FeatureAttribute(Type type)
        {
            Fx.Assert(type != null, "type should not be null");

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

        public override object TypeId
        {
            get
            {
                return this.type;
            }
        }
    }
}
