// 
// PathElement`1.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths {
	class PathElement<T> : PathElementBase {
		public readonly string Description;
		public readonly T Element;
		protected string castTo;
		protected bool isManagedPointer;
		protected bool isStatic;
		protected bool isUnmanagedPointer;

		#region Overrides of PathElement
		public override bool IsStatic
		{
			get { return this.isStatic; }
		}

		public override bool IsUnmanagedPointer
		{
			get { return this.isUnmanagedPointer; }
		}

		public override bool IsManagedPointer
		{
			get { return this.isManagedPointer; }
		}

		public override bool IsParameterRef
		{
			get { return typeof (T) == typeof (Parameter); }
		}

		public override string CastTo
		{
			get { return this.castTo; }
		}

		public override bool IsAddressOf
		{
			get { return true; }
		}

		public override bool TryField (out Field f)
		{
			if (typeof (T) == typeof (Field)) {
				f = (Field) (object) this.Element;
				return true;
			}

			f = default(Field);
			return false;
		}

		public override bool TryGetResultType (out TypeNode type)
		{
			type = ResultType;
			return true;
		}

		public override TResult Decode<TData, TResult, TVisitor, TLabel> (TLabel label, TVisitor visitor, TData data)
		{
			if (typeof (T) == typeof (Field)) {
				var field = (Field) (object) this.Element;
				if (this.isStatic)
					return visitor.LoadStaticFieldAddress (label, field, Dummy.Value, data);
				return visitor.LoadFieldAddress (label, field, Dummy.Value, Dummy.Value, data);
			}

			if (typeof (T) == typeof (Local)) {
				var local = (Local) (object) this.Element;
				return visitor.LoadLocalAddress (label, local, Dummy.Value, data);
			}

			if (typeof (T) == typeof (Method)) {
				var method = (Method) (object) this.Element;
				bool isVirtualMethod = this.Func.IsVirtualMethod;
				return visitor.Call (label, method, isVirtualMethod, Indexable<TypeNode>.Empty, Dummy.Value, Indexable<Dummy>.Empty, data);
			}

			if (typeof (T) == typeof (Parameter)) {
				var parameter = (Parameter) (object) this.Element;
				return visitor.LoadArgAddress (label, parameter, false, Dummy.Value, data);
			}

			throw new InvalidOperationException ("Field, Local, Method or Parameter expected");
		}

		public override string ToString ()
		{
			return this.Description;
		}
		#endregion

		public PathElement (T element, string description, SymFunction c) : base (c)
		{
			this.Element = element;
			this.Description = description;
			this.isStatic = false;
			this.isUnmanagedPointer = false;
			this.isManagedPointer = false;
		}

		public TypeNode ResultType { get; protected set; }

		public virtual bool IsCallerVisible ()
		{
			return (typeof (T) == typeof (Parameter));
		}

		#region Overrides of PathElementBase
		public override bool TrySetType (TypeNode expectedType, IMetaDataProvider metaDataProvider, out TypeNode resultType)
		{
			if (typeof (T) == typeof (Parameter)) {
				var p = (Parameter) (object) this.Element;
				TypeNode type = metaDataProvider.ParameterType (p);
				this.isManagedPointer = metaDataProvider.IsManagedPointer (type);
				ResultType = resultType = metaDataProvider.ManagedPointer (type);
				return true;
			}

			if (typeof (T) == typeof (Field)) {
				var f = (Field) (object) this.Element;
				TypeNode type = metaDataProvider.FieldType (f);
				this.isStatic = metaDataProvider.IsStatic (f);
				this.isManagedPointer = metaDataProvider.IsManagedPointer (type);
				ResultType = resultType = metaDataProvider.ManagedPointer (type);

				TypeNode declaringType = metaDataProvider.DeclaringType (f);
				if (metaDataProvider.IsManagedPointer (expectedType))
					expectedType = metaDataProvider.ElementType (expectedType);
				expectedType = metaDataProvider.Unspecialized (expectedType);

				if (!metaDataProvider.IsStatic (f) && declaringType.Equals (expectedType) &&
				    (!metaDataProvider.DerivesFrom (expectedType, declaringType) ||
				     !metaDataProvider.IsProtected (f) && !metaDataProvider.IsPublic (f)))
					this.castTo = metaDataProvider.FullName (declaringType);

				return true;
			}

			if (typeof (T) == typeof (Local)) {
				var local = (Local) (object) this.Element;
				TypeNode type = metaDataProvider.LocalType (local);
				this.isManagedPointer = metaDataProvider.IsManagedPointer (type);
				ResultType = resultType = metaDataProvider.ManagedPointer (type);

				return true;
			}

			if (typeof (T) == typeof (Method)) {
				var method = (Method) (object) this.Element;
				ResultType = resultType = !IsAddressOf
				                          	? metaDataProvider.ReturnType (method)
				                          	: metaDataProvider.ManagedPointer (metaDataProvider.ReturnType (method));

				if (metaDataProvider.IsManagedPointer (expectedType))
					expectedType = metaDataProvider.ElementType (expectedType);
				expectedType = metaDataProvider.Unspecialized (expectedType);

				TypeNode declaringType = metaDataProvider.DeclaringType (method);
				if (!metaDataProvider.IsStatic (method) && declaringType.Equals (expectedType) &&
				    (!metaDataProvider.DerivesFrom (expectedType, declaringType)
				     || !metaDataProvider.IsProtected (method) && !metaDataProvider.IsPublic (method)))
					this.castTo = metaDataProvider.FullName (declaringType);

				return true;
			}

			ResultType = resultType = default(TypeNode);
			return false;
		}
		#endregion
	}
}
