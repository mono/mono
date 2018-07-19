//------------------------------------------------------------------------------
// <copyright file="HttpListenerException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;

    [Serializable]
    public class HttpListenerException : Win32Exception
    {
        public HttpListenerException() : base(Marshal.GetLastWin32Error())
        {
            GlobalLog.Print("HttpListenerException::.ctor() " + NativeErrorCode.ToString() + ":" + Message);
        }

        public HttpListenerException(int errorCode) : base(errorCode)
        {
            GlobalLog.Print("HttpListenerException::.ctor(int) " + NativeErrorCode.ToString() + ":" + Message);
        }

        public HttpListenerException(int errorCode, string message) : base(errorCode, message)
        {
            GlobalLog.Print("HttpListenerException::.ctor(int) " + NativeErrorCode.ToString() + ":" + Message);
        }

        protected HttpListenerException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            GlobalLog.Print("HttpListenerException::.ctor(serialized) " + NativeErrorCode.ToString() + ":" + Message);
        }

        public override int ErrorCode
        {
            //
            // the base class returns the HResult with this property
            // we need the Win32 Error Code, hence the override.
            //
            get
            {
                return NativeErrorCode;
            }
        }
    }
}
