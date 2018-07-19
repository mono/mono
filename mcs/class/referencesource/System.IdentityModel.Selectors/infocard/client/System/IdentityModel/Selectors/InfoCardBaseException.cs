//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.Serialization;


    [Serializable]
    internal abstract class InfoCardBaseException : System.Exception
    {
        
        private bool m_logged = false;
        private string m_extendedMessage;  // Extended message string
                
        protected InfoCardBaseException( int result )
            : base()
        {
            HResult = result;
        }

        protected InfoCardBaseException( int result, string message )
            : base( message )
        {
            HResult = result;
        }
                
        protected InfoCardBaseException( int result, string message, string extendedMessage )
            : base( message )
        {
            HResult = result;
            m_extendedMessage = extendedMessage;
        }

        protected InfoCardBaseException( int result, string message, Exception innerException )
            : base( message, innerException )
        {
            HResult = result;
        }

        protected InfoCardBaseException( int result, SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            HResult = result;
        }

        public int NativeHResult
        {
            get { return HResult; }
        }
        public bool Logged
        {
            get
            {
                return m_logged;
            }
        }
        public void MarkLogged()
        {
            m_logged = true;
        }
        public string ExtendedMessage
        {
            get { return m_extendedMessage; }                    
        }
        
    }
}

