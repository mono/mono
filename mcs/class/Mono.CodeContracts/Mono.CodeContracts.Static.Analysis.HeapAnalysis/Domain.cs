// 
// Domain.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class Domain : IAbstractDomain<Domain> {
		private static Domain BottomValue;
		public readonly FunctionsTable Functions;
		private readonly Dictionary<APC, Domain> begin_old_saved_states;
		private readonly SymGraph<SymFunction, AbstractType> egraph;
		private readonly HeapAnalysis parent;
		private TypeCache IEnumerable1Type = new TypeCache (@"System.Collections.IEnumerable`1");
		private TypeCache IEnumerableType = new TypeCache (@"System.Collections.IEnumerable");
		public int InsideOld;
		public IImmutableSet<SymValue> ModifiedAtCall;
		private IImmutableMap<SymValue, SymFunction> constantLookup;
		private IImmutableSet<SymValue> unmodifiedFieldsSinceEntry;
		private IImmutableSet<SymValue> unmodifiedSinceEntry;

		private Domain (SymGraph<SymFunction, AbstractType> newEgraph,
		                IImmutableMap<SymValue, SymFunction> constantMap,
		                IImmutableSet<SymValue> unmodifiedSinceEntry, IImmutableSet<SymValue> unmodifiedFieldsSinceEntry, IImmutableSet<SymValue> modifiedAtCall,
		                Domain from, Domain oldDomain)
		{
			this.egraph = newEgraph;
			this.Functions = from.Functions;
			this.parent = from.parent;
			this.begin_old_saved_states = from.begin_old_saved_states;
			this.constantLookup = constantMap;
			this.unmodifiedSinceEntry = unmodifiedSinceEntry;
			this.unmodifiedFieldsSinceEntry = unmodifiedFieldsSinceEntry;
			this.ModifiedAtCall = modifiedAtCall;
			this.InsideOld = from.InsideOld;
			OldDomain = oldDomain;
			BeginOldPC = from.BeginOldPC;
		}

		public Domain (HeapAnalysis parent)
		{
			this.parent = parent;
			this.egraph = new SymGraph<SymFunction, AbstractType> (AbstractType.TopValue, AbstractType.BottomValue);
			this.constantLookup = ImmutableIntKeyMap<SymValue, SymFunction>.Empty (SymValue.GetUniqueKey);
			this.unmodifiedSinceEntry = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			this.unmodifiedFieldsSinceEntry = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			this.ModifiedAtCall = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			this.Functions = new FunctionsTable (parent.MetaDataProvider);
			this.begin_old_saved_states = new Dictionary<APC, Domain> ();

			SymValue nullValue = this.egraph.FreshSymbol ();
			this.egraph [this.Functions.NullValue] = nullValue;
			this.egraph [nullValue] = AbstractType.BottomValue;
			this.egraph [ConstantValue (0, MetaDataProvider.System_Int32)] = new AbstractType (MetaDataProvider.System_Int32, true);
		}

		public Domain OldDomain { get; set; }
		public APC BeginOldPC { get; set; }

		public SymValue Globals
		{
			get { return this.egraph.ConstRoot; }
		}

		private SymValue Null
		{
			get { return this.egraph [this.Functions.NullValue]; }
		}

		private SymValue Zero
		{
			get { return this.egraph [this.Functions.ZeroValue]; }
		}

		public SymValue VoidAddr
		{
			get { return this.egraph [this.Functions.VoidAddr]; }
		}

		private IMetaDataProvider MetaDataProvider
		{
			get { return this.parent.MetaDataProvider; }
		}

		public IEnumerable<SymValue> Variables
		{
			get { return this.egraph.Variables; }
		}

		#region IAbstractDomain<Domain> Members
		public void Dump (TextWriter tw)
		{
			this.egraph.Dump (tw);
			tw.WriteLine ("Unmodified locations:");
			foreach (SymValue sv in this.unmodifiedSinceEntry.Elements)
				tw.Write ("{0} ", sv);
			tw.WriteLine ();

			tw.WriteLine ("Unmodified locations for fields:");
			foreach (SymValue sv in this.unmodifiedFieldsSinceEntry.Elements)
				tw.Write ("{0} ", sv);
			tw.WriteLine ();

			if (this.ModifiedAtCall != null) {
				tw.WriteLine ("Modified locations at last call");
				foreach (SymValue sv in this.ModifiedAtCall.Elements)
					tw.Write ("{0} ", sv);
				tw.WriteLine ();
			}
			if (OldDomain == null)
				return;

			tw.WriteLine ("## has old domain ##");
			OldDomain.egraph.Dump (tw);
			tw.WriteLine ("## end old domain ##");
		}

		public Domain Clone ()
		{
			return new Domain (this.egraph.Clone (), this.constantLookup, this.unmodifiedSinceEntry, this.unmodifiedFieldsSinceEntry, this.ModifiedAtCall, this, OldDomain == null ? null : OldDomain.Clone ());
		}
		#endregion

		public static bool IsRootedInParameter (Sequence<PathElement> path)
		{
			return path.Head is PathElement<Parameter>;
		}

		public SymValue Address (Local local)
		{
			SymFunction function = this.Functions.For (local);
			SymValue sv = this.egraph.TryLookup (function);
			if (sv == null) {
				sv = this.egraph [function];
				SetType (sv, MetaDataProvider.ManagedPointer (MetaDataProvider.LocalType (local)));
			}
			return sv;
		}

		public SymValue Address (Parameter v)
		{
			SymFunction function = this.Functions.For (v);
			SymValue sv = this.egraph.TryLookup (function);
			if (sv == null) {
				sv = this.egraph [function];
				SetType (sv, MetaDataProvider.ManagedPointer (MetaDataProvider.ParameterType (v)));
			}
			return sv;
		}

		public SymValue Address (SymFunction v)
		{
			return this.egraph [v];
		}

		public SymValue Address (int v)
		{
			return Address (this.Functions.For (v));
		}

		public void AssignConst (int dest, TypeNode type, object constant)
		{
			AssignConst (Address (this.Functions.For (dest)), type, constant);
		}

		public void AssignConst (SymValue address, TypeNode type, object constant)
		{
			SymValue value = ConstantValue (constant, type);
			SetType (address, MetaDataProvider.ManagedPointer (type));
			this.egraph [this.Functions.ValueOf, address] = value;

			var str = constant as string;
			if (str != null)
				this.egraph [this.Functions.Length, value] = ConstantValue (str.Length, MetaDataProvider.System_Int32);
		}

		public void AssignValue (int dest, FlatDomain<TypeNode> type)
		{
			AssignValue (Address (this.Functions.For (dest)), type);
		}

		private void AssignValue (SymValue address, FlatDomain<TypeNode> type)
		{
			Havoc (address);
			SetType (address, type.IsNormal() ? MetaDataProvider.ManagedPointer (type.Value) : type);
			if (IsStructWithFields (type))
				return;

			SymValue fresh = this.egraph.FreshSymbol ();
			SetType (fresh, type);
			this.egraph [this.Functions.ValueOf, address] = fresh;

			if (!type.IsNormal())
				return;

			if (NeedsArrayLengthManifested (type.Value))
				ManifestArrayLength (fresh);
		}

		public void AssignNull (int addr)
		{
			AssignNull (Address (this.Functions.For (addr)));
		}

		public void AssignNull (SymValue addr)
		{
			Havoc (addr);
			this.egraph [this.Functions.ValueOf, addr] = Null;
		}

		private void AssignZero (SymValue addr)
		{
			this.egraph [this.Functions.ValueOf, addr] = Zero;
		}

		public void AssignZeroEquivalent (SymValue addr, TypeNode t)
		{
			if (MetaDataProvider.IsReferenceType (t))
				AssignNull (addr);
			else if (MetaDataProvider.HasValueRepresentation (t))
				AssignZero (addr);
		}

		public void AssignSpecialUnary (int dest, SymFunction op, int source, FlatDomain<TypeNode> typeOfValue)
		{
			SymValue specialUnary = GetSpecialUnary (op, source, typeOfValue);
			AssignSpecialUnary (dest, specialUnary, typeOfValue);
		}

		public void AssignSpecialUnary (int dest, SymValue result, FlatDomain<TypeNode> typeOfValue)
		{
			SymValue address = Address (dest);
			AssignValue (address, typeOfValue);
			this.egraph [this.Functions.ValueOf, address] = result;
		}

		public void AssignPureBinary (int dest, BinaryOperator op, FlatDomain<TypeNode> typeOpt, int op1, int op2)
		{
			var args = new[] {Value (op1), Value (op2)};
			SymFunction c = this.Functions.For (op);
			TypeNode type = !typeOpt.IsNormal() ? MetaDataProvider.System_Int32 : typeOpt.Value;

			bool fresh;

			SymValue pointerValue = LookupAddressAndManifestType (args, c, type, out fresh);
			SymValue destAddr = Address (dest);

			SetType (destAddr, MetaDataProvider.ManagedPointer (type));
			this.egraph [this.Functions.ValueOf, destAddr] = pointerValue;
		}

		public void AssignPureUnary (int dest, UnaryOperator op, FlatDomain<TypeNode> typeOpt, int operand)
		{
			SymFunction c = this.Functions.For (op);
			TypeNode type = !typeOpt.IsNormal() ? MetaDataProvider.System_Int32 : typeOpt.Value;

			SymValue unaryOperand = this.egraph [c, Value (operand)];
			SymValue sv = Address (dest);

			SetType (sv, MetaDataProvider.ManagedPointer (type));
			this.egraph [this.Functions.ValueOf, sv] = unaryOperand;
		}

		private SymValue ConstantValue (object constant, TypeNode type)
		{
			SymFunction symFunction = this.Functions.ForConstant (constant, type);
			SymValue sv = this.egraph.TryLookup (symFunction);
			if (sv == null) {
				sv = this.egraph [symFunction];
				SetType (sv, type);
				this.constantLookup = this.constantLookup.Add (sv, symFunction);
			}
			return sv;
		}

		public void Havoc (int i)
		{
			Havoc (Address (i));
		}

		public void Havoc (SymValue addr)
		{
			this.egraph.EliminateAll (addr);
		}

		private void HavocIfStruct (SymValue address)
		{
			AbstractType aType = this.egraph [address];
			if (aType.IsBottom || (aType.IsNormal() && MetaDataProvider.IsStruct (aType.ConcreteType)))
				Havoc (address);
		}

		private void HavocMutableFields (SymFunction except, SymValue address, ref IImmutableSet<SymValue> havoced)
		{
			HavocFields (except, address, ref havoced, false);
		}

		private void HavocFields (SymFunction except, SymValue address, ref IImmutableSet<SymValue> havoced, bool havocImmutable)
		{
			if (havoced.Contains (address))
				return;

			havoced = havoced.Add (address);
			MakeTotallyModified (address);
			foreach (SymFunction c in this.egraph.Functions (address))
				HavocConstructor (except, address, c, ref havoced, havocImmutable);
		}

		private void HavocConstructor (SymFunction except, SymValue address, SymFunction c, ref IImmutableSet<SymValue> havoced, bool havocImmutable)
		{
			if (c == this.Functions.ValueOf)
				this.egraph [this.Functions.OldValueOf, address] = this.egraph [c, address];
			if (c == this.Functions.ValueOf || c == this.Functions.ObjectVersion || c == this.Functions.StructId) {
				this.egraph.Eliminate (c, address);
				havoced = havoced.Add (address);
			} else {
				var fieldWrapper = c as Wrapper<Field>;
				if (fieldWrapper != null && fieldWrapper != except) {
					if (havocImmutable || !MetaDataProvider.IsReadonly (fieldWrapper.Item))
						HavocFields (except, this.egraph [c, address], ref havoced, havocImmutable);

					return;
				}

				var methodWrapper = c as Wrapper<Method>;
				if (methodWrapper != null && methodWrapper != except)
					HavocFields (except, this.egraph [c, address], ref havoced, havocImmutable);
			}
		}

		public void HavocPseudoFields (SymValue address)
		{
			IImmutableSet<SymValue> havoced = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			HavocPseudoFields (null, address, ref havoced);
		}

		private void HavocPseudoFields (SymFunction except, SymValue address, ref IImmutableSet<SymValue> havoced)
		{
			if (havoced.Contains (address))
				return;
			havoced = havoced.Add (address);
			if (IsUnmodified (address)) {
				MakeModified (address);
				MakeUnmodifiedField (address);
			}
			this.egraph.Eliminate (this.Functions.ObjectVersion, address);
			this.egraph.Eliminate (this.Functions.StructId, address);
			foreach (SymFunction c in this.egraph.Functions (address)) {
				if (c != except && c is Wrapper<Method>)
					HavocMutableFields (except, this.egraph [c, address], ref havoced);
			}
		}

		private void HavocPseudoFields (SymFunction except, IEnumerable<Method> getters, SymValue address, ref IImmutableSet<SymValue> havoced)
		{
			if (havoced.Contains (address))
				return;
			havoced = havoced.Add (address);
			if (IsUnmodified (address)) {
				MakeModified (address);
				MakeUnmodifiedField (address);
			}
			this.egraph.Eliminate (this.Functions.ObjectVersion, address);
			this.egraph.Eliminate (this.Functions.StructId, address);
			foreach (Method m in getters)
				HavocMutableFields (except, this.egraph [this.Functions.For (m), address], ref havoced);
		}

		public void HavocPseudoFields (IEnumerable<Method> getters, SymValue address)
		{
			IImmutableSet<SymValue> havoced = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			HavocPseudoFields (null, getters, address, ref havoced);
		}

		private void MakeUnmodifiedField (SymValue address)
		{
			this.unmodifiedFieldsSinceEntry = this.unmodifiedFieldsSinceEntry.Add (address);
		}

		private void MakeModified (SymValue address)
		{
			this.unmodifiedSinceEntry = this.unmodifiedSinceEntry.Remove (address);
		}

		public void CopyValue (SymValue destAddr, SymValue sourceAddr)
		{
			AbstractType type = this.egraph [sourceAddr];
			CopyValue (destAddr, sourceAddr, type.Type);
		}

		public void CopyValue (SymValue destAddr, SymValue sourceAddr, FlatDomain<TypeNode> addrType)
		{
			CopyValue (destAddr, sourceAddr, addrType, true, false);
		}

		private void CopyValue (SymValue destAddr, SymValue sourceAddr, FlatDomain<TypeNode> addrType, bool setTargetAddrType, bool cast)
		{
			MakeTotallyModified (destAddr);
			if (destAddr != sourceAddr)
				HavocIfStruct (destAddr);
			if (setTargetAddrType)
				SetType (destAddr, addrType);

			SetValue (destAddr, sourceAddr, addrType, cast);
		}

		public void CopyOldValue (APC pc, int dest, int source, Domain target, bool atEndOld)
		{
			CopyOldValue (pc, target.Address (dest), Address (source), target, atEndOld);
		}

		public void CopyOldValue (APC pc, SymValue destAddr, SymValue srcAddr, Domain target, bool atEndOld)
		{
			AbstractType abstractType = this.egraph [srcAddr];
			CopyOldValue (pc, destAddr, srcAddr, abstractType.Type, target, atEndOld);
		}

		private void CopyOldValue (APC pc, SymValue destAddr, SymValue srcAddr, FlatDomain<TypeNode> addrType, Domain target, bool atEndOld)
		{
			CopyOldValue (pc, destAddr, srcAddr, addrType, true, atEndOld, target);
		}

		private void CopyOldValue (APC pc, SymValue destAddr, SymValue srcAddr, FlatDomain<TypeNode> addrType, bool setTargetAddrType, bool atEndOld, Domain target)
		{
			target.MakeTotallyModified (destAddr);
			if (destAddr != srcAddr)
				target.HavocIfStruct (destAddr);
			if (setTargetAddrType)
				target.SetType (destAddr, addrType);

			FlatDomain<TypeNode> targetType = TargetType (addrType);

			if (IsStructWithFields (targetType))
				CopyOldStructValue (pc, destAddr, srcAddr, targetType.Value, target, atEndOld);
			else if (atEndOld && targetType.IsNormal() && MetaDataProvider.IsManagedPointer (targetType.Value)) {
				srcAddr = TryValue (srcAddr);
				if (srcAddr == null)
					return;
				destAddr = target.Value (destAddr);
				CopyOldValue (pc, destAddr, srcAddr, targetType, setTargetAddrType, atEndOld, target);
			} else {
				SymValue source = this.egraph.TryLookup (this.Functions.ValueOf, srcAddr);
				if (source == null) {
					if (this.egraph.IsImmutable)
						return;
					source = this.egraph [this.Functions.ValueOf, srcAddr];
				}
				CopyOldValueToDest (pc, destAddr, source, addrType, target);
			}
		}

		public void CopyOldValueToDest (APC pc, SymValue destAddr, SymValue source, FlatDomain<TypeNode> addrType, Domain target)
		{
			if (pc.InsideEnsuresAtCall) {
				TypeNode type;
				object constant;

				if (IsConstant (source, out type, out constant)) {
					SymValue sv = target.ConstantValue (constant, type);
					target.CopyNonStructWithFieldValue (destAddr, sv, addrType);
					return;
				}

				foreach (var sv in GetAccessPathsRaw (source, AccessPathFilter<Method>.NoFilter, false))
					throw new NotImplementedException ();
			}
		}

		private IEnumerable<Sequence<PathElementBase>> GetAccessPathsRaw (SymValue sv, AccessPathFilter<Method> filter, bool compress)
		{
			var visited = new HashSet<SymValue> ();
			return GetAccessPathsRaw (sv, null, visited, filter, compress);
		}

		private IEnumerable<Sequence<PathElementBase>> GetAccessPathsRaw (SymValue sv, Sequence<PathElementBase> path, HashSet<SymValue> visited, AccessPathFilter<Method> filter, bool compress)
		{
			if (sv == this.egraph.ConstRoot)
				yield return path;
			else if (!visited.Contains (sv)) {
				visited.Add (sv);
				foreach (var term in this.egraph.EqTerms (sv)) {
					if (!(term.Function is Wrapper<object>) && !(term.Function is Wrapper<int>)) {
						PathElementBase next = term.Function.ToPathElement (compress);
						if (next != null && !filter.FilterOutPathElement (term.Function)) {
							Sequence<PathElementBase> newPath;

							if (path == null || !compress || (!(next is PathElement<Method>)))
								newPath = path.Cons (next);
							else
								newPath = path.Tail.Cons (next);

							foreach (var item in GetAccessPathsRaw (term.Args [0], newPath, visited, filter, compress))
								yield return item;
						}
					}
				}
			}
		}

		private Sequence<PathElementBase> GetBestAccessPath (SymValue sv, AccessPathFilter<Method> filter, bool compress, bool allowLocal, bool preferLocal)
		{
			Sequence<PathElementBase> bestParameterPath = null;
			Sequence<PathElementBase> bestLocalPath = null;
			Sequence<PathElementBase> bestFieldMethodPath = null;

			foreach (var path in GetAccessPathsFiltered (sv, filter, compress)) {
				if (path != null) {
					if (path.Head is PathElement<Parameter>) {
						if (bestParameterPath == null || bestParameterPath.Length () > path.Length ())
							bestParameterPath = path;
					} else if (filter.AllowLocal && allowLocal && path.Head is PathElement<Local>) {
						if (!path.Head.ToString ().Contains ("$") && (bestLocalPath == null || bestLocalPath.Length () > path.Length ()))
							bestLocalPath = path;
					} else if (path.Head is PathElement<Field> && path.Head.Func.IsStatic) {
						if (bestFieldMethodPath == null || bestFieldMethodPath.Length () > path.Length ())
							bestFieldMethodPath = path;
					} else if (path.Head is PathElement<Method> && path.Head.Func.IsStatic) {
						if (bestFieldMethodPath == null || bestFieldMethodPath.Length () > path.Length ())
							bestFieldMethodPath = path;
					}
				}
			}
			if (preferLocal && bestLocalPath != null)
				return bestLocalPath;
			if (bestParameterPath != null)
				return bestParameterPath;
			if (allowLocal && bestLocalPath != null)
				return bestLocalPath;

			return bestFieldMethodPath;
		}

		public IEnumerable<Sequence<PathElementBase>> GetAccessPathsFiltered (SymValue sv, AccessPathFilter<Method> filter, bool compress)
		{
			return GetAccessPathsTyped (sv, filter, compress).Where (path => PathIsVisibleAccordingToFilter (path, filter));
		}

		private bool PathIsVisibleAccordingToFilter (Sequence<PathElementBase> path, AccessPathFilter<Method> filter)
		{
			if (path.Length () == 0 || !filter.HasVisibilityMember)
				return true;

			Method visibilityMember = filter.VisibilityMember;
			TypeNode t = MetaDataProvider.DeclaringType (visibilityMember);
			TypeNode type = default(TypeNode);

			while (path != null) {
				PathElement pathElement = path.Head;
				if (!pathElement.TryGetResultType (out type))
					return true;
				while (MetaDataProvider.IsManagedPointer (type))
					type = MetaDataProvider.ElementType (type);
				if ("this" != pathElement.ToString ()) {
					var peMethod = pathElement as PathElement<Method>;
					if (peMethod != null) {
						Method method = peMethod.Element;
						if (!MetaDataProvider.IsVisibleFrom (method, t))
							return false;
					}

					var peProperty = pathElement as PathElement<Property>;
					if (peProperty != null) {
						Property property = peProperty.Element;
						Method method;
						if (!MetaDataProvider.HasGetter (property, out method))
							MetaDataProvider.HasSetter (property, out method);

						if (!MetaDataProvider.IsVisibleFrom (method, t))
							return false;
					}

					var peField = pathElement as PathElement<Field>;
					if (peField != null) {
						Field field = peField.Element;
						if (!MetaDataProvider.IsVisibleFrom (field, t))
							return false;
					}
				}
				path = path.Tail;
			}
			return true;
		}

		private bool TryPropagateTypeInfo (Sequence<PathElementBase> path, out Sequence<PathElementBase> result)
		{
			if (path == null) {
				result = null;
				return true;
			}
			PathElementBase head = path.Head;
			TypeNode prevType;
			if (!head.TrySetType (MetaDataProvider.System_IntPtr, MetaDataProvider, out prevType)) {
				result = null;
				return false;
			}

			Sequence<PathElementBase> result1;
			if (!TryPropagateTypeInfoRecurse (path.Tail, prevType, out result1)) {
				result = null;
				return false;
			}
			if (head.IsAddressOf && head is PathElement<Parameter> && result1 != null && result1.Head.IsDeref) {
				var pathElement = (PathElement<Parameter>) head;
				result = result1.Tail.Cons (new ParameterPathElement (pathElement.Element, pathElement.Description, pathElement.Func, MetaDataProvider));
				return true;
			}

			result = result1.Cons (head);
			return true;
		}

		private bool TryPropagateTypeInfoRecurse (Sequence<PathElementBase> path, TypeNode prevType, out Sequence<PathElementBase> result)
		{
			if (path == null) {
				result = null;
				return true;
			}
			PathElementBase head = path.Head;
			if (head.TrySetType (prevType, MetaDataProvider, out prevType)) {
				Sequence<PathElementBase> updatedPath;
				if (TryPropagateTypeInfoRecurse (path.Tail, prevType, out updatedPath)) {
					result = updatedPath.Cons (head);
					return true;
				}
			}

			result = null;
			return false;
		}

		private IEnumerable<Sequence<PathElementBase>> GetAccessPathsTyped (SymValue sv, AccessPathFilter<Method> filter, bool compress)
		{
			var visited = new HashSet<SymValue> ();
			foreach (var path in GetAccessPathsRaw (sv, null, visited, filter, compress)) {
				Sequence<PathElementBase> result;
				if (TryPropagateTypeInfo (path, out result))
					yield return result;
			}
		}

		private void CopyNonStructWithFieldValue (SymValue destAddr, SymValue sv, FlatDomain<TypeNode> addrType)
		{
			this.egraph [this.Functions.ValueOf, destAddr] = sv;
			SetType (destAddr, addrType);
		}

		public bool IsConstant (SymValue source, out TypeNode type, out object constant)
		{
			SymFunction c = this.constantLookup [source];
			if (c != null)
				return this.Functions.IsConstant (c, out type, out constant);

			type = default(TypeNode);
			constant = null;
			return false;
		}

		private void CopyOldStructValue (APC pc, SymValue destAddr, SymValue srcAddr, TypeNode type, Domain target, bool atEndOld)
		{
			throw new NotImplementedException ();
		}

		public void SetValue (SymValue destAddr, SymValue sourceAddr, FlatDomain<TypeNode> addrType, bool cast)
		{
			FlatDomain<TypeNode> type = TargetType (addrType);
			if (IsStructWithFields (type))
				CopyStructValue (destAddr, sourceAddr, type.Value);

			CopyPrimValue (destAddr, sourceAddr, cast, type);
		}

		private void CopyPrimValue (SymValue destAddr, SymValue sourceAddr, bool cast, FlatDomain<TypeNode> elementType)
		{
			SymValue value = this.egraph [this.Functions.ValueOf, sourceAddr];
			if (cast)
				SetType (value, elementType);
			else
				SetTypeIfUnknown (value, elementType);

			if (elementType.IsNormal()) {
				if (NeedsArrayLengthManifested (elementType.Value))
					ManifestArrayLength (value);
			}

			this.egraph [this.Functions.ValueOf, destAddr] = value;
		}

		public void CopyValueToOldState (APC pc, TypeNode type, int dest, int source, Domain target)
		{
			SymValue destAddress = target.Address (dest);
			SymValue srcAddress = Address (source);

			AbstractType aType = GetType (srcAddress);
			TypeNode addrType = aType.IsNormal() ? aType.ConcreteType : MetaDataProvider.ManagedPointer (type);
			CopyValueToOldState (pc, addrType, destAddress, srcAddress, target);
		}

		private void CopyValueToOldState (APC pc, TypeNode addrType, SymValue destAddress, SymValue srcAddress, Domain target)
		{
			target.MakeTotallyModified (destAddress);
			if (destAddress != srcAddress)
				target.HavocIfStruct (destAddress);
			target.SetType (destAddress, addrType);
			FlatDomain<TypeNode> targetType = TargetType (addrType);
			if (IsStructWithFields (targetType))
				CopyStructValueToOldState (pc, destAddress, srcAddress, targetType, target);
			else {
				SymValue sv = this.egraph.TryLookup (this.Functions.ValueOf, srcAddress);
				if (sv == null) {
					if (this.egraph.IsImmutable)
						return;
					sv = this.egraph [this.Functions.ValueOf, srcAddress];
				}
				CopyPrimitiveValueToOldState (pc, addrType, destAddress, sv, target);
			}
		}

		private void CopyPrimitiveValueToOldState (APC pc, TypeNode addrType, SymValue destAddress, SymValue sv, Domain target)
		{
			if (target.IsValidSymbol (sv))
				target.CopyNonStructWithFieldValue (destAddress, sv, addrType);
			else
				target.Assign (destAddress, addrType);
		}

		private bool IsValidSymbol (SymValue sv)
		{
			return this.egraph.IsValidSymbol (sv);
		}

		private void Assign (SymValue destAddress, TypeNode addrType)
		{
			Havoc (destAddress);
			SetType (destAddress, addrType);
			FlatDomain<TypeNode> targetType = TargetType (addrType);
			if (IsStructWithFields (targetType))
				return;

			SymValue fresh = this.egraph.FreshSymbol ();
			SetType (fresh, targetType);
			this.egraph [this.Functions.ValueOf, destAddress] = fresh;
			if (!targetType.IsNormal())
				return;

			if (NeedsArrayLengthManifested (targetType.Value))
				ManifestArrayLength (fresh);
		}

		private void CopyStructValueToOldState (APC pc, SymValue destAddress, SymValue srcAddress, FlatDomain<TypeNode> targetType, Domain target)
		{
			throw new NotImplementedException ();
		}

		public void ManifestArrayLength (SymValue arrayValue)
		{
			SetType (this.egraph [this.Functions.Length, arrayValue], MetaDataProvider.System_Int32);
		}

		public bool NeedsArrayLengthManifested (TypeNode type)
		{
			return MetaDataProvider.IsArray (type) || MetaDataProvider.System_String.Equals (type);
		}

		public void CopyStructValue (SymValue destAddr, SymValue sourceAddr, TypeNode type)
		{
			if (destAddr == null)
				return;
			this.egraph [this.Functions.StructId, destAddr] = this.egraph [this.Functions.StructId, sourceAddr];
			foreach (SymFunction function in this.egraph.Functions (sourceAddr)) {
				if (function.ActsAsField) {
					TypeNode functionType = function.FieldAddressType ();
					CopyValue (this.egraph [function, destAddr], this.egraph [function, sourceAddr], functionType);
				}
			}
		}

		public SymValue OldValueAddress (Parameter p)
		{
			return Address (OldValueParameterConstructor (p));
		}

		private SymFunction OldValueParameterConstructor (Parameter argument)
		{
			return this.Functions.For ("$OldParameter_" + MetaDataProvider.Name (argument));
		}

		public void CopyParameterIntoShadow (Parameter p)
		{
			CopyValue (Address (OldValueParameterConstructor (p)), Address (this.Functions.For (p)));
		}

		public void Copy (int dest, int source)
		{
			CopyValue (Address (this.Functions.For (dest)), Address (this.Functions.For (source)));
		}

		private bool IsIEnumerable (TypeNode type)
		{
			TypeNode ienumerableType;
			return (this.IEnumerable1Type.TryGet (MetaDataProvider, out ienumerableType) && MetaDataProvider.DerivesFrom (type, ienumerableType)
			        || this.IEnumerableType.TryGet (MetaDataProvider, out ienumerableType) && MetaDataProvider.DerivesFrom (type, ienumerableType));
		}

		public SymValue GetSpecialUnary (SymFunction op, int source, FlatDomain<TypeNode> type)
		{
			SymValue value = Value (source);
			SymValue sv = this.egraph [op, value];
			SetType (sv, type);
			return sv;
		}

		private SymValue LookupAddressAndManifestType (SymValue[] args, SymFunction op, TypeNode type, out bool fresh)
		{
			SymValue sv = this.egraph.TryLookup (op, args);
			if (sv != null) {
				fresh = false;
				return sv;
			}
			fresh = true;
			sv = this.egraph [op, args];
			SetType (sv, type);
			return sv;
		}

		public bool IsZero (SymValue symValue)
		{
			return this.egraph [symValue].IsZero;
		}

		public SymValue Value (int v)
		{
			return Value (Address (this.Functions.For (v)));
		}

		public SymValue Value (SymValue address)
		{
			bool fresh;
			SymValue symbol = this.egraph.LookupOrManifest (this.Functions.ValueOf, address, out fresh);

			if (fresh && IsUnmodified (address))
				MakeUnmodified (symbol);

			return symbol;
		}

		public SymValue Value (Parameter p)
		{
			return Value (Address (this.Functions.For (p)));
		}

		public void MakeUnmodified (SymValue value)
		{
			this.unmodifiedSinceEntry = this.unmodifiedSinceEntry.Add (value);
		}

		private bool IsUnmodified (SymValue value)
		{
			return this.unmodifiedSinceEntry.Contains (value);
		}

		public AbstractType CurrentType (int stackPos)
		{
			return CurrentType (Value (Address (this.Functions.For (stackPos))));
		}

		private AbstractType CurrentType (SymValue address)
		{
			return this.egraph [address];
		}

		public Domain Assume (int t, bool truth)
		{
			return Assume (Value (t), truth);
		}

		private Domain Assume (SymValue sv, bool truth)
		{
			if (!truth) {
				if (IsNonZero (sv))
					return Bottom;
				this.egraph [sv] = this.egraph [sv].ButZero;
				foreach (var term in this.egraph.EqTerms (sv)) {
					if (term.Function == this.Functions.UnaryNot || term.Function == this.Functions.NeZero)
						return Assume (term.Args [0], true);
				}
			} else {
				if (this.egraph [sv].IsZero)
					return Bottom;
				foreach (var term in this.egraph.EqTerms (sv)) {
					if (term.Function == this.Functions.UnaryNot || term.Function == this.Functions.NeZero)
						return Assume (term.Args [0], false);
				}
			}

			return this;
		}

		private bool IsNonZero (SymValue sv)
		{
			foreach (var term in this.egraph.EqTerms (sv)) {
				var wrapper = term.Function as Wrapper<object>;
				if (wrapper != null && wrapper.Item is int && (int) wrapper.Item != 0)
					return true;
			}
			return false;
		}

		public FlatDomain<TypeNode> BinaryResultType (BinaryOperator op, AbstractType t1, AbstractType t2)
		{
			switch (op) {
			case BinaryOperator.Add:
			case BinaryOperator.Add_Ovf:
			case BinaryOperator.Add_Ovf_Un:
				if (t1.IsZero)
					return t2.Type;
				return t1.Type;
			case BinaryOperator.Sub:
			case BinaryOperator.Sub_Ovf:
			case BinaryOperator.Sub_Ovf_Un:
				return t1.Type;
			case BinaryOperator.Ceq:
			case BinaryOperator.Cobjeq:
			case BinaryOperator.Cne_Un:
			case BinaryOperator.Cge:
			case BinaryOperator.Cge_Un:
			case BinaryOperator.Cgt:
			case BinaryOperator.Cgt_Un:
			case BinaryOperator.Cle:
			case BinaryOperator.Cle_Un:
			case BinaryOperator.Clt:
			case BinaryOperator.Clt_Un:
				return MetaDataProvider.System_Boolean;
			default:
				return t1.Type;
			}
		}

		private bool IsStructWithFields (FlatDomain<TypeNode> type)
		{
			if (!type.IsNormal())
				return false;

			return !MetaDataProvider.HasValueRepresentation (type.Value);
		}

		private FlatDomain<TypeNode> TargetType (FlatDomain<TypeNode> type)
		{
			if (!type.IsNormal())
				return type;

			TypeNode normalType = type.Value;
			if (MetaDataProvider.IsManagedPointer (normalType))
				return MetaDataProvider.ElementType (normalType);

			return FlatDomain<TypeNode>.TopValue;
		}

		public void SetType (SymValue sv, FlatDomain<TypeNode> type)
		{
			AbstractType abstractType = this.egraph [sv];
			if (abstractType.IsZero)
				return;

			this.egraph [sv] = abstractType.With (type);
		}

		private void SetTypeIfUnknown (SymValue sv, FlatDomain<TypeNode> type)
		{
			AbstractType abstractType = this.egraph [sv];

			if (!abstractType.IsZero && (!abstractType.Type.IsNormal() || abstractType.Type.Equals (MetaDataProvider.System_IntPtr)))
				this.egraph [sv] = abstractType.With (type);
		}

		private void MakeTotallyModified (SymValue dest)
		{
			this.unmodifiedSinceEntry = this.unmodifiedSinceEntry.Remove (dest);
			this.unmodifiedFieldsSinceEntry = this.unmodifiedFieldsSinceEntry.Remove (dest);
		}

		private bool AreUnmodified (IEnumerable<SymValue> values)
		{
			return values.All (IsUnmodified);
		}

		private bool AnyAreModifiedAtCall (IEnumerable<SymValue> values)
		{
			return values.Any (IsModifiedAtCall);
		}

		private bool IsModifiedAtCall (SymValue sv)
		{
			return (this.ModifiedAtCall != null && this.ModifiedAtCall.Contains (sv));
		}

		public Domain GetStateAt (APC pc)
		{
			Domain ifFound;
			this.parent.PreStateLookup (pc, out ifFound);

			return ifFound;
		}

		public bool TryGetCorrespondingValueAbstraction (int v, out SymbolicValue sv)
		{
			return TryGetCorrespondingValueAbstraction (this.Functions.For (v), out sv);
		}

		private bool TryGetCorrespondingValueAbstraction (SymFunction v, out SymbolicValue sv)
		{
			if (!IsBottom) {
				SymValue loc = TryAddress (v);
				if (loc != null) {
					SymValue symbol = TryCorrespondingValue (loc);
					if (symbol != null) {
						sv = new SymbolicValue (symbol);
						return true;
					}
				}
			}

			sv = default(SymbolicValue);
			return false;
		}

		private SymValue TryCorrespondingValue (SymValue address)
		{
			if (IsStructAddress (this.egraph [address]))
				return address;

			return TryValue (address);
		}

		private bool IsStructAddress (AbstractType abstractType)
		{
			if (!abstractType.IsNormal())
				return false;

			TypeNode normalType = abstractType.ConcreteType;
			if (MetaDataProvider.IsManagedPointer (normalType))
				return !MetaDataProvider.HasValueRepresentation (MetaDataProvider.ElementType (normalType));

			return false;
		}

		private SymValue TryAddress (SymFunction c)
		{
			return this.egraph.TryLookup (c);
		}

		public bool TryGetCorrespondingValueAbstraction (Local v, out SymbolicValue sv)
		{
			return TryGetCorrespondingValueAbstraction (this.Functions.For (v), out sv);
		}

		public bool TryGetUnboxedValue (int source, out SymbolicValue sv)
		{
			return TryGetUnboxedValue (TryValue (Address (source)), out sv);
		}

		private bool TryGetUnboxedValue (SymValue box, out SymbolicValue sv)
		{
			if (box != null) {
				SymValue symbol = TryValue (box);
				if (symbol != null && this.egraph.TryLookup (this.Functions.BoxOperator, symbol) == box) {
					sv = new SymbolicValue (symbol);
					return true;
				}
			}
			sv = new SymbolicValue ();
			return false;
		}

		public SymValue TryValue (SymValue address)
		{
			return this.egraph.TryLookup (this.Functions.ValueOf, address);
		}

		public AbstractType GetType (SymValue symbol)
		{
			return this.egraph [symbol];
		}

		public SymValue CreateObject (TypeNode type)
		{
			SymValue sv = this.egraph.FreshSymbol ();
			SetType (sv, type);
			return sv;
		}

		public SymValue CreateValue (TypeNode type)
		{
			SymValue sv = this.egraph.FreshSymbol ();
			SetType (sv, MetaDataProvider.ManagedPointer (type));
			return sv;
		}

		public void HavocArrayAtIndex (SymValue arrayValue, SymValue indexValue)
		{
			this.egraph.Eliminate (this.Functions.ElementAddress, arrayValue);
		}

		public SymValue CreateArray (TypeNode type, SymValue len)
		{
			SymValue sv = this.egraph.FreshSymbol ();
			this.egraph [this.Functions.Length, sv] = len;
			SetType (sv, MetaDataProvider.ArrayType (type, 1));
			return sv;
		}

		public SymValue ElementAddress (SymValue array, SymValue index, TypeNode elementAddressType)
		{
			SymValue objVersion = this.egraph [this.Functions.ObjectVersion, array];
			SymValue sv = this.egraph.TryLookup (this.Functions.ElementAddress, objVersion, index);
			if (sv == null) {
				sv = this.egraph [this.Functions.ElementAddress, objVersion, index];
				SetType (sv, elementAddressType);
				this.egraph [this.Functions.ResultOfLoadElement, sv] = Zero;
			}
			return sv;
		}

		public void CopyValueAndCast (SymValue destAddr, SymValue srcAddr, TypeNode addrType)
		{
			CopyValue (destAddr, srcAddr, addrType, true, true);
		}

		public void HavocObjectAtCall (SymValue obj, ref IImmutableSet<SymValue> havoced, bool havocFields, bool havocReadonlyFields)
		{
			HavocFields (null, obj, ref havoced, havocReadonlyFields);
			HavocUp (obj, ref havoced, havocFields);
		}

		public bool IsThis (Method currentMethod, int i)
		{
			if (MetaDataProvider.IsStatic (currentMethod))
				return false;
			return Value (MetaDataProvider.This (currentMethod)) == Value (i);
		}

		public void AssignValueAndNullnessAtConv_IU (int dest, int source, bool unsigned)
		{
			AbstractType aType = CurrentType (source);
			AssignValueAndNullnessAtConv_IU (Address (this.Functions.For (dest)), unsigned, aType);
		}

		private void AssignValueAndNullnessAtConv_IU (SymValue address, bool unsigned, AbstractType aType)
		{
			Havoc (address);
			FlatDomain<TypeNode> type = aType.Type;
			if (!IsStructWithFields (type)) {
				SymValue ptrValue = this.egraph.FreshSymbol ();
				if (type.IsNormal())
					aType = new AbstractType (!unsigned ? MetaDataProvider.System_IntPtr : MetaDataProvider.System_UIntPtr, aType.IsZero);
				SetType (address, type.IsNormal() ? MetaDataProvider.ManagedPointer (type.Value) : type);
				this.egraph [ptrValue] = aType;
				this.egraph [this.Functions.ValueOf, address] = ptrValue;
			} else
				SetType (address, type.IsNormal() ? MetaDataProvider.ManagedPointer (type.Value) : type);
		}

		public TypeNode UnaryResultType (UnaryOperator op, AbstractType type)
		{
			switch (op) {
			case UnaryOperator.Conv_i1:
				return MetaDataProvider.System_Int8;
			case UnaryOperator.Conv_i2:
				return MetaDataProvider.System_Int16;
			case UnaryOperator.Conv_i4:
				return MetaDataProvider.System_Int32;
			case UnaryOperator.Conv_i8:
				return MetaDataProvider.System_Int64;
			case UnaryOperator.Conv_u1:
				return MetaDataProvider.System_UInt8;
			case UnaryOperator.Conv_u2:
				return MetaDataProvider.System_UInt16;
			case UnaryOperator.Conv_u4:
				return MetaDataProvider.System_UInt32;
			case UnaryOperator.Conv_u8:
				return MetaDataProvider.System_UInt64;
			default:
				if (type.IsNormal())
					return type.ConcreteType;
				return MetaDataProvider.System_Int32;
			}
		}

		public void CopyStackAddress (SymValue destAddr, int temporaryForWhichAddressIsTaken)
		{
			SymValue srcAddr = Address (temporaryForWhichAddressIsTaken);
			this.egraph [this.Functions.ValueOf, destAddr] = srcAddr;
			AbstractType aType = CurrentType (srcAddr);
			FlatDomain<TypeNode> t = !aType.IsNormal() ? new FlatDomain<TypeNode> () : MetaDataProvider.ManagedPointer (aType.Type.Value);
			SetType (destAddr, t);
		}

		public SymValue PseudoFieldAddress (SymValue ptr, Method getter)
		{
			TypeNode pseudoFieldAddressType;
			return PseudoFieldAddress (ptr, getter, out pseudoFieldAddressType, false);
		}

		public SymValue PseudoFieldAddress (SymValue sv, Method m, out TypeNode pseudoFieldAddressType, bool materialize)
		{
			pseudoFieldAddressType = MetaDataProvider.ManagedPointer (MetaDataProvider.ReturnType (m));
			m = MetaDataProvider.Unspecialized (m);
			bool fresh;
			SymValue elem = LookupAddressAndManifestType (sv, this.Functions.For (m), pseudoFieldAddressType, out fresh);
			if (fresh) {
				if (IsUnmodified (sv))
					MakeModified (elem);
				if (IsModifiedAtCall (sv))
					this.ModifiedAtCall = this.ModifiedAtCall.Add (elem);
				if (materialize)
					MaterializeAccordingToType (elem, pseudoFieldAddressType, 0);
			}
			return elem;
		}

		public SymValue FieldAddress (SymValue ptr, Field field)
		{
			TypeNode fieldAddressType;
			return FieldAddress (ptr, field, out fieldAddressType);
		}

		public SymValue FieldAddress (SymValue ptr, Field field, out TypeNode fieldAddressType)
		{
			fieldAddressType = MetaDataProvider.ManagedPointer (MetaDataProvider.FieldType (field));
			bool fresh;
			SymValue sv = LookupAddressAndManifestType (ptr, this.Functions.For (field), fieldAddressType, out fresh);
			if (fresh) {
				if (IsUnmodifiedForFields (ptr) || MetaDataProvider.IsReadonly (field))
					MakeUnmodified (sv);
				if (IsModifiedAtCall (ptr))
					this.ModifiedAtCall = this.ModifiedAtCall.Add (sv);
			}
			return sv;
		}

		public void Assign (Parameter parameter, TypeNode type)
		{
			AssignValue (Address (this.Functions.For (parameter)), type);
		}

		public SymValue PseudoFieldAddressOfOutParameter (int index, SymValue fieldAddr, TypeNode pType)
		{
			SymValue sv = this.egraph [this.Functions.ForConstant (index, MetaDataProvider.System_Int32), fieldAddr];
			MaterializeAccordingToType (sv, pType, 2);
			return sv;
		}

		public SymValue ObjectVersion (SymValue sv)
		{
			return this.egraph [this.Functions.ObjectVersion, sv];
		}

		public SymValue PseudoFieldAddress (SymValue[] args, Method method, out TypeNode pseudoFieldAddressType, bool materialize)
		{
			if (args.Length == 0 || args.Length == 1)
				return PseudoFieldAddress (Globals, method, out pseudoFieldAddressType, materialize);
			pseudoFieldAddressType = MetaDataProvider.ManagedPointer (MetaDataProvider.ReturnType (method));
			method = MetaDataProvider.Unspecialized (method);

			bool fresh;
			SymValue sv = LookupAddressAndManifestType (args, this.Functions.For (method), pseudoFieldAddressType, out fresh);
			if (fresh) {
				if (AreUnmodified (args))
					MakeUnmodified (sv);
				if (AnyAreModifiedAtCall (args))
					this.ModifiedAtCall = this.ModifiedAtCall.Add (sv);
				if (materialize)
					MaterializeAccordingToType (sv, pseudoFieldAddressType, 0);
			}
			return sv;
		}

		public void AssignReturnValue (int dest, TypeNode type)
		{
			AssignValue (dest, type);
			if (MetaDataProvider.HasValueRepresentation (type))
				this.egraph [this.Functions.ResultOfCall, Value (dest)] = Zero;
			else
				MaterializeAccordingToType (Address (dest), MetaDataProvider.ManagedPointer (type), 0);
		}

		public void MaterializeAccordingToType (SymValue sv, TypeNode type, int depth)
		{
			SetType (sv, type);
			if (depth > 2)
				return;

			if (MetaDataProvider.IsManagedPointer (type)) {
				TypeNode elementType = MetaDataProvider.ElementType (type);
				if (!MetaDataProvider.HasValueRepresentation (elementType)) {
					ManifestStructId (sv);
					foreach (Field field in MetaDataProvider.Fields (elementType)) {
						if (!MetaDataProvider.IsStatic (field)) {
							TypeNode fieldAddressType;
							MaterializeAccordingToType (FieldAddress (sv, field, out fieldAddressType), fieldAddressType, depth + 1);
						}
					}
					foreach (Property property in MetaDataProvider.Properties (elementType)) {
						Method getter;
						if (!MetaDataProvider.IsStatic (property) && MetaDataProvider.HasGetter (property, out getter)) {
							TypeNode pseudoFieldAddressType;
							MaterializeAccordingToType (PseudoFieldAddress (sv, getter, out pseudoFieldAddressType, false), pseudoFieldAddressType, depth + 1);
						}
					}
				} else
					MaterializeAccordingToType (Value (sv), elementType, depth + 1);
			}
		}

		public void ManifestStructId (SymValue sv)
		{
			StructId (sv);
		}

		public SymValue StructId (SymValue sv)
		{
			return this.egraph [this.Functions.StructId, sv];
		}

		private SymValue LookupAddressAndManifestType (SymValue sv, SymFunction c, TypeNode type, out bool fresh)
		{
			SymValue value = this.egraph.TryLookup (c, sv);
			if (value != null) {
				fresh = false;
				return value;
			}

			fresh = true;
			value = this.egraph [c, sv];
			SetType (value, type);

			return value;
		}

		public void HavocFields (SymValue sv, IEnumerable<Field> fields, ref IImmutableSet<SymValue> havoced)
		{
			foreach (Field f in fields)
				HavocConstructor (null, sv, this.Functions.For (f), ref havoced, false);
		}

		private bool IsUnmodifiedForFields (SymValue sv)
		{
			return this.unmodifiedFieldsSinceEntry.Contains (sv);
		}

		public void HavocUp (SymValue srcAddr, ref IImmutableSet<SymValue> havocedAtCall, bool havocFields)
		{
			foreach (var term in this.egraph.EqTerms (srcAddr)) {
				if (term.Function == this.Functions.ValueOf)
					HavocUpField (term.Args [0], ref havocedAtCall, havocFields);
			}
		}

		private void HavocUpField (SymValue sv, ref IImmutableSet<SymValue> havocedAtCall, bool havocFields)
		{
			foreach (var term in this.egraph.EqTerms (sv)) {
				SymFunction accessedVia = term.Function;
				if (accessedVia is Wrapper<Field>)
					HavocUpObjectVersion (accessedVia, term.Args [0], ref havocedAtCall, havocFields);
				else {
					var methodWrapper = accessedVia as Wrapper<Method>;
					Property property;
					if (methodWrapper != null && !MetaDataProvider.IsStatic (methodWrapper.Item) && MetaDataProvider.IsPropertyGetter (methodWrapper.Item, out property))
						HavocUpObjectVersion (accessedVia, term.Args [0], ref havocedAtCall, havocFields);
				}
			}
		}

		private void HavocUpObjectVersion (SymFunction accessedVia, SymValue sv, ref IImmutableSet<SymValue> havocedAtCall, bool havocFields)
		{
			if (havocedAtCall.Contains (sv))
				return;
			this.egraph.Eliminate (this.Functions.ObjectVersion, sv);
			if (havocFields)
				HavocMutableFields (accessedVia, sv, ref havocedAtCall);
			else
				HavocPseudoFields (accessedVia, sv, ref havocedAtCall);

			HavocUp (sv, ref havocedAtCall, false);
		}

		public void CopyAddress (SymValue destAddr, SymValue srcAddr, TypeNode type)
		{
			this.egraph [this.Functions.ValueOf, destAddr] = srcAddr;
			SetType (destAddr, MetaDataProvider.ManagedPointer (type));
		}

		public void AssignArrayLength (SymValue destAddr, SymValue array)
		{
			SymValue length = this.egraph [this.Functions.Length, array];
			SetType (length, MetaDataProvider.System_Int32);
			this.egraph [this.Functions.ValueOf, destAddr] = length;
			SetType (destAddr, MetaDataProvider.ManagedPointer (MetaDataProvider.System_Int32));
		}

		public void ResetModifiedAtCall ()
		{
			this.ModifiedAtCall = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
		}

		public SymValue LoadValue (int source)
		{
			return Value (source);
		}

		public SymbolicValue TryLoadValue (SymValue addr)
		{
			return new SymbolicValue (Value (addr));
		}

		public bool TryGetArrayLength (SymValue arrayValue, out SymValue length)
		{
			length = this.egraph.TryLookup (this.Functions.Length, arrayValue);
			return length != null;
		}

		public bool TryGetCorrespondingValueAbstraction (Parameter p, out SymbolicValue sv)
		{
			return TryGetCorrespondingValueAbstraction (this.Functions.For (p), out sv);
		}

		public string GetAccessPath (SymValue sv)
		{
			Sequence<PathElementBase> bestAccessPath = GetBestAccessPath (sv, AccessPathFilter<Method>.NoFilter, true, true, false);
			if (bestAccessPath == null)
				return null;

			return bestAccessPath.Select (i => (PathElement) i).ToCodeString ();
		}

		public Sequence<PathElement> GetAccessPathList (SymValue symbol, AccessPathFilter<Method> filter, bool allowLocal, bool preferLocal)
		{
			return GetBestAccessPath (symbol, filter, true, allowLocal, preferLocal).Coerce<PathElementBase, PathElement> ();
		}

		public bool LessEqual (Domain that, out IImmutableMap<SymValue, Sequence<SymValue>> forward, out IImmutableMap<SymValue, SymValue> backward)
		{
			return this.egraph.LessEqual (that.egraph, out forward, out backward);
		}

		public bool IsResultEGraph (IMergeInfo mi)
		{
			return mi.IsResultGraph (this.egraph);
		}

		public bool IsGraph1 (IMergeInfo mi)
		{
			return mi.IsGraph1 (this.egraph);
		}

		public bool IsGraph2 (IMergeInfo mi)
		{
			return mi.IsGraph2 (this.egraph);
		}

		public IImmutableMap<SymValue, Sequence<SymValue>> GetForwardIdentityMap ()
		{
			return this.egraph.GetForwardIdentityMap ();
		}

		public Domain Join (Domain that, bool widening, out bool weaker, out IMergeInfo mergeInfo)
		{
			SymGraph<SymFunction, AbstractType> graph = this.egraph.Join (that.egraph, out mergeInfo, widening);
			weaker = mergeInfo.Changed;

			IImmutableSet<SymValue> resultUnmodifiedSinceEntry;
			IImmutableSet<SymValue> resultUnmodifiedFieldsSinceEntry;
			IImmutableSet<SymValue> resultModifiedAtCall;

			ComputeJoinOfSets (mergeInfo, this, that, out resultUnmodifiedSinceEntry, out resultUnmodifiedFieldsSinceEntry, out resultModifiedAtCall, ref weaker);
			Domain oldDomain = null;
			if (OldDomain != null) {
				bool oldWeaker;
				IMergeInfo oldMergeInfo;
				oldDomain = OldDomain.Join (that.OldDomain, widening, out oldWeaker, out oldMergeInfo);
			}

			return new Domain (graph, RecomputeConstantMap (graph), resultUnmodifiedSinceEntry, resultUnmodifiedFieldsSinceEntry, resultModifiedAtCall, this, oldDomain);
		}

		private IImmutableMap<SymValue, SymFunction> RecomputeConstantMap (SymGraph<SymFunction, AbstractType> graph)
		{
			IImmutableMap<SymValue, SymFunction> result = ImmutableIntKeyMap<SymValue, SymFunction>.Empty (SymValue.GetUniqueKey);
			foreach (SymFunction constant in graph.Constants) {
				if (this.Functions.IsConstantOrMethod (constant))
					result.Add (graph [constant], constant);
			}
			return result;
		}

		private void ComputeJoinOfSets (IMergeInfo mergeInfo, Domain domain, Domain that,
		                                out IImmutableSet<SymValue> resultUnmodifiedSinceEntry,
		                                out IImmutableSet<SymValue> resultUnmodifiedFieldsSinceEntry,
		                                out IImmutableSet<SymValue> resultModifiedAtCall, ref bool weaker)
		{
			resultUnmodifiedSinceEntry = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			resultUnmodifiedFieldsSinceEntry = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			resultModifiedAtCall = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);

			foreach (var tuple in mergeInfo.MergeTriples) {
				SymValue symValue1 = tuple.Item1;
				SymValue symValue2 = tuple.Item2;
				SymValue elem = tuple.Item3;

				bool unmodifiedSinceEntryContains = domain.unmodifiedSinceEntry.ContainsSafe (symValue1);
				bool modifiedAtCallContains = domain.ModifiedAtCall.ContainsSafe (symValue1);

				if (unmodifiedSinceEntryContains) {
					if (that.unmodifiedSinceEntry.ContainsSafe (symValue2))
						resultUnmodifiedSinceEntry = resultUnmodifiedSinceEntry.Add (elem);
					else if (that.unmodifiedFieldsSinceEntry.ContainsSafe (symValue2))
						resultUnmodifiedFieldsSinceEntry = resultUnmodifiedFieldsSinceEntry.Add (elem);
				}

				if (modifiedAtCallContains)
					resultModifiedAtCall = resultModifiedAtCall.Add (elem);
			}

			if (that.unmodifiedSinceEntry.Count () > resultUnmodifiedSinceEntry.Count ()) {
				weaker = true;
				if (DebugOptions.Debug)
					Console.WriteLine ("---Result changed due to fewer unmodified locations since entry");
			} else {
				if (that.unmodifiedFieldsSinceEntry.Count () > resultUnmodifiedFieldsSinceEntry.Count ()) {
					weaker = true;
					if (DebugOptions.Debug)
						Console.WriteLine ("---Result changed due to fewer unmodified locations for fields since entry");
				}
			}
		}

		#region Implementation of IAbstractDomain<Domain>
		public Domain Top
		{
			get
			{
				return new Domain (this.egraph.Top, ImmutableIntKeyMap<SymValue, SymFunction>.Empty (SymValue.GetUniqueKey), ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey),
				                   ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey), ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey), this, null);
			}
		}

		public Domain Bottom
		{
			get
			{
				if (BottomValue == null) {
					BottomValue = new Domain (this.egraph.Bottom, ImmutableIntKeyMap<SymValue, SymFunction>.Empty (SymValue.GetUniqueKey), ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey),
					                          ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey), ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey),
					                          this, null);
					BottomValue.ImmutableVersion ();
				}
				return BottomValue;
			}
		}

		public bool IsTop
		{
			get { return this.egraph.IsTop; }
		}

		public bool IsBottom
		{
			get
			{
				if (this == BottomValue || this.egraph.IsBottom)
					return true;
				if (OldDomain != null)
					return OldDomain.IsBottom;

				return false;
			}
		}

	    public Domain Join(Domain that)
	    {
	        throw new NotImplementedException();
	    }

	    public Domain Join (Domain that, bool widening, out bool weaker)
		{
			IMergeInfo mergeInfo;
			return Join (that, widening, out weaker, out mergeInfo);
		}

	    public Domain Widen(Domain that)
	    {
	        throw new NotImplementedException();
	    }

	    public Domain Meet (Domain that)
		{
			SymGraph<SymFunction, AbstractType> graph = this.egraph.Meet (that.egraph);
			return new Domain (graph, RecomputeConstantMap (graph), this.unmodifiedSinceEntry, this.unmodifiedFieldsSinceEntry, null, this, OldDomain);
		}

		public bool LessEqual (Domain that)
		{
			return this.egraph.LessEqual (that.egraph);
		}

		public Domain ImmutableVersion ()
		{
			this.egraph.ImmutableVersion ();
			return this;
		}
		#endregion
	}
}
