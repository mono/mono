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

    class EditorAttributeInfo : AttributeInfo<EditorAttribute>
    {
        public override ICollection GetConstructorArguments(EditorAttribute attribute, ref ConstructorInfo constructor)
        {
            return new List<object>() { Type.GetType(attribute.EditorTypeName), Type.GetType(attribute.EditorBaseTypeName) };
        }

        public override ConstructorInfo GetConstructor()
        {
            Type editorAttributeType = typeof(EditorAttribute);
            ConstructorInfo constructor = editorAttributeType.GetConstructor(new Type[] { typeof(Type), typeof(Type) });
            SharedFx.Assert(constructor != null, "designerAttribute has a constructor that takes two argument of type System.Type and System.Type.");
            return constructor;
        }
    }
}
