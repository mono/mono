//
// System.CodeDom.Compiler GeneratorSupport Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2002 Ximian, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		Win32Resources = 1 << 20,
		Resources = 1 << 21,
		PartialTypes = 1 << 22,
		GenericTypeReference = 1 << 23,
		GenericTypeDeclaration = 1 << 24,
		DeclareIndexerProperties = 1 << 25,
	}
}

