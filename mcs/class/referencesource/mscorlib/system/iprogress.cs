// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  IProgress<T>
** 
** <OWNER>stoub</OWNER>
**
**
** Purpose: Interface for providing progress updates.
**
**
===========================================================*/

namespace System
{
    /// <summary>Defines a provider for progress updates.</summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    public interface IProgress<in T>
    {
        /// <summary>Reports a progress update.</summary>
        /// <param name="value">The value of the updated progress.</param>
        void Report(T value);
    }
}