using System.IO;
using System.Collections;

namespace PEAPI {

	/**************************************************************************/  
	/// <summary>
	/// Descriptor for an IL instruction
	/// </summary>
	internal abstract class CILInstruction {
		protected static readonly sbyte maxByteVal = 127;
		protected static readonly sbyte minByteVal = -128;
		protected static readonly byte leadByte = 0xFE;
		protected static readonly uint USHeapIndex = 0x70000000;
		protected static readonly int longInstrStart = (int)Op.arglist;
		public bool twoByteInstr = false;
		public uint size = 0;
		public uint offset;

		internal virtual bool Check(MetaData md) 
		{
			return false;
		}

		internal virtual void Write(FileImage output) { }

	}

	internal class CILByte : CILInstruction {
		byte byteVal;

		internal CILByte(byte bVal) 
		{
			byteVal = bVal;
			size = 1;
		}

		internal override void Write(FileImage output) 
		{
			output.Write(byteVal);
		}

	}

	internal class Instr : CILInstruction {
		protected int instr;

		internal Instr(int inst) 
		{
			if (inst >= longInstrStart) {
				instr = inst - longInstrStart;
				twoByteInstr = true;
				size = 2;
			} else {
				instr = inst;
				size = 1;
			}
		}

		internal override void Write(FileImage output) 
		{
			//Console.WriteLine("Writing instruction " + instr + " with size " + size);
			if (twoByteInstr) output.Write(leadByte);
			output.Write((byte)instr);
		}

	}

	internal class IntInstr : Instr {
		int val;
		bool byteNum;

		internal IntInstr(int inst, int num, bool byteSize) : base(inst) 
		{
			val = num;
			byteNum = byteSize;
			if (byteNum) size++;
			else size += 4;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			if (byteNum) 
				output.Write((sbyte)val);
			else 
				output.Write(val); 
		}

	}

	internal class UIntInstr : Instr {
		int val;
		bool byteNum;

		internal UIntInstr(int inst, int num, bool byteSize) : base(inst) 
		{
			val = num;
			byteNum = byteSize;
			if (byteNum) size++;
			else size += 2;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			if (byteNum)
				output.Write((byte)val);
			else
				output.Write((ushort)val); 
		}

	}

	internal class LongInstr : Instr {
		long val;

		internal LongInstr(int inst, long l) : base(inst) 
		{
			val = l;
			size += 8;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(val);
		}

	}

	internal class FloatInstr : Instr {
		float fVal;

		internal FloatInstr(int inst, float f) : base(inst) 
		{
			fVal = f;
			size += 4;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(fVal);
		}

	}

	internal class DoubleInstr : Instr {
		double val;

		internal DoubleInstr(int inst, double d) : base(inst) 
		{
			val = d;
			size += 8;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(val);
		}

	}

	internal class StringInstr : Instr {
		string val;
		byte[] bval;                                                  
		uint strIndex;

		internal StringInstr(int inst, string str) : base(inst) 
		{
			val = str;  
			size += 4;
		}

		internal StringInstr (int inst, byte[] str) : base (inst) 
		{
			bval = str;
			size += 4;
		}

		internal sealed override bool Check(MetaData md) 
		{
			if (val != null)
				strIndex = md.AddToUSHeap(val);
			else
				strIndex = md.AddToUSHeap (bval);
			return false;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(USHeapIndex  | strIndex);
		}

	}

	internal class LabelInstr : CILInstruction {
		CILLabel label;

		internal LabelInstr(CILLabel lab) 
		{
			label = lab;
			label.AddLabelInstr(this);
		}
	}

	internal class FieldInstr : Instr {
		Field field;

		internal FieldInstr(int inst, Field f) : base(inst) 
		{
			field = f;
			size += 4;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(field.Token());
		}

	}

	internal class MethInstr : Instr {
		Method meth;

		internal MethInstr(int inst, Method m) : base(inst) 
		{
			meth = m;
			size += 4;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(meth.Token());
		}

	}

	internal class SigInstr : Instr {
		CalliSig signature;

		internal SigInstr(int inst, CalliSig sig) : base(inst) 
		{
			signature = sig;
			size += 4;
		}

		internal sealed override bool Check(MetaData md) 
		{
			md.AddToTable(MDTable.StandAloneSig,signature);
			signature.BuildTables(md);
			return false;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(signature.Token());
		}
	}

	internal class TypeInstr : Instr {
		MetaDataElement theType;

		internal TypeInstr(int inst, Type aType, MetaData md) : base(inst) 
		{
			theType = aType.GetTypeSpec(md);
			size += 4;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(theType.Token());
		}

	}

