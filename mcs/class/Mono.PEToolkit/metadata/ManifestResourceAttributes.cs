/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Flags for ManifestResource.
	/// </summary>
	/// <remarks>
	/// See Partition II, 22.1.8
	/// </remarks>
	[Flags]
	public enum ManifestResourceAttributes {
		VisibilityMask = 0x0007,

		/// <summary>
		/// The Resource is exported from the Assembly.
		/// </summary>
		Public = 0x0001,

		/// <summary>
		/// The Resource is private to the Assembly.
		/// </summary>
		Private = 0x0002,
	}
}
