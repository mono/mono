//
// System.Runtime.InteropServices.TypeLibImporterFlags.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Flags]
	public enum TypeLibImporterFlags
	{
		PrimaryInteropAssembly = 1,
		UnsafeInterfaces = 2,
		SafeArrayAsSystemArray = 4,
		TransformDispRetVals = 8
	}
}
