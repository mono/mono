#if MONO_FEATURE_APPLETLS
//
// SslConnection
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2014 Xamarin Inc.
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using ObjCRuntime;

namespace Mono.AppleTls 
{
	delegate SslStatus SslReadFunc (IntPtr connection, IntPtr data, /* size_t* */ ref IntPtr dataLength);
	delegate SslStatus SslWriteFunc (IntPtr connection, IntPtr data, /* size_t* */ ref IntPtr dataLength);
}
#endif
