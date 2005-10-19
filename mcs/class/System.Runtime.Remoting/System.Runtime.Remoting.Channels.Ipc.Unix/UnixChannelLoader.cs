//
// System.Runtime.Remoting.Channels.Ipc.Unix.UnixChannelLoader.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Reflection;

namespace System.Runtime.Remoting.Channels.Ipc.Unix
{
        internal sealed class UnixChannelLoader
        {
                UnixChannelLoader ()
                {
                }

                static object _asmLock = new object ();
                static Assembly _asm;

                static Assembly channelAssembly
                {
                        get {
                                lock (_asmLock) {
                                        if (_asm == null)
                                                _asm = Assembly.Load (Consts.AssemblyMono_Posix);
                                }
                                return _asm;
                        }
                }

                static Type Load (string className)
                {
                        return channelAssembly.GetType ("Mono.Remoting.Channels.Unix." + className, true);
                }

                public static Type LoadChannel ()
                {
                        return Load ("UnixChannel");
                }

                public static Type LoadClientChannel ()
                {
                        return Load ("UnixClientChannel");
                }

                public static Type LoadServerChannel ()
                {
                        return Load ("UnixServerChannel");
                }
        }
}

#endif
