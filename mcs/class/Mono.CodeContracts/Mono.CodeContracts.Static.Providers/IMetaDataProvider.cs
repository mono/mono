// 
// IMetaDataProvider.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Providers {
	interface IMetaDataProvider {
                TypeNode System_Single { get; }
                TypeNode System_Double { get; }
		TypeNode System_Int32 { get; }
		TypeNode System_String { get; }
		TypeNode System_Boolean { get; }
		TypeNode System_IntPtr { get; }
		TypeNode System_UIntPtr { get; }
		TypeNode System_Void { get; }
		TypeNode System_Array { get; }
		TypeNode System_Object { get; }
		TypeNode System_Int8 { get; }
		TypeNode System_Int64 { get; }
		TypeNode System_Int16 { get; }
		TypeNode System_UInt8 { get; }
		TypeNode System_UInt64 { get; }
		TypeNode System_UInt32 { get; }
		TypeNode System_UInt16 { get; }

		Result AccessMethodBody<Data, Result> (Method method, IMethodCodeConsumer<Data, Result> consumer, Data data);

		IEnumerable<Method> Methods (AssemblyNode assembly);
		IEnumerable<Property> Properties (TypeNode type);
		IEnumerable<Field> Fields (TypeNode type);
		IEnumerable<TypeNode> Interfaces (TypeNode type);
		IIndexable<Local> Locals (Method method);
		IIndexable<Parameter> Parameters (Method method);

		IEnumerable<Method> ImplementedMethods (Method method);
		IEnumerable<Method> OverridenAndImplementedMethods (Method method);

		Parameter This (Method method);

		string Name (Local local);
		string Name (Method method);
		string Name (Field field);
		string Name (TypeNode type);

		string FullName (Method method);
		string FullName (TypeNode type);

		TypeNode FieldType (Field field);
		TypeNode ParameterType (Parameter parameter);
		TypeNode LocalType (Local local);
		TypeNode ReturnType (Method method);
		TypeNode ManagedPointer (TypeNode type);
		TypeNode ElementType (TypeNode type);
		TypeNode ArrayType (TypeNode type, int rank);


		TypeNode DeclaringType (Method method);
		TypeNode DeclaringType (Field field);

		bool Equal (Method thisMethod, Method thatMethod);

		bool IsMain (Method method);

		bool IsStatic (Method method);
		bool IsStatic (Field field);
		bool IsStatic (Property property);

		bool IsPrivate (Method method);

		bool IsProtected (Method method);
		bool IsProtected (Field field);

		bool IsPublic (Method method);
		bool IsPublic (Field field);

		bool IsVirtual (Method method);
		bool IsNewSlot (Method method);
		bool IsOverride (Method method);
		bool IsFinal (Method method);
		bool IsConstructor (Method method);
		bool IsAbstract (Method method);
		bool IsCompilerGenerated (Field field);
		bool IsCompilerGenerated (Method method);

		bool IsAutoPropertyMember (Method method);
		bool IsPropertySetter (Method method, out Property property);
		bool IsPropertyGetter (Method method, out Property property);
		bool HasSetter (Property property, out Method method);
		bool HasGetter (Property property, out Method method);

		bool HasBody (Method method);
		bool DerivesFrom (TypeNode sub, TypeNode type);
		bool Equal (TypeNode type, TypeNode otherType);

		bool TryGetImplementingMethod (TypeNode type, Method calledMethod, out Method implementingMethod);
		bool TryGetRootMethod (Method method, out Method rootMethod);

		Field Unspecialized (Field field);
		Method Unspecialized (Method method);
		TypeNode Unspecialized (TypeNode type);

		Method DeclaringMethod (Parameter parameter);

		int ParameterIndex (Parameter parameter);
		int ParameterStackIndex (Parameter parameter);

		bool TryLoadAssembly (string filename, out AssemblyNode assembly, out string reasonOnFailure);

		string Name (Parameter parameter);

		bool IsReferenceType (TypeNode type);
		bool IsManagedPointer (TypeNode value);
		bool IsStruct (TypeNode type);
		bool IsInterface (TypeNode type);
		bool IsArray (TypeNode type);
		bool IsVoid (TypeNode type);
		bool IsReadonly (Field value);
		bool IsFinalizer (Method method);
		bool IsDispose (Method method);
		bool IsVoidMethod (Method method);
		bool IsOut (Parameter p);
		bool IsPrimitive (TypeNode type);
		bool IsClass (TypeNode type);

		bool IsAsVisibleAs (Field value, Method method);
		bool IsAsVisibleAs (Method value, Method method);
		bool IsVisibleFrom (Field field, TypeNode declaringType);
		bool IsVisibleFrom (Method value, TypeNode declaringType);
		bool IsVisibleFrom (TypeNode type, TypeNode fromType);

		bool TryGetSystemType (string fullName, out TypeNode type);

		bool HasValueRepresentation (TypeNode type);

		void IsSpecialized (Method calledMethod, ref IImmutableMap<TypeNode, TypeNode> specialization);
		bool IsSpecialized (Method method);

		bool IsFormalTypeParameter (TypeNode type);
		bool IsMethodFormalTypeParameter (TypeNode type);

		int TypeSize (TypeNode type);
	}
}
