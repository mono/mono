//
// System.CodeDom.Compiler GeneratorSupport Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

namespace System.CodeDom.Compiler
{
	[Flags]
	[Serializable]
	public enum GeneratorSupport {
		ArraysOfArrays = 1,
		EntryPointMethod = 1 << 1,
		GotoStatements = 1 << 2,
		MultidimensionalArrays = 1 << 3,
		StaticConstructors = 1 << 4,
		TryCatchStatements = 1 << 5,
		ReturnTypeAttributes = 1 << 6,
		DeclareValueTypes = 1 << 7,
		DeclareEnums = 1 << 8,
		DeclareDelegates = 1 << 9,
		DeclareInterfaces = 1 << 10,
		DeclareEvents = 1 << 11,
		AssemblyAttributes = 1 << 12,
		ParameterAttributes = 1 << 13,
		ReferenceParameters = 1 << 14,
		ChainedConstructorArguments = 1 << 15,
		NestedTypes = 1 << 16,
		MultipleInterfaceMembers = 1 << 17,
		PublicStaticMembers = 1 << 18,
		ComplexExpressions = 1 << 19,
		Win32Resources = 1 << 20
	}
}