	internal class BranchInstr : Instr {
		CILLabel dest;
		private bool shortVer = true;
		private int target = 0;

		internal BranchInstr(int inst, CILLabel dst) : base(inst) 
		{
			dest = dst;
			dest.AddBranch(this);
			size++;

			if (inst >= (int) BranchOp.br && inst != (int) BranchOp.leave_s) {
				shortVer = false;
				size += 3;
			}
		}

		internal sealed override bool Check(MetaData md) 
		{
			target = (int)dest.GetLabelOffset() - (int)(offset + size);
			return false;
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			if (shortVer)
				output.Write((sbyte)target);
			else
				output.Write(target);
		}

	}

	internal class SwitchInstr : Instr {
		CILLabel[] cases;
		uint numCases = 0;

		internal SwitchInstr(int inst, CILLabel[] dsts) : base(inst) 
		{
			cases = dsts;
			if (cases != null) numCases = (uint)cases.Length;
			size += 4 + (numCases * 4);
			for (int i=0; i < numCases; i++) {
				cases[i].AddBranch(this);
			}
		}

		internal sealed override void Write(FileImage output) 
		{
			base.Write(output);
			output.Write(numCases);
			for (int i=0; i < numCases; i++) {
				int target = (int)cases[i].GetLabelOffset() - (int)(offset + size);
				output.Write(target);
			}
		}

	}

	/**************************************************************************/  
	/// <summary>
	/// The IL instructions for a method
	/// </summary>
	public class CILInstructions  {
		private static readonly uint ExHeaderSize = 4;
		private static readonly uint FatExClauseSize = 24;
		private static readonly uint SmlExClauseSize = 12;
		private static readonly sbyte maxByteVal = 127;
		private static readonly sbyte minByteVal = -128;
		private static readonly byte maxUByteVal = 255;
		private static readonly int smallSize = 64;
		private static readonly ushort TinyFormat = 0x2;
		private static readonly ushort FatFormat = 0x3003;
		private static readonly ushort MoreSects = 0x8;
		private static readonly ushort InitLocals = 0x10;
		private static readonly uint FatSize = 12;
		private static readonly byte FatExceptTable = 0x41;
		private static readonly byte SmlExceptTable = 0x01; 

		private MetaData metaData;
		private ArrayList exceptions, blockStack;
		//private bool codeChecked = false;
		private static readonly int INITSIZE = 5;
		private CILInstruction[] buffer = new CILInstruction[INITSIZE];
		private int tide = 0;
		private uint offset = 0;
		private ushort headerFlags = 0;
		private short maxStack;
		private uint paddingNeeded = 0;
		private byte exceptHeader = 0;
		uint localSigIx = 0;
		uint codeSize = 0, exceptSize = 0;
		bool tinyFormat, fatExceptionFormat = false;

		public uint Offset {
			get { return offset; }
		}	

		internal CILInstructions(MetaData md) 
		{
			metaData = md;
		}

		private void AddToBuffer(CILInstruction inst) 
		{
			if (tide >= buffer.Length) {
				CILInstruction[] tmp = buffer;
				buffer = new CILInstruction[tmp.Length * 2];
				for (int i=0; i < tide; i++) {
					buffer[i] = tmp[i];
				}
			}
			//Console.WriteLine("Adding instruction at offset " + offset + " with size " + inst.size);
			inst.offset = offset;
			offset += inst.size;
			buffer[tide++] = inst;
		}

		/// <summary>
		/// Add a simple IL instruction
		/// </summary>
		/// <param name="inst">the IL instruction</param>
		public void Inst(Op inst) 
		{
			AddToBuffer(new Instr((int)inst));
		}

		/// <summary>
		/// Add an IL instruction with an integer parameter
		/// </summary>
		/// <param name="inst">the IL instruction</param>
		/// <param name="val">the integer parameter value</param>
		public void IntInst(IntOp inst, int val) 
		{
			int instr = (int)inst;
			if ((inst == IntOp.ldc_i4_s) || (inst == IntOp.ldc_i4)) 
				AddToBuffer(new IntInstr(instr,val,(inst == IntOp.ldc_i4_s)));
			else
				AddToBuffer(new UIntInstr(instr,val,((inst < IntOp.ldc_i4_s) ||
								(inst == IntOp.unaligned))));
		}

		/// <summary>
		/// Add the load long instruction
		/// </summary>
		/// <param name="cVal">the long value</param>
		public void ldc_i8(long cVal) 
		{
			AddToBuffer(new LongInstr(0x21,cVal));
		}

		/// <summary>
		/// Add the load float32 instruction
		/// </summary>
		/// <param name="cVal">the float value</param>
		public void ldc_r4(float cVal) 
		{
			AddToBuffer(new FloatInstr(0x22,cVal));
		}

