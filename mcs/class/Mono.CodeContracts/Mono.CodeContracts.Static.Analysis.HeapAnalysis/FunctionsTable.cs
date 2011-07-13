// 
// FunctionsTable.cs
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class FunctionsTable
	{
		public readonly SymFunction BoxOperator;
		public readonly SymFunction ElementAddress;
		public readonly SymFunction Length;
		public readonly SymFunction NeZero;
		public readonly SymFunction NullValue;
		public readonly SymFunction ObjectVersion;
		public readonly SymFunction OldValueOf;
		public readonly SymFunction ResultOfCall;
		public readonly SymFunction ResultOfLoadElement;
		public readonly SymFunction StructId;
		public readonly SymFunction UnaryNot;
		public readonly SymFunction ValueOf;
		public readonly SymFunction VoidAddr;
		public readonly SymFunction ZeroValue;
		private readonly Dictionary<BinaryOperator, Wrapper<BinaryOperator>> binary_operators;
		private readonly Dictionary<Field, Wrapper<Field>> fields;
		private readonly Dictionary<Local, Wrapper<Local>> locals;
		private readonly IMetaDataProvider meta_data_provider;
		private readonly Dictionary<Method, Wrapper<Method>> method_pointers;
		private readonly Dictionary<Parameter, Wrapper<Parameter>> parameters;
		private readonly Dictionary<object, Wrapper<object>> program_constants;
		private readonly Dictionary<Method, Wrapper<Method>> pseudo_fields;
		private readonly Dictionary<string, Wrapper<string>> strings;
		private readonly Dictionary<int, Wrapper<int>> temp;
		private readonly Dictionary<UnaryOperator, Wrapper<UnaryOperator>> unary_operators;

		private int id_gen;

		public FunctionsTable(IMetaDataProvider metaDataProvider)
		{
			this.meta_data_provider = metaDataProvider;
			this.locals = new Dictionary<Local, Wrapper<Local>> ();
			this.parameters = new Dictionary<Parameter, Wrapper<Parameter>> ();
			this.fields = new Dictionary<Field, Wrapper<Field>> ();
			this.pseudo_fields = new Dictionary<Method, Wrapper<Method>> ();
			this.temp = new Dictionary<int, Wrapper<int>> ();
			this.strings = new Dictionary<string, Wrapper<string>> ();
			this.program_constants = new Dictionary<object, Wrapper<object>> ();
			this.method_pointers = new Dictionary<Method, Wrapper<Method>> ();
			this.binary_operators = new Dictionary<BinaryOperator, Wrapper<BinaryOperator>> ();
			this.unary_operators = new Dictionary<UnaryOperator, Wrapper<UnaryOperator>> ();

			this.ValueOf = For ("$Value");
			this.OldValueOf = For ("$OldValue");
			this.StructId = For ("$StructId");
			this.ObjectVersion = For ("$ObjectVersion");
			this.NullValue = For ("$Null");
			this.ElementAddress = For ("$ElementAddress");
			this.Length = For ("$Length");
			this.VoidAddr = For ("$VoidAddr");
			this.UnaryNot = For ("$UnaryNot");
			this.NeZero = For ("$NeZero");
			this.BoxOperator = For ("$Box");
			this.ResultOfCall = For ("$ResultOfCall");
			this.ResultOfLoadElement = For ("$ResultOfLoadElement");
			this.ZeroValue = ForConstant (0, this.meta_data_provider.System_Int32);
		}

		private Wrapper<T> For<T>(T key, Dictionary<T, Wrapper<T>> cache)
		{
			Wrapper<T> wrapper;
			if (!cache.TryGetValue (key, out wrapper)) {
				wrapper = SymFunction.For (key, ref this.id_gen, this.meta_data_provider);
				cache.Add (key, wrapper);
			}
			return wrapper;
		}

		public SymFunction For(Local v)
		{
			return For (v, this.locals);
		}

		public SymFunction For(Parameter v)
		{
			return For (v, this.parameters);
		}

		public SymFunction For(Field v)
		{
			v = this.meta_data_provider.Unspecialized (v);
			return For (v, this.fields);
		}

		public SymFunction For(Method v)
		{
			return For (v, this.pseudo_fields);
		}

		public SymFunction For(string v)
		{
			return For (v, this.strings);
		}

		public SymFunction For(int v)
		{
			return For (v, this.temp);
		}

		public SymFunction For(BinaryOperator v)
		{
			return For (v, this.binary_operators);
		}

		public SymFunction For(UnaryOperator v)
		{
			return For (v, this.unary_operators);
		}

		public SymFunction ForConstant(object constant, TypeNode type)
		{
			Wrapper<object> wrapper = For (constant, this.program_constants);
			wrapper.Type = type;
			return wrapper;
		}

		public SymFunction ForMethod(Method method, TypeNode type)
		{
			Wrapper<Method> wrapper = For (method, this.method_pointers);
			wrapper.Type = type;
			return wrapper;
		}

		public bool IsConstantOrMethod(SymFunction constant)
		{
			var wrapper = constant as Wrapper<object>;
			if (wrapper != null && this.program_constants.ContainsKey (wrapper.Item))
				return true;

			var wrapper1 = constant as Wrapper<Method>;
			if (wrapper1 != null && this.method_pointers.ContainsKey (wrapper1.Item))
				return true;

			return false;
		}

		public bool IsConstant(SymFunction c, out TypeNode type, out object value)
		{
			var wrapper = c as Wrapper<object>;
			if (wrapper != null && this.program_constants.ContainsKey (wrapper.Item)) {
				type = wrapper.Type;
				value = wrapper.Item;
				return true;
			}

			type = null;
			value = null;
			return false;
		}
	}
}