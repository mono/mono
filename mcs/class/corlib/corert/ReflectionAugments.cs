using System;
using System.Reflection;

namespace Internal.Reflection.Augments {
    static class ReflectionAugments {
        internal static Type MakeGenericSignatureType (Type genericTypeDefinition, Type[] genericTypeArguments)
        {
            return new SignatureConstructedGenericType (genericTypeDefinition, genericTypeArguments);
        }
    }
}
