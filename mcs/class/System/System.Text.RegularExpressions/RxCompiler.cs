using System;
using System.Collections;

namespace System.Text.RegularExpressions {

	/* This behaves like a growing list of tuples (base, offsetpos) */
	class RxLinkRef: LinkRef {
		public int[] offsets;
		public int current = 0;

		public RxLinkRef ()
		{
			offsets = new int [8];
		}

		// the start of the branch instruction
		// in the program stream
		public void PushInstructionBase (int offset)
		{
			if ((current & 1) != 0)
				throw new Exception ();
			if (current == offsets.Length) {
				int[] newarray = new int [offsets.Length * 2];
				Buffer.BlockCopy (offsets, 0, newarray, 0, offsets.Length);
				offsets = newarray;
			}
			offsets [current++] = offset;
		}

		// the position in the program stream where the jump offset is stored
		public void PushOffsetPosition (int offset)
		{
			if ((current & 1) == 0)
				throw new Exception ();
			offsets [current++] = offset;
		}

	}

	class RxCompiler : ICompiler {
		byte[] program = new byte [32];
		int curpos = 0;

		public RxCompiler () {
		}

		void MakeRoom (int bytes)
		{
			while (curpos + bytes > program.Length) {
				int newsize = program.Length * 2;
				byte[] newp = new byte [newsize];
				Buffer.BlockCopy (program, 0, newp, 0, program.Length);
				program = newp;
			}
		}

		void Emit (byte val)
		{
			MakeRoom (1);
			program [curpos] = val;
			++curpos;
		}

		void Emit (RxOp opcode)
		{
			Emit ((byte)opcode);
		}

		void Emit (ushort val)
		{
			MakeRoom (2);
			program [curpos] = (byte)val;
			program [curpos + 1] = (byte)(val >> 8);
			curpos += 2;
		}

		void Emit (int val)
		{
			MakeRoom (4);
			program [curpos] = (byte)val;
			program [curpos + 1] = (byte)(val >> 8);
			program [curpos + 2] = (byte)(val >> 16);
			program [curpos + 3] = (byte)(val >> 24);
			curpos += 4;
		}

		void BeginLink (LinkRef lref) {
			RxLinkRef link = lref as RxLinkRef;
			link.PushInstructionBase (curpos);
		}

		void EmitLink (LinkRef lref)
		{
			RxLinkRef link = lref as RxLinkRef;
			link.PushOffsetPosition (curpos);
			Emit ((ushort)0);
		}

		// ICompiler implementation
		public void Reset ()
		{
			curpos = 0;
		}

		public IMachineFactory GetMachineFactory ()
		{
			byte[] code = new byte [curpos];
			Buffer.BlockCopy (program, 0, code, 0, curpos);
			//Console.WriteLine ("Program size: {0}", curpos);

			return new RxInterpreterFactory (code);
		}

		public void EmitFalse ()
		{
			Emit (RxOp.False);
		}

		public void EmitTrue ()
		{
			Emit (RxOp.True);
		}

		public void EmitCharacter (char c, bool negate, bool ignore, bool reverse)
		{
			int offset = 0;
			if (negate)
				offset += 1;
			if (ignore) {
				offset += 2;
				c = Char.ToLower (c);
			}
			if (reverse)
				offset += 4;
			if (c < 256) {
				Emit ((RxOp)((int)RxOp.Char + offset));
				Emit ((byte)c);
			} else {
				Emit ((RxOp)((int)RxOp.UnicodeChar + offset));
				Emit ((ushort)c);
			}
		}

		public void EmitCategory (Category cat, bool negate, bool reverse)
		{
			if (negate | reverse)
				throw new NotSupportedException ();
			switch (cat) {
			case Category.Any:
				Emit (RxOp.CategoryAny);
				break;
			default:
				Console.WriteLine ("Missing cat: {0}", cat);
				throw new NotSupportedException ();
			}
		}

		public void EmitNotCategory (Category cat, bool negate, bool reverse)
		{
			throw new NotSupportedException ();
		}

		public void EmitRange (char lo, char hi, bool negate, bool ignore, bool reverse)
		{
			int offset = 0;
			if (negate)
				offset += 1;
			if (ignore)
				offset += 2;
			if (reverse)
				offset += 4;
			if (lo < 256 && hi < 256) {
				Emit ((RxOp)((int)RxOp.Range + offset));
				Emit ((byte)lo);
				Emit ((byte)hi);
			} else {
				Emit ((RxOp)((int)RxOp.UnicodeRange + offset));
				Emit ((ushort)lo);
				Emit ((ushort)hi);
			}
		}

		public void EmitSet (char lo, BitArray set, bool negate, bool ignore, bool reverse)
		{
			int offset = 0;
			if (negate)
				offset += 1;
			if (ignore)
				offset += 2;
			if (reverse)
				offset += 4;
			int len = (set.Length + 0x7) >> 3;
			if (lo < 256 && len < 256) {
				Emit ((RxOp)((int)RxOp.Bitmap + offset));
				Emit ((byte)lo);
				Emit ((byte)len);
			} else {
				Emit ((RxOp)((int)RxOp.UnicodeBitmap + offset));
				Emit ((ushort)lo);
				Emit ((ushort)len);
			}
			// emit the bitmap bytes
			int b = 0;
			while (len-- != 0) {
				int word = 0;
				for (int i = 0; i < 8; ++ i) {
					if (b >= set.Length)
						break;
					if (set [b ++])
						word |= 1 << i;
				}
				Emit ((byte)word);
			}
		}

