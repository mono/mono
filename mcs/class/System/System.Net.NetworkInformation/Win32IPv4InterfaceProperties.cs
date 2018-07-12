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
#if WIN_PLATFORM
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	sealed class Win32IPv4InterfaceProperties : IPv4InterfaceProperties
	{
		[DllImport ("iphlpapi.dll")]
		static extern int GetPerAdapterInfo (int IfIndex, Win32_IP_PER_ADAPTER_INFO pPerAdapterInfo, ref int pOutBufLen);

		Win32_IP_ADAPTER_INFO ainfo;
		Win32_IP_PER_ADAPTER_INFO painfo;
		Win32_MIB_IFROW mib;

		public Win32IPv4InterfaceProperties (Win32_IP_ADAPTER_INFO ainfo, Win32_MIB_IFROW mib)
		{
			this.ainfo = ainfo;
			this.mib = mib;

			// get per-adapter info.
			int size = 0;
			GetPerAdapterInfo (mib.Index, null, ref size);
			painfo = new Win32_IP_PER_ADAPTER_INFO ();
			int ret = GetPerAdapterInfo (mib.Index, painfo, ref size);
			if (ret != 0)
				throw new NetworkInformationException (ret);
		}

		public override int Index {
			get { return mib.Index; }
		}

		public override bool IsAutomaticPrivateAddressingActive {
			get { return painfo.AutoconfigActive != 0; }
		}

		public override bool IsAutomaticPrivateAddressingEnabled {
			get { return painfo.AutoconfigEnabled != 0; }
		}

		public override bool IsDhcpEnabled {
			get { return ainfo.DhcpEnabled != 0; }
		}

		public override bool IsForwardingEnabled {
			// Is it the right answer? In Vista there is MIB_IPINTERFACEROW.ForwardingEnabled, but not in former versions.
			get { return Win32NetworkInterface.FixedInfo.EnableRouting != 0; }
		}

		public override int Mtu {
			get { return mib.Mtu; }
		}

		public override bool UsesWins {
			get { return ainfo.HaveWins; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	class Win32_IP_PER_ADAPTER_INFO
	{
		public uint AutoconfigEnabled;
		public uint AutoconfigActive;
		public IntPtr CurrentDnsServer; // to Win32_IP_ADDR_STRING
		public Win32_IP_ADDR_STRING DnsServerList;
	}
}
#endif
