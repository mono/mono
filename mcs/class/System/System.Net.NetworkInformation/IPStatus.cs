//
// System.Net.NetworkInformation.IPStatus
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
namespace System.Net.NetworkInformation {
	public enum IPStatus {
		Unknown = -1,
		Success = 0,
		DestinationNetworkUnreachable = 11002,
		DestinationHostUnreachable = 11003,
		DestinationProhibited = 11004,
		DestinationProtocolUnreachable = 11004,
		DestinationPortUnreachable = 11005,
		NoResources = 11006,
		BadOption = 11007,
		HardwareError = 11008,
		PacketTooBig = 11009,
		TimedOut = 11010,
		BadRoute = 11012,
		TtlExpired = 11013,
		TtlReassemblyTimeExceeded = 11014,
		ParameterProblem = 11015,
		SourceQuench = 11016,
		BadDestination = 11018,
		DestinationUnreachable = 11040,
		TimeExceeded = 11041,
		BadHeader = 11042,
		UnrecognizedNextHeader = 11043,
		IcmpError = 11044,
		DestinationScopeMismatch = 11045
	}
}


