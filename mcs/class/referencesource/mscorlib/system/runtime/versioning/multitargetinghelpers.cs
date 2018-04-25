// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  MultitargetingHelpers
** 
** <OWNER>Microsoft</OWNER>
**
**
** Purpose: Central repository for helpers supporting 
** multitargeting, such as emitting the correct version numbers
** and assembly names.
**
** 
===========================================================*/
namespace System.Runtime.Versioning
{
    using System;
    using System.IO;
    using System.Text;
    using System.Diagnostics.Contracts;

    internal static class MultitargetingHelpers
    {

        // default type converter
        private static Func<Type, String> defaultConverter = (t) => t.AssemblyQualifiedName;

        // This method gets assembly info for the corresponding type. If the typeConverter
        // is provided it is used to get this information.
        internal static string GetAssemblyQualifiedName(Type type, Func<Type, String> converter)
        {
            string assemblyFullName = null;

            if (type != null)
            {
                if (converter != null)
                {
                    try
                    {
                        assemblyFullName = converter(type);
                        // 
                    }
                    catch (Exception e)
                    {
                        if (IsSecurityOrCriticalException(e))
                        {
                            throw;
                        }
                    }
                }

                if (assemblyFullName == null)
                {
                    assemblyFullName = defaultConverter(type);
                }
            }

            return assemblyFullName;
        }

        private static bool IsCriticalException(Exception ex)
        {
            return ex is NullReferenceException
                    || ex is StackOverflowException
                    || ex is OutOfMemoryException
                    || ex is System.Threading.ThreadAbortException
                    || ex is IndexOutOfRangeException
                    || ex is AccessViolationException;
        }

        private static bool IsSecurityOrCriticalException(Exception ex)
        {
            return (ex is System.Security.SecurityException) || IsCriticalException(ex);
        }

    }
}
