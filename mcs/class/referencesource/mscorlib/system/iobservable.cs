// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  IObservable<T>
** 
** <OWNER>[....]</OWNER>
**
**
** Purpose: Interface for exposing an Observable in the 
** Observer pattern
**
**
===========================================================*/

namespace System
{
    public interface IObservable<out T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }

}
