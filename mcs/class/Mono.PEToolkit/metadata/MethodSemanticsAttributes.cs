/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Flags for MethodSemantics.
	/// </summary>
	/// <remarks>
	/// See Partiotion II, 22.1.10
	/// </remarks>
	[Flags]
	public enum MethodSemanticsAttributes {
		Setter   = 0x0001,
		Getter   = 0x0002,
		Other    = 0x0004,
		AddOn    = 0x0008,
		RemoveOn = 0x0010,
		Fire     = 0x0020,
	}

}

