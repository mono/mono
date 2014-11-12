// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// TypeFilter defines a delegate that is as a callback function for filtering
// 
// <OWNER>[....]</OWNER>
//    a list of Types.
//
// <EMAIL>Author: darylo</EMAIL>
// Date: [....] 98
//
namespace System.Reflection {
    
    // Define the delegate
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public delegate bool TypeFilter(Type m, Object filterCriteria);
}
