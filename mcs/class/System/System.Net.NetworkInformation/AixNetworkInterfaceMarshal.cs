using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	namespace AixStructs {
		//[StructLayout(LayoutKind.Sequential)]
		[StructLayout(LayoutKind.Explicit, Size = 16)]
		internal struct ifconf
		{
			[FieldOffset (0)]
			public int ifc_len; /* size of buffer */
			[FieldOffset (8)]
			public IntPtr ifc_buf; /* buffer address/array of structures returned */
		}

		// approximate the union members after the name with different structs
		[StructLayout (LayoutKind.Explicit, CharSet=CharSet.Ansi, Size=18)]
		internal unsafe struct ifreq
		{
			// it must be a byte array; a char array seems to want to be 2 bytes per element,
			// and a ByVal string doesn't want to seem to marshal properly for being written to
			[FieldOffset (0)]
			public fixed byte ifr_name [16];
			// you must peer into the family and length, then use ptr arith to get the rest
			[FieldOffset (16)]
			public sockaddr ifru_addr;
		}

		[StructLayout (LayoutKind.Explicit, CharSet=CharSet.Ansi, Size=24)]
		internal unsafe struct ifreq_addrin
		{
			[FieldOffset (0)]
			public fixed byte ifr_name [16];
			[FieldOffset (16)]
			public sockaddr_in ifru_addr;
		}

		// For SIOCGIFFLAGS
		[StructLayout (LayoutKind.Explicit, CharSet=CharSet.Ansi, Size=20)]
		internal unsafe struct ifreq_flags
		{
			[FieldOffset (0)]
			public fixed byte ifr_name [16];
			[FieldOffset (16)]
			public uint ifru_flags;
		}

		// For SIOCGIFMTU
		[StructLayout (LayoutKind.Explicit, CharSet=CharSet.Ansi, Size=20)]
		internal unsafe struct ifreq_mtu
		{
			[FieldOffset (0)]
			public fixed byte ifr_name [16];
			[FieldOffset (16)]
			public int ifru_mtu;
		}

		// the rest copied from Mac OS defs
		[StructLayout(LayoutKind.Sequential)]
		internal struct sockaddr
		{
			public byte  sa_len;
			public byte  sa_family;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct sockaddr_in
		{
			public byte   sin_len;
			public byte   sin_family;
			public ushort sin_port;
			public uint   sin_addr;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct in6_addr
		{
			[MarshalAs (UnmanagedType.ByValArray, SizeConst=16)]
			public byte[] u6_addr8;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct sockaddr_in6
		{
			public byte     sin6_len;
			public byte     sin6_family;
			public ushort   sin6_port;
			public uint     sin6_flowinfo;
			public in6_addr sin6_addr;
			public uint     sin6_scope_id;
		}

		[StructLayout(LayoutKind.Sequential)]
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

	// see: net/if_types.h
	internal enum AixArpHardware {
		ETHER = 0x6,
		ATM = 0x25,
		SLIP = 0x1c,
		PPP = 0x17,
		LOOPBACK = 0x18,
		FDDI = 0xf
	}

	// see: net/if.h
	internal enum AixInterfaceFlags {
		IFF_UP = 0x1,				/* interface is up */
		IFF_BROADCAST = 0x2,		/* broadcast address valid */
		IFF_DEBUG = 0x4,			/* turn on debugging */
		IFF_LOOPBACK = 0x8,			/* is a loopback net */
		IFF_POINTOPOINT = 0x10,		/* interface is point-to-point link */
		IFF_NOTRAILERS = 0x20,		/* avoid use of trailers */
		IFF_RUNNING = 0x40,			/* resources allocated */
		IFF_NOARP = 0x80,			/* no address resolution protocol */
		IFF_PROMISC = 0x100,		/* receive all packets */
		IFF_ALLMULTI = 0x200,		/* receive all multicast packets */
		IFF_OACTIVE = 0x400,		/* transmission in progress */
		IFF_SIMPLEX = 0x800,		/* can't hear own transmissions */
		IFF_LINK0 = 0x100000,			/* per link layer defined bit */
		IFF_LINK1 = 0x200000,			/* per link layer defined bit */
		IFF_LINK2 = 0x400000,			/* per link layer defined bit */
		IFF_MULTICAST = 0x8000000		/* supports multicast */
	}

	// Address families that matter to us
	internal enum AixAddressFamily {
		AF_INET  = 2,
		AF_INET6 = 24,
		AF_LINK  = 18,
	}

	// ioctl commands that matter to us
	internal enum AixIoctlRequest : uint {
		SIOCGSIZIFCONF = 0x4004696a, /* get the buffer size for SIOCGIFCONF */
                SIOCGIFCONF    = 0xc0106945, /* list network interfaces */
                SIOCGIFFLAGS   = 0xc0286911, /* get interface flags */
                SIOCGIFNETMASK = 0xc0286925, /* get netmask for iface */
                SIOCGIFMTU     = 0xc0286956, /* get mtu for iface */
	}
}

