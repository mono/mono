// 
// AnalysisDecoder.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	struct AnalysisDecoder : IILVisitor<APC, int, int, Domain, Domain> {
		private readonly HeapAnalysis parent;

		public AnalysisDecoder (HeapAnalysis parent)
		{
			this.parent = parent;
		}

		public IContractProvider ContractProvider
		{
			get { return this.parent.ContractProvider; }
		}

		public IMetaDataProvider MetaDataProvider
		{
			get { return this.parent.MetaDataProvider; }
		}
		#region Helper Methods
		private void UnaryEffect (UnaryOperator op, int dest, int source, Domain data)
		{
			switch (op)
			{
			case UnaryOperator.Conv_i:
				data.AssignValueAndNullnessAtConv_IU (dest, source, false);
				break;
			case UnaryOperator.Conv_u:
				data.AssignValueAndNullnessAtConv_IU (dest, source, true);
				break;
			case UnaryOperator.Not:
				data.AssignSpecialUnary (dest, data.Functions.UnaryNot, source, MetaDataProvider.System_Int32);
				break;
			default:
				data.AssignPureUnary (dest, op, data.UnaryResultType (op, data.CurrentType (source)), source);
				break;
			}
		}

		private Domain BinaryEffect (APC pc, BinaryOperator op, int dest, int op1, int op2, Domain data)
		{
			FlatDomain<TypeNode> resultType = data.BinaryResultType (op, data.CurrentType (op1), data.CurrentType (op2));
			switch (op) {
			case BinaryOperator.Ceq:
			case BinaryOperator.Cobjeq:
				{
					SymValue srcValue = data.Value (op1);
					if (data.IsZero (srcValue)) {
						data.AssignSpecialUnary (dest, data.Functions.UnaryNot, op2, resultType);
						break;
					}
					SymValue val2 = data.Value (op2);
					if (data.IsZero (val2)) {
						data.AssignSpecialUnary (dest, data.Functions.UnaryNot, op1, resultType);
						break;
					}
					goto default;
				}
			case BinaryOperator.Cne_Un:
				{
					SymValue val1 = data.Value (op1);
					if (data.IsZero (val1)) {
						data.AssignSpecialUnary (dest, data.Functions.NeZero, op2, resultType);
						break;
					}
					SymValue val2 = data.Value (op2);
					if (data.IsZero (val2)) {
						data.AssignSpecialUnary (dest, data.Functions.NeZero, op1, resultType);
						break;
					}
					goto default;
				}
			default:
				data.AssignPureBinary (dest, op, resultType, op1, op2);
				break;
			}

			data.Havoc (2);
			return data;
		}

		private void LoadArgEffect (Parameter argument, bool isOld, int dest, Domain data)
		{
			SymValue address = isOld ? data.OldValueAddress (argument) : data.Address (argument);
			IMetaDataProvider metadataDecoder = MetaDataProvider;
			data.CopyValue (data.Address (dest), address, metadataDecoder.ManagedPointer (metadataDecoder.ParameterType (argument)));
		}

		private void StoreLocalEffect (Local local, int source, Domain data)
		{
			data.CopyValue (data.Address (local), data.Address (source), MetaDataProvider.ManagedPointer (MetaDataProvider.LocalType (local)));
			data.Havoc (source);
		}

		private void IsinstEffect (TypeNode type, int dest, Domain data)
		{
			data.AssignValue (dest, type);
		}

		private void LoadLocalEffect (Local local, int dest, Domain data)
		{
			data.CopyValue (data.Address (dest), data.Address (local), MetaDataProvider.ManagedPointer (MetaDataProvider.LocalType (local)));
		}

		private Domain AssumeEffect (APC pc, EdgeTag tag, int condition, Domain data)
		{
			data = data.Assume (condition, tag != EdgeTag.False);
			if (!data.IsBottom)
				data.Havoc (condition);

			return data;
		}

		private Domain AssertEffect (APC pc, EdgeTag tag, int condition, Domain data)
		{
			data = data.Assume (condition, true);
			if (!data.IsBottom)
				data.Havoc (condition);

			return data;
		}

		private static void LoadStackAddressEffect (APC pc, int offset, int dest, int source, Domain data)
		{
			data.CopyStackAddress (data.Address (dest), source);
		}

		private Domain CallEffect<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, int dest, ArgList args, Domain data, TypeNode constraint, bool constrainedCall)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			TypeNode t = constraint;
			if (!pc.InsideContract)
				data.ResetModifiedAtCall ();

			IImmutableMap<TypeNode, TypeNode> instantiationMap = ComputeTypeInstantiationMap (pc, method);
			bool derefThis = false;
			if (virt)
			{
				if (MetaDataProvider.IsStruct (constraint))
					DevirtualizeImplementingMethod (constraint, ref method);
				else
				{
					if (constrainedCall && MetaDataProvider.IsReferenceType (Specialize (instantiationMap, constraint)))
						derefThis = true;
					SymValue loc = data.Value (args[0]);
					if (derefThis)
						loc = data.Value (loc);
					AbstractType aType = data.GetType (loc);
					if (aType.IsNormal())
						DevirtualizeImplementingMethod (aType.ConcreteType, ref method);
				}
			}
			string name = MetaDataProvider.Name (method);
			if (args.Count > 0)
			{
				if ((MetaDataProvider.Equal (t, MetaDataProvider.System_String) || MetaDataProvider.Equal (t, MetaDataProvider.System_Array))
				    && (name == "get_Length" || name == "get_LongLength"))
				{
					data.AssignArrayLength (data.Address (dest), data.Value (args[0]));
					return data;
				}
				if (MetaDataProvider.Equal (t, MetaDataProvider.System_Object) && name == "MemberwiseClone")
				{
					TypeNode t2 = data.GetType (data.Value (args[0])).ConcreteType;
					SymValue obj = data.CreateObject (t2);
					data.CopyStructValue (obj, data.Value (args[0]), t2);
					data.CopyAddress (data.Address (dest), obj, t2);
					return data;
				}
				if (args.Count > 1 && MetaDataProvider.IsReferenceType (t))
				{
					if (name.EndsWith ("op_Inequality"))
						return Binary (pc, BinaryOperator.Cne_Un, dest, args[0], args[1], data);
					if (name.EndsWith ("op_Equality"))
						return Binary (pc, BinaryOperator.Cobjeq, dest, args[0], args[1], data);
				}
				if (MetaDataProvider.Equal (t, MetaDataProvider.System_IntPtr) && name.StartsWith ("op_Explicit"))
				{
					data.Copy (dest, args[0]);
					return data;
				}
			}
			//todo:
			//				if (extraVarargs.Count == 0 && !this.MetaDataProvider.IsVoidMethod(method) && this.ContractProvider.)

			{
				Property property;
				if (MetaDataProvider.IsPropertySetter (method, out property))
				{
					if (args.Count <= 2)
					{
						Method getter;
						if (MetaDataProvider.HasGetter (property, out getter))
						{
							SymValue obj;
							SymValue srcAddr;
							if (args.Count == 1)
							{
								obj = data.Globals;
								srcAddr = data.Address (args[0]);
							}
							else
							{
								obj = data.Value (args[0]);
								if (derefThis)
									obj = data.Value (obj);
								srcAddr = data.Address (args[1]);
							}
							if (MetaDataProvider.Equal (MetaDataProvider.DeclaringType (this.parent.CurrentMethod), MetaDataProvider.Unspecialized (MetaDataProvider.DeclaringType (method))))
							{
								data.HavocUp (obj, ref data.ModifiedAtCall, false);

								if (MetaDataProvider.IsAutoPropertyMember (method))
								{
									foreach (Field f in this.parent.StackContextProvider.MethodContext.Modifies (method))
									{
										TypeNode fieldAddressType;
										SymValue destAddr = data.FieldAddress (obj, f, out fieldAddressType);
										data.CopyValue (destAddr, srcAddr, fieldAddressType);
									}
								}
								else
									data.HavocFields (obj, this.parent.StackContextProvider.MethodContext.Modifies (method), ref data.ModifiedAtCall);
							}
							TypeNode pseudoFieldAddressType;
							SymValue destAddr1 = data.PseudoFieldAddress (obj, getter, out pseudoFieldAddressType, true);
							data.CopyValue (destAddr1, srcAddr, pseudoFieldAddressType);
							data.AssignValue (dest, MetaDataProvider.System_Void);
							return data;
						}
					}
					else
					{
						Method getter;
						if (MetaDataProvider.HasGetter (property, out getter))
						{
							var args1 = new SymValue[GetNonOutArgs (method) - 1];
							int num = 0;
							for (int i = 0; i < args.Count - 1; i++)
							{
								bool isOut;
								SymValue sv = KeyForPureFunctionArgument (method, i, args[i], data, instantiationMap, out isOut);
								if (!isOut)
									args1[num++] = sv;
							}
							SymValue thisSV = data.Value (args[0]);
							if (derefThis)
								thisSV = data.Value (thisSV);
							TypeNode pseudoFieldAddressType;
							SymValue pseudoField = data.PseudoFieldAddress (args1, getter, out pseudoFieldAddressType, false);
							AssignAllOutParameters (data, pseudoField, method, args);
							SymValue srcAddr = data.Address (args[args.Count - 1]);
							data.CopyValue (pseudoField, srcAddr, pseudoFieldAddressType);
							if (MetaDataProvider.Equal (MetaDataProvider.DeclaringType (this.parent.CurrentMethod), MetaDataProvider.Unspecialized (MetaDataProvider.DeclaringType (method))))
							{
								data.HavocUp (thisSV, ref data.ModifiedAtCall, false);
								data.HavocFields (thisSV, this.parent.StackContextProvider.MethodContext.Modifies (method), ref data.ModifiedAtCall);
							}
						}
						data.AssignValue (dest, MetaDataProvider.System_Void);
						return data;
					}
				}
				bool insideConstructor = MetaDataProvider.IsConstructor (method);
				HavocParameters (pc, method, extraVarargs, args, data, constraint, insideConstructor, false, derefThis);
				data.AssignReturnValue (dest, MetaDataProvider.ReturnType (method));
				return data;
			}
		}

		private static Domain DoWithBothDomains (Domain data, Func<Domain, Domain> action)
		{
			data = action (data);
			if (data.OldDomain != null)
				data.OldDomain = action (data.OldDomain);
			return data;
		}

		private static Domain DoWithBothDomains (Domain data, Action<Domain> action)
		{
			action (data);
			if (data.OldDomain != null)
				action (data.OldDomain);
			return data;
		}
		#endregion

		#region IILVisitor<APC,int,int,Domain,Domain> Members
		public Domain Binary (APC pc, BinaryOperator op, int dest, int operand1, int operand2, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.BinaryEffect (pc, op, dest, operand1, operand2, d));
		}

		public Domain Isinst (APC pc, TypeNode type, int dest, int obj, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.IsinstEffect (type, dest, d));
		}

		public Domain LoadNull (APC pc, int dest, Domain polarity)
		{
			return DoWithBothDomains (polarity, d => d.AssignNull (dest));
		}

		public Domain LoadConst (APC pc, TypeNode type, object constant, int dest, Domain data)
		{
			return DoWithBothDomains (data, d => d.AssignConst (dest, type, constant));
		}

		public Domain Sizeof (APC pc, TypeNode type, int dest, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => d.AssignValue (dest, it.MetaDataProvider.System_Int32));
		}

		public Domain Unary (APC pc, UnaryOperator op, bool unsigned, int dest, int source, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.UnaryEffect (op, dest, source, d));
		}

		public Domain Entry (APC pc, Method method, Domain data)
		{
			IIndexable<Local> locals = MetaDataProvider.Locals (method);
			for (int i = 0; i < locals.Count; i++)
				MaterializeLocal (locals [i], method, data);

			TypeNode declaringType = MetaDataProvider.DeclaringType (method);
			IIndexable<Parameter> parameters = MetaDataProvider.Parameters (method);
			for (int i = 0; i < parameters.Count; i++)
				MaterializeParameter (parameters [i], declaringType, data, false);

			if (!MetaDataProvider.IsStatic (method))
				MaterializeParameter (MetaDataProvider.This (method), declaringType, data, true);

			if (MetaDataProvider.IsConstructor (method)) {
				Parameter p = MetaDataProvider.This (method);
				SymValue ptr = data.Value (p);

				foreach (Field field in MetaDataProvider.Fields (declaringType)) {
					if (MetaDataProvider.IsStatic (field))
						continue;

					TypeNode fieldType = MetaDataProvider.FieldType (field);
					if (MetaDataProvider.IsStruct (fieldType))
						data.AssignConst (data.FieldAddress (ptr, field), fieldType, 0);
					else
						data.AssignNull (data.FieldAddress (ptr, field));
				}

				foreach (Property property in MetaDataProvider.Properties (declaringType)) {
					Method getter;
					if (!MetaDataProvider.IsStatic (property) && MetaDataProvider.HasGetter (property, out getter)
					    && MetaDataProvider.IsCompilerGenerated (getter) && MetaDataProvider.Parameters (getter).Count == 0) {
						TypeNode propertyType = MetaDataProvider.ReturnType (getter);
						if (MetaDataProvider.IsStruct (propertyType))
							data.AssignConst (data.PseudoFieldAddress (ptr, getter), propertyType, 0);
						else
							data.AssignNull (data.PseudoFieldAddress (ptr, getter));
					}
				}
			}

			return data;
		}

		public Domain Assume (APC pc, EdgeTag tag, int condition, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.AssumeEffect (pc, tag, condition, data));
		}

		public Domain Assert (APC pc, EdgeTag tag, int condition, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.AssertEffect (pc, tag, condition, data));
		}

		public Domain BeginOld (APC pc, APC matchingEnd, Domain data)
		{
			if (data.InsideOld++ == 0) {
				Domain oldState = FindOldState (pc, data);
				if (oldState == null)
					throw new InvalidOperationException ("begin_old in weird calling context");
				Domain domain = oldState.Clone ();
				domain.BeginOldPC = pc;
				data.OldDomain = domain;
			}

			return data;
		}

		public Domain EndOld (APC pc, APC matchingBegin, TypeNode type, int dest, int source, Domain data)
		{
			if (--data.InsideOld == 0)
				data.OldDomain = null;

			data.Copy (dest, source);
			return data;
		}

		public Domain LoadStack (APC pc, int offset, int dest, int source, bool isOld, Domain data)
		{
			if (isOld) {
				Domain oldState = FindOldState (pc, data);
				oldState.CopyOldValue (pc, dest, source, data, false);
				if (data.OldDomain != null)
					oldState.CopyOldValue (pc, dest, source, data.OldDomain, false);
			} else {
				data.Copy (dest, source);
				if (data.OldDomain != null)
					data.OldDomain.Copy (dest, source);
			}

			return data;
		}

		public Domain LoadStackAddress (APC pc, int offset, int dest, int source, TypeNode type, bool isOld, Domain data)
		{
			if (isOld) {
				TypeNode ptrType = MetaDataProvider.ManagedPointer (type);
				Domain oldState = FindOldState (pc, data);
				SymValue srcOldAddr = oldState.Address (source);
				SymValue sv = data.CreateValue (type);

				oldState.CopyOldValue (pc, sv, srcOldAddr, data, true);
				data.CopyAddress (data.Address (dest), sv, ptrType);
				if (data.OldDomain != null) {
					TypeNode ptrPtrType = MetaDataProvider.ManagedPointer (ptrType);
					oldState.CopyOldValueToDest (pc, data.OldDomain.Address (dest), srcOldAddr, ptrPtrType, data.OldDomain);
				}
			} else {
				LoadStackAddressEffect (pc, offset, dest, source, data);
				if (data.OldDomain != null)
					LoadStackAddressEffect (pc, offset, dest, source, data.OldDomain);
			}
			return data;
		}

		public Domain LoadResult (APC pc, TypeNode type, int dest, int source, Domain data)
		{
			return DoWithBothDomains (data, d => d.Copy (dest, source));
		}

		public Domain Arglist (APC pc, int dest, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain Branch (APC pc, APC target, bool leavesExceptionBlock, Domain data)
		{
			throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
		}

		public Domain BranchCond (APC pc, APC target, BranchOperator bop, int value1, int value2, Domain data)
		{
			throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
		}

		public Domain BranchTrue (APC pc, APC target, int cond, Domain data)
		{
			throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
		}

		public Domain BranchFalse (APC pc, APC target, int cond, Domain data)
		{
			throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
		}

		public Domain Break (APC pc, Domain data)
		{
			return data;
		}

		public Domain Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, int dest, ArgList args, Domain data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			TypeNode declaringType = MetaDataProvider.DeclaringType (method);
			if (data.OldDomain == null)
				return CallEffect (pc, method, virt, extraVarargs, dest, args, data, declaringType, false);
			data.OldDomain = CallEffect (pc, method, virt, extraVarargs, dest, args, data.OldDomain, declaringType, false);
			if (!MetaDataProvider.IsVoidMethod (method))
				data.OldDomain.CopyOldValue (data.OldDomain.BeginOldPC, dest, dest, data, true);
			return data;
		}

		public Domain Calli<TypeList, ArgList> (APC pc, TypeNode returnType, TypeList argTypes, bool instance, int dest, int functionPointer, ArgList args, Domain data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			if (data.OldDomain == null)
				return CalliEffect (pc, returnType, argTypes, instance, dest, functionPointer, args, data);
			data.OldDomain = CalliEffect (pc, returnType, argTypes, instance, dest, functionPointer, args, data.OldDomain);
			if (!MetaDataProvider.IsVoid (returnType))
				data.OldDomain.CopyOldValue (data.OldDomain.BeginOldPC, dest, dest, data, true);
			return data;
		}

		public Domain CheckFinite (APC pc, int dest, int source, Domain data)
		{
			data.Copy (dest, source);
			if (data.OldDomain != null)
				data.OldDomain.Copy (dest, source);

			return data;
		}

		public Domain CopyBlock (APC pc, int destAddress, int srcAddress, int len, Domain data)
		{
			data.Havoc (destAddress);
			data.Havoc (srcAddress);
			data.Havoc (len);

			return data;
		}

		public Domain EndFilter (APC pc, int decision, Domain data)
		{
			data.Havoc (decision);

			return data;
		}

		public Domain EndFinally (APC pc, Domain data)
		{
			return data;
		}

		public Domain Jmp (APC pc, Method method, Domain data)
		{
			return data;
		}

		public Domain LoadArg (APC pc, Parameter argument, bool isOld, int dest, Domain data)
		{
			LoadArgEffect (argument, isOld, dest, data);
			if (data.OldDomain != null)
				LoadArgEffect (argument, isOld, dest, data.OldDomain);

			return data;
		}

		public Domain LoadArgAddress (APC pc, Parameter argument, bool isOld, int dest, Domain data)
		{
			LoadArgAddressEffect (argument, isOld, dest, data);
			if (data.OldDomain != null)
				LoadArgAddressEffect (argument, isOld, dest, data.OldDomain);
			return data;
		}

		public Domain LoadLocal (APC pc, Local local, int dest, Domain data)
		{
			LoadLocalEffect (local, dest, data);
			if (data.OldDomain != null)
				data.CopyValueToOldState (data.OldDomain.BeginOldPC, MetaDataProvider.LocalType (local), dest, dest, data.OldDomain);

			return data;
		}

		public Domain LoadLocalAddress (APC pc, Local local, int dest, Domain data)
		{
			LoadLocalAddressEffect (local, dest, data);
			if (data.OldDomain != null)
				LoadLocalAddressEffect (local, dest, data.OldDomain);

			return data;
		}

		public Domain Nop (APC pc, Domain data)
		{
			return data;
		}

		public Domain Pop (APC pc, int source, Domain data)
		{
			data.Havoc (source);
			if (data.OldDomain != null)
				data.OldDomain.Havoc (source);

			return data;
		}

		public Domain Return (APC pc, int source, Domain data)
		{
			data.Havoc (source);

			return data;
		}

		public Domain StoreArg (APC pc, Parameter argument, int source, Domain data)
		{
			data.CopyValue (data.Address (argument), data.Address (source), MetaDataProvider.ManagedPointer (MetaDataProvider.ParameterType (argument)));
			data.Havoc (source);

			return data;
		}

		public Domain StoreLocal (APC pc, Local local, int source, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.StoreLocalEffect (local, source, d));
		}

		public Domain Switch (APC pc, TypeNode type, IEnumerable<Pair<object, APC>> cases, int value, Domain data)
		{
			throw new InvalidOperationException ("Should only see assumes");
		}

		public Domain Box (APC pc, TypeNode type, int dest, int source, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, (d) => it.BoxEffect (type, dest, source, d));
		}

		public Domain ConstrainedCallvirt<TypeList, ArgList> (APC pc, Method method, TypeNode constraint, TypeList extraVarargs, int dest, ArgList args, Domain data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			if (data.OldDomain == null)
				return CallEffect (pc, method, true, extraVarargs, dest, args, data, constraint, true);

			data.OldDomain = CallEffect (pc, method, true, extraVarargs, dest, args, data.OldDomain, constraint, true);
			if (!MetaDataProvider.IsVoidMethod (method))
				data.OldDomain.CopyOldValue (data.OldDomain.BeginOldPC, dest, dest, data, true);
			return data;
		}

		public Domain CastClass (APC pc, TypeNode type, int dest, int obj, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.CastClassEffect (type, dest, obj, d));
		}

		public Domain CopyObj (APC pc, TypeNode type, int destPtr, int sourcePtr, Domain data)
		{
			data.CopyValue (data.Value (destPtr), data.Value (sourcePtr), MetaDataProvider.ManagedPointer (type));
			data.Havoc (destPtr);
			data.Havoc (sourcePtr);
			return data;
		}

		public Domain Initobj (APC pc, TypeNode type, int ptr, Domain data)
		{
			SymValue obj = data.Value (ptr);
			data.Havoc (obj);
			foreach (Field field in MetaDataProvider.Fields (type)) {
				SymValue dest = data.FieldAddress (obj, field);
				data.AssignZeroEquivalent (dest, MetaDataProvider.FieldType (field));
			}
			data.Havoc (ptr);
			return data;
		}

		public Domain LoadElement (APC pc, TypeNode type, int dest, int array, int index, Domain data)
		{
			if (data.OldDomain != null) {
				LoadElementEffect (type, dest, array, index, data.OldDomain);
				data.OldDomain.CopyOldValue (data.OldDomain.BeginOldPC, dest, dest, data, true);
			} else
				LoadElementEffect (type, dest, array, index, data);

			return data;
		}

		public Domain LoadField (APC pc, Field field, int dest, int obj, Domain data)
		{
			if (data.OldDomain != null) {
				LoadFieldEffect (field, dest, obj, data.OldDomain);
				data.OldDomain.CopyOldValue (data.OldDomain.BeginOldPC, dest, dest, data, true);
			} else
				LoadFieldEffect (field, dest, obj, data);

			return data;
		}

		public Domain LoadFieldAddress (APC pc, Field field, int dest, int obj, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, domain => it.LoadFieldAddressEffect (pc, field, dest, obj, domain));
		}

		public Domain LoadLength (APC pc, int dest, int array, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, domain => it.LoadLengthEffect (dest, array, domain));
		}

		public Domain LoadStaticField (APC pc, Field field, int dest, Domain data)
		{
			if (data.OldDomain != null) {
				LoadStaticFieldEffect (field, dest, data.OldDomain);
				data.OldDomain.CopyOldValue (data.OldDomain.BeginOldPC, dest, dest, data, true);
			} else
				LoadStaticFieldEffect (field, dest, data);
			return data;
		}

		public Domain LoadStaticFieldAddress (APC pc, Field field, int dest, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, domain => it.LoadStaticFieldAddressEffect (field, dest, domain));
		}

		public Domain LoadTypeToken (APC pc, TypeNode type, int dest, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain LoadFieldToken (APC pc, Field type, int dest, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain LoadMethodToken (APC pc, Method type, int dest, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain NewArray<ArgList> (APC pc, TypeNode type, int dest, ArgList lengths, Domain data) where ArgList : IIndexable<int>
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.NewArrayEffect<ArgList> (type, dest, lengths, d));
		}

		public Domain NewObj<ArgList> (APC pc, Method ctor, int dest, ArgList args, Domain data) where ArgList : IIndexable<int>
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.NewObjEffect (pc, ctor, dest, args, d));
		}

		public Domain MkRefAny (APC pc, TypeNode type, int dest, int obj, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain RefAnyType (APC pc, int dest, int source, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain RefAnyVal (APC pc, TypeNode type, int dest, int source, Domain data)
		{
			throw new NotImplementedException ();
		}

		public Domain Rethrow (APC pc, Domain data)
		{
			return data.Bottom;
		}

		public Domain StoreElement (APC pc, TypeNode type, int array, int index, int value, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => it.StoreElementEffect (type, array, index, value, d));
		}

		public Domain StoreField (APC pc, Field field, int obj, int value, Domain data)
		{
			data.CopyValue (data.FieldAddress (data.Value (obj), field), data.Address (value));
			if (!MetaDataProvider.IsCompilerGenerated (field))
				data.HavocPseudoFields (data.Value (obj));
			data.Havoc (obj);
			data.Havoc (value);
			return data;
		}

		public Domain StoreStaticField (APC pc, Field field, int value, Domain data)
		{
			data.CopyValue (data.FieldAddress (data.Globals, field), data.Address (value));
			data.Havoc (value);
			return data;
		}

		public Domain Throw (APC pc, int exception, Domain data)
		{
			data.Havoc (exception);
			return data;
		}

		public Domain Unbox (APC pc, TypeNode type, int dest, int obj, Domain data)
		{
			AnalysisDecoder it = this;
			return DoWithBothDomains (data, d => d.AssignValue (dest, it.MetaDataProvider.ManagedPointer (type)));
		}

		public Domain UnboxAny (APC pc, TypeNode type, int dest, int obj, Domain data)
		{
			return DoWithBothDomains (data, d => d.AssignValue (dest, type));
		}
		#endregion

		private void MaterializeParameter (Parameter parameter, TypeNode declaringType, Domain data, bool aggressiveMaterialization)
		{
			TypeNode type = MetaDataProvider.ParameterType (parameter);
			data.Assign (parameter, type);
			MaterializeParameterInfo (data.Address (parameter), MetaDataProvider.ManagedPointer (type), 0, data, declaringType, aggressiveMaterialization);
			data.CopyParameterIntoShadow (parameter);
		}

		private void MaterializeParameterInfo (SymValue sv, TypeNode t, int depth, Domain data, TypeNode fromType, bool aggressiveMaterialization)
		{
			data.MakeUnmodified (sv);
			data.SetType (sv, t);
			if (depth > 2 && !aggressiveMaterialization || depth > 5)
				return;
			if (MetaDataProvider.IsManagedPointer (t)) {
				TypeNode elemType = MetaDataProvider.ElementType (t);
				if (!MetaDataProvider.HasValueRepresentation (elemType)) {
					data.ManifestStructId (sv);
					foreach (Field field in MetaDataProvider.Fields (elemType)) {
						if (!MetaDataProvider.IsStatic (field) && MetaDataProvider.IsVisibleFrom (field, fromType)) {
							TypeNode fieldAddressType;
							MaterializeParameterInfo (data.FieldAddress (sv, field, out fieldAddressType), fieldAddressType, depth + 1, data, fromType, aggressiveMaterialization);
						}
					}
					ManifestProperties (sv, depth + 1, data, fromType, aggressiveMaterialization, elemType);
				} else
					MaterializeParameterInfo (data.Value (sv), elemType, depth + 1, data, fromType, aggressiveMaterialization);
			} else {
				if (MetaDataProvider.IsClass (t)) {
					foreach (Field field in MetaDataProvider.Fields (t)) {
						if (!MetaDataProvider.IsStatic (field) && MetaDataProvider.IsVisibleFrom (field, fromType)) {
							TypeNode fieldAddressType;
							MaterializeParameterInfo (data.FieldAddress (sv, field, out fieldAddressType), fieldAddressType, depth + 1, data, fromType, aggressiveMaterialization);
						}
					}
				} else if (data.NeedsArrayLengthManifested (t))
					data.ManifestArrayLength (sv);
			}
		}

		private void ManifestProperties (SymValue sv, int depth, Domain data, TypeNode fromType, bool aggressiveMaterialization, TypeNode type)
		{
			foreach (Property property in MetaDataProvider.Properties (type)) {
				Method getter;
				if (MetaDataProvider.IsStatic (property) || !MetaDataProvider.HasGetter (property, out getter) || !MetaDataProvider.IsVisibleFrom (getter, fromType))
					continue;

				TypeNode pseudoFieldAddressType;
				MaterializeParameterInfo (data.PseudoFieldAddress (sv, getter, out pseudoFieldAddressType, false), pseudoFieldAddressType, depth + 1, data, fromType, aggressiveMaterialization);
			}

			if (!MetaDataProvider.IsInterface (type))
				return;

			foreach (TypeNode iface in MetaDataProvider.Interfaces (type)) {
				if (MetaDataProvider.IsVisibleFrom (iface, fromType))
					ManifestProperties (sv, depth, data, fromType, aggressiveMaterialization, iface);
			}
		}

		private void MaterializeLocal (Local local, Method method, Domain data)
		{
			TypeNode t = MetaDataProvider.LocalType (local);
			data.AssignZeroEquivalent (data.Address (local), t);
		}

		private void HavocParameters (APC pc, Method method, IIndexable<TypeNode> extraVarargs, IIndexable<int> args, Domain data, TypeNode declaringType, bool insideConstructor, bool thisArgMissing,
		                              bool derefThis)
		{
			IIndexable<Parameter> parameters = MetaDataProvider.Parameters (method);
			IIndexable<Parameter> unspecializedParameters = null;
			if (MetaDataProvider.IsSpecialized (method))
				unspecializedParameters = MetaDataProvider.Parameters (MetaDataProvider.Unspecialized (method));
			int index2 = 0;
			int num1 = 0;
			for (int index1 = 0; index1 < args.Count; ++index1) {
				bool materialize = false;
				bool nonFirstThis = false;
				int num2 = args [index1];
				bool parameterHasGenericType = false;
				bool havocReadonlyFields = false;
				TypeNode ype;

				if (index1 == 0 && !thisArgMissing && !MetaDataProvider.IsStatic (method)) {
					num1 = 1;
					ype = MetaDataProvider.ParameterType (MetaDataProvider.This (method));
					if (MetaDataProvider.IsPrimitive (declaringType) || MetaDataProvider.Equal (declaringType, MetaDataProvider.System_Object)) {
						if (MetaDataProvider.IsStruct (declaringType))
							data.Value (data.Value (args [0]));
						continue;
					}
					if (MetaDataProvider.IsConstructor (method)) {
						if (insideConstructor && data.IsThis (this.parent.CurrentMethod, num2)) {
							if (TypesEqualModuloInstantiation (declaringType, MetaDataProvider.DeclaringType (this.parent.CurrentMethod)))
								havocReadonlyFields = true;
							else
								continue;
						}
						if (MetaDataProvider.IsStruct (declaringType))
							materialize = true;
					}
				} else if (index2 < parameters.Count) {
					if (data.IsThis (this.parent.CurrentMethod, num2))
						nonFirstThis = true;
					Parameter p = parameters [index2];
					materialize = MetaDataProvider.IsOut (p);
					ype = MetaDataProvider.ParameterType (p);
					if (unspecializedParameters != null) {
						TypeNode unspecType = MetaDataProvider.ParameterType (unspecializedParameters [index2]);
						parameterHasGenericType = MetaDataProvider.IsFormalTypeParameter (unspecType) || MetaDataProvider.IsMethodFormalTypeParameter (unspecType);
					}
					++index2;
				} else if (!data.IsThis (this.parent.CurrentMethod, num2))
					ype = extraVarargs [index1 - index2 - num1];
				else
					continue;
				if (!pc.InsideContract) {
					bool havocFields = AggressiveUpHavocMethod (method);
					if (havocFields || materialize || MustHavocParameter (method, declaringType, ype, parameterHasGenericType, nonFirstThis)) {
						SymValue loc = data.Value (num2);
						if (derefThis && index1 == 0 && num1 == 1)
							loc = data.Value (loc);
						data.HavocObjectAtCall (loc, ref data.ModifiedAtCall, havocFields, havocReadonlyFields);
						if (materialize)
							data.MaterializeAccordingToType (loc, ype, 0);
					}
				}
				data.Havoc (num2);
			}
		}

		private bool MustHavocParameter (Method method, TypeNode declaringType, TypeNode pt, bool parameterHasGenericType, bool nonFirstThis)
		{
			if (parameterHasGenericType || MetaDataProvider.IsStruct (pt) || MetaDataProvider.Equal (declaringType, MetaDataProvider.System_Object) ||
			    IsImmutable (pt) || nonFirstThis || MetaDataProvider.Equal (pt, MetaDataProvider.System_Object))
				return false;

			return true;
		}

		private bool IsImmutable (TypeNode pt)
		{
			return MetaDataProvider.Equal (pt, MetaDataProvider.System_String);
		}

		private bool AggressiveUpHavocMethod (Method method)
		{
			if (MetaDataProvider.Name (MetaDataProvider.DeclaringType (method)) != "Monitor")
				return false;
			string name = MetaDataProvider.Name (method);

			return (name == "Exit" || name == "Wait");
		}

		private bool TypesEqualModuloInstantiation (TypeNode a, TypeNode b)
		{
			a = MetaDataProvider.Unspecialized (a);
			b = MetaDataProvider.Unspecialized (b);
			return MetaDataProvider.Equal (a, b);
		}

		private SymValue KeyForPureFunctionArgument (Method method, int index, int arg, Domain data, IImmutableMap<TypeNode, TypeNode> specialization, out bool isOut)
		{
			bool isByRef;
			TypeNode type;
			bool isPrimitive;
			if (!ParameterHasValueRepresentation (method, index, specialization, out isPrimitive, out isOut, out isByRef, out type))
				return data.StructId (isByRef ? data.Value (arg) : data.Address (arg));
			SymValue sv = data.Value (arg);
			if (isByRef)
				sv = data.Value (sv);
			return isPrimitive || IsImmutableType (type) ? sv : data.ObjectVersion (sv);
		}

		private bool ParameterHasValueRepresentation (Method method, int index, IImmutableMap<TypeNode, TypeNode> specialization, out bool isPrimitive, out bool isOut, out bool isByRef, out TypeNode type)
		{
			if (index == 0 && !MetaDataProvider.IsStatic (method)) {
				isOut = false;
				type = MetaDataProvider.DeclaringType (method);
				isPrimitive = MetaDataProvider.IsPrimitive (type);
				bool hasValueRepresentation = MetaDataProvider.HasValueRepresentation (type);
				isByRef = (isPrimitive || !hasValueRepresentation);
				return hasValueRepresentation;
			}

			int paramIndex = MetaDataProvider.IsStatic (method) ? index : (index - 1);
			Parameter p = MetaDataProvider.Parameters (method) [paramIndex];
			type = MetaDataProvider.ParameterType (p);
			type = Specialize (specialization, type);
			isOut = MetaDataProvider.IsOut (p);
			if (isByRef = MetaDataProvider.IsManagedPointer (type))
				type = MetaDataProvider.ElementType (type);

			isPrimitive = MetaDataProvider.IsPrimitive (type);
			return MetaDataProvider.HasValueRepresentation (type);
		}

		private bool IsImmutableType (TypeNode type)
		{
			//todo: implement
			return false;
		}

		private int GetNonOutArgs (Method method)
		{
			int res = MetaDataProvider.IsStatic (method) ? 1 : 0;
			IIndexable<Parameter> parameters = MetaDataProvider.Parameters (method);
			for (int i = 0; i < parameters.Count; ++i) {
				if (!MetaDataProvider.IsOut (parameters [i]))
					++res;
			}

			return res;
		}

		private void AssignAllOutParameters<ArgList> (Domain data, SymValue fieldAddr, Method method, ArgList args)
			where ArgList : IIndexable<int>
		{
			int index = 0;

			if (!MetaDataProvider.IsStatic (method))
				++index;
			IIndexable<Parameter> parameters = MetaDataProvider.Parameters (method);
			for (; index < parameters.Count; index++) {
				Parameter p = parameters [index];
				if (MetaDataProvider.IsOut (p)) {
					TypeNode pType = MetaDataProvider.ParameterType (p);
					SymValue srcAddr = data.PseudoFieldAddressOfOutParameter (index, fieldAddr, pType);
					SymValue destAddr = data.Value (args [index]);
					data.CopyValue (destAddr, srcAddr, pType);
				}
			}
		}

		private TypeNode Specialize (IImmutableMap<TypeNode, TypeNode> instantiationMap, TypeNode declaringType)
		{
			throw new NotImplementedException ();
		}

		private void DevirtualizeImplementingMethod (TypeNode declaringType, ref Method method)
		{
			throw new NotImplementedException ();
		}

		private IImmutableMap<TypeNode, TypeNode> ComputeTypeInstantiationMap (APC pc, Method method)
		{
			IImmutableMap<TypeNode, TypeNode> specialization = ImmutableMap<TypeNode, TypeNode>.Empty;
			foreach (var edge in pc.SubroutineContext.AsEnumerable ()) {
				Method calledMethod;
				bool isVirtual;
				bool isNewObj;
				if (edge.From.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual))
					MetaDataProvider.IsSpecialized (calledMethod, ref specialization);
				else if (edge.To.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual))
					MetaDataProvider.IsSpecialized (calledMethod, ref specialization);
			}
			return specialization;
		}

		private Domain CalliEffect<TypeList, ArgList> (APC pc, TypeNode returnType, TypeList argTypes, bool isInstance, int dest, int functionPointer, ArgList args, Domain data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			for (int i = 0; i < args.Count; i++) {
				int num = args [i];
				TypeNode type;
				if (isInstance) {
					if (i > 0)
						type = argTypes [i - 1];
					else {
						SymValue sv = data.Value (num);
						AbstractType aType = data.GetType (sv);
						type = aType.IsNormal() ? aType.ConcreteType : MetaDataProvider.System_Object;
					}
				} else
					type = argTypes [i];
				if (!MetaDataProvider.IsStruct (type)) {
					data.ResetModifiedAtCall ();
					data.HavocObjectAtCall (data.Value (num), ref data.ModifiedAtCall, false, false);
				}
				data.Havoc (num);
			}
			data.AssignValue (dest, returnType);
			return data;
		}

		private void LoadArgAddressEffect (Parameter p, bool isOld, int dest, Domain data)
		{
			SymValue srcAddr = isOld ? data.OldValueAddress (p) : data.Address (p);
			data.CopyAddress (data.Address (dest), srcAddr, MetaDataProvider.ManagedPointer (MetaDataProvider.ParameterType (p)));
		}

		private void LoadLocalAddressEffect (Local local, int dest, Domain data)
		{
			data.CopyAddress (data.Address (dest), data.Address (local), MetaDataProvider.ManagedPointer (MetaDataProvider.LocalType (local)));
		}

		private void BoxEffect (TypeNode type, int dest, int source, Domain data)
		{
			if (MetaDataProvider.IsReferenceType (type))
				data.Copy (dest, source);
			else {
				SymValue srcAddr = data.Address (source);
				TypeNode systemObject = MetaDataProvider.System_Object;
				SymValue specialUnary = data.GetSpecialUnary (data.Functions.BoxOperator, source, systemObject);
				data.SetValue (specialUnary, srcAddr, MetaDataProvider.ManagedPointer (type), false);
				data.AssignSpecialUnary (dest, specialUnary, systemObject);
			}
		}

		private void CastClassEffect (TypeNode type, int dest, int obj, Domain data)
		{
			SymValue destAddr = data.Address (dest);
			TypeNode mp = MetaDataProvider.ManagedPointer (type);
			data.CopyValueAndCast (destAddr, data.Address (obj), mp);
		}

		private void LoadElementEffect (TypeNode type, int dest, int array, int index, Domain data)
		{
			TypeNode t = type;
			if (!MetaDataProvider.IsStruct (type)) {
				SymValue sv = data.Value (array);
				AbstractType aType = data.GetType (sv);
				if (aType.IsNormal() && MetaDataProvider.IsArray (aType.ConcreteType)) {
					t = MetaDataProvider.ElementType (aType.ConcreteType);
					if (t == null)
						t = type;
				}
			}
			TypeNode elementAddressType = MetaDataProvider.ManagedPointer (t);
			data.CopyValue (data.Address (dest), data.ElementAddress (data.Value (array), data.Value (index), elementAddressType), elementAddressType);
			data.Havoc (index);
		}

		private void LoadFieldEffect (Field field, int dest, int obj, Domain data)
		{
			SymValue ptr;
			if (MetaDataProvider.IsStruct (MetaDataProvider.DeclaringType (field))) {
				AbstractType abstractType = data.GetType (data.Address (obj));
				ptr = abstractType.IsNormal()
				      && MetaDataProvider.IsManagedPointer (abstractType.ConcreteType)
				      && MetaDataProvider.IsStruct (MetaDataProvider.ElementType (abstractType.ConcreteType))
				      	? data.Address (obj)
				      	: data.Value (obj);
			} else
				ptr = data.Value (obj);
			TypeNode fieldAddressType;
			SymValue srcAddr = data.FieldAddress (ptr, field, out fieldAddressType);
			data.CopyValue (data.Address (dest), srcAddr, fieldAddressType);
		}

		private void LoadFieldAddressEffect (APC pc, Field field, int dest, int obj, Domain data)
		{
			SymValue sv = data.Value (obj);
			TypeNode fieldAddressType;
			SymValue srcAddr = data.FieldAddress (sv, field, out fieldAddressType);
			data.CopyAddress (data.Address (dest), srcAddr, fieldAddressType);
			if (!pc.InsideContract && MetaDataProvider.Equal (MetaDataProvider.DeclaringType (this.parent.CurrentMethod), MetaDataProvider.Unspecialized (MetaDataProvider.DeclaringType (field))))
				data.HavocPseudoFields (this.parent.StackContextProvider.MethodContext.AffectedGetters (field), sv);
		}

		private void LoadLengthEffect (int dest, int array, Domain data)
		{
			data.AssignArrayLength (data.Address (dest), data.Value (array));
		}

		private void LoadStaticFieldEffect (Field field, int dest, Domain data)
		{
			TypeNode fieldAddressType;
			SymValue srcAddr = data.FieldAddress (data.Globals, field, out fieldAddressType);
			data.CopyValue (data.Address (dest), srcAddr, fieldAddressType);
		}

		private void LoadStaticFieldAddressEffect (Field field, int dest, Domain data)
		{
			TypeNode fieldAddressType;
			SymValue srcAddr = data.FieldAddress (data.Globals, field, out fieldAddressType);
			data.CopyAddress (data.Address (dest), srcAddr, fieldAddressType);
		}

		private void NewArrayEffect<T> (TypeNode type, int dest, IIndexable<int> list, Domain data)
		{
			if (list.Count == 1) {
				SymValue array = data.CreateArray (type, data.Value (list [0]));
				data.CopyAddress (data.Address (dest), array, MetaDataProvider.ArrayType (type, 1));
			} else
				data.AssignValue (dest, type);
		}

		private void NewObjEffect<ArgList> (APC pc, Method ctor, int dest, ArgList args, Domain data)
			where ArgList : IIndexable<int>
		{
			for (int i = 0; i < args.Count; ++i)
				data.Havoc (args [i]);

			TypeNode declaringType = MetaDataProvider.DeclaringType (ctor);
			if (MetaDataProvider.IsStruct (declaringType)) {
				SymValue srcAddr = data.CreateValue (declaringType);
				data.MaterializeAccordingToType (srcAddr, MetaDataProvider.ManagedPointer (declaringType), 0);
				HavocParameters (pc, ctor, Indexable<TypeNode>.Empty, args, data, declaringType, false, true, false);
				data.CopyValue (data.Address (dest), srcAddr, MetaDataProvider.ManagedPointer (declaringType));
				return;
			}

			SymValue sv = data.CreateObject (declaringType);
			HavocParameters (pc, ctor, Indexable<TypeNode>.Empty, args, data, declaringType, false, true, false);
			data.CopyAddress (data.Address (dest), sv, declaringType);
		}

		private void StoreElementEffect (TypeNode type, int array, int index, int value, Domain data)
		{
			TypeNode elementAddressType = MetaDataProvider.ManagedPointer (type);
			SymValue arrayValue = data.Value (array);
			SymValue indexValue = data.Value (index);
			data.HavocArrayAtIndex (arrayValue, indexValue);
			data.CopyValue (data.ElementAddress (arrayValue, indexValue, elementAddressType), data.Address (value), elementAddressType);
			data.Havoc (array);
			data.Havoc (index);
			data.Havoc (value);
		}

		public static Domain FindOldState (APC pc, Domain data)
		{
			for (Sequence<Edge<CFGBlock, EdgeTag>> list = pc.SubroutineContext; list != null; list = list.Tail) {
				Edge<CFGBlock, EdgeTag> pair = list.Head;
				if (pair.Tag == EdgeTag.Exit || pair.Tag.Is (EdgeTag.AfterMask))
					return data.GetStateAt (new APC (pair.From.Subroutine.EntryAfterRequires, 0, list.Tail));
			}
			return null;
		}
	}
}
