// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Xaml.Schema;

    internal class DefaultValueAttributeInfo : AttributeInfo<DefaultValueAttribute>
    {
        public override XamlTypeInvoker Invoker
        {
            get { return new DefaultValueAttributeInvoker(); }
        }

        public override ICollection GetConstructorArguments(DefaultValueAttribute attribute, ref ConstructorInfo constructor)
        {
            return new List<object>() { attribute.Value };
        }

        public override ConstructorInfo GetConstructor()
        {
            Type defaultValueAttributeType = typeof(DefaultValueAttribute);
            ConstructorInfo constructor = defaultValueAttributeType.GetConstructor(new Type[] { typeof(object) });
            SharedFx.Assert(constructor != null, "designerAttribute has a constructor that takes an argument of type System.Object.");
            return constructor;
        }

        private class DefaultValueAttributeInvoker : XamlTypeInvoker
        {
            public override object CreateInstance(object[] arguments)
            {
                if (arguments != null && arguments.Length == 1)
                {
                    // This helps to disambiguate the different constructors when arguments[0] is null.
                    return new DefaultValueAttribute(arguments[0]);
                }
                else
                {
                    return base.CreateInstance(arguments);
                }
            }
        }
    }
}
