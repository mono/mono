//
// System.Net.NetworkInformation.IPv4InterfaceProperties
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//      Marek Habersack (mhabersack@novell.com)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
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
	abstract class UnixIPv4InterfaceProperties : IPv4InterfaceProperties
	{
		protected UnixNetworkInterface iface;

		public UnixIPv4InterfaceProperties (UnixNetworkInterface iface)
		{
			this.iface = iface;
		}

		public override int Index {
			get { return iface.NameIndex; }
		}

		// TODO: how to discover that?
		public override bool IsAutomaticPrivateAddressingActive {
			get { return false; }
		}

		// TODO: how to discover that?
		public override bool IsAutomaticPrivateAddressingEnabled {
			get { return false; }
		}

		// TODO: how to discover that? The only way is distribution-specific...
		public override bool IsDhcpEnabled {
			get { return false; }
		}

		public override bool UsesWins {
			get { return false; }
		}
	}
}
