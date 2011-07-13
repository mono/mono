// 
// CoreSystemTypes.cs
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
using Mono.Cecil;

namespace Mono.CodeContracts.Static.AST {
	sealed class CoreSystemTypes {
		private static CoreSystemTypes _instance;

		private readonly ModuleDefinition Module;
		private Lazy<AssemblyNode> systemAssembly;
		private Lazy<TypeNode> typeArray;
		private Lazy<TypeNode> typeBoolean;
		private Lazy<TypeNode> typeByte;
		private Lazy<TypeNode> typeChar;
		private Lazy<TypeNode> typeDouble;
		private Lazy<TypeNode> typeInt16;
		private Lazy<TypeNode> typeInt32;
		private Lazy<TypeNode> typeInt64;
		private Lazy<TypeNode> typeIntPtr;
		private Lazy<TypeNode> typeObject;
		private Lazy<TypeNode> typeSByte;
		private Lazy<TypeNode> typeSingle;
		private Lazy<TypeNode> typeString;
		private Lazy<TypeNode> typeSystemType;
		private Lazy<TypeNode> typeUInt16;
		private Lazy<TypeNode> typeUInt32;
		private Lazy<TypeNode> typeUInt64;

		private Lazy<TypeNode> typeUIntPtr;
		private Lazy<TypeNode> typeVoid;

		public CoreSystemTypes (ModuleDefinition module)
		{
			this.Module = module;

			InitializeLazyTypes ();
		}

		public static ModuleDefinition ModuleDefinition { get; set; }

		public static CoreSystemTypes Instance
		{
			get { return GetOrCreateInstance (ModuleDefinition); }
		}

		public TypeNode TypeObject
		{
			get { return this.typeObject.Value; }
		}

		public TypeNode TypeString
		{
			get { return this.typeString.Value; }
		}

		public TypeNode TypeBoolean
		{
			get { return this.typeBoolean.Value; }
		}

		public TypeNode TypeVoid
		{
			get { return this.typeVoid.Value; }
		}

		public TypeNode TypeSByte
		{
			get { return this.typeSByte.Value; }
		}

		public TypeNode TypeByte
		{
			get { return this.typeByte.Value; }
		}

		public TypeNode TypeInt16
		{
			get { return this.typeInt16.Value; }
		}

		public TypeNode TypeInt32
		{
			get { return this.typeInt32.Value; }
		}

		public TypeNode TypeInt64
		{
			get { return this.typeInt64.Value; }
		}

		public TypeNode TypeSingle
		{
			get { return this.typeSingle.Value; }
		}

		public TypeNode TypeDouble
		{
			get { return this.typeDouble.Value; }
		}

		public TypeNode TypeUInt16
		{
			get { return this.typeUInt16.Value; }
		}

		public TypeNode TypeUInt32
		{
			get { return this.typeUInt32.Value; }
		}

		public TypeNode TypeUInt64
		{
			get { return this.typeUInt64.Value; }
		}

		public AssemblyNode SystemAssembly
		{
			get { return this.systemAssembly.Value; }
		}

		public TypeNode TypeIntPtr
		{
			get { return this.typeIntPtr.Value; }
		}

		public TypeNode TypeArray
		{
			get { return this.typeArray.Value; }
		}

		public TypeNode TypeUIntPtr
		{
			get { return this.typeUIntPtr.Value; }
		}

		public TypeNode TypeChar
		{
			get { return this.typeChar.Value; }
		}

		public TypeNode TypeSystemType
		{
			get { return this.typeSystemType.Value; }
		}

		private static CoreSystemTypes GetOrCreateInstance (ModuleDefinition module)
		{
			if (_instance == null)
				_instance = new CoreSystemTypes (module);

			return _instance;
		}

		private void InitializeLazyTypes ()
		{
			this.typeVoid = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (void))));
			this.typeSByte = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (SByte))));
			this.typeByte = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Byte))));
			this.typeInt16 = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Int16))));
			this.typeInt32 = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Int32))));
			this.typeInt64 = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Int64))));
			this.typeUInt16 = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (UInt16))));
			this.typeUInt32 = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (UInt32))));
			this.typeUInt64 = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (UInt64))));
			this.typeSingle = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Single))));
			this.typeDouble = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Double))));
			this.typeBoolean = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Boolean))));
			this.typeObject = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (object))));
			this.typeString = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (string))));
			this.typeArray = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Array))));
			this.typeIntPtr = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (IntPtr))));
			this.typeUIntPtr = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (UIntPtr))));
			this.typeChar = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (char))));
			this.typeSystemType = new Lazy<TypeNode> (() => TypeNode.Create (this.Module.Import (typeof (Type))));

			this.systemAssembly = new Lazy<AssemblyNode> (() => AssemblyNode.GetSystemAssembly ());
		}
	}
}
