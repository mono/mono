//
// System.Data.SqlClient.SqlAsyncResult.cs
//
// Author:
//   T Sureshkumar <tsureshkumar@novell.com>
//   Ankit Jain <radical@corewars.org>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


#if NET_2_0
using System;
using System.Threading;

namespace System.Data.SqlClient
{
        internal class SqlAsyncResult : IAsyncResult
        {
                private SqlAsyncState   _sqlState;
                private WaitHandle      _waitHandle;
                private bool            _completed = false;
                private bool            _completedSyncly = false;
                private bool            _ended = false;
                private AsyncCallback   _userCallback = null;
                private object          _retValue;
                private string          _endMethod;
                private IAsyncResult    _internal;

                public SqlAsyncResult (AsyncCallback userCallback, SqlAsyncState sqlState)
                {
                        _sqlState = sqlState;
                        _userCallback = userCallback;
                        _waitHandle = new ManualResetEvent (false);
                }

                public SqlAsyncResult (AsyncCallback userCallback, object state)
                {
                        _sqlState = new SqlAsyncState (state);
                        _userCallback = userCallback;
                        _waitHandle = new ManualResetEvent (false);
                }
                
                public object AsyncState 
                {
                        get {   return _sqlState.UserState; }
                }

                internal SqlAsyncState SqlAsyncState
                {
                        get {   return _sqlState; }
                }

                public WaitHandle AsyncWaitHandle 
                {
                        get { return _waitHandle; }

                }

                public bool IsCompleted 
                {
                        get {   return _completed; }
                }

                public bool CompletedSynchronously 
                {
                        get {   return _completedSyncly; }
                }

                internal object ReturnValue
                {
                        get {   return _retValue; }
                        set {   _retValue = value; }
                }

                public string EndMethod
                {
                        get {   return _endMethod; }
                        set {   _endMethod = value; }
                }

                public bool Ended
                {
                        get {   return _ended; }
                        set {   _ended = value; }
                }


                internal IAsyncResult InternalResult
                {
                        get {   return _internal; }
                        set {   _internal = value; }
                }

                public AsyncCallback BubbleCallback
                {
                        get { return new AsyncCallback (Bubbleback); }
                        
                }

                internal void MarkComplete () 
                {
                        _completed = true;
                        ((ManualResetEvent)_waitHandle).Set ();

                        if (_userCallback != null)
                                _userCallback (this);
                }
                
                public void Bubbleback (IAsyncResult ar)
                {
                        this.MarkComplete ();
                }
        }
}

#endif // NET_2_0