		/// <summary>
		/// Add the load float64 instruction
		/// </summary>
		/// <param name="cVal">the float value</param>
		public void ldc_r8(double cVal) 
		{
			AddToBuffer(new DoubleInstr(0x23,cVal));
		}

		/// <summary>
		/// Add the load string instruction
		/// </summary>
		/// <param name="str">the string value</param>
		public void ldstr(string str) 
		{
			AddToBuffer(new StringInstr(0x72,str));
		}

		/// <summary>
		/// Add the load string instruction
		/// </summary>
		public void ldstr (byte[] str) 
		{
			AddToBuffer (new StringInstr (0x72, str));
		}

		/// <summary>
		/// Add the calli instruction
		/// </summary>
		/// <param name="sig">the signature for the calli</param>
		public void calli(CalliSig sig) 
		{
			AddToBuffer(new SigInstr(0x29,sig));
		}

		/// <summary>
		/// Add a label to the CIL instructions
		/// </summary>
		/// <param name="lab">the label to be added</param>
		public void CodeLabel(CILLabel lab) 
		{
			AddToBuffer(new LabelInstr(lab));
		}

		/// <summary>
		/// Add an instruction with a field parameter
		/// </summary>
		/// <param name="inst">the CIL instruction</param>
		/// <param name="f">the field parameter</param>
		public void FieldInst(FieldOp inst, Field f) 
		{
			AddToBuffer(new FieldInstr((int)inst,f));
		}

		/// <summary>
		/// Add an instruction with a method parameter
		/// </summary>
		/// <param name="inst">the CIL instruction</param>
		/// <param name="m">the method parameter</param>
		public void MethInst(MethodOp inst, Method m) 
		{
			AddToBuffer(new MethInstr((int)inst,m));
		}

		/// <summary>
		/// Add an instruction with a type parameter
		/// </summary>
		/// <param name="inst">the CIL instruction</param>
		/// <param name="t">the type argument for the CIL instruction</param>
		public void TypeInst(TypeOp inst, Type aType) 
		{
			AddToBuffer(new TypeInstr((int)inst,aType,metaData));
		}

		/// <summary>
		/// Add a branch instruction
		/// </summary>
		/// <param name="inst">the branch instruction</param>
		/// <param name="lab">the label that is the target of the branch</param>
		public void Branch(BranchOp inst,  CILLabel lab) 
		{
			AddToBuffer(new BranchInstr((int)inst,lab));
		}

		/// <summary>
		/// Add a switch instruction
		/// </summary>
		/// <param name="labs">the target labels for the switch</param>
		public void Switch(CILLabel[] labs) 
		{
			AddToBuffer(new SwitchInstr(0x45,labs));
		}

		/// <summary>
		/// Add a byte to the CIL instructions (.emitbyte)
		/// </summary>
		/// <param name="bVal"></param>
		public void emitbyte(byte bVal) 
		{
			AddToBuffer(new CILByte(bVal));
		}

		/// <summary>
		/// Add an instruction which puts an integer on TOS.  This method
		/// selects the correct instruction based on the value of the integer.
		/// </summary>
		/// <param name="i">the integer value</param>
		public void PushInt(int i) 
		{
			if (i == -1) {
				AddToBuffer(new Instr((int)Op.ldc_i4_m1));
			} else if ((i >= 0) && (i <= 8)) {
				Op op = (Op)(Op.ldc_i4_0 + i);
				AddToBuffer(new Instr((int)op));
			} else if ((i >= minByteVal) && (i <= maxByteVal)) {
				AddToBuffer(new IntInstr((int)IntOp.ldc_i4_s,i,true));
			} else {
				AddToBuffer(new IntInstr((int)IntOp.ldc_i4,i,false)); 
			}
		}

		/// <summary>
		/// Add the instruction to load a long on TOS
		/// </summary>
		/// <param name="l">the long value</param>
		public void PushLong(long l) 
		{
			AddToBuffer(new LongInstr(0x21,l));
		}

		/// <summary>
		/// Add an instruction to push the boolean value true on TOS
		/// </summary>
		public void PushTrue() 
		{
			AddToBuffer(new Instr((int)Op.ldc_i4_1));
		}

		/// <summary>
		///  Add an instruction to push the boolean value false on TOS
		/// </summary>
		public void PushFalse() 
		{
			AddToBuffer(new Instr((int)Op.ldc_i4_0));
		}

		/// <summary>
		/// Add the instruction to load an argument on TOS.  This method
		/// selects the correct instruction based on the value of argNo
		/// </summary>
		/// <param name="argNo">the number of the argument</param>
		public void LoadArg(int argNo) 
		{
			if (argNo < 4) {
				int op = (int)Op.ldarg_0 + argNo;
				AddToBuffer(new Instr(op));
			} else if (argNo <= maxUByteVal) {
				AddToBuffer(new UIntInstr((int)IntOp.ldarg,argNo,true));
			} else {
				AddToBuffer(new UIntInstr(0x09,argNo,false)); 
			}
		}

		/// <summary>
		/// Add the instruction to load the address of an argument on TOS.
		/// This method selects the correct instruction based on the value
		/// of argNo.
		/// </summary>
		/// <param name="argNo">the number of the argument</param>
		public void LoadArgAdr(int argNo) 
		{
			if (argNo <= maxUByteVal) {
				AddToBuffer(new UIntInstr((int)IntOp.ldarga,argNo,true));
			} else {
				AddToBuffer(new UIntInstr(0x0A,argNo,false)); 
			}
		}

		/// <summary>
		/// Add the instruction to load a local on TOS.  This method selects
		/// the correct instruction based on the value of locNo.
		/// </summary>
		/// <param name="locNo">the number of the local to load</param>
		public void LoadLocal(int locNo) 
		{
			if (locNo < 4) {
				int op = (int)Op.ldloc_0 + locNo;
				AddToBuffer(new Instr(op));
			} else if (locNo <= maxUByteVal) {
				AddToBuffer(new UIntInstr((int)IntOp.ldloc,locNo,true));
			} else {
				AddToBuffer(new UIntInstr(0x0C,locNo,false)); 
			}
		}

		/// <summary>
		/// Add the instruction to load the address of a local on TOS.
		/// This method selects the correct instruction based on the 
		/// value of locNo.
		/// </summary>
		/// <param name="locNo">the number of the local</param>
		public void LoadLocalAdr(int locNo) 
		{
			if (locNo <= maxUByteVal) {
				AddToBuffer(new UIntInstr((int)IntOp.ldloca,locNo,true));
			} else {
				AddToBuffer(new UIntInstr(0x0D,locNo,false)); 
			}
		}

		/// <summary>
		/// Add the instruction to store to an argument.  This method
		/// selects the correct instruction based on the value of argNo.
		/// </summary>
		/// <param name="argNo">the argument to be stored to</param>
		public void StoreArg(int argNo) 
		{
			if (argNo <= maxUByteVal) {
				AddToBuffer(new UIntInstr((int)IntOp.starg,argNo,true));
			} else {
				AddToBuffer(new UIntInstr(0x0B,argNo,false)); 
			}
		}

		/// <summary>
		/// Add the instruction to store to a local.  This method selects
		/// the correct instruction based on the value of locNo.
		/// </summary>
		/// <param name="locNo">the local to be stored to</param>
		public void StoreLocal(int locNo) 
		{
			if (locNo < 4) {
				int op = (int)Op.stloc_0 + locNo;
				AddToBuffer(new Instr(op));
			} else if (locNo <= maxUByteVal) {
				AddToBuffer(new UIntInstr((int)IntOp.stloc,locNo,true));
			} else {
				AddToBuffer(new UIntInstr(0x0E,locNo,false)); 
			}
		}

		/// <summary>
		/// Create a new CIL label.  To place the label in the CIL instruction
		/// stream use CodeLabel.
		/// </summary>
		/// <returns>a new CIL label</returns>
		public CILLabel NewLabel() 
		{
			return new CILLabel();
		}

		public void AddTryBlock(TryBlock tryBlock) 
		{
			if (exceptions == null) 
				exceptions = new ArrayList();
			else if (exceptions.Contains(tryBlock)) return;
			exceptions.Add(tryBlock);
			tryBlock.ResolveCatchBlocks (metaData);
		}

		/// <summary>
		/// Create a new label at this position in the code buffer
		/// </summary>
		/// <returns>the label at the current position</returns>
		public CILLabel NewCodedLabel() 
		{
			CILLabel lab = new CILLabel();
			AddToBuffer(new LabelInstr(lab));
			return lab;
		}

		/// <summary>
		/// Mark this position as the start of a new block
		/// (try, catch, filter, finally or fault)
		/// </summary>
		public void StartBlock() 
		{
			if (blockStack == null) blockStack = new ArrayList();
			blockStack.Insert(0,NewCodedLabel());
		}

		/// <summary>
		/// Mark this position as the end of the last started block and
		/// make it a try block.  This try block is added to the current 
		/// instructions (ie do not need to call AddTryBlock)
		/// </summary>
		/// <returns>The try block just ended</returns>
		public TryBlock EndTryBlock() 
		{
			TryBlock tBlock = new TryBlock((CILLabel)blockStack[0],NewCodedLabel());
			blockStack.RemoveAt(0);
			AddTryBlock(tBlock);
			return tBlock;
		}

		/// <summary>
		/// Mark this position as the end of the last started block and
		/// make it a catch block.  This catch block is associated with the
		/// specified try block.
		/// </summary>
		/// <param name="exceptType">the exception type to be caught</param>
		/// <param name="tryBlock">the try block associated with this catch block</param>
		public void EndCatchBlock(Class exceptType, TryBlock tryBlock) 
		{
			Catch catchBlock = new Catch(exceptType,(CILLabel)blockStack[0],
					NewCodedLabel());
			tryBlock.AddHandler(catchBlock);
		}

		/// <summary>
		/// Mark this position as the end of the last started block and
		/// make it a filter block.  This filter block is associated with the
		/// specified try block.
		/// </summary>
		/// <param name="filterLab">the label where the filter code is</param>
		/// <param name="tryBlock">the try block associated with this filter block</param>
		public void EndFilterBlock(CILLabel filterLab, TryBlock tryBlock) 
		{
			Filter filBlock = new Filter(filterLab,(CILLabel)blockStack[0],NewCodedLabel());
			tryBlock.AddHandler(filBlock);
		}

		/// <summary>
		/// Mark this position as the end of the last started block and
		/// make it a finally block.  This finally block is associated with the
		/// specified try block.
		/// </summary>
		/// <param name="tryBlock">the try block associated with this finally block</param>
		public void EndFinallyBlock(TryBlock tryBlock) 
		{
			Finally finBlock= new Finally((CILLabel)blockStack[0],NewCodedLabel());
			tryBlock.AddHandler(finBlock);
		}

		/// <summary>
		/// Mark this position as the end of the last started block and
		/// make it a fault block.  This fault block is associated with the
		/// specified try block.
		/// </summary>
		/// <param name="tryBlock">the try block associated with this fault block</param>
		public void EndFaultBlock(TryBlock tryBlock) 
		{
			Fault fBlock= new Fault((CILLabel)blockStack[0],NewCodedLabel());
			tryBlock.AddHandler(fBlock);
		}

		internal uint GetCodeSize() 
		{
			return codeSize + paddingNeeded + exceptSize;
		}

		internal void CheckCode(uint locSigIx, bool initLocals, int maxStack) 
		{
			if (tide == 0) return;
			bool changed = true;
			while (changed) {
				changed = false;
				for (int i=0; i < tide; i++) {
					changed = buffer[i].Check(metaData) || changed;
				}
				if (changed) {
					for (int i=1; i < tide; i++) {
						buffer[i].offset = buffer[i-1].offset + buffer[i-1].size;
					}
					offset = buffer[tide-1].offset + buffer[tide-1].size;
				}
			}
			codeSize = offset;
			// Console.WriteLine("codeSize before header added = " + codeSize);
			if ((offset < smallSize) && (maxStack <= 8) && (locSigIx == 0) && (exceptions == null)) {
				// can use tiny header
				//Console.WriteLine("Tiny Header");
				tinyFormat = true;
				headerFlags = (ushort)(TinyFormat | ((ushort)codeSize << 2));
				codeSize++;
				if ((codeSize % 4) != 0) { paddingNeeded = 4 - (codeSize % 4); }
			} else {
				//Console.WriteLine("Fat Header");
				tinyFormat = false;
				localSigIx = locSigIx;
				this.maxStack = (short)maxStack;
				headerFlags = FatFormat;
				if (exceptions != null) {
					// Console.WriteLine("Got exceptions");
					headerFlags |= MoreSects;
					uint numExceptClauses = 0;
					for (int i=0; i < exceptions.Count; i++) {
						TryBlock tryBlock = (TryBlock)exceptions[i];
						tryBlock.SetSize();
						numExceptClauses += (uint)tryBlock.NumHandlers();
						if (tryBlock.isFat()) fatExceptionFormat = true;
					}

					uint data_size = ExHeaderSize + numExceptClauses *
						(fatExceptionFormat ? FatExClauseSize : SmlExClauseSize);

					if (data_size > 255)
						fatExceptionFormat = true;

					// Console.WriteLine("numexceptclauses = " + numExceptClauses);
					if (fatExceptionFormat) {
						// Console.WriteLine("Fat exception format");
						exceptHeader = FatExceptTable;
						exceptSize = ExHeaderSize + numExceptClauses * FatExClauseSize;
					} else {
						// Console.WriteLine("Tiny exception format");
						exceptHeader = SmlExceptTable;
						exceptSize = ExHeaderSize + numExceptClauses * SmlExClauseSize;
					}
					// Console.WriteLine("exceptSize = " + exceptSize);
				}
				if (initLocals) headerFlags |= InitLocals;
				if ((offset % 4) != 0) { paddingNeeded = 4 - (offset % 4); }
				codeSize += FatSize;
			}
			// Console.WriteLine("codeSize = " + codeSize + "  headerFlags = " + 
			//                   Hex.Short(headerFlags));
		}

		internal void Write(FileImage output) 
		{
			// Console.WriteLine("Writing header flags = " + Hex.Short(headerFlags));
			if (tinyFormat) {
				// Console.WriteLine("Writing tiny code");
				output.Write((byte)headerFlags);
			} else {
				// Console.WriteLine("Writing fat code");
				output.Write(headerFlags);
				output.Write((ushort)maxStack);
				output.Write(offset);
				output.Write(localSigIx);
			}
			// Console.WriteLine(Hex.Int(tide) + " CIL instructions");
			// Console.WriteLine("starting instructions at " + output.Seek(0,SeekOrigin.Current));
			for (int i=0; i < tide; i++) {
				buffer[i].Write(output);
			}
			// Console.WriteLine("ending instructions at " + output.Seek(0,SeekOrigin.Current));
			for (int i=0; i < paddingNeeded; i++) { output.Write((byte)0); }
			if (exceptions != null) {
				// Console.WriteLine("Writing exceptions");
				// Console.WriteLine("header = " + Hex.Short(exceptHeader) + " exceptSize = " + Hex.Int(exceptSize));
				output.Write(exceptHeader);
				output.Write3Bytes((uint)exceptSize);
				for (int i=0; i < exceptions.Count; i++) {
					TryBlock tryBlock = (TryBlock)exceptions[i];
					tryBlock.Write(output,fatExceptionFormat);
				}
			}
		}

	}

	/**************************************************************************/  
	public abstract class CodeBlock {

		private static readonly int maxCodeSize = 255;
		protected CILLabel start, end;
		protected bool small = true;

		public CodeBlock(CILLabel start, CILLabel end) 
		{
			this.start = start;
			this.end = end;
		}

		internal virtual bool isFat() 
		{
			// Console.WriteLine("block start = " + start.GetLabelOffset() +
			//                  "  block end = " + end.GetLabelOffset());
			return (end.GetLabelOffset() - start.GetLabelOffset()) > maxCodeSize;
		}

		internal virtual void Write(FileImage output, bool fatFormat) 
		{
			if (fatFormat) output.Write(start.GetLabelOffset());
			else output.Write((short)start.GetLabelOffset());
			uint len = end.GetLabelOffset() - start.GetLabelOffset();
			if (fatFormat) output.Write(len);
			else output.Write((byte)len);
		}

	}

	/// <summary>
	/// The descriptor for a guarded block (.try)
	/// </summary>
	public class TryBlock : CodeBlock {
		protected bool fatFormat = false;
		protected int flags = 0;
		ArrayList handlers = new ArrayList();

		/// <summary>
		/// Create a new try block
		/// </summary>
		/// <param name="start">start label for the try block</param>
		/// <param name="end">end label for the try block</param>
		public TryBlock(CILLabel start, CILLabel end) : base(start,end) { }

		/// <summary>
		/// Add a handler to this try block
		/// </summary>
		/// <param name="handler">a handler to be added to the try block</param>
		public void AddHandler(HandlerBlock handler) 
		{
			flags = handler.GetFlag();
			handlers.Add(handler);
		}

		internal void SetSize() 
		{
			fatFormat = base.isFat();
			if (fatFormat) return;
			for (int i=0; i < handlers.Count; i++) {
				HandlerBlock handler = (HandlerBlock)handlers[i];
				if (handler.isFat()) {
					fatFormat = true;
					return;
				}
			}
		}

		internal int NumHandlers() 
		{
			return handlers.Count;
		}

		internal override bool isFat() 
		{
			return fatFormat;
		}

		//Hackish
		internal void ResolveCatchBlocks (MetaData md)
		{
			for (int i=0; i < handlers.Count; i++) {
				Catch c = handlers [i] as Catch;
				if (c != null)
					c.ResolveType (md);
			}
		}

		internal override void Write(FileImage output, bool fatFormat) 
		{
			// Console.WriteLine("writing exception details");
			for (int i=0; i < handlers.Count; i++) {
				// Console.WriteLine("Except block " + i);
				HandlerBlock handler = (HandlerBlock)handlers[i];
				if (fatFormat) output.Write(flags);
				else output.Write((short)flags);
				// Console.WriteLine("flags = " + Hex.Short(flags));
				base.Write(output,fatFormat);
				handler.Write(output,fatFormat);
			}
		}
	}

	public abstract class HandlerBlock : CodeBlock  {

		protected static readonly short ExceptionFlag = 0;
		protected static readonly short FilterFlag = 0x01;
		protected static readonly short FinallyFlag = 0x02;
		protected static readonly short FaultFlag = 0x04;

		public HandlerBlock(CILLabel start, CILLabel end) : base(start,end) { }

		internal virtual short GetFlag() { return ExceptionFlag; }

		internal override void Write(FileImage output, bool fatFormat) 
		{
			base.Write(output,fatFormat);
		}

	}

	/// <summary>
	/// The descriptor for a catch clause (.catch)
	/// </summary>
	public class Catch : HandlerBlock  {

		MetaDataElement exceptType;

		/// <summary>
		/// Create a new catch clause
		/// </summary>
		/// <param name="except">the exception to be caught</param>
		/// <param name="handlerStart">start of the handler code</param>
		/// <param name="handlerEnd">end of the handler code</param>
		public Catch(Class except, CILLabel handlerStart, CILLabel handlerEnd)
			: base(handlerStart, handlerEnd)
		{
			exceptType = except;
		}

		public Catch(Type except, CILLabel handlerStart, CILLabel handlerEnd)
			: base(handlerStart,handlerEnd) 
		{
			exceptType = except;
		}

		internal void ResolveType (MetaData md)
		{
		       exceptType = ((Type) exceptType).GetTypeSpec (md);
		}

		internal override void Write(FileImage output, bool fatFormat) 
		{
			base.Write(output,fatFormat);
			output.Write(exceptType.Token());
		}
	}

	/// <summary>
	/// The descriptor for a filter clause (.filter)
	/// </summary>
	public class Filter : HandlerBlock  {

		CILLabel filterLabel;

		/// <summary>
		/// Create a new filter clause
		/// </summary>
		/// <param name="filterLabel">the label where the filter code starts</param>
		/// <param name="handlerStart">the start of the handler code</param>
		/// <param name="handlerEnd">the end of the handler code</param>
		public Filter(CILLabel filterLabel, CILLabel handlerStart, 
				CILLabel handlerEnd) : base(handlerStart,handlerEnd) 
				{
			this.filterLabel = filterLabel;
		}

		internal override short GetFlag() 
		{
			return FilterFlag; 
		}

		internal override void Write(FileImage output, bool fatFormat) 
		{
			base.Write(output,fatFormat);
			output.Write(filterLabel.GetLabelOffset());
		}

	}

	/// <summary>
	/// Descriptor for a finally block (.finally)
	/// </summary>
	public class Finally : HandlerBlock  {

		/// <summary>
		/// Create a new finally clause
		/// </summary>
		/// <param name="finallyStart">start of finally code</param>
		/// <param name="finallyEnd">end of finally code</param>
		public Finally(CILLabel finallyStart, CILLabel finallyEnd)
			: base(finallyStart,finallyEnd) { }

		internal override short GetFlag() 
		{
			return FinallyFlag; 
		}

		internal override void Write(FileImage output, bool fatFormat) 
		{
			base.Write(output,fatFormat);
			output.Write((int)0);
		}

	}

	/// <summary>
	/// Descriptor for a fault block (.fault)
	/// </summary>
	public class Fault : HandlerBlock  {

		/// <summary>
		/// Create a new fault clause
		/// </summary>
		/// <param name="faultStart">start of the fault code</param>
		/// <param name="faultEnd">end of the fault code</param>
		public Fault(CILLabel faultStart, CILLabel faultEnd)
			: base(faultStart,faultEnd) { }

		internal override short GetFlag() 
		{
			return FaultFlag; 
		}

		internal override void Write(FileImage output, bool fatFormat) 
		{
			base.Write(output,fatFormat);
			output.Write((int)0);

		}
	}

	/**************************************************************************/  
	/// <summary>
	/// Descriptor for the locals for a method
	/// </summary>
	public class LocalSig : Signature {

		private static readonly byte LocalSigByte = 0x7;
		Local[] locals;

		public LocalSig(Local[] locals)         
		{
			this.locals = locals;
			tabIx = MDTable.StandAloneSig;
		}

		internal sealed override void BuildTables(MetaData md) 
		{
			if (done) return;
			MemoryStream sig = new MemoryStream();
			sig.WriteByte(LocalSigByte);
			MetaData.CompressNum((uint)locals.Length,sig);
			for (int i=0; i < locals.Length; i++) {
				((Local)locals[i]).TypeSig(sig);
			}
			sigIx = md.AddToBlobHeap(sig.ToArray());
			done = true;
		}

	}

