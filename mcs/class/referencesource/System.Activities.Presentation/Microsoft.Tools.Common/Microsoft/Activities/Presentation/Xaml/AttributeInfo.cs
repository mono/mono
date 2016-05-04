// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xaml.Schema;

    // AttributeInfo is a helper class to provide type specfic info for each Attribute class
    internal abstract class AttributeInfo<TAttribute> where TAttribute : Attribute
    {
        // false if the attribute has additional (mutable) properties that aren't set in the constructor
        public virtual bool IsComplete
        {
            get { return true; }
        }

        // whether to use argumented-ctor for serialization even when there's default ctor
        public virtual bool LookupConstructionRequiresArguments
        {
            get { return true; }
        }

        public virtual XamlTypeInvoker Invoker
        {
            get { return null; }
        }

        public abstract ConstructorInfo GetConstructor();

        public abstract ICollection GetConstructorArguments(TAttribute attribute, ref ConstructorInfo constructor);
    }
}
