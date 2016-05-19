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

    class DesignerAttributeInfo : AttributeInfo<DesignerAttribute>
    {
        public override ICollection GetConstructorArguments(DesignerAttribute attribute, ref ConstructorInfo constructor)
        {
            return new List<object>() { Type.GetType(attribute.DesignerTypeName) };
        }

        public override ConstructorInfo GetConstructor()
        {
            Type designerAttributeType = typeof(DesignerAttribute);
            ConstructorInfo constructor = designerAttributeType.GetConstructor(new Type[] { typeof(Type) });
            SharedFx.Assert(constructor != null, "designerAttribute has a constructor that takes an argument of type System.Type.");
            return constructor;
        }
    }
}
