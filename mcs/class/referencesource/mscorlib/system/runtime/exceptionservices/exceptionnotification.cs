#if FEATURE_EXCEPTION_NOTIFICATIONS
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** File: ExceptionNotification.cs
**
**
** Purpose: Contains definitions for supporting Exception Notifications.
**
** Created: 10/07/2008
** 
** <owner>[....]</owner>
** 
=============================================================================*/

namespace System.Runtime.ExceptionServices {
    using System;
    using System.Runtime.ConstrainedExecution;
    
    // Definition of the argument-type passed to the FirstChanceException event handler
    public class FirstChanceExceptionEventArgs : EventArgs
    {
        // Constructor
        public FirstChanceExceptionEventArgs(Exception exception)
        {
            m_Exception = exception;
        }

        // Returns the exception object pertaining to the first chance exception
        public Exception Exception
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get { return m_Exception; }
        }

        // Represents the FirstChance exception instance
        private Exception m_Exception;
    }
}
#endif // FEATURE_EXCEPTION_NOTIFICATIONS
