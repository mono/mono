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
		ArraysOfArrays,
		EntryPointMethod,
		GotoStatements,
		MultidimensionalArrays,
		StaticConstructors,
		TryCatchStatements,
		ReturnTypeAttributes,
		DeclareValueTypes,
		DeclareEnums,
		DeclareDelegates,
		DeclareInterfaces,
		DeclareEvents,
		AssemblyAttributes,
		ParameterAttributes,
		ReferenceParameters,
		ChainedConstructorArguments,
		NestedTypes,
		MultipleInterfaceMembers,
		PublicStaticMembers,
		ComplexExpressions,
		Win32Resources
	}
}

