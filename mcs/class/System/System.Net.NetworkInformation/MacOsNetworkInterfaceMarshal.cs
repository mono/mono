using System;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	namespace MacOsStructs {
		internal struct ifaddrs
		{
			public IntPtr  ifa_next;
			public string  ifa_name;
			public uint    ifa_flags;
			public IntPtr  ifa_addr;
			public IntPtr  ifa_netmask;
			public IntPtr  ifa_dstaddr;
			public IntPtr  ifa_data;
		}

		internal struct sockaddr
		{
			public byte  sa_len;
			public byte  sa_family;
		}

		internal struct sockaddr_in
		{
			public byte   sin_len;
			public byte   sin_family;
			public ushort sin_port;
			public uint   sin_addr;
		}

		internal struct in6_addr
		{
			[MarshalAs (UnmanagedType.ByValArray, SizeConst=16)]
			public byte[] u6_addr8;
		}

		internal struct sockaddr_in6
		{
			public byte     sin6_len;
			public byte     sin6_family;
			public ushort   sin6_port;
			public uint     sin6_flowinfo;
			public in6_addr sin6_addr;
			public uint     sin6_scope_id;
		}

		internal struct sockaddr_dl
		{
			public byte   sdl_len;
			public byte   sdl_family;
			public ushort sdl_index;
			public byte   sdl_type;
			public byte   sdl_nlen;
			public byte   sdl_alen;
			public byte   sdl_slen;
			public byte[] sdl_data;
			
			internal void Read (IntPtr ptr)
			{
				sdl_len = Marshal.ReadByte (ptr, 0);
				sdl_family = Marshal.ReadByte (ptr, 1);
				sdl_index = (ushort) Marshal.ReadInt16 (ptr, 2);
				sdl_type = Marshal.ReadByte (ptr, 4);
				sdl_nlen = Marshal.ReadByte (ptr, 5);
				sdl_alen = Marshal.ReadByte (ptr, 6);
				sdl_slen = Marshal.ReadByte (ptr, 7);
				sdl_data = new byte [Math.Max (12, sdl_len - 8)];
				Marshal.Copy (new IntPtr (ptr.ToInt64 () + 8), sdl_data, 0, sdl_data.Length);
			}
		}

	}

	internal enum MacOsArpHardware {
		ETHER = 0x6,
		ATM = 0x25,
		SLIP = 0x1c,
		PPP = 0x17,
		LOOPBACK = 0x18,
		FDDI = 0xf
	}
}
