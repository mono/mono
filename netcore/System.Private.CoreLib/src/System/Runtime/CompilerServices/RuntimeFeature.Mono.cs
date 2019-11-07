// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices
{
	partial class RuntimeFeature
	{
		// the JIT/AOT compiler will change these flags to false for FullAOT scenarios
		public static bool IsDynamicCodeSupported => true;
		public static bool IsDynamicCodeCompiled => true;
	}
}
