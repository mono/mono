//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace System.IdentityModel
{
    /// <summary>
    /// TypedAsyncResult: an asyncResult which will wrap the result T and return it
    /// when End() method is called.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public class TypedAsyncResult<T> : AsyncResult
    {
        T _result;

        /// <summary>
        /// Constructor for async results that do not need a callback.
        /// </summary>
        /// <param name="state">A user-defined object that qualifies or contains information about an asynchronous operation.</param>
        public TypedAsyncResult( object state )
            : base( state )
        {
        }

        /// <summary>
        /// Constructor for async results that need a callback and a state.
        /// </summary>
        /// <param name="callback">The method to be called when the async operation completes.</param>
        /// <param name="state">A user-defined object that qualifies or contains information about an asynchronous operation.</param>
        public TypedAsyncResult( AsyncCallback callback, object state )
            : base( callback, state )
        {
        }

        /// <summary>
        /// Call this version of complete when your asynchronous operation is complete.  This will save the
        /// result of the operation and notify the callback.
        /// </summary>
        /// <param name="result">The result to be wrapped.</param>
        /// <param name="completedSynchronously">True if the asynchronous operation completed synchronously.</param>
        public void Complete( T result, bool completedSynchronously )
        {
            _result = result;

            Complete( completedSynchronously );
        }

        /// <summary>
        /// Call this version of complete if you raise an exception during processing.  In addition to notifying
        /// the callback, it will capture the exception and store it to be thrown during AsyncResult.End.
        /// </summary>
        /// <param name="result">The result to be wrapped.</param>
        /// <param name="completedSynchronously">True if the asynchronous operation completed synchronously.</param>
        /// <param name="exception">The exception during the processing of the asynchronous operation.</param>
        public void Complete( T result, bool completedSynchronously, Exception exception )
        {
            _result = result;

            Complete( completedSynchronously, exception);
        }

        /// <summary>
        /// End should be called when the End function for the asynchronous operation is complete.  It
        /// ensures the asynchronous operation is complete, and does some common validation.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> representing the status of an asynchronous operation.</param>
        /// <returns>The typed result of the asynchronous operation.</returns>
        public new static T End( IAsyncResult result )
        {
            if ( result == null )
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "result" );

            TypedAsyncResult<T> completedResult = result as TypedAsyncResult<T>;

            if ( completedResult == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "result", SR.GetString( SR.ID2004, typeof( TypedAsyncResult<T>), result.GetType() ) );
            }

            AsyncResult.End( completedResult );

            return completedResult.Result;
        }

        /// <summary>
        /// Gets the typed result of the completed asynchronous operation.
        /// </summary>
        public T Result
        {
            get 
            { 
                return _result; 
            }
        }
    }
}
