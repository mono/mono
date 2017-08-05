//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell (http://www.novell.com)
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

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;



#if MONO_POSIX_NETSTANDARD_BUILD
[assembly: AssemblyVersion ("1.0.0.0")]
[assembly: AssemblyTitle("Mono.Posix.NETStandard.dll")]
#else
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: AssemblyTitle("Mono.Posix.dll")]
#endif

[assembly: AssemblyDescription("Unix Integration Classes")]

[assembly: CLSCompliant (true)]
[assembly: ComVisible (false)]

/* TODO COMPLETE INFORMATION

[assembly: AssemblyFileVersion ("0.0.0.1")]

*/

#if !MONO_POSIX_NETSTANDARD_BUILD
// We are using ../Open.snk for MONO_POSIX_NETSTANDARD_BUILD
[assembly: AssemblyDelaySign (true)]
#endif
/*
 * TODO:
 * 
 * Anything implementing IDisposable should derive from MarshalByRefObject.
 * This is for remoting situations (e.g. across AppDomains).
 * Impacts UnixClient, UnixListener.
 * 
 * UnixPath.InvalidPathChars should be const, not readonly.
 * 
 * Mono.Remoting.Channels.Unix.UnixChannel.CreateMessageSink should have a LinkDemand
 * idential to IChannelSender's CreateMessageSink LinkDemand.
 * Repeat for all other members of UnixChannel, UnixClient, UnixServer.
 * 
 * Override .Equals and the == operator for all structures.
 */
