//
// System.ArgIterator.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System 
{
	public struct ArgIterator
	{
		IntPtr sig;
		IntPtr args;
		int    next_arg;
		int    num_args;

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern void Setup (IntPtr argsp, IntPtr start);

		public ArgIterator (RuntimeArgumentHandle arglist)
		{
			sig = IntPtr.Zero;
			args = IntPtr.Zero;
			next_arg = num_args = 0;
			Setup (arglist.args, IntPtr.Zero);
		}

		[CLSCompliant (false)]
		unsafe public ArgIterator (RuntimeArgumentHandle arglist, void *ptr)
		{
			sig = IntPtr.Zero;
			args = IntPtr.Zero;
			next_arg = num_args = 0;
			Setup (arglist.args, (IntPtr) ptr);
		}

		public void End ()
		{
			next_arg = num_args;
		}

		public override bool Equals (object o)
		{
			throw new NotSupportedException("This operation is not supported for this type");
		}

		public override int GetHashCode ()
		{
			return sig.GetHashCode ();
		}

		[CLSCompliant (false)]
		public TypedReference GetNextArg ()
		{
			if (num_args == next_arg)
				throw new InvalidOperationException ("invalid iterator position");
			return IntGetNextArg ();
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern TypedReference IntGetNextArg ();

		[CLSCompliant (false)]
		public TypedReference GetNextArg (RuntimeTypeHandle rth)
		{
			if (num_args == next_arg)
				throw new InvalidOperationException ("invalid iterator position");
			return IntGetNextArg (rth);
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern TypedReference IntGetNextArg (RuntimeTypeHandle rth);

		public RuntimeTypeHandle GetNextArgType ()
		{
			if (num_args == next_arg)
				throw new InvalidOperationException ("invalid iterator position");
			return new RuntimeTypeHandle (IntGetNextArgType ());
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern IntPtr IntGetNextArgType ();

		public int GetRemainingCount ()
		{
			return num_args - next_arg;
		}
	}
}
