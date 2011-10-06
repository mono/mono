//
// UdpReceiveResult.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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

using System.Runtime.CompilerServices;

namespace System.Net.Sockets
{
	public struct UdpReceiveResult : IEquatable<UdpReceiveResult>
	{
		public UdpReceiveResult (byte[] buffer, IPEndPoint remoteEndPoint)
			: this ()
		{
			if (buffer == null) {
				throw new ArgumentNullException ("buffer");
			}

			if (remoteEndPoint == null) {
				throw new ArgumentNullException ("remoteEndPoint");
			}

			this.Buffer = buffer;
			this.RemoteEndPoint = remoteEndPoint;
		}

		public byte[] Buffer { get; private set; }
		public IPEndPoint RemoteEndPoint { get; private set; }

		public override int GetHashCode ()
		{
			return RuntimeHelpers.GetHashCode (Buffer) ^ RuntimeHelpers.GetHashCode (RemoteEndPoint);
		}

		public override bool Equals (object obj)
		{
			return obj is UdpReceiveResult && Equals ((UdpReceiveResult) obj);
		}

		public bool Equals (UdpReceiveResult other)
		{
			return Buffer == other.Buffer && Equals (RemoteEndPoint, other.RemoteEndPoint);
		}

		public static bool operator == (UdpReceiveResult left, UdpReceiveResult right)
		{
			return left.Equals (right);
		}

		public static bool operator != (UdpReceiveResult left, UdpReceiveResult right)
		{
			return !left.Equals (right);
		}
	}
}