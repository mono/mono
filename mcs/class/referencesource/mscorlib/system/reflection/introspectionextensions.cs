// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: TypeInfoExtension
**
** <OWNER>Microsoft</OWNER>
**
**
** Purpose: go from type to type info
**
**
=============================================================================*/

namespace System.Reflection
{
    using System.Reflection;

    public static class IntrospectionExtensions
    {
	    public static TypeInfo GetTypeInfo(this Type type){
            if(type == null){
                throw new ArgumentNullException("type");
            }
            var rcType=(IReflectableType)type;
            if(rcType==null){
                return null;
            }else{
                return rcType.GetTypeInfo();
            }
        }   
    }
}

