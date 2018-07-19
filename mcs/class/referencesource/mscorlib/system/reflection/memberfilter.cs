// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// MemberFilter is a delegate used to filter Members.  This delegate is used
// 
// <OWNER>WESU</OWNER>
//    as a callback from Type.FindMembers.
//
// <EMAIL>Author: darylo</EMAIL>
// Date: March 98
//
namespace System.Reflection {
    
    // Define the delegate
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public delegate bool MemberFilter(MemberInfo m, Object filterCriteria);
}
