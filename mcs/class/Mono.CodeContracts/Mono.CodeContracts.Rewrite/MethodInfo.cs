using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite {
	class MethodInfo {

		public MethodInfo (MethodDefinition method)
		{
			this.Method = method;
			this.Module = method.Module;

			this.typeVoid = new Lazy<TypeReference> (() => this.Module.Import (typeof (void)));
			this.typeObject = new Lazy<TypeReference> (() => this.Module.Import (typeof (object)));
			this.typeInt32 = new Lazy<TypeReference> (() => this.Module.Import (typeof (int)));
			this.typeInt64 = new Lazy<TypeReference> (() => this.Module.Import (typeof (long)));
			this.typeUInt32 = new Lazy<TypeReference> (() => this.Module.Import (typeof (uint)));
			this.typeBoolean = new Lazy<TypeReference> (() => this.Module.Import (typeof (bool)));
			this.typeString = new Lazy<TypeReference> (() => this.Module.Import (typeof (string)));
		}

		public MethodDefinition Method{get;private set;}
		public ModuleDefinition Module { get; private set; }

		private Lazy<TypeReference> typeVoid;
		private Lazy<TypeReference> typeObject;
		private Lazy<TypeReference> typeInt32;
		private Lazy<TypeReference> typeInt64;
		private Lazy<TypeReference> typeUInt32;
		private Lazy<TypeReference> typeBoolean;
		private Lazy<TypeReference> typeString;

		public TypeReference TypeVoid
		{
			get { return this.typeVoid.Value; }
		}

		public TypeReference TypeObject
		{
			get { return this.typeObject.Value; }
		}

		public TypeReference TypeInt32
		{
			get { return this.typeInt32.Value; }
		}

		public TypeReference TypeInt64
		{
			get { return this.typeInt64.Value; }
		}

		public TypeReference TypeUInt32
		{
			get { return this.typeUInt32.Value; }
		}

		public TypeReference TypeBoolean
		{
			get { return this.typeBoolean.Value; }
		}

		public TypeReference TypeString
		{
			get { return this.typeString.Value; }
		}

	}
}
