// 
// Wrapper.cs
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

using System;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class Wrapper<T> : SymFunction {
		public readonly T Item;
		protected PathElementBase PathElement = null;

		public Wrapper (T item, ref int idGen, IMetaDataProvider metaDataProvider)
			: base (ref idGen, metaDataProvider)
		{
			this.Item = item;
		}

		#region Overrides of SymFunction
		public override bool ActsAsField
		{
			get { return this.Item is Field; }
		}

		public override bool IsVirtualMethod
		{
			get { return this.Item is Method && this.MetaDataProvider.IsVirtual ((Method) (object) this.Item); }
		}

		public override bool KeepAsBottomField
		{
			get
			{
				var str = this.Item as string;
				if (str == null)
					return true;

				return str != "$UnaryNot" && str != "$NeZero";
			}
		}

		public override bool ManifestField
		{
			get
			{
				var str = this.Item as string;
				if (str != null)
					return str == "$Value" || str == "$Length";

				return (this.Item is Field || this.Item is Method);
			}
		}

		public override bool IsStatic
		{
			get
			{
				if (this.Item is Field)
					return this.MetaDataProvider.IsStatic ((Field) (object) this.Item);
				if (this.Item is Method)
					return this.MetaDataProvider.IsStatic ((Method) (object) this.Item);

				return false;
			}
		}

		public override bool IfRootIsParameter
		{
			get
			{
				if (this.Item is Field)
					return !this.MetaDataProvider.IsStatic ((Field) (object) this.Item);
				if (this.Item is Method)
					return !this.MetaDataProvider.IsStatic ((Method) (object) this.Item);
				if (this.Item is Parameter)
					return true;
				if (this.Item is Local)
					return false;

				return true;
			}
		}

		public override bool IsAsVisibleAs (Method method)
		{
			if (this.Item is Field)
				return this.MetaDataProvider.IsAsVisibleAs ((Field) (object) this.Item, method);
			if (this.Item is Method)
				return this.MetaDataProvider.IsAsVisibleAs ((Method) (object) this.Item, method);
			if (this.MetaDataProvider.IsConstructor (method) && this.Item is Parameter
			    && this.MetaDataProvider.Name ((Parameter) (object) this.Item) == "this")
				return false;

			return true;
		}

		public override bool IsVisibleFrom (Method method)
		{
			TypeNode declaringType = this.MetaDataProvider.DeclaringType (method);
			if (this.Item is Field) {
				var f = ((Field) (object) this.Item);
				return this.MetaDataProvider.IsVisibleFrom (f, declaringType);
			}

			if (this.Item is Method)
				this.MetaDataProvider.IsVisibleFrom ((Method) (object) this.Item, declaringType);
			if (this.Item is Parameter)
				this.MetaDataProvider.Equal (this.MetaDataProvider.DeclaringMethod ((Parameter) (object) this.Item), method);
			if (this.Item is Local) {
				var local = (Local) (object) this.Item;
				IIndexable<Local> locals = this.MetaDataProvider.Locals (method);
				for (int i = 0; i < locals.Count; i++) {
					if (locals [i].Equals (local))
						return true;
				}
				return false;
			}
			return true;
		}

		public override TypeNode FieldAddressType ()
		{
			if (this.Item is Field)
				return this.MetaDataProvider.ManagedPointer (this.MetaDataProvider.FieldType ((Field) (object) this.Item));

			throw new InvalidOperationException ();
		}

		public override PathElementBase ToPathElement (bool tryCompact)
		{
			throw new NotImplementedException ();
		}
		#endregion

		public TypeNode Type { get; set; }

		public override string ToString ()
		{
			if (typeof (T).Equals (typeof (int)))
				return String.Format ("s{0}", this.Item);
			if (this.Item is Field) {
				var field = (Field) (object) this.Item;
				if (this.MetaDataProvider.IsStatic (field)) {
					return String.Format ("{0}.{1}",
					                      this.MetaDataProvider.FullName (this.MetaDataProvider.DeclaringType (field)),
					                      this.MetaDataProvider.Name (field));
				}

				return this.MetaDataProvider.Name (field);
			}
			if (this.Item is Method) {
				var method = (Method) (object) this.Item;
				if (this.MetaDataProvider.IsStatic (method)) {
					return String.Format ("{0}.{1}",
					                      this.MetaDataProvider.FullName (this.MetaDataProvider.DeclaringType (method)),
					                      this.MetaDataProvider.Name (method));
				}

				return this.MetaDataProvider.Name (method);
			}

			return this.Item.ToString ();
		}
	}
}
