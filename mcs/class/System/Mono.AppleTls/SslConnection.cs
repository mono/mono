//
// SslConnection
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2014 Xamarin Inc.
//

using System;

namespace Mono.AppleTls 
{
	delegate SslStatus SslReadFunc (IntPtr connection, IntPtr data, /* size_t* */ ref IntPtr dataLength);
	delegate SslStatus SslWriteFunc (IntPtr connection, IntPtr data, /* size_t* */ ref IntPtr dataLength);
}
