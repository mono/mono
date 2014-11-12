// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: NonSerializedAttribute
**
**
** Purpose: Used to mark a member as being not-serialized
**
**
============================================================*/
namespace System 
{
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Field, Inherited=false)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class NonSerializedAttribute : Attribute 
    {
        internal static Attribute GetCustomAttribute(RuntimeFieldInfo field) 
        { 
            if ((field.Attributes & FieldAttributes.NotSerialized) == 0)
                return null;

            return new NonSerializedAttribute();
        }

        internal static bool IsDefined(RuntimeFieldInfo field) 
        { 
            return (field.Attributes & FieldAttributes.NotSerialized) != 0;
        }

        public NonSerializedAttribute() { }
    }
}
