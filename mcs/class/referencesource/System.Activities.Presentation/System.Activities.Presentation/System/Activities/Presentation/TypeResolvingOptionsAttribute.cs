//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Activities.Presentation.View;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefineAccessorsForAttributeArguments,
        Justification = "TypeResolvingOptions property can be set.")]
    public sealed class TypeResolvingOptionsAttribute : Attribute
    {
        public TypeResolvingOptions TypeResolvingOptions
        {
            get;
            set;
        }

        public TypeResolvingOptionsAttribute()
        {
        }

        public TypeResolvingOptionsAttribute(TypeResolvingOptions options)
        {
            this.TypeResolvingOptions = options;
        }
    }
}
