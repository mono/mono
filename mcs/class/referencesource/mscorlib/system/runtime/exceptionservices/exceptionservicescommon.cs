// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** File: ExceptionServicesCommon.cs
**
**
** Purpose: Contains common usage support entities for advanced exception
**          handling/processing scenarios.
**
** Created: 11/2/2010
** 
** <owner>Microsoft</owner>
** 
=============================================================================*/

#if FEATURE_EXCEPTIONDISPATCHINFO
namespace System.Runtime.ExceptionServices {
    using System;
    
    // This class defines support for seperating the exception dispatch details
    // (like stack trace, watson buckets, etc) from the actual managed exception
    // object. This allows us to track error (via the exception object) independent
    // of the path the error takes.
    //
    // This is particularly useful for frameworks like PFX, APM, etc that wish to
    // propagate exceptions (i.e. errors to be precise) across threads.
    public sealed class ExceptionDispatchInfo
    {
        // Private members that will hold the relevant details.
        private Exception m_Exception;
#if !MONO
        private string m_remoteStackTrace;
#endif
        private object m_stackTrace;
#if !MONO
        private object m_dynamicMethods;
        private UIntPtr m_IPForWatsonBuckets;
        private Object m_WatsonBuckets;
#endif
        
        private ExceptionDispatchInfo(Exception exception)
        {
            // Copy over the details we need to save.
            m_Exception = exception;
#if MONO
			var traces = exception.captured_traces;
			var count = traces == null ? 0 : traces.Length;
			var stack_traces = new System.Diagnostics.StackTrace [count + 1];
			if (count != 0)
				Array.Copy (traces, 0, stack_traces, 0, count);

			stack_traces [count] = new System.Diagnostics.StackTrace (exception, 0, true);
			m_stackTrace = stack_traces;
#else
            m_remoteStackTrace = exception.RemoteStackTrace;
            
            // NOTE: don't be tempted to pass the fields for the out params; the containing object
            //       might be relocated during the call so the pointers will no longer be valid.
            object stackTrace;
            object dynamicMethods;
            m_Exception.GetStackTracesDeepCopy(out stackTrace, out dynamicMethods);
            m_stackTrace = stackTrace;
            m_dynamicMethods = dynamicMethods;

            m_IPForWatsonBuckets = exception.IPForWatsonBuckets;
            m_WatsonBuckets = exception.WatsonBuckets;                                                        
#endif
        }

#if !MONO
        internal UIntPtr IPForWatsonBuckets
        {
            get
            {
                return m_IPForWatsonBuckets;   
            }
        }

        internal object WatsonBuckets
        {
            get
            {
                return m_WatsonBuckets;   
            }
        }
#endif
        
        internal object BinaryStackTraceArray
        {
            get
            {
                return m_stackTrace;
            }
        }

#if !MONO
        internal object DynamicMethodArray
        {
            get
            {
                return m_dynamicMethods;
            }
        }

        internal string RemoteStackTrace
        {
            get
            {
                return m_remoteStackTrace;
            }
        }
#endif

        // This static method is used to create an instance of ExceptionDispatchInfo for
        // the specified exception object and save all the required details that maybe
        // needed to be propagated when the exception is "rethrown" on a different thread.
        public static ExceptionDispatchInfo Capture(Exception source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            
            return new ExceptionDispatchInfo(source);
        }
    
        // Return the exception object represented by this ExceptionDispatchInfo instance
        public Exception SourceException
        {

            get
            {
                return m_Exception;   
            }
        }
        
        // When a framework needs to "Rethrow" an exception on a thread different (but not necessarily so) from
        // where it was thrown, it should invoke this method against the ExceptionDispatchInfo (EDI)
        // created for the exception in question.
        //
        // This method will restore the original stack trace and bucketing details before throwing
        // the exception so that it is easy, from debugging standpoint, to understand what really went wrong on
        // the original thread.
#if MONO
        [System.Diagnostics.StackTraceHidden]
#endif
        public void Throw()
        {
            // Restore the exception dispatch details before throwing the exception.
            m_Exception.RestoreExceptionDispatchInfo(this);
            throw m_Exception; 
        }

#if MONO
        [System.Diagnostics.StackTraceHidden]
        public static void Throw (Exception source) => Capture (source).Throw ();
#endif
    }
}
#endif // FEATURE_EXCEPTIONDISPATCHINFO
