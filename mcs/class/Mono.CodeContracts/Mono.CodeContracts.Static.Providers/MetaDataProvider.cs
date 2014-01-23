// 
// MetaDataProvider.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ContractExtraction;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Providers {
	class MetaDataProvider : IMetaDataProvider {
		public static readonly MetaDataProvider Instance = new MetaDataProvider ();

		#region IMetaDataProvider Members
		public Result AccessMethodBody<Data, Result> (Method method, IMethodCodeConsumer<Data, Result> consumer, Data data)
		{
			return consumer.Accept (CodeProviderImpl.Instance, CodeProviderImpl.Instance.Entry (method), method, data);
		}

		public bool IsReferenceType (TypeNode type)
		{
			return (!type.IsValueType);
		}

		public Method DeclaringMethod (Parameter parameter)
		{
			return parameter.DeclaringMethod;
		}

		public int ParameterIndex (Parameter parameter)
		{
			return parameter.Index;
		}

		public bool IsVoidMethod (Method method)
		{
			return IsVoid (method.ReturnType);
		}

		public bool TryLoadAssembly (string filename, out AssemblyNode assembly, out string reasonOfFailure)
		{
			assembly = AssemblyNode.ReadAssembly (filename);
			if (!TryLoadContractNodes (ref assembly)) {
				reasonOfFailure = "Couldn't find CodeContracts assembly in referencing assemblies";
				return false;
			}
			reasonOfFailure = null;
			return true;
		}

		public TypeNode ReturnType (Method method)
		{
			return method.ReturnType;
		}

		public IIndexable<Parameter> Parameters (Method method)
		{
			return new Indexable<Parameter> (method.Parameters);
		}

		public Parameter This (Method method)
		{
			return method.ThisParameter;
		}

		public string Name (Method method)
		{
			return method.Name;
		}

		public string Name (Field field)
		{
			return field.Name;
		}

		public string Name (TypeNode type)
		{
			return type.Name;
		}

		public TypeNode FieldType (Field field)
		{
			return field.FieldType;
		}

		public string FullName (Method method)
		{
			return method.FullName;
		}

		public string FullName (TypeNode type)
		{
			return type.FullName;
		}

		public TypeNode DeclaringType (Field field)
		{
			return field.DeclaringType;
		}

		public bool IsMain (Method method)
		{
			MethodDefinition entryPoint = method.Definition.Module.EntryPoint;

			return entryPoint != null && entryPoint.Equals (method);
		}

		public bool IsStatic (Method method)
		{
			return method.IsStatic;
		}

		public bool IsStatic (Field field)
		{
			return field.IsStatic;
		}

		public bool IsPrivate (Method method)
		{
			return method.IsPrivate;
		}

		public bool IsProtected (Method method)
		{
			return method.IsProtected;
		}

		public bool IsPublic (Method method)
		{
			return method.IsPublic;
		}

		public bool IsVirtual (Method method)
		{
			return method.IsVirtual;
		}

		public bool IsNewSlot (Method method)
		{
			return method.IsNewSlot;
		}

		public bool IsOverride (Method method)
		{
			return !method.IsNewSlot && method.HasOverrides;
		}

		public bool IsFinal (Method method)
		{
			return method.IsFinal;
		}

		public bool IsConstructor (Method method)
		{
			return method.IsConstructor;
		}

		public bool IsAbstract (Method method)
		{
			return method.IsAbstract;
		}

		public bool IsPropertySetter (Method method, out Property property)
		{
			//todo: implement this

			property = null;
			return false;
		}

		public bool IsPropertyGetter (Method method, out Property property)
		{
			//todo: implement this

			property = null;
			return false;
		}

		public TypeNode DeclaringType (Method method)
		{
			return method.DeclaringType;
		}

		public bool HasBody (Method method)
		{
			return method.HasBody;
		}

		public bool DerivesFrom (TypeNode sub, TypeNode type)
		{
			return sub.IsAssignableTo (type);
		}

		public bool Equal (TypeNode type, TypeNode otherType)
		{
			return type == otherType;
		}

		public bool TryGetImplementingMethod (TypeNode type, Method calledMethod, out Method implementingMethod)
		{
			//todo: implement this
			implementingMethod = null;
			return false;
		}

		public Method Unspecialized (Method method)
		{
			if (method.HasGenericParameters)
				throw new NotImplementedException ();

			return method;
		}

		public IEnumerable<Method> OverridenAndImplementedMethods (Method method)
		{
			//todo: implement this
			yield break;
		}

		public TypeNode ManagedPointer (TypeNode type)
		{
			return type.GetReferenceType ();
		}

		public bool TryGetRootMethod (Method method, out Method rootMethod)
		{
			//todo: implement this
			rootMethod = method;
			return true;
		}

		public IEnumerable<Method> ImplementedMethods (Method method)
		{
			yield break;
		}

		public bool IsAutoPropertyMember (Method method)
		{
			//todo: implement this
			return false;
		}

		public bool IsFinalizer (Method method)
		{
			return "Finalize" == method.Name;
		}

		public bool IsDispose (Method method)
		{
			if (method.Name != "Dispose" && method.Name != "System.IDisposable.Dispose")
				return false;
			if (method.Parameters == null || method.Parameters.Count == 0)
				return true;
			if (method.Parameters.Count == 1)
				return Equal(method.Parameters [0].Type, CoreSystemTypes.Instance.TypeBoolean);

			return false;
		}
		#endregion

		#region Implementation of IMetaDataProvider<Local,Parameter,Method,FieldReference,PropertyReference,EventReference,TypeNode,Attribute,AssemblyNode>

	        public TypeNode System_Single
	        {
	                get { return CoreSystemTypes.Instance.TypeSingle; }
	        }

	        public TypeNode System_Double
		{
			get { return CoreSystemTypes.Instance.TypeDouble; }
		}

	        public TypeNode System_Type
		{
			get { return CoreSystemTypes.Instance.TypeSystemType; }
		}

		public TypeNode System_Char
		{
			get { return CoreSystemTypes.Instance.TypeChar; }
		}

		public TypeNode System_Int32
		{
			get { return CoreSystemTypes.Instance.TypeInt32; }
		}

		public TypeNode System_String
		{
			get { return CoreSystemTypes.Instance.TypeString; }
		}

		public TypeNode System_Boolean
		{
			get { return CoreSystemTypes.Instance.TypeBoolean; }
		}

		public TypeNode System_IntPtr
		{
			get { return CoreSystemTypes.Instance.TypeIntPtr; }
		}

		public TypeNode System_UIntPtr
		{
			get { return CoreSystemTypes.Instance.TypeUIntPtr; }
		}

		public TypeNode System_Void
		{
			get { return CoreSystemTypes.Instance.TypeVoid; }
		}

		public TypeNode System_Array
		{
			get { return CoreSystemTypes.Instance.TypeArray; }
		}

		public TypeNode System_Object
		{
			get { return CoreSystemTypes.Instance.TypeObject; }
		}

		public TypeNode System_Int8
		{
			get { return CoreSystemTypes.Instance.TypeSByte; }
		}

		public TypeNode System_Int64
		{
			get { return CoreSystemTypes.Instance.TypeInt64; }
		}

		public TypeNode System_Int16
		{
			get { return CoreSystemTypes.Instance.TypeInt16; }
		}

		public TypeNode System_UInt8
		{
			get { return CoreSystemTypes.Instance.TypeByte; }
		}

		public TypeNode System_UInt64
		{
			get { return CoreSystemTypes.Instance.TypeUInt64; }
		}

		public TypeNode System_UInt32
		{
			get { return CoreSystemTypes.Instance.TypeUInt32; }
		}

		public TypeNode System_UInt16
		{
			get { return CoreSystemTypes.Instance.TypeUInt16; }
		}

		public IEnumerable<Method> Methods (AssemblyNode assembly)
		{
			return assembly.Modules.SelectMany (a => a.Types).SelectMany (t => t.Methods);
		}

		public string Name (Parameter parameter)
		{
			return parameter.Name;
		}

		public TypeNode ParameterType (Parameter parameter)
		{
			return parameter.Type;
		}

		public TypeNode LocalType (Local local)
		{
			return local.Type;
		}

		public bool IsStruct (TypeNode type)
		{
			return type.IsValueType;
		}

		public Field Unspecialized (Field field)
		{
			return field;
		}

		public bool IsManagedPointer (TypeNode value)
		{
			return value is Reference;
		}

		public bool IsArray (TypeNode type)
		{
			return type.IsArray;
		}

		public IEnumerable<TypeNode> Interfaces (TypeNode type)
		{
			return type.Interfaces;
		}

		public bool TryGetSystemType (string fullName, out TypeNode type)
		{
			int len = fullName.LastIndexOf (".");
			string ns = "";
			string className = fullName;
			if (len > 0) {
				ns = fullName.Substring (0, len);
				className = fullName.Substring (len + 1);
			}
			type = CoreSystemTypes.Instance.SystemAssembly.GetType (ns, className);
			return type != null;
		}

		public bool IsInterface (TypeNode type)
		{
			return type.IsInterface;
		}

		public IEnumerable<Property> Properties (TypeNode type)
		{
			return type.Properties;
		}

		public bool IsStatic (Property property)
		{
			return property.IsStatic;
		}

		public bool IsAsVisibleAs (Field value, Method method)
		{
			return HelperMethods.IsReferenceAsVisibleAs (value, method);
		}

		public bool IsAsVisibleAs (Method value, Method method)
		{
			return HelperMethods.IsReferenceAsVisibleAs (value, method);
		}

		public bool IsVisibleFrom (Field field, TypeNode declaringType)
		{
			return field.IsVisibleFrom (declaringType);
		}

		public bool IsVisibleFrom (Method value, TypeNode declaringType)
		{
			return value.IsVisibleFrom (declaringType);
		}

		public bool Equal (Method thisMethod, Method thatMethod)
		{
			return thisMethod == thatMethod;
		}

		public IIndexable<Local> Locals (Method method)
		{
			return new Indexable<Local> (method.Locals);
		}

		public TypeNode ElementType (TypeNode type)
		{
			var reference = type as Reference;
			if (reference != null)
				return reference.ElementType;
			//todo: array

			throw new NotImplementedException ();
		}

		public TypeNode Unspecialized (TypeNode type)
		{
			return type;
		}

		public bool IsProtected (Field field)
		{
			return field.IsFamily || field.IsFamilyAndAssembly || field.IsFamilyOrAssembly;
		}

		public bool IsPublic (Field field)
		{
			return field.IsPublic;
		}

		public int ParameterStackIndex (Parameter parameter)
		{
			Method declaringMethod = parameter.DeclaringMethod;
			return declaringMethod.Parameters.Count + (declaringMethod.IsStatic ? 0 : 1) - parameter.Index - 1;
		}

		public bool HasGetter (Property property, out Method method)
		{
			if (property.HasGetter) {
				method = property.Getter;
				return true;
			}
			method = null;
			return false;
		}

		public bool HasSetter (Property property, out Method method)
		{
			if (property.HasSetter) {
				method = property.Setter;
				return true;
			}
			method = null;
			return false;
		}

		public bool IsReadonly (Field value)
		{
			return value.IsReadonly;
		}

		public bool HasValueRepresentation (TypeNode type)
		{
			return !IsStruct (type) || IsPrimitive (type) || IsEnum (type);
		}

		public bool IsVoid (TypeNode type)
		{
			return type.Equals (System_Void);
		}

		public void IsSpecialized (Method calledMethod, ref IImmutableMap<TypeNode, TypeNode> specialization)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<Field> Fields (TypeNode type)
		{
			return type.Fields;
		}

		public bool IsOut (Parameter p)
		{
			return p.IsOut;
		}

		public bool IsPrimitive (TypeNode type)
		{
			return type.IsPrimitive;
		}

		public bool IsClass (TypeNode type)
		{
			return type.IsClass;
		}

		public bool IsVisibleFrom (TypeNode type, TypeNode fromType)
		{
			return type.IsVisibleFrom (fromType);
		}

		public bool IsCompilerGenerated (Method method)
		{
			return method.IsCompilerGenerated;
		}

		public bool IsSpecialized (Method method)
		{
			return false;
		}

		public bool IsFormalTypeParameter (TypeNode type)
		{
			return false;
		}

		public bool IsMethodFormalTypeParameter (TypeNode type)
		{
			return false;
		}

		public TypeNode ArrayType (TypeNode type, int rank)
		{
			return type.GetArrayType (rank);
		}

		public bool IsCompilerGenerated (Field field)
		{
			return field.IsCompilerGenerated;
		}

		public int TypeSize (TypeNode type)
		{
			int classSize = type.ClassSize;
			if (classSize > 0)
				return classSize;
			int size = TypeSizeHelper (type);
			type.ClassSize = size;
			return size;
		}

		public string Name (Local local)
		{
			return local.Name;
		}

		private int TypeSizeHelper (TypeNode type)
		{
			if (IsManagedPointer (type))
				return 4;
			if (type == System_Boolean)
				return 1;
			if (type == System_Char)
				return 2;
			if (type == System_Int8)
				return 1;
			if (type == System_Int16)
				return 2;
			if (type == System_Int32)
				return 4;
			if (type == System_Int64)
				return 8;
			if (type == System_IntPtr || type == System_Object || type == System_String || type == System_Type)
				return 4;

			if (type == System_UInt8)
				return 1;
			if (type == System_UInt16)
				return 2;
			if (type == System_UInt32)
				return 4;
			if (type == System_UInt64)
				return 8;

			if (type == System_UIntPtr || type == System_Single)
				return 4;
			if (type == System_Double)
				return 8;

			return -1;
		}

		private bool IsEnum (TypeNode type)
		{
			return type.IsEnum;
		}
		#endregion

		private bool TryLoadContractNodes (ref AssemblyNode assembly)
		{
			ContractNodes nodes = null;
			foreach (Module module in assembly.Modules) {
				IAssemblyResolver assemblyResolver = module.Definition.AssemblyResolver;
				foreach (AssemblyNameReference reference in module.Definition.AssemblyReferences) {
					AssemblyDefinition def = assemblyResolver.Resolve (reference);
					nodes = ContractNodes.GetContractNodes (new AssemblyNode (def), (s) => { });
					if (nodes != null)
						break;
				}
			}

			if (nodes == null)
				return false;

			var extractor = new ContractExtractor (nodes, assembly, true);
			assembly = (AssemblyNode) extractor.Visit (assembly);
			return true;
		}
	}
}
