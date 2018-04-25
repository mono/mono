// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: ParamArrayAttribute
**
**
** Purpose: Container for assemblies.
**
**
=============================================================================*/
namespace System
{
//This class contains only static members and does not need to be serializable 
   [AttributeUsage (AttributeTargets.Parameter, Inherited=true, AllowMultiple=false)]
[System.Runtime.InteropServices.ComVisible(true)]
   public sealed class ParamArrayAttribute : Attribute
   {
      public ParamArrayAttribute () {}  
   }
}