		public void EmitString (string str, bool ignore, bool reverse)
		{
			bool islatin1 = false;
			int i;
			int offset = 0;
			if (ignore)
				offset += 1;
			if (reverse)
				offset += 2;
			if (str.Length < 256) {
				islatin1 = true;
				for (i = 0; i < str.Length; ++i) {
					if (str [i] >= 256) {
						islatin1 = false;
						break;
					}
				}
			}
			if (islatin1) {
				Emit ((RxOp)((int)RxOp.String + offset));
				Emit ((byte)str.Length);
				for (i = 0; i < str.Length; ++i)
					Emit ((byte)str [i]);
			} else {
				Emit ((RxOp)((int)RxOp.UnicodeString + offset));
				if (str.Length > ushort.MaxValue)
					throw new NotSupportedException ();
				Emit ((ushort)str.Length);
				for (i = 0; i < str.Length; ++i)
					Emit ((ushort)str [i]);
			}
		}

		public void EmitPosition (Position pos)
		{
			switch (pos) {
			case Position.Any:
				Emit (RxOp.AnyPosition);
				break;
			case Position.Start:
				Emit (RxOp.StartOfString);
				break;
			case Position.StartOfString:
				Emit (RxOp.StartOfString);
				break;
			case Position.StartOfLine:
				Emit (RxOp.StartOfLine);
				break;
			case Position.StartOfScan:
				Emit (RxOp.StartOfScan);
				break;
			case Position.End:
				Emit (RxOp.End);
				break;
			case Position.EndOfString:
				Emit (RxOp.EndOfString);
				break;
			case Position.EndOfLine:
				Emit (RxOp.EndOfLine);
				break;
			case Position.Boundary:
				Emit (RxOp.WordBoundary);
				break;
			case Position.NonBoundary:
				Emit (RxOp.NoWordBoundary);
				break;
			default:
				throw new NotSupportedException ();
			}
		}

		public void EmitOpen (int gid)
		{
			if (gid > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit (RxOp.OpenGroup);
			Emit ((ushort)gid);
		}

		public void EmitClose (int gid)
		{
			if (gid > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit (RxOp.CloseGroup);
			Emit ((ushort)gid);
		}

		public void EmitBalanceStart(int gid, int balance, bool capture,  LinkRef tail)
		{
			throw new NotSupportedException ();
		}

		public void EmitBalance ()
		{
			throw new NotSupportedException ();
		}

		public void EmitReference (int gid, bool ignore, bool reverse)
		{
			int offset = 0;
			if (ignore)
				offset += 1;
			if (reverse)
				offset += 2;
			Emit ((RxOp)((int)RxOp.Reference + offset));
			Emit ((ushort)gid);
		}

		public void EmitIfDefined (int gid, LinkRef tail)
		{
			throw new NotSupportedException ();
		}

		public void EmitSub (LinkRef tail)
		{
			throw new NotSupportedException ();
		}

		public void EmitTest (LinkRef yes, LinkRef tail)
		{
			throw new NotSupportedException ();
		}

		public void EmitBranch (LinkRef next)
		{
			BeginLink (next);
			Emit (RxOp.Branch);
			EmitLink (next);
		}

		public void EmitJump (LinkRef target)
		{
			BeginLink (target);
			Emit (RxOp.Jump);
			EmitLink (target);
		}

		public void EmitRepeat (int min, int max, bool lazy, LinkRef until)
		{
			throw new NotSupportedException ();
		}

		public void EmitUntil (LinkRef repeat)
		{
			throw new NotSupportedException ();
		}

		public void EmitIn (LinkRef tail)
		{
			throw new NotSupportedException ();
		}

		public void EmitInfo (int count, int min, int max)
		{
			Emit (RxOp.Info);
			if (count > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit ((ushort)count);
			Emit (min);
			Emit (max);
		}

		public void EmitFastRepeat (int min, int max, bool lazy, LinkRef tail)
		{
			BeginLink (tail);
			if (lazy)
				Emit (RxOp.RepeatLazy);
			else
				Emit (RxOp.Repeat);
			EmitLink (tail);
			Emit (min);
			Emit (max);
		}

		public void EmitAnchor (bool reverse, int offset, LinkRef tail)
		{
			BeginLink (tail);
			if (reverse)
				Emit (RxOp.AnchorReverse);
			else
				Emit (RxOp.Anchor);
			EmitLink (tail);
			if (offset > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit ((ushort)offset);
		}

		// event for the CILCompiler
		public void EmitBranchEnd ()
		{
		}

		public void EmitAlternationEnd ()
		{
		}

		public LinkRef NewLink ()
		{
			return new RxLinkRef ();
		}

		public void ResolveLink (LinkRef link)
		{
			RxLinkRef l = link as RxLinkRef;
			for (int i = 0; i < l.current; i += 2) {
				int offset = curpos - l.offsets [i];
				if (offset > ushort.MaxValue)
					throw new NotSupportedException ();
				int offsetpos = l.offsets [i + 1];
				program [offsetpos] = (byte)offset;
				program [offsetpos + 1] = (byte)(offset >> 8);
			}
		}

	}

	class RxInterpreterFactory : IMachineFactory {
		public RxInterpreterFactory (byte[] program) {
			this.program = program;
		}
		
		public IMachine NewInstance () {
			return new RxInterpreter (program);
		}

		public int GroupCount {
			get { return program [1] | (program [2] << 8); }
		}

		public IDictionary Mapping {
			get { return mapping; }
			set { mapping = value; }
		}

		public string [] NamesMapping {
			get { return namesMapping; }
			set { namesMapping = value; }
		}

		private IDictionary mapping;
		private byte[] program;
		private string[] namesMapping;
	}

}

