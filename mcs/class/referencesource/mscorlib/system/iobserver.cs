// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  IObserver<T>
** 
** <OWNER>kimhamil</OWNER>
**
**
** Purpose: Interface for exposing an Observer in the 
** Observer pattern
**
**
===========================================================*/

using System;

namespace System
{
    public interface IObserver<in T>
    {
        void OnNext(T value);
        void OnError(Exception error);
        void OnCompleted();
    }
}