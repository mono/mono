/*
  Copyright (C) 2008-2010 Jeroen Frijters

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
using IKVM.Reflection.Metadata;
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

	public sealed class LocalBuilder
	{
		private readonly Type localType;
		private readonly int index;
		private readonly bool pinned;
		internal string name;
		internal int startOffset;
		internal int endOffset;

		internal LocalBuilder(Type localType, int index, bool pinned)
		{
			this.localType = localType;
			this.index = index;
			this.pinned = pinned;
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

		public Type LocalType
		{
			get { return localType; }
		}

		public int LocalIndex
		{
			get { return index; }
		}

		public bool IsPinned
		{
			get { return pinned; }
		}
	}

	public sealed class ILGenerator
	{
		private static readonly Type FAULT = new BakedType(null); // the type we use here doesn't matter, as long as it can never be used as a real exception type
		private readonly ModuleBuilder moduleBuilder;
		private readonly ByteBuffer code;
		private readonly List<LocalBuilder> locals = new List<LocalBuilder>();
		private readonly List<int> tokenFixups = new List<int>();
		private readonly List<int> labels = new List<int>();
		private readonly List<int> labelStackHeight = new List<int>();
		private readonly List<LabelFixup> labelFixups = new List<LabelFixup>();
		private readonly List<SequencePoint> sequencePoints = new List<SequencePoint>();
		private readonly List<ExceptionBlock> exceptions = new List<ExceptionBlock>();
		private readonly Stack<ExceptionBlock> exceptionStack = new Stack<ExceptionBlock>();
		private ushort maxStack;
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

		private sealed class ExceptionBlock : IComparer<ExceptionBlock>
		{
			internal readonly int ordinal;
			internal Label labelEnd;
			internal int tryOffset;
			internal int tryLength;
			internal int handlerOffset;
			internal int handlerLength;
			internal Type exceptionType;	// null = finally block or handler with filter, FAULT = fault block
			internal int filterOffset;

			internal ExceptionBlock(int ordinal)
			{
				this.ordinal = ordinal;
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
			if (moduleBuilder.symbolWriter != null)
			{
				scope = new Scope(null);
			}
		}

		private bool IsLabelReachable(Label label)
		{
			return labelStackHeight[label.Index] != -1;
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

		// new in .NET 4.0
		public int ILOffset
		{
			get { return code.Position; }
		}

		public void BeginCatchBlock(Type exceptionType)
		{
			ExceptionBlock block = exceptionStack.Peek();
			if (exceptionBlockAssistanceMode == EBAM_COMPAT || (exceptionBlockAssistanceMode == EBAM_CLEVER && stackHeight != -1))
			{
				if (exceptionType == null)
				{
					Emit(OpCodes.Endfilter);
				}
				else
				{
					Emit(OpCodes.Leave, block.labelEnd);
				}
			}
			stackHeight = 0;
			UpdateStack(1);
			if (block.tryLength == 0)
			{
				block.tryLength = code.Position - block.tryOffset;
			}
			else if (exceptionType != null)
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
			block.handlerOffset = code.Position;
			block.exceptionType = exceptionType;
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
			ExceptionBlock block = BeginFinallyFilterFaultBlock();
			block.filterOffset = code.Position;
			UpdateStack(1);
		}

		public void BeginFaultBlock()
		{
			ExceptionBlock block = BeginFinallyFilterFaultBlock();
			block.handlerOffset = code.Position;
			block.exceptionType = FAULT;
		}

		public void BeginFinallyBlock()
		{
			ExceptionBlock block = BeginFinallyFilterFaultBlock();
			block.handlerOffset = code.Position;
		}

		private ExceptionBlock BeginFinallyFilterFaultBlock()
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
			stackHeight = 0;
			return block;
		}

		public void EndExceptionBlock()
		{
			ExceptionBlock block = exceptionStack.Pop();
			if (exceptionBlockAssistanceMode == EBAM_COMPAT || (exceptionBlockAssistanceMode == EBAM_CLEVER && stackHeight != -1))
			{
				if (block.filterOffset != 0 || (block.exceptionType != null && block.exceptionType != FAULT))
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
			LocalBuilder local = new LocalBuilder(localType, locals.Count, pinned);
			locals.Add(local);
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
			WriteToken(moduleBuilder.GetFieldToken(field));
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

		private void WriteToken(FieldToken token)
		{
			if (token.IsPseudoToken)
			{
				tokenFixups.Add(code.Position);
			}
			code.Write(token.Token);
		}

		private void WriteToken(MethodToken token)
		{
			if (token.IsPseudoToken)
			{
				tokenFixups.Add(code.Position);
			}
			code.Write(token.Token);
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
			Emit(opc);
			WriteToken(moduleBuilder.GetMethodTokenForIL(method));
			UpdateStack(opc, method.HasThis, method.ReturnType, method.ParameterCount);
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
			code.Write(0x70000000 | moduleBuilder.UserStrings.Add(str));
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
			code.Write(0x11000000 | moduleBuilder.StandAloneSig.FindOrAddRecord(moduleBuilder.Blobs.Add(signature.GetSignature(moduleBuilder))));
		}

		public void EmitCall(OpCode opc, MethodInfo method, Type[] optionalParameterTypes)
		{
			if (optionalParameterTypes == null || optionalParameterTypes.Length == 0)
			{
				Emit(opc, method);
			}
			else
			{
				Emit(opc);
				UpdateStack(opc, method.HasThis, method.ReturnType, method.ParameterCount + optionalParameterTypes.Length);
				ByteBuffer sig = new ByteBuffer(16);
				method.MethodSignature.WriteMethodRefSig(moduleBuilder, sig, optionalParameterTypes);
				MemberRefTable.Record record = new MemberRefTable.Record();
				if (method.Module == moduleBuilder)
				{
					record.Class = method.MetadataToken;
				}
				else
				{
					record.Class = moduleBuilder.GetTypeTokenForMemberRef(method.DeclaringType ?? method.Module.GetModuleType());
				}
				record.Name = moduleBuilder.Strings.Add(method.Name);
				record.Signature = moduleBuilder.Blobs.Add(sig);
				code.Write(0x0A000000 | moduleBuilder.MemberRef.FindOrAddRecord(record));
			}
		}

		public void __EmitCall(OpCode opc, ConstructorInfo constructor, Type[] optionalParameterTypes)
		{
			EmitCall(opc, constructor.GetMethodInfo(), optionalParameterTypes);
		}

		public void EmitCalli(OpCode opc, CallingConvention callingConvention, Type returnType, Type[] parameterTypes)
		{
			returnType = returnType ?? moduleBuilder.universe.System_Void;
			Emit(opc);
			UpdateStack(opc, false, returnType, parameterTypes.Length);
			ByteBuffer sig = new ByteBuffer(16);
			Signature.WriteStandAloneMethodSig(moduleBuilder, sig, callingConvention, returnType, parameterTypes);
			code.Write(0x11000000 | moduleBuilder.StandAloneSig.FindOrAddRecord(moduleBuilder.Blobs.Add(sig)));
		}

		public void EmitCalli(OpCode opc, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			returnType = returnType ?? moduleBuilder.universe.System_Void;
			optionalParameterTypes = optionalParameterTypes ?? Type.EmptyTypes;
			Emit(opc);
			UpdateStack(opc, (callingConvention & CallingConventions.HasThis | CallingConventions.ExplicitThis) == CallingConventions.HasThis, returnType, parameterTypes.Length + optionalParameterTypes.Length);
			ByteBuffer sig = new ByteBuffer(16);
			Signature.WriteStandAloneMethodSig(moduleBuilder, sig, callingConvention, returnType, parameterTypes, optionalParameterTypes);
			code.Write(0x11000000 | moduleBuilder.StandAloneSig.FindOrAddRecord(moduleBuilder.Blobs.Add(sig)));
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
			if (locals.Count == 0 && exceptions.Count == 0 && maxStack <= 8 && code.Length < 64)
			{
				rva = WriteTinyHeaderAndCode(bb);
			}
			else
			{
				rva = WriteFatHeaderAndCode(bb, ref localVarSigTok, initLocals);
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

		private int WriteTinyHeaderAndCode(ByteBuffer bb)
		{
			int rva = bb.Position;
			const byte CorILMethod_TinyFormat = 0x2;
			bb.Write((byte)(CorILMethod_TinyFormat | (code.Length << 2)));
			WriteCode(bb);
			return rva;
		}

		private int WriteFatHeaderAndCode(ByteBuffer bb, ref int localVarSigTok, bool initLocals)
		{
			// fat headers require 4-byte alignment
			bb.Align(4);
			int rva = bb.Position;

			if (locals.Count != 0)
			{
				ByteBuffer localVarSig = new ByteBuffer(locals.Count + 2);
				Signature.WriteLocalVarSig(moduleBuilder, localVarSig, locals);
				localVarSigTok = 0x11000000 | moduleBuilder.StandAloneSig.FindOrAddRecord(moduleBuilder.Blobs.Add(localVarSig));
			}

			const byte CorILMethod_FatFormat = 0x03;
			const byte CorILMethod_MoreSects = 0x08;
			const byte CorILMethod_InitLocals = 0x10;

			short flagsAndSize = (short)(CorILMethod_FatFormat | (3 << 12));
			if (initLocals)
			{
				flagsAndSize |= CorILMethod_InitLocals;
			}

			if (exceptions.Count > 0)
			{
				flagsAndSize |= CorILMethod_MoreSects;
			}

			bb.Write(flagsAndSize);
			bb.Write(maxStack);
			bb.Write(code.Length);
			bb.Write(localVarSigTok);

			WriteCode(bb);

			if (exceptions.Count > 0)
			{
				bb.Align(4);

				bool fat = false;
				foreach (ExceptionBlock block in exceptions)
				{
					if (block.tryOffset > 65535 || block.tryLength > 255 || block.handlerOffset > 65535 || block.handlerLength > 255)
					{
						fat = true;
						break;
					}
				}
				exceptions.Sort(exceptions[0]);
				if (exceptions.Count * 12 + 4 > 255)
				{
					fat = true;
				}
				const byte CorILMethod_Sect_EHTable = 0x1;
				const byte CorILMethod_Sect_FatFormat = 0x40;
				const short COR_ILEXCEPTION_CLAUSE_EXCEPTION = 0x0000;
				const short COR_ILEXCEPTION_CLAUSE_FILTER = 0x0001;
				const short COR_ILEXCEPTION_CLAUSE_FINALLY = 0x0002;
				const short COR_ILEXCEPTION_CLAUSE_FAULT = 0x0004;

				if (fat)
				{
					bb.Write((byte)(CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat));
					int dataSize = exceptions.Count * 24 + 4;
					bb.Write((byte)dataSize);
					bb.Write((short)(dataSize >> 8));
					foreach (ExceptionBlock block in exceptions)
					{
						if (block.exceptionType == FAULT)
						{
							bb.Write((int)COR_ILEXCEPTION_CLAUSE_FAULT);
						}
						else if (block.filterOffset != 0)
						{
							bb.Write((int)COR_ILEXCEPTION_CLAUSE_FILTER);
						}
						else if (block.exceptionType != null)
						{
							bb.Write((int)COR_ILEXCEPTION_CLAUSE_EXCEPTION);
						}
						else
						{
							bb.Write((int)COR_ILEXCEPTION_CLAUSE_FINALLY);
						}
						bb.Write(block.tryOffset);
						bb.Write(block.tryLength);
						bb.Write(block.handlerOffset);
						bb.Write(block.handlerLength);
						if (block.exceptionType != null && block.exceptionType != FAULT)
						{
							bb.Write(moduleBuilder.GetTypeTokenForMemberRef(block.exceptionType));
						}
						else
						{
							bb.Write(block.filterOffset);
						}
					}
				}
				else
				{
					bb.Write(CorILMethod_Sect_EHTable);
					bb.Write((byte)(exceptions.Count * 12 + 4));
					bb.Write((short)0);
					foreach (ExceptionBlock block in exceptions)
					{
						if (block.exceptionType == FAULT)
						{
							bb.Write(COR_ILEXCEPTION_CLAUSE_FAULT);
						}
						else if (block.filterOffset != 0)
						{
							bb.Write(COR_ILEXCEPTION_CLAUSE_FILTER);
						}
						else if (block.exceptionType != null)
						{
							bb.Write(COR_ILEXCEPTION_CLAUSE_EXCEPTION);
						}
						else
						{
							bb.Write(COR_ILEXCEPTION_CLAUSE_FINALLY);
						}
						bb.Write((short)block.tryOffset);
						bb.Write((byte)block.tryLength);
						bb.Write((short)block.handlerOffset);
						bb.Write((byte)block.handlerLength);
						if (block.exceptionType != null && block.exceptionType != FAULT)
						{
							bb.Write(moduleBuilder.GetTypeTokenForMemberRef(block.exceptionType));
						}
						else
						{
							bb.Write(block.filterOffset);
						}
					}
				}
			}
			return rva;
		}

		private void WriteCode(ByteBuffer bb)
		{
			int codeOffset = bb.Position;
			foreach (int fixup in this.tokenFixups)
			{
				moduleBuilder.tokenFixupOffsets.Add(fixup + codeOffset);
			}
			bb.Write(code);
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
