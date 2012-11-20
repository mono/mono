/*
  Copyright (C) 2008-2012 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Diagnostics;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public struct Label
	{
		// 1-based here, to make sure that an uninitialized Label isn't valid
		private readonly int index1;

		internal Label(int index)
		{
			this.index1 = index + 1;
		}

		internal int Index
		{
			get { return index1 - 1; }
		}

		public bool Equals(Label other)
		{
			return other.index1 == index1;
		}

		public override bool Equals(object obj)
		{
			return this == obj as Label?;
		}

		public override int GetHashCode()
		{
			return index1;
		}

		public static bool operator ==(Label arg1, Label arg2)
		{
			return arg1.index1 == arg2.index1;
		}

		public static bool operator !=(Label arg1, Label arg2)
		{
			return !(arg1 == arg2);
		}
	}

	public sealed class LocalBuilder : LocalVariableInfo
	{
		internal string name;
		internal int startOffset;
		internal int endOffset;

		internal LocalBuilder(Type localType, int index, bool pinned)
			: base(index, localType, pinned)
		{
		}

		internal LocalBuilder(Type localType, int index, bool pinned, CustomModifiers customModifiers)
			: base(index, localType, pinned, customModifiers)
		{
		}

		public void SetLocalSymInfo(string name)
		{
			this.name = name;
		}

		public void SetLocalSymInfo(string name, int startOffset, int endOffset)
		{
			this.name = name;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
		}
	}

	public sealed class ILGenerator
	{
		private readonly ModuleBuilder moduleBuilder;
		private readonly ByteBuffer code;
		private readonly SignatureHelper locals;
		private int localsCount;
		private readonly List<int> tokenFixups = new List<int>();
		private readonly List<int> labels = new List<int>();
		private readonly List<int> labelStackHeight = new List<int>();
		private readonly List<LabelFixup> labelFixups = new List<LabelFixup>();
		private readonly List<SequencePoint> sequencePoints = new List<SequencePoint>();
		private readonly List<ExceptionBlock> exceptions = new List<ExceptionBlock>();
		private readonly Stack<ExceptionBlock> exceptionStack = new Stack<ExceptionBlock>();
		private ushort maxStack;
		private bool fatHeader;
		private int stackHeight;
		private Scope scope;
		private byte exceptionBlockAssistanceMode = EBAM_COMPAT;
		private const byte EBAM_COMPAT = 0;
		private const byte EBAM_DISABLE = 1;
		private const byte EBAM_CLEVER = 2;

		private struct LabelFixup
		{
			internal int label;
			internal int offset;
		}

		internal sealed class ExceptionBlock : IComparer<ExceptionBlock>
		{
			internal readonly int ordinal;
			internal Label labelEnd;
			internal int tryOffset;
			internal int tryLength;
			internal int handlerOffset;
			internal int handlerLength;
			internal int filterOffsetOrExceptionTypeToken;
			internal ExceptionHandlingClauseOptions kind;

			internal ExceptionBlock(int ordinal)
			{
				this.ordinal = ordinal;
			}

			internal ExceptionBlock(ExceptionHandler h)
			{
				this.ordinal = -1;
				this.tryOffset = h.TryOffset;
				this.tryLength = h.TryLength;
				this.handlerOffset = h.HandlerOffset;
				this.handlerLength = h.HandlerLength;
				this.kind = h.Kind;
				this.filterOffsetOrExceptionTypeToken = kind == ExceptionHandlingClauseOptions.Filter ? h.FilterOffset : h.ExceptionTypeToken;
			}

			int IComparer<ExceptionBlock>.Compare(ExceptionBlock x, ExceptionBlock y)
			{
				// Mono's sort insists on doing unnecessary comparisons
				if (x == y)
				{
					return 0;
				}
				else if (x.tryOffset == y.tryOffset && x.tryLength == y.tryLength)
				{
					return x.ordinal < y.ordinal ? -1 : 1;
				}
				else if (x.tryOffset >= y.tryOffset && x.handlerOffset + x.handlerLength <= y.handlerOffset + y.handlerLength)
				{
					return -1;
				}
				else if (y.tryOffset >= x.tryOffset && y.handlerOffset + y.handlerLength <= x.handlerOffset + x.handlerLength)
				{
					return 1;
				}
				else
				{
					return x.ordinal < y.ordinal ? -1 : 1;
				}
			}
		}

		private struct SequencePoint
		{
			internal ISymbolDocumentWriter document;
			internal int offset;
			internal int startLine;
			internal int startColumn;
			internal int endLine;
			internal int endColumn;
		}

		private sealed class Scope
		{
			internal readonly Scope parent;
			internal readonly List<Scope> children = new List<Scope>();
			internal readonly List<LocalBuilder> locals = new List<LocalBuilder>();
			internal int startOffset;
			internal int endOffset;

			internal Scope(Scope parent)
			{
				this.parent = parent;
			}
		}

		internal ILGenerator(ModuleBuilder moduleBuilder, int initialCapacity)
		{
			this.code = new ByteBuffer(initialCapacity);
			this.moduleBuilder = moduleBuilder;
			this.locals = SignatureHelper.GetLocalVarSigHelper(moduleBuilder);
			if (moduleBuilder.symbolWriter != null)
			{
				scope = new Scope(null);
			}
		}

		// non-standard API
		public void __DisableExceptionBlockAssistance()
		{
			exceptionBlockAssistanceMode = EBAM_DISABLE;
		}

		// non-standard API
		public void __CleverExceptionBlockAssistance()
		{
			exceptionBlockAssistanceMode = EBAM_CLEVER;
		}

		// non-standard API
		public int __MaxStackSize
		{
			get { return maxStack; }
			set
			{
				maxStack = (ushort)value;
				fatHeader = true;
			}
		}

		// non-standard API
		// returns -1 if the current position is currently unreachable
		public int __StackHeight
		{
			get { return stackHeight; }
		}

		// new in .NET 4.0
		public int ILOffset
		{
			get { return code.Position; }
		}

		public void BeginCatchBlock(Type exceptionType)
		{
			if (exceptionType == null)
			{
				// this must be a catch block after a filter
				ExceptionBlock block = exceptionStack.Peek();
				if (block.kind != ExceptionHandlingClauseOptions.Filter || block.handlerOffset != 0)
				{
					throw new ArgumentNullException("exceptionType");
				}
				if (exceptionBlockAssistanceMode == EBAM_COMPAT || (exceptionBlockAssistanceMode == EBAM_CLEVER && stackHeight != -1))
				{
					Emit(OpCodes.Endfilter);
				}
				stackHeight = 0;
				UpdateStack(1);
				block.handlerOffset = code.Position;
			}
			else
			{
				ExceptionBlock block = BeginCatchOrFilterBlock();
				block.kind = ExceptionHandlingClauseOptions.Clause;
				block.filterOffsetOrExceptionTypeToken = moduleBuilder.GetTypeTokenForMemberRef(exceptionType);
				block.handlerOffset = code.Position;
			}
		}

		private ExceptionBlock BeginCatchOrFilterBlock()
		{
			ExceptionBlock block = exceptionStack.Peek();
			if (exceptionBlockAssistanceMode == EBAM_COMPAT || (exceptionBlockAssistanceMode == EBAM_CLEVER && stackHeight != -1))
			{
				Emit(OpCodes.Leave, block.labelEnd);
			}
			stackHeight = 0;
			UpdateStack(1);
			if (block.tryLength == 0)
			{
				block.tryLength = code.Position - block.tryOffset;
			}
			else
			{
				block.handlerLength = code.Position - block.handlerOffset;
				exceptionStack.Pop();
				ExceptionBlock newBlock = new ExceptionBlock(exceptions.Count);
				newBlock.labelEnd = block.labelEnd;
				newBlock.tryOffset = block.tryOffset;
				newBlock.tryLength = block.tryLength;
				block = newBlock;
				exceptions.Add(block);
				exceptionStack.Push(block);
			}
			return block;
		}

		public Label BeginExceptionBlock()
		{
			ExceptionBlock block = new ExceptionBlock(exceptions.Count);
			block.labelEnd = DefineLabel();
			block.tryOffset = code.Position;
			exceptionStack.Push(block);
			exceptions.Add(block);
			stackHeight = 0;
			return block.labelEnd;
		}

		public void BeginExceptFilterBlock()
		{
			ExceptionBlock block = BeginCatchOrFilterBlock();
			block.kind = ExceptionHandlingClauseOptions.Filter;
			block.filterOffsetOrExceptionTypeToken = code.Position;
		}

		public void BeginFaultBlock()
		{
			BeginFinallyFaultBlock(ExceptionHandlingClauseOptions.Fault);
		}

		public void BeginFinallyBlock()
		{
			BeginFinallyFaultBlock(ExceptionHandlingClauseOptions.Finally);
		}

		private void BeginFinallyFaultBlock(ExceptionHandlingClauseOptions kind)
		{
			ExceptionBlock block = exceptionStack.Peek();
			if (exceptionBlockAssistanceMode == EBAM_COMPAT || (exceptionBlockAssistanceMode == EBAM_CLEVER && stackHeight != -1))
			{
				Emit(OpCodes.Leave, block.labelEnd);
			}
			if (block.handlerOffset == 0)
			{
				block.tryLength = code.Position - block.tryOffset;
			}
			else
			{
				block.handlerLength = code.Position - block.handlerOffset;
				Label labelEnd;
				if (exceptionBlockAssistanceMode != EBAM_COMPAT)
				{
					labelEnd = block.labelEnd;
				}
				else
				{
					MarkLabel(block.labelEnd);
					labelEnd = DefineLabel();
					Emit(OpCodes.Leave, labelEnd);
				}
				exceptionStack.Pop();
				ExceptionBlock newBlock = new ExceptionBlock(exceptions.Count);
				newBlock.labelEnd = labelEnd;
				newBlock.tryOffset = block.tryOffset;
				newBlock.tryLength = code.Position - block.tryOffset;
				block = newBlock;
				exceptions.Add(block);
				exceptionStack.Push(block);
			}
			block.handlerOffset = code.Position;
			block.kind = kind;
			stackHeight = 0;
		}

		public void EndExceptionBlock()
		{
			ExceptionBlock block = exceptionStack.Pop();
			if (exceptionBlockAssistanceMode == EBAM_COMPAT || (exceptionBlockAssistanceMode == EBAM_CLEVER && stackHeight != -1))
			{
				if (block.kind != ExceptionHandlingClauseOptions.Finally && block.kind != ExceptionHandlingClauseOptions.Fault)
				{
					Emit(OpCodes.Leave, block.labelEnd);
				}
				else
				{
					Emit(OpCodes.Endfinally);
				}
			}
			MarkLabel(block.labelEnd);
			block.handlerLength = code.Position - block.handlerOffset;
		}

		public void BeginScope()
		{
			Scope newScope = new Scope(scope);
			scope.children.Add(newScope);
			scope = newScope;
			scope.startOffset = code.Position;
		}

		public void UsingNamespace(string usingNamespace)
		{
			if (moduleBuilder.symbolWriter != null)
			{
				moduleBuilder.symbolWriter.UsingNamespace(usingNamespace);
			}
		}

		public LocalBuilder DeclareLocal(Type localType)
		{
			return DeclareLocal(localType, false);
		}

		public LocalBuilder DeclareLocal(Type localType, bool pinned)
		{
			LocalBuilder local = new LocalBuilder(localType, localsCount++, pinned);
			locals.AddArgument(localType, pinned);
			if (scope != null)
			{
				scope.locals.Add(local);
			}
			return local;
		}

		public LocalBuilder __DeclareLocal(Type localType, bool pinned, CustomModifiers customModifiers)
		{
			LocalBuilder local = new LocalBuilder(localType, localsCount++, pinned, customModifiers);
			locals.__AddArgument(localType, pinned, customModifiers);
			if (scope != null)
			{
				scope.locals.Add(local);
			}
			return local;
		}

		public Label DefineLabel()
		{
			Label label = new Label(labels.Count);
			labels.Add(-1);
			labelStackHeight.Add(-1);
			return label;
		}

		public void Emit(OpCode opc)
		{
			Debug.Assert(opc != OpCodes.Ret || (opc == OpCodes.Ret && stackHeight <= 1));
			if (opc.Value < 0)
			{
				code.Write((byte)(opc.Value >> 8));
			}
			code.Write((byte)opc.Value);
			switch (opc.FlowControl)
			{
				case FlowControl.Branch:
				case FlowControl.Break:
				case FlowControl.Return:
				case FlowControl.Throw:
					stackHeight = -1;
					break;
				default:
					UpdateStack(opc.StackDiff);
					break;
			}
		}

		private void UpdateStack(int stackdiff)
		{
			if (stackHeight == -1)
			{
				// we're about to emit code that is either unreachable or reachable only via a backward branch
				stackHeight = 0;
			}
			Debug.Assert(stackHeight >= 0 && stackHeight <= ushort.MaxValue);
			stackHeight += stackdiff;
			Debug.Assert(stackHeight >= 0 && stackHeight <= ushort.MaxValue);
			maxStack = Math.Max(maxStack, (ushort)stackHeight);
		}

		public void Emit(OpCode opc, byte arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, double arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, FieldInfo field)
		{
			Emit(opc);
			WriteToken(moduleBuilder.GetFieldToken(field).Token);
		}

		public void Emit(OpCode opc, short arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, int arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, long arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, Label label)
		{
			// We need special stackHeight handling for unconditional branches,
			// because the branch and next flows have differing stack heights.
			// Note that this assumes that unconditional branches do not push/pop.
			int flowStackHeight = this.stackHeight;
			Emit(opc);
			if (opc == OpCodes.Leave || opc == OpCodes.Leave_S)
			{
				flowStackHeight = 0;
			}
			else if (opc.FlowControl != FlowControl.Branch)
			{
				flowStackHeight = this.stackHeight;
			}
			// if the label has already been marked, we can emit the branch offset directly
			if (labels[label.Index] != -1)
			{
				if (labelStackHeight[label.Index] != flowStackHeight && (labelStackHeight[label.Index] != 0 || flowStackHeight != -1))
				{
					// the "backward branch constraint" prohibits this, so we don't need to support it
					throw new NotSupportedException("'Backward branch constraints' violated");
				}
				if (opc.OperandType == OperandType.ShortInlineBrTarget)
				{
					WriteByteBranchOffset(labels[label.Index] - (code.Position + 1));
				}
				else
				{
					code.Write(labels[label.Index] - (code.Position + 4));
				}
			}
			else
			{
				Debug.Assert(labelStackHeight[label.Index] == -1 || labelStackHeight[label.Index] == flowStackHeight || (flowStackHeight == -1 && labelStackHeight[label.Index] == 0));
				labelStackHeight[label.Index] = flowStackHeight;
				LabelFixup fix = new LabelFixup();
				fix.label = label.Index;
				fix.offset = code.Position;
				labelFixups.Add(fix);
				if (opc.OperandType == OperandType.ShortInlineBrTarget)
				{
					code.Write((byte)1);
				}
				else
				{
					code.Write(4);
				}
			}
		}

		private void WriteByteBranchOffset(int offset)
		{
			if (offset < -128 || offset > 127)
			{
				throw new NotSupportedException("Branch offset of " + offset + " does not fit in one-byte branch target at position " + code.Position);
			}
			code.Write((byte)offset);
		}

		public void Emit(OpCode opc, Label[] labels)
		{
			Emit(opc);
			LabelFixup fix = new LabelFixup();
			fix.label = -1;
			fix.offset = code.Position;
			labelFixups.Add(fix);
			code.Write(labels.Length);
			foreach (Label label in labels)
			{
				code.Write(label.Index);
				if (this.labels[label.Index] != -1)
				{
					if (labelStackHeight[label.Index] != stackHeight)
					{
						// the "backward branch constraint" prohibits this, so we don't need to support it
						throw new NotSupportedException();
					}
				}
				else
				{
					Debug.Assert(labelStackHeight[label.Index] == -1 || labelStackHeight[label.Index] == stackHeight);
					labelStackHeight[label.Index] = stackHeight;
				}
			}
		}

		public void Emit(OpCode opc, LocalBuilder local)
		{
			if ((opc == OpCodes.Ldloc || opc == OpCodes.Ldloca || opc == OpCodes.Stloc) && local.LocalIndex < 256)
			{
				if (opc == OpCodes.Ldloc)
				{
					switch (local.LocalIndex)
					{
						case 0:
							Emit(OpCodes.Ldloc_0);
							break;
						case 1:
							Emit(OpCodes.Ldloc_1);
							break;
						case 2:
							Emit(OpCodes.Ldloc_2);
							break;
						case 3:
							Emit(OpCodes.Ldloc_3);
							break;
						default:
							Emit(OpCodes.Ldloc_S);
							code.Write((byte)local.LocalIndex);
							break;
					}
				}
				else if (opc == OpCodes.Ldloca)
				{
					Emit(OpCodes.Ldloca_S);
					code.Write((byte)local.LocalIndex);
				}
				else if (opc == OpCodes.Stloc)
				{
					switch (local.LocalIndex)
					{
						case 0:
							Emit(OpCodes.Stloc_0);
							break;
						case 1:
							Emit(OpCodes.Stloc_1);
							break;
						case 2:
							Emit(OpCodes.Stloc_2);
							break;
						case 3:
							Emit(OpCodes.Stloc_3);
							break;
						default:
							Emit(OpCodes.Stloc_S);
							code.Write((byte)local.LocalIndex);
							break;
					}
				}
			}
			else
			{
				Emit(opc);
				switch (opc.OperandType)
				{
					case OperandType.InlineVar:
						code.Write((ushort)local.LocalIndex);
						break;
					case OperandType.ShortInlineVar:
						code.Write((byte)local.LocalIndex);
						break;
				}
			}
		}

		private void WriteToken(int token)
		{
			if (ModuleBuilder.IsPseudoToken(token))
			{
				tokenFixups.Add(code.Position);
			}
			code.Write(token);
		}

		private void UpdateStack(OpCode opc, bool hasthis, Type returnType, int parameterCount)
		{
			if (opc == OpCodes.Jmp)
			{
				stackHeight = -1;
			}
			else if (opc.FlowControl == FlowControl.Call)
			{
				int stackdiff = 0;
				if ((hasthis && opc != OpCodes.Newobj) || opc == OpCodes.Calli)
				{
					// pop this
					stackdiff--;
				}
				// pop parameters
				stackdiff -= parameterCount;
				if (returnType != moduleBuilder.universe.System_Void)
				{
					// push return value
					stackdiff++;
				}
				UpdateStack(stackdiff);
			}
		}

		public void Emit(OpCode opc, MethodInfo method)
		{
			UpdateStack(opc, method.HasThis, method.ReturnType, method.ParameterCount);
			Emit(opc);
			WriteToken(moduleBuilder.GetMethodTokenForIL(method).Token);
		}

		public void Emit(OpCode opc, ConstructorInfo constructor)
		{
			Emit(opc, constructor.GetMethodInfo());
		}

		public void Emit(OpCode opc, sbyte arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, float arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, string str)
		{
			Emit(opc);
			code.Write(moduleBuilder.GetStringConstant(str).Token);
		}

		public void Emit(OpCode opc, Type type)
		{
			Emit(opc);
			if (opc == OpCodes.Ldtoken)
			{
				code.Write(moduleBuilder.GetTypeToken(type).Token);
			}
			else
			{
				code.Write(moduleBuilder.GetTypeTokenForMemberRef(type));
			}
		}

		public void Emit(OpCode opcode, SignatureHelper signature)
		{
			Emit(opcode);
			UpdateStack(opcode, signature.HasThis, signature.ReturnType, signature.ParameterCount);
			code.Write(moduleBuilder.GetSignatureToken(signature).Token);
		}

		public void EmitCall(OpCode opc, MethodInfo method, Type[] optionalParameterTypes)
		{
			__EmitCall(opc, method, optionalParameterTypes, null);
		}

		public void __EmitCall(OpCode opc, MethodInfo method, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			if (optionalParameterTypes == null || optionalParameterTypes.Length == 0)
			{
				Emit(opc, method);
			}
			else
			{
				Emit(opc);
				UpdateStack(opc, method.HasThis, method.ReturnType, method.ParameterCount + optionalParameterTypes.Length);
				code.Write(moduleBuilder.__GetMethodToken(method, optionalParameterTypes, customModifiers).Token);
			}
		}

		public void __EmitCall(OpCode opc, ConstructorInfo constructor, Type[] optionalParameterTypes)
		{
			EmitCall(opc, constructor.GetMethodInfo(), optionalParameterTypes);
		}

		public void __EmitCall(OpCode opc, ConstructorInfo constructor, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			__EmitCall(opc, constructor.GetMethodInfo(), optionalParameterTypes, customModifiers);
		}

		public void EmitCalli(OpCode opc, CallingConvention callingConvention, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper sig = SignatureHelper.GetMethodSigHelper(moduleBuilder, callingConvention, returnType);
			sig.AddArguments(parameterTypes, null, null);
			Emit(opc, sig);
		}

		public void EmitCalli(OpCode opc, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			SignatureHelper sig = SignatureHelper.GetMethodSigHelper(moduleBuilder, callingConvention, returnType);
			sig.AddArguments(parameterTypes, null, null);
			sig.AddSentinel();
			sig.AddArguments(optionalParameterTypes, null, null);
			Emit(opc, sig);
		}

		public void __EmitCalli(OpCode opc, __StandAloneMethodSig sig)
		{
			Emit(opc);
			if (sig.IsUnmanaged)
			{
				UpdateStack(opc, false, sig.ReturnType, sig.ParameterCount);
			}
			else
			{
				CallingConventions callingConvention = sig.CallingConvention;
				UpdateStack(opc, (callingConvention & CallingConventions.HasThis | CallingConventions.ExplicitThis) == CallingConventions.HasThis, sig.ReturnType, sig.ParameterCount);
			}
			ByteBuffer bb = new ByteBuffer(16);
			Signature.WriteStandAloneMethodSig(moduleBuilder, bb, sig);
			code.Write(0x11000000 | moduleBuilder.StandAloneSig.FindOrAddRecord(moduleBuilder.Blobs.Add(bb)));
		}

		public void EmitWriteLine(string text)
		{
			Universe u = moduleBuilder.universe;
			Emit(OpCodes.Ldstr, text);
			Emit(OpCodes.Call, u.Import(typeof(Console)).GetMethod("WriteLine", new Type[] { u.System_String }));
		}

		public void EmitWriteLine(FieldInfo field)
		{
			Universe u = moduleBuilder.universe;
			Emit(OpCodes.Call, u.Import(typeof(Console)).GetMethod("get_Out"));
			if (field.IsStatic)
			{
				Emit(OpCodes.Ldsfld, field);
			}
			else
			{
				Emit(OpCodes.Ldarg_0);
				Emit(OpCodes.Ldfld, field);
			}
			Emit(OpCodes.Callvirt, u.Import(typeof(System.IO.TextWriter)).GetMethod("WriteLine", new Type[] { field.FieldType }));
		}

		public void EmitWriteLine(LocalBuilder local)
		{
			Universe u = moduleBuilder.universe;
			Emit(OpCodes.Call, u.Import(typeof(Console)).GetMethod("get_Out"));
			Emit(OpCodes.Ldloc, local);
			Emit(OpCodes.Callvirt, u.Import(typeof(System.IO.TextWriter)).GetMethod("WriteLine", new Type[] { local.LocalType }));
		}

		public void EndScope()
		{
			scope.endOffset = code.Position;
			scope = scope.parent;
		}

		public void MarkLabel(Label loc)
		{
			Debug.Assert(stackHeight == -1 || labelStackHeight[loc.Index] == -1 || stackHeight == labelStackHeight[loc.Index]);
			labels[loc.Index] = code.Position;
			if (labelStackHeight[loc.Index] == -1)
			{
				if (stackHeight == -1)
				{
					// We're at a location that can only be reached by a backward branch,
					// so according to the "backward branch constraint" that must mean the stack is empty,
					// but note that this may be an unused label followed by another label that is used and
					// that does have a non-zero stack height, so we don't yet set stackHeight here.
					labelStackHeight[loc.Index] = 0;
				}
				else
				{
					labelStackHeight[loc.Index] = stackHeight;
				}
			}
			else
			{
				Debug.Assert(stackHeight == -1 || stackHeight == labelStackHeight[loc.Index]);
				stackHeight = labelStackHeight[loc.Index];
			}
		}

		public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
		{
			SequencePoint sp = new SequencePoint();
			sp.document = document;
			sp.offset = code.Position;
			sp.startLine = startLine;
			sp.startColumn = startColumn;
			sp.endLine = endLine;
			sp.endColumn = endColumn;
			sequencePoints.Add(sp);
		}

		public void ThrowException(Type excType)
		{
			Emit(OpCodes.Newobj, excType.GetConstructor(Type.EmptyTypes));
			Emit(OpCodes.Throw);
		}

		internal int WriteBody(bool initLocals)
		{
			if (moduleBuilder.symbolWriter != null)
			{
				Debug.Assert(scope != null && scope.parent == null);
				scope.endOffset = code.Position;
			}

			ResolveBranches();

			ByteBuffer bb = moduleBuilder.methodBodies;

			int localVarSigTok = 0;

			int rva;
			if (localsCount == 0 && exceptions.Count == 0 && maxStack <= 8 && code.Length < 64 && !fatHeader)
			{
				rva = WriteTinyHeaderAndCode(bb);
			}
			else
			{
				if (localsCount != 0)
				{
					localVarSigTok = moduleBuilder.GetSignatureToken(locals).Token;
				}
				rva = WriteFatHeaderAndCode(bb, localVarSigTok, initLocals);
			}

			if (moduleBuilder.symbolWriter != null)
			{
				if (sequencePoints.Count != 0)
				{
					ISymbolDocumentWriter document = sequencePoints[0].document;
					int[] offsets = new int[sequencePoints.Count];
					int[] lines = new int[sequencePoints.Count];
					int[] columns = new int[sequencePoints.Count];
					int[] endLines = new int[sequencePoints.Count];
					int[] endColumns = new int[sequencePoints.Count];
					for (int i = 0; i < sequencePoints.Count; i++)
					{
						if (sequencePoints[i].document != document)
						{
							throw new NotImplementedException();
						}
						offsets[i] = sequencePoints[i].offset;
						lines[i] = sequencePoints[i].startLine;
						columns[i] = sequencePoints[i].startColumn;
						endLines[i] = sequencePoints[i].endLine;
						endColumns[i] = sequencePoints[i].endColumn;
					}
					moduleBuilder.symbolWriter.DefineSequencePoints(document, offsets, lines, columns, endLines, endColumns);
				}

				WriteScope(scope, localVarSigTok);
			}
			return rva;
		}

		private void ResolveBranches()
		{
			foreach (LabelFixup fixup in labelFixups)
			{
				// is it a switch?
				if (fixup.label == -1)
				{
					code.Position = fixup.offset;
					int count = code.GetInt32AtCurrentPosition();
					int offset = fixup.offset + 4 + 4 * count;
					code.Position += 4;
					for (int i = 0; i < count; i++)
					{
						int index = code.GetInt32AtCurrentPosition();
						code.Write(labels[index] - offset);
					}
				}
				else
				{
					code.Position = fixup.offset;
					byte size = code.GetByteAtCurrentPosition();
					int branchOffset = labels[fixup.label] - (code.Position + size);
					if (size == 1)
					{
						WriteByteBranchOffset(branchOffset);
					}
					else
					{
						code.Write(branchOffset);
					}
				}
			}
		}

		internal static void WriteTinyHeader(ByteBuffer bb, int length)
		{
			const byte CorILMethod_TinyFormat = 0x2;
			bb.Write((byte)(CorILMethod_TinyFormat | (length << 2)));
		}

		private int WriteTinyHeaderAndCode(ByteBuffer bb)
		{
			int rva = bb.Position;
			WriteTinyHeader(bb, code.Length);
			AddTokenFixups(bb.Position, moduleBuilder.tokenFixupOffsets, tokenFixups);
			bb.Write(code);
			return rva;
		}

		internal static void WriteFatHeader(ByteBuffer bb, bool initLocals, bool exceptions, ushort maxStack, int codeLength, int localVarSigTok)
		{
			const byte CorILMethod_FatFormat = 0x03;
			const byte CorILMethod_MoreSects = 0x08;
			const byte CorILMethod_InitLocals = 0x10;

			short flagsAndSize = (short)(CorILMethod_FatFormat | (3 << 12));
			if (initLocals)
			{
				flagsAndSize |= CorILMethod_InitLocals;
			}

			if (exceptions)
			{
				flagsAndSize |= CorILMethod_MoreSects;
			}

			bb.Write(flagsAndSize);
			bb.Write(maxStack);
			bb.Write(codeLength);
			bb.Write(localVarSigTok);
		}

		private int WriteFatHeaderAndCode(ByteBuffer bb, int localVarSigTok, bool initLocals)
		{
			// fat headers require 4-byte alignment
			bb.Align(4);
			int rva = bb.Position;
			WriteFatHeader(bb, initLocals, exceptions.Count > 0, maxStack, code.Length, localVarSigTok);
			AddTokenFixups(bb.Position, moduleBuilder.tokenFixupOffsets, tokenFixups);
			bb.Write(code);
			if (exceptions.Count > 0)
			{
				exceptions.Sort(exceptions[0]);
				WriteExceptionHandlers(bb, exceptions);
			}
			return rva;
		}

		internal static void WriteExceptionHandlers(ByteBuffer bb, List<ExceptionBlock> exceptions)
		{
			bb.Align(4);

			bool fat = false;
			if (exceptions.Count * 12 + 4 > 255)
			{
				fat = true;
			}
			else
			{
				foreach (ExceptionBlock block in exceptions)
				{
					if (block.tryOffset > 65535 || block.tryLength > 255 || block.handlerOffset > 65535 || block.handlerLength > 255)
					{
						fat = true;
						break;
					}
				}
			}

			const byte CorILMethod_Sect_EHTable = 0x1;
			const byte CorILMethod_Sect_FatFormat = 0x40;

			if (fat)
			{
				bb.Write((byte)(CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat));
				int dataSize = exceptions.Count * 24 + 4;
				bb.Write((byte)dataSize);
				bb.Write((short)(dataSize >> 8));
				foreach (ExceptionBlock block in exceptions)
				{
					bb.Write((int)block.kind);
					bb.Write(block.tryOffset);
					bb.Write(block.tryLength);
					bb.Write(block.handlerOffset);
					bb.Write(block.handlerLength);
					bb.Write(block.filterOffsetOrExceptionTypeToken);
				}
			}
			else
			{
				bb.Write(CorILMethod_Sect_EHTable);
				bb.Write((byte)(exceptions.Count * 12 + 4));
				bb.Write((short)0);
				foreach (ExceptionBlock block in exceptions)
				{
					bb.Write((short)block.kind);
					bb.Write((short)block.tryOffset);
					bb.Write((byte)block.tryLength);
					bb.Write((short)block.handlerOffset);
					bb.Write((byte)block.handlerLength);
					bb.Write(block.filterOffsetOrExceptionTypeToken);
				}
			}
		}

		internal static void AddTokenFixups(int codeOffset, List<int> tokenFixupOffsets, IEnumerable<int> tokenFixups)
		{
			foreach (int fixup in tokenFixups)
			{
				tokenFixupOffsets.Add(fixup + codeOffset);
			}
		}

		private void WriteScope(Scope scope, int localVarSigTok)
		{
			moduleBuilder.symbolWriter.OpenScope(scope.startOffset);
			foreach (LocalBuilder local in scope.locals)
			{
				if (local.name != null)
				{
					int startOffset = local.startOffset;
					int endOffset = local.endOffset;
					if (startOffset == 0 && endOffset == 0)
					{
						startOffset = scope.startOffset;
						endOffset = scope.endOffset;
					}
					moduleBuilder.symbolWriter.DefineLocalVariable2(local.name, 0, localVarSigTok, SymAddressKind.ILOffset, local.LocalIndex, 0, 0, startOffset, endOffset);
				}
			}
			foreach (Scope child in scope.children)
			{
				WriteScope(child, localVarSigTok);
			}
			moduleBuilder.symbolWriter.CloseScope(scope.endOffset);
		}
	}
}
