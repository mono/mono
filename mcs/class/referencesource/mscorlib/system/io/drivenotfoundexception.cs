// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//============================================================
//
//  Class:  DriveNotFoundException
// 
// <OWNER>Microsoft</OWNER>
//
//  Purpose: Exception for accessing a drive that is not available.
//
//
//============================================================
using System;
using System.Runtime.Serialization;

namespace System.IO {

    //Thrown when trying to access a drive that is not availabe.
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class DriveNotFoundException : IOException {
        public DriveNotFoundException() 
            : base(Environment.GetResourceString("Arg_DriveNotFoundException")) {
            SetErrorCode(__HResults.COR_E_DIRECTORYNOTFOUND);
        }
    
        public DriveNotFoundException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_DIRECTORYNOTFOUND);
        }
    
        public DriveNotFoundException(String message, Exception innerException) 
            : base(message, innerException) {
            SetErrorCode(__HResults.COR_E_DIRECTORYNOTFOUND);
        }
        
        protected DriveNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
