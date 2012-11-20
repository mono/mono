/*
  Copyright (C) 2009-2012 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum AssemblyNameFlags
	{
		None = 0,
		PublicKey = 1,
		Retargetable = 256,
		EnableJITcompileOptimizer = 16384,
		EnableJITcompileTracking = 32768,
	}

	public enum AssemblyContentType
	{
		Default = 0,
		WindowsRuntime = 1,
	}

	[Flags]
	public enum BindingFlags
	{
		Default = 0,
		IgnoreCase = 1,
		DeclaredOnly = 2,
		Instance = 4,
		Static = 8,
		Public = 16,
		NonPublic = 32,
		FlattenHierarchy = 64,
	}

	[Flags]
	public enum CallingConventions
	{
		Standard = 1,
		VarArgs = 2,
		Any = 3,
		HasThis = 32,
		ExplicitThis = 64,
	}

	[Flags]
	public enum EventAttributes
	{
		None = 0,
		SpecialName = 512,
		RTSpecialName = 1024,
		ReservedMask = 1024,
	}

	[Flags]
	public enum FieldAttributes
	{
		PrivateScope = 0,
		Private = 1,
		FamANDAssem = 2,
		Assembly = 3,
		Family = 4,
		FamORAssem = 5,
		Public = 6,
		FieldAccessMask = 7,
		Static = 16,
		InitOnly = 32,
		Literal = 64,
		NotSerialized = 128,
		HasFieldRVA = 256,
		SpecialName = 512,
		RTSpecialName = 1024,
		HasFieldMarshal = 4096,
		PinvokeImpl = 8192,
		HasDefault = 32768,
		ReservedMask = 38144,
	}

	[Flags]
	public enum GenericParameterAttributes
	{
		None = 0,
		Covariant = 1,
		Contravariant = 2,
		VarianceMask = 3,
		ReferenceTypeConstraint = 4,
		NotNullableValueTypeConstraint = 8,
		DefaultConstructorConstraint = 16,
		SpecialConstraintMask = 28,
	}

	public enum ImageFileMachine
	{
		I386 = 332,
		ARM = 452,
		IA64 = 512,
		AMD64 = 34404,
	}

	[FlagsAttribute]
	public enum MemberTypes
	{
		Constructor = 0x01,
		Event = 0x02,
		Field = 0x04,
		Method = 0x08,
		Property = 0x10,
		TypeInfo = 0x20,
		Custom = 0x40,
		NestedType = 0x80,
		All = Constructor | Event | Field | Method | Property | TypeInfo | NestedType
	}

	[Flags]
	public enum MethodAttributes
	{
		MemberAccessMask		= 0x0007,
		PrivateScope			= 0x0000,
		Private					= 0x0001,
		FamANDAssem				= 0x0002,
		Assembly				= 0x0003,
		Family					= 0x0004,
		FamORAssem				= 0x0005,
		Public					= 0x0006,
		Static					= 0x0010,
		Final					= 0x0020,
		Virtual					= 0x0040,
		HideBySig				= 0x0080,
		VtableLayoutMask		= 0x0100,
		ReuseSlot				= 0x0000,
		NewSlot					= 0x0100,
		CheckAccessOnOverride	= 0x0200,
		Abstract				= 0x0400,
		SpecialName				= 0x0800,

		PinvokeImpl				= 0x2000,
		UnmanagedExport			= 0x0008,

		RTSpecialName			= 0x1000,
		HasSecurity				= 0x4000,
		RequireSecObject		= 0x8000,

		ReservedMask			= 0xd000,
	}

	[Flags]
	public enum MethodImplAttributes
	{
		CodeTypeMask		= 0x0003,
		IL					= 0x0000,
		Native				= 0x0001,
		OPTIL				= 0x0002,
		Runtime				= 0x0003,
		ManagedMask			= 0x0004,
		Unmanaged			= 0x0004,
		Managed				= 0x0000,

		ForwardRef			= 0x0010,
		PreserveSig			= 0x0080,
		InternalCall		= 0x1000,
		Synchronized		= 0x0020,
		NoInlining			= 0x0008,
		NoOptimization		= 0x0040,
		AggressiveInlining  = 0x0100,

		MaxMethodImplVal	= 0xffff,
	}

	[Flags]
	public enum ParameterAttributes
	{
		None = 0,
		In = 1,
		Out = 2,
		Lcid = 4,
		Retval = 8,
		Optional = 16,
		HasDefault = 4096,
		HasFieldMarshal = 8192,
		Reserved3 = 16384,
		Reserved4 = 32768,
		ReservedMask = 61440,
	}

	[Flags]
	public enum PortableExecutableKinds
	{
		NotAPortableExecutableImage = 0,
		ILOnly = 1,
		Required32Bit = 2,
		PE32Plus = 4,
		Unmanaged32Bit = 8,
		Preferred32Bit = 16,
	}

	public enum ProcessorArchitecture
	{
		None = 0,
		MSIL = 1,
		X86 = 2,
		IA64 = 3,
		Amd64 = 4,
		Arm = 5,
		// if an item is added here, make sure to update AssemblyName.ProcessorArchitecture,
		// AssemblyReader.GetNameImpl() and Fusion.ParseAssemblyName as well
	}

	[Flags]
	public enum PropertyAttributes
	{
		None = 0,
		SpecialName = 512,
		RTSpecialName = 1024,
		HasDefault = 4096,
	}

	[Flags]
	public enum ResourceAttributes
	{
		Public = 1,
		Private = 2,
	}

	[Flags]
	public enum ResourceLocation
	{
		Embedded = 1,
		ContainedInAnotherAssembly = 2,
		ContainedInManifestFile = 4,
	}

	[Flags]
	public enum TypeAttributes
	{
		AnsiClass = 0,
		Class = 0,
		AutoLayout = 0,
		NotPublic = 0,
		Public = 1,
		NestedPublic = 2,
		NestedPrivate = 3,
		NestedFamily = 4,
		NestedAssembly = 5,
		NestedFamANDAssem = 6,
		VisibilityMask = 7,
		NestedFamORAssem = 7,
		SequentialLayout = 8,
		ExplicitLayout = 16,
		LayoutMask = 24,
		ClassSemanticsMask = 32,
		Interface = 32,
		Abstract = 128,
		Sealed = 256,
		SpecialName = 1024,
		RTSpecialName = 2048,
		Import = 4096,
		Serializable = 8192,
		WindowsRuntime = 16384,
		UnicodeClass = 65536,
		AutoClass = 131072,
		CustomFormatClass = 196608,
		StringFormatMask = 196608,
		HasSecurity = 262144,
		ReservedMask = 264192,
		BeforeFieldInit = 1048576,
		CustomFormatMask = 12582912,
	}

	// IKVM.Reflection specific type
	[Flags]
	public enum DllCharacteristics
	{
		HighEntropyVA = 0x0020,			// IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA
		DynamicBase = 0x0040,			// IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE
		NoSEH = 0x0400,					// IMAGE_DLLCHARACTERISTICS_NO_SEH
		NXCompat = 0x0100,				// IMAGE_DLLCHARACTERISTICS_NX_COMPAT
		AppContainer = 0x1000,			// IMAGE_DLLCHARACTERISTICS_APPCONTAINER
		TerminalServerAware = 0x8000,	// IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE
	}

	// IKVM.Reflection specific type
	[Flags]
	public enum ImplMapFlags
	{
		NoMangle = 0x0001,
		CharSetMask = 0x0006,
		CharSetNotSpec = 0x0000,
		CharSetAnsi = 0x0002,
		CharSetUnicode = 0x0004,
		CharSetAuto = 0x0006,
		SupportsLastError = 0x0040,
		CallConvMask = 0x0700,
		CallConvWinapi = 0x0100,
		CallConvCdecl = 0x0200,
		CallConvStdcall = 0x0300,
		CallConvThiscall = 0x0400,
		CallConvFastcall = 0x0500,
		// non-standard flags (i.e. CLR specific)
		BestFitOn = 0x0010,
		BestFitOff = 0x0020,
		CharMapErrorOn = 0x1000,
		CharMapErrorOff = 0x2000,
	}
}
