// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: TypeInfoExtension
**
** <OWNER>[....]</OWNER>
**
**
** Purpose: go from type to type info
**
**
=============================================================================*/

namespace System.Reflection
{
    using System.Reflection;
    using System.Diagnostics.Tracing;

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
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.IntrospectionExtensionsGetTypeInfo(type.GetFullNameForEtw());
                }
#endif
                return rcType.GetTypeInfo();
            }
        }   
    }
}

