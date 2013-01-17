// 
// PrinterFactory.cs
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
using System.IO;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis {
	static class PrinterFactory {
		public static ILPrinter<Label> Create<Label, Source, Dest, Context, EdgeData>
			(IILDecoder<Label, Source, Dest, Context, EdgeData> ilDecoder,
			 IMetaDataProvider metaDataProvider,
			 Func<Source, string> sourceToString, Func<Dest, string> destToString)
		{
			return new Printer<Label, Source, Dest, Context, EdgeData> (ilDecoder, metaDataProvider, sourceToString, destToString).PrintCodeAt;
		}

		#region Nested type: Printer
		private class Printer<Label, Source, Dest, Context, EdgeData> : IILVisitor<Label, Source, Dest, TextWriter, Dummy> {
			private readonly Func<Dest, string> dest_to_string;
			private readonly IILDecoder<Label, Source, Dest, Context, EdgeData> il_decoder;
			private readonly IMetaDataProvider meta_data_provider;
			private readonly Func<Source, string> source_to_string;
			private string prefix = "";

			public Printer (IILDecoder<Label, Source, Dest, Context, EdgeData> ilDecoder, IMetaDataProvider metaDataProvider,
			                Func<Source, string> sourceToString, Func<Dest, string> destToString)
			{
				this.il_decoder = ilDecoder;
				this.meta_data_provider = metaDataProvider;
				this.source_to_string = sourceToString;
				this.dest_to_string = destToString;
			}

			#region IILVisitor<Label,Source,Dest,TextWriter,Dummy> Members
			public Dummy Binary (Label pc, BinaryOperator op, Dest dest, Source operand1, Source operand2, TextWriter data)
			{
				data.WriteLine ("{0}{1} = {2} {3} {4}", this.prefix, DestName (dest), SourceName (operand1), op.ToString (), SourceName (operand2));
				return Dummy.Value;
			}

			public Dummy Isinst (Label pc, TypeNode type, Dest dest, Source obj, TextWriter data)
			{
				data.WriteLine ("{0}{2} = isinst {1} {3}", this.prefix, this.meta_data_provider.FullName (type), DestName (dest), SourceName (obj));
				return Dummy.Value;
			}

			public Dummy LoadNull (Label pc, Dest dest, TextWriter polarity)
			{
				polarity.WriteLine ("{0}{1} = ldnull", this.prefix, DestName (dest));
				return Dummy.Value;
			}

			public Dummy LoadConst (Label pc, TypeNode type, object constant, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldc ({3}) '{1}'", this.prefix, constant, DestName (dest), this.meta_data_provider.FullName (type));
				return Dummy.Value;
			}

			public Dummy Sizeof (Label pc, TypeNode type, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = sizeof {1}", this.prefix, this.meta_data_provider.FullName (type), DestName (dest));
				return Dummy.Value;
			}

			public Dummy Unary (Label pc, UnaryOperator op, bool unsigned, Dest dest, Source source, TextWriter data)
			{
				data.WriteLine ("{0}{3} = {2}{1} {4}", this.prefix, unsigned ? "_un" : null, op, DestName (dest), SourceName (source));
				return Dummy.Value;
			}

			public Dummy Entry (Label pc, Method method, TextWriter data)
			{
				data.WriteLine ("{0}method_entry {1}", this.prefix, this.meta_data_provider.FullName (method));
				return Dummy.Value;
			}

			public Dummy Assume (Label pc, EdgeTag tag, Source condition, TextWriter data)
			{
				data.WriteLine ("{0}assume({1}) {2}", this.prefix, tag, SourceName (condition));
				return Dummy.Value;
			}

			public Dummy Assert (Label pc, EdgeTag tag, Source condition, TextWriter data)
			{
				data.WriteLine ("{0}assert({1}) {2}", this.prefix, tag, SourceName (condition));
				return Dummy.Value;
			}

			public Dummy BeginOld (Label pc, Label matchingEnd, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy EndOld (Label pc, Label matchingBegin, TypeNode type, Dest dest, Source source, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy LoadStack (Label pc, int offset, Dest dest, Source source, bool isOld, TextWriter data)
			{
				data.WriteLine ("{0}{1} = {4}ldstack.{2} {3}", this.prefix, DestName (dest), offset, SourceName (source), isOld ? "old." : null);
				return Dummy.Value;
			}

			public Dummy LoadStackAddress (Label pc, int offset, Dest dest, Source source, TypeNode type, bool isOld, TextWriter data)
			{
				data.WriteLine ("{0}{1} = {4}ldstacka.{2} {3}", this.prefix, DestName (dest), offset, SourceName (source), isOld ? "old." : null);
				return Dummy.Value;
			}

			public Dummy LoadResult (Label pc, TypeNode type, Dest dest, Source source, TextWriter data)
			{
				data.WriteLine ("{0}{1} = ldresult {2}", this.prefix, DestName (dest), SourceName (source));
				return Dummy.Value;
			}

			public Dummy Arglist (Label pc, Dest dest, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy Branch (Label pc, Label target, bool leavesExceptionBlock, TextWriter data)
			{
				data.WriteLine ("{0}branch", this.prefix);
				return Dummy.Value;
			}

			public Dummy BranchCond (Label pc, Label target, BranchOperator bop, Source value1, Source value2, TextWriter data)
			{
				data.WriteLine ("{0}br.{1} {2},{3}", this.prefix, bop, SourceName (value1), SourceName (value2));
				return Dummy.Value;
			}

			public Dummy BranchTrue (Label pc, Label target, Source cond, TextWriter data)
			{
				data.WriteLine ("{0}br.true {1}", this.prefix, SourceName (cond));
				return Dummy.Value;
			}

			public Dummy BranchFalse (Label pc, Label target, Source cond, TextWriter data)
			{
				data.WriteLine ("{0}br.false {1}", this.prefix, SourceName (cond));
				return Dummy.Value;
			}

			public Dummy Break (Label pc, TextWriter data)
			{
				data.WriteLine ("{0}break", this.prefix);
				return Dummy.Value;
			}

			public Dummy Call<TypeList, ArgList> (Label pc, Method method, bool virt, TypeList extraVarargs, Dest dest, ArgList args, TextWriter data) where TypeList : IIndexable<TypeNode>
				where ArgList : IIndexable<Source>
			{
				data.Write ("{0}{3} = call{2} {1}(", this.prefix, this.meta_data_provider.FullName (method), virt ? "virt" : null, DestName (dest));
				if (args != null) {
					for (int i = 0; i < args.Count; i++)
						data.Write ("{0} ", SourceName (args [i]));
				}
				data.WriteLine (")");
				return Dummy.Value;
			}

			public Dummy Calli<TypeList, ArgList> (Label pc, TypeNode returnType, TypeList argTypes, bool instance, Dest dest, Source functionPointer, ArgList args, TextWriter data)
				where TypeList : IIndexable<TypeNode> where ArgList : IIndexable<Source>
			{
				data.Write ("{0}{1} = calli {2}(", this.prefix, DestName (dest), SourceName (functionPointer));
				if (args != null) {
					for (int i = 0; i < args.Count; i++)
						data.Write ("{0} ", SourceName (args [i]));
				}
				data.WriteLine (")");
				return Dummy.Value;
			}

			public Dummy CheckFinite (Label pc, Dest dest, Source source, TextWriter data)
			{
				data.WriteLine ("{0}{1} = chfinite {2}", this.prefix, DestName (dest), SourceName (source));
				return Dummy.Value;
			}

			public Dummy CopyBlock (Label pc, Source destAddress, Source srcAddress, Source len, TextWriter data)
			{
				data.WriteLine ("{0}cpblk {1} {2} {3}", this.prefix, SourceName (destAddress), SourceName (srcAddress), SourceName (len));
				return Dummy.Value;
			}

			public Dummy EndFilter (Label pc, Source decision, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy EndFinally (Label pc, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy Jmp (Label pc, Method method, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy LoadArg (Label pc, Parameter argument, bool isOld, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = {3}ldarg {1}", this.prefix, this.meta_data_provider.Name (argument), DestName (dest), isOld ? "old." : null);
				return Dummy.Value;
			}

			public Dummy LoadArgAddress (Label pc, Parameter argument, bool isOld, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = {3}ldarga {1}", this.prefix, this.meta_data_provider.Name (argument), DestName (dest), isOld ? "old." : null);
				return Dummy.Value;
			}

			public Dummy LoadLocal (Label pc, Local local, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldloc {1}", this.prefix, this.meta_data_provider.Name (local), DestName (dest));
				return Dummy.Value;
			}

			public Dummy LoadLocalAddress (Label pc, Local local, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldloca {1}", this.prefix, this.meta_data_provider.Name (local), DestName (dest));
				return Dummy.Value;
			}

			public Dummy Nop (Label pc, TextWriter data)
			{
				data.WriteLine ("{0}nop", this.prefix);
				return Dummy.Value;
			}

			public Dummy Pop (Label pc, Source source, TextWriter data)
			{
				data.WriteLine ("{0}pop {1}", this.prefix, SourceName (source));
				return Dummy.Value;
			}

			public Dummy Return (Label pc, Source source, TextWriter data)
			{
				data.WriteLine ("{0}ret {1}", this.prefix, SourceName (source));
				return Dummy.Value;
			}

			public Dummy StoreArg (Label pc, Parameter argument, Source source, TextWriter data)
			{
				data.WriteLine ("{0}starg {1} {2}", this.prefix, this.meta_data_provider.Name (argument), SourceName (source));
				return Dummy.Value;
			}

			public Dummy StoreLocal (Label pc, Local local, Source source, TextWriter data)
			{
				data.WriteLine ("{0}stloc {1} {2}", this.prefix, this.meta_data_provider.Name (local), SourceName (source));
				return Dummy.Value;
			}

			public Dummy Switch (Label pc, TypeNode type, IEnumerable<Pair<object, Label>> cases, Source value, TextWriter data)
			{
				data.WriteLine ("{0}switch {1}", this.prefix, SourceName (value));
				return Dummy.Value;
			}

			public Dummy Box (Label pc, TypeNode type, Dest dest, Source source, TextWriter data)
			{
				data.WriteLine ("{0}{2} = box {1} {3}", this.prefix, this.meta_data_provider.FullName (type), DestName (dest), SourceName (source));
				return Dummy.Value;
			}

			public Dummy ConstrainedCallvirt<TypeList, ArgList> (Label pc, Method method, TypeNode constraint, TypeList extraVarargs, Dest dest, ArgList args, TextWriter data)
				where TypeList : IIndexable<TypeNode> where ArgList : IIndexable<Source>
			{
				data.Write ("{0}{3} = constrained({1}).callvirt {2}(", this.prefix, this.meta_data_provider.FullName (constraint), this.meta_data_provider.FullName (method), DestName (dest));
				if (args != null) {
					for (int i = 0; i < args.Count; i++)
						data.Write ("{0} ", SourceName (args [i]));
				}
				data.WriteLine (")");
				return Dummy.Value;
			}

			public Dummy CastClass (Label pc, TypeNode type, Dest dest, Source obj, TextWriter data)
			{
				data.WriteLine ("{0}{2} = castclass {1} {3}", this.prefix, this.meta_data_provider.FullName (type), DestName (dest), SourceName (obj));
				return Dummy.Value;
			}

			public Dummy CopyObj (Label pc, TypeNode type, Source destPtr, Source sourcePtr, TextWriter data)
			{
				data.WriteLine ("{0}cpobj {1} {2} {3}", this.prefix, this.meta_data_provider.FullName (type), SourceName (destPtr), SourceName (sourcePtr));
				return Dummy.Value;
			}

			public Dummy Initobj (Label pc, TypeNode type, Source ptr, TextWriter data)
			{
				data.WriteLine ("{0}initobj {1} {2}", this.prefix, this.meta_data_provider.FullName (type), SourceName (ptr));
				return Dummy.Value;
			}

			public Dummy LoadElement (Label pc, TypeNode type, Dest dest, Source array, Source index, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldelem {1} {3}[{4}]", this.prefix, this.meta_data_provider.FullName (type), DestName (dest), SourceName (array), SourceName (index));
				return Dummy.Value;
			}

			public Dummy LoadField (Label pc, Field field, Dest dest, Source obj, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldfld {1} {3}", this.prefix, this.meta_data_provider.Name (field), DestName (dest), SourceName (obj));
				return Dummy.Value;
			}

			public Dummy LoadFieldAddress (Label pc, Field field, Dest dest, Source obj, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldflda {1} {3}", this.prefix, this.meta_data_provider.Name (field), DestName (dest), SourceName (obj));
				return Dummy.Value;
			}

			public Dummy LoadLength (Label pc, Dest dest, Source array, TextWriter data)
			{
				data.WriteLine ("{0}{1} = ldlen {2}", this.prefix, DestName (dest), SourceName (array));
				return Dummy.Value;
			}

			public Dummy LoadStaticField (Label pc, Field field, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldsfld {1}", this.prefix, this.meta_data_provider.Name (field), DestName (dest));
				return Dummy.Value;
			}

			public Dummy LoadStaticFieldAddress (Label pc, Field field, Dest dest, TextWriter data)
			{
				data.WriteLine ("{0}{2} = ldsflda {1}", this.prefix, this.meta_data_provider.Name (field), DestName (dest));
				return Dummy.Value;
			}

			public Dummy LoadTypeToken (Label pc, TypeNode type, Dest dest, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy LoadFieldToken (Label pc, Field type, Dest dest, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy LoadMethodToken (Label pc, Method type, Dest dest, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy NewArray<ArgList> (Label pc, TypeNode type, Dest dest, ArgList lengths, TextWriter data) where ArgList : IIndexable<Source>
			{
				throw new NotImplementedException ();
			}

			public Dummy NewObj<ArgList> (Label pc, Method ctor, Dest dest, ArgList args, TextWriter data) where ArgList : IIndexable<Source>
			{
				data.Write ("{0}{2} = newobj {1}(", this.prefix, this.meta_data_provider.FullName (ctor), DestName (dest));
				if (args != null) {
					for (int i = 0; i < args.Count; ++i)
						data.Write ("{0} ", SourceName (args [i]));
				}

				data.WriteLine (")");
				return Dummy.Value;
			}

			public Dummy MkRefAny (Label pc, TypeNode type, Dest dest, Source obj, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy RefAnyType (Label pc, Dest dest, Source source, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy RefAnyVal (Label pc, TypeNode type, Dest dest, Source source, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy Rethrow (Label pc, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy StoreElement (Label pc, TypeNode type, Source array, Source index, Source value, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy StoreField (Label pc, Field field, Source obj, Source value, TextWriter data)
			{
				data.WriteLine ("{0}stfld {1} {2} {3}", this.prefix, this.meta_data_provider.Name (field), SourceName (obj), SourceName (value));
				return Dummy.Value;
			}

			public Dummy StoreStaticField (Label pc, Field field, Source value, TextWriter data)
			{
				data.WriteLine ("{0}stsfld {1} {2}", this.prefix, this.meta_data_provider.Name (field), SourceName (value));
				return Dummy.Value;
			}

			public Dummy Throw (Label pc, Source exception, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy Unbox (Label pc, TypeNode type, Dest dest, Source obj, TextWriter data)
			{
				throw new NotImplementedException ();
			}

			public Dummy UnboxAny (Label pc, TypeNode type, Dest dest, Source obj, TextWriter data)
			{
				throw new NotImplementedException ();
			}
			#endregion

			public void PrintCodeAt (Label label, string prefix, TextWriter tw)
			{
				this.prefix = prefix;
				this.il_decoder.ForwardDecode<TextWriter, Dummy, Printer<Label, Source, Dest, Context, EdgeData>> (label, this, tw);
			}

			private string SourceName (Source src)
			{
				return this.source_to_string != null ? this.source_to_string (src) : null;
			}

			private string DestName (Dest dest)
			{
				return this.dest_to_string != null ? this.dest_to_string (dest) : null;
			}
		}
		#endregion
	}
}