	/**************************************************************************/  
	/// <summary>
	/// Signature for calli instruction
	/// </summary>
	public class CalliSig : Signature {

		private static readonly byte Sentinel = 0x41;
		CallConv callConv;
		Type returnType;
		Type[] parameters, optParams;
		uint numPars = 0, numOptPars = 0;

		/// <summary>
		/// Create a signature for a calli instruction
		/// </summary>
		/// <param name="cconv">calling conventions</param>
		/// <param name="retType">return type</param>
		/// <param name="pars">parameter types</param>
		public CalliSig(CallConv cconv, Type retType, Type[] pars) 
		{
			tabIx = MDTable.StandAloneSig;
			callConv = cconv;
			returnType = retType;
			parameters = pars;
			if (pars != null) numPars = (uint)pars.Length;
		}

		/// <summary>
		/// Add the optional parameters to a vararg method
		/// This method sets the vararg calling convention
		/// </summary>
		/// <param name="optPars">the optional pars for the vararg call</param>
		public void AddVarArgs(Type[] optPars) 
		{
			optParams = optPars;
			if (optPars != null) numOptPars = (uint)optPars.Length;
			callConv |= CallConv.Vararg;
		}

		/// <summary>
		/// Add extra calling conventions to this callsite signature
		/// </summary>
		/// <param name="cconv"></param>
		public void AddCallingConv(CallConv cconv) 
		{
			callConv |= cconv;
		}

		internal sealed override void BuildTables(MetaData md) 
		{
			if (done) return;
			MemoryStream sig = new MemoryStream();
			sig.WriteByte((byte)callConv);
			MetaData.CompressNum(numPars+numOptPars,sig);
			returnType.TypeSig(sig);
			for (int i=0; i < numPars; i++) {
				parameters[i].TypeSig(sig);
			}
			sigIx = md.AddToBlobHeap(sig.ToArray());
			if (numOptPars > 0) {
				sig.WriteByte(Sentinel);
				for (int i=0; i < numOptPars; i++) {
					optParams[i].TypeSig(sig);
				}
			}
			done = true;
		}

	}

	/**************************************************************************/  
	/// <summary>
	/// Descriptor for a local of a method
	/// </summary>
	public class Local {

		private static readonly byte Pinned = 0x45;
		string name;
		Type type;
		bool pinned = false, byref = false;

		/// <summary>
		/// Create a new local variable 
		/// </summary>
		/// <param name="lName">name of the local variable</param>
		/// <param name="lType">type of the local variable</param>
		public Local(string lName, Type lType) 
		{
			name = lName;
			type = lType;
		}

		/// <summary>
		/// Create a new local variable that is byref and/or pinned
		/// </summary>
		/// <param name="lName">local name</param>
		/// <param name="lType">local type</param>
		/// <param name="byRef">is byref</param>
		/// <param name="isPinned">has pinned attribute</param>
		public Local(string lName, Type lType, bool byRef, bool isPinned)
		{
			name = lName;
			type = lType;
			byref = byRef;
			pinned = isPinned;
		}

		internal void TypeSig(MemoryStream str) 
		{
			if (pinned) str.WriteByte(Pinned);
			type.TypeSig(str);
		}

	}

	/**************************************************************************/  
	/// <summary>
	/// A label in the IL
	/// </summary>
	public class CILLabel {

		CILInstruction branch;
		CILInstruction[] multipleBranches;
		int tide = 0;
		CILInstruction labInstr;
		uint offset = 0;
		bool absolute;


		public CILLabel (uint offset, bool absolute) 
		{
			this.offset = offset;
			this.absolute = absolute;
		}

		public CILLabel (uint offset) : this (offset, false)
		{
		}


		internal CILLabel() 
		{
		}

		internal void AddBranch(CILInstruction instr) 
		{
			if (branch == null) {
				branch = instr;
				return;
			}
			if (multipleBranches == null) {
				multipleBranches = new CILInstruction[2];
			} else if (tide >= multipleBranches.Length) {
				CILInstruction[] tmp = multipleBranches;
				multipleBranches = new CILInstruction[tmp.Length*2];
				for (int i=0; i < tide; i++) {
					multipleBranches[i] = tmp[i];
				}
			}
			multipleBranches[tide++] = instr;
		}

		internal void AddLabelInstr(LabelInstr lInstr) 
		{
			labInstr = lInstr;
		}

		internal uint GetLabelOffset() 
		{
			if (absolute) return offset;
			if (labInstr == null) return 0;
			return labInstr.offset + offset;
		}

	}


}


