// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 *
 * Class:  INormalizeForIsolatedStorage
// 
// <OWNER>[....]</OWNER>
 *
 * <EMAIL>Author: Sh----n Dasan</EMAIL>
 *
 * Purpose: Evidence types can optionaly implement this interface.
 *          IsolatedStorage calls Normalize method before evidence
 *          is serialized. The Normalize method should return a copy
 *          of the evidence instance if any of it's fields is changed.
 *
 * Date:  Oct 17, 2000
 *
 ===========================================================*/
namespace System.IO.IsolatedStorage {

    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public interface INormalizeForIsolatedStorage
    {
        // Return a copy of the normalized version of this instance,
        // so that a the serialized version of this object can be 
        // mem-compared to another serialized object
        //
        // 1. Eg.  (pseudo code to illustrate usage)
        //
        // obj1 = MySite(WWW.MSN.COM)
        // obj2 = MySite(www.msn.com)
        //
        // obj1Norm = obj1.Normalize() 
        // obj2Norm = obj1.Normalize() 
        //
        // stream1 = Serialize(obj1Norm)
        // stream2 = Serialize(obj2Norm)
        //
        // AreStreamsEqual(stream1, stream2) returns true
        //
        // If the Object returned is a stream, the stream will be used without 
        // serialization. If the Object returned is a string, the string will 
        // be used in naming the Store. If the string is too long or if the
        // string contains chars that are illegal to use in naming the store,
        // the string will be serialized.
        //
        // 2. Eg. (pseudo code to illustrate returning string)
        //
        // obj1 = MySite(WWW.MSN.COM)
        // obj2 = MySite(www.msn.com)
        //
        // string1 = obj1.Normalize() 
        // string2 = obj1.Normalize() 
        //
        // AreStringsEqual(string1, string2) returns true
        //
        // 3. Eg. (pseudo code to illustrate returning stream)
        //
        // obj1 = MySite(WWW.MSN.COM)
        // obj2 = MySite(www.msn.com)
        //
        // stream1 = obj1.Normalize() 
        // stream2 = obj1.Normalize() 
        //
        // AreStreamsEqual(stream1, stream2) returns true

        Object Normalize();
    }
}

