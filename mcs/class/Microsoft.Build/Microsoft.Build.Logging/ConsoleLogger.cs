// ConsoleLogger.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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

using Microsoft.Build.Framework;

using System;

namespace Microsoft.Build.Logging
{
        public class ConsoleLogger : INodeLogger
        {
                public ConsoleLogger ()
                        : this (LoggerVerbosity.Normal)
                {
                }

                public ConsoleLogger (LoggerVerbosity verbosity)
                {
                        throw new NotImplementedException ();
                }

                public ConsoleLogger (LoggerVerbosity verbosity, WriteHandler write, ColorSetter colorSet, ColorResetter colorReset)
                {
                        throw new NotImplementedException ();
                }

                #region INodeLogger implementation
                public void Initialize (IEventSource eventSource, int nodeCount)
                {
                        throw new NotImplementedException ();
                }
                #endregion

                #region ILogger implementation
                public void Initialize (IEventSource eventSource)
                {
                        throw new NotImplementedException ();
                }

                public void Shutdown ()
                {
                        throw new NotImplementedException ();
                }

                public string Parameters {
                        get {
                                throw new NotImplementedException ();
                        }
                        set {
                                throw new NotImplementedException ();
                        }
                }

                public LoggerVerbosity Verbosity {
                        get {
                                throw new NotImplementedException ();
                        }
                        set {
                                throw new NotImplementedException ();
                        }
                }
                #endregion
        }
}

