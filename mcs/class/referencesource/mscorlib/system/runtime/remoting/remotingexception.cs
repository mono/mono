// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: RemotingException
**
**
** Purpose: Exception class for remoting 
**
**
=============================================================================*/

namespace System.Runtime.Remoting
{
    using System.Runtime.Remoting;
    using System;
    using System.Runtime.Serialization;
    // The Exception thrown when something has gone
    // wrong during remoting
    // 
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class RemotingException : SystemException {

        private static String _nullMessage = Environment.GetResourceString("Remoting_Default");

        // Creates a new RemotingException with its message 
        // string set to a default message.
        public RemotingException() 
                : base(_nullMessage) {
            SetErrorCode(__HResults.COR_E_REMOTING);
        }

        public RemotingException(String message) 
                : base(message) {
            SetErrorCode(__HResults.COR_E_REMOTING);
        }


        public RemotingException(String message, Exception InnerException) 
                : base(message, InnerException) {
            SetErrorCode(__HResults.COR_E_REMOTING);
        }    

        protected RemotingException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }




    // The Exception thrown when something has gone
    // wrong on the server during remoting. This exception is thrown
    // on the client.
    // 
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class ServerException : SystemException {
        private static String _nullMessage = Environment.GetResourceString("Remoting_Default");
        // Creates a new ServerException with its message 
        // string set to a default message.
        public ServerException()
                : base(_nullMessage) {
            SetErrorCode(__HResults.COR_E_SERVER);            
        }

        public ServerException(String message) 
                : base(message) {
            SetErrorCode(__HResults.COR_E_SERVER);
        }


        public ServerException(String message, Exception InnerException) 
                : base(message, InnerException) {
            SetErrorCode(__HResults.COR_E_SERVER);
        }    

        internal ServerException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class RemotingTimeoutException : RemotingException {

        private static String _nullMessage = Environment.GetResourceString("Remoting_Default");

        // Creates a new RemotingException with its message 
        // string set to a default message.
        public RemotingTimeoutException() : base(_nullMessage) 
        {
        }

        public RemotingTimeoutException(String message) : base(message) {
            SetErrorCode(__HResults.COR_E_REMOTING);
        }


        public RemotingTimeoutException(String message, Exception InnerException) 
            : base(message, InnerException) 
        {
            SetErrorCode(__HResults.COR_E_REMOTING);
        }

        internal RemotingTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) {}
        
    } // RemotingTimeoutException

}
