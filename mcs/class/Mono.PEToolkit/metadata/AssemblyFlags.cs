/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Values for AssemblyFlags.
	/// </summary>
	/// <remarks>
	/// See Partition II, 22.1.2
	/// </remarks>
	[Flags]
	public enum AssemblyFlags {
		/// <summary>
		/// The assembly reference holds the full (unhashed) public key.
		/// </summary>
		PublicKey = 0x0001,

		/// <summary>
		/// The assembly is side by side compatible.
		/// </summary>
		SideBySideCompatible = 0x0000,

		/// <summary>
		/// The assembly cannot execute with other versions
		/// if they are executing in the same application domain.
		/// </summary>
		NonSideBySideAppDomain = 0x0010,

		/// <summary>
		/// The assembly cannot execute with other versions
		/// if they are executing in the same process.
		/// </summary>
		NonSideBySideProcess = 0x0020,

		/// <summary>
		/// The assembly cannot execute with other versions
		/// if they are executing on the same machine.
		/// </summary>
		NonSideBySideMachine = 0x0030,

		/// <summary>
		/// JIT should generate CIL-to-native code map.
		/// </summary>
		EnableJITcompileTracking = 0x8000,

		/// <summary>
		/// JIT should not generate optimized code.
		/// </summary>
		DisableJITcompileOptimizer = 0x4000,
	}

}
