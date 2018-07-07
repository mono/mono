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
using System.IO;

namespace System.Net.NetworkInformation {
	sealed class LinuxIPv4InterfaceProperties : UnixIPv4InterfaceProperties
	{
		public LinuxIPv4InterfaceProperties (LinuxNetworkInterface iface)
			: base (iface)
		{
		}

		public override bool IsForwardingEnabled {
			get {
				string iface_path = "/proc/sys/net/ipv4/conf/" + iface.Name + "/forwarding";

				if (File.Exists (iface_path)) {
					string val = LinuxNetworkInterface.ReadLine (iface_path);

					return val != "0";
				}

				return false;
			}
		}

		public override int Mtu {
			get {
				string iface_path = (iface as LinuxNetworkInterface).IfacePath + "mtu";
				int ret = 0;

				if (File.Exists (iface_path)) {
					string val = LinuxNetworkInterface.ReadLine (iface_path);

					try {
						ret = Int32.Parse (val);
					} catch {
					}
				}

				return ret;

			}
		}
	}
}
