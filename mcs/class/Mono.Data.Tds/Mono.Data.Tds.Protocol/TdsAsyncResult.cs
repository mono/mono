//
// Mono.Data.Tds.Protocol.TdsAsyncResult.cs
//
// Author:
//   T Sureshkumar <tsureshkumar@novell.com>
//   Ankit Jain <radical@corewars.org>

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

namespace Mono.Data.Tds.Protocol
{
        internal class TdsAsyncResult : IAsyncResult
        {
                private TdsAsyncState _tdsState;
                private WaitHandle _waitHandle;
                private bool _completed = false;
                private bool _completedSyncly = false;
                private AsyncCallback _userCallback;
                private object _retValue;
                private Exception _exception = null;

                public TdsAsyncResult (AsyncCallback userCallback, TdsAsyncState tdsState)
                {
                        _tdsState = tdsState;
                        _userCallback = userCallback;
                        _waitHandle = new ManualResetEvent (false);
                }

                public TdsAsyncResult (AsyncCallback userCallback, object state)
                {
                        _tdsState = new TdsAsyncState (state);
                        _userCallback = userCallback;
                        _waitHandle = new ManualResetEvent (false);
                }
                

                public object AsyncState 
                {
                        get {   return _tdsState.UserState; }
                }

                internal TdsAsyncState TdsAsyncState
                {
                        get {   return _tdsState; }
                }

                public WaitHandle AsyncWaitHandle 
                {
                        get { return _waitHandle; }

                }

                public bool IsCompleted 
                {
                        get {   return _completed; }
                }

                public bool IsCompletedWithException
                {
                        get {   return _exception != null; }
                }

                public Exception Exception
                {
                        get {   return _exception; }
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

                internal void MarkComplete () 
                {
                        _completed = true;
                        _exception = null;
                        ((ManualResetEvent)_waitHandle).Set ();

                        if (_userCallback != null)
                                _userCallback (this);
                }

                internal void MarkComplete (Exception e) 
                {
                        _completed = true;
                        _exception = e;
                        ((ManualResetEvent)_waitHandle).Set ();

                        if (_userCallback != null)
                                _userCallback (this);
                }
        }
}

#endif // NET_2_0
