using System;

namespace Mono.Security.Interface
{
	[Flags]
	// Keep in sync with SchProtocols / native SChannel.h
	// Unfortunately, the definition in System.dll is not public, so we need to duplicate it here.
	public enum TlsProtocols {
		Zero                = 0,
		Tls10Client         = 0x00000080,
		Tls10Server         = 0x00000040,
		Tls10               = (Tls10Client | Tls10Server),
		Tls11Client         = 0x00000200,
		Tls11Server         = 0x00000100,
		Tls11               = (Tls11Client | Tls11Server),
		Tls12Client         = 0x00000800,
		Tls12Server         = 0x00000400,
		Tls12               = (Tls12Client | Tls12Server),
		ClientMask          = (Tls10Client | Tls11Client | Tls12Client),
		ServerMask          = (Tls10Server | Tls11Server | Tls12Server)
	};
}

