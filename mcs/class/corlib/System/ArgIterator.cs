//
// System.ArgIterator.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System 
{
	public struct ArgIterator
	{
		[MonoTODO]
		public ArgIterator(RuntimeArgumentHandle arglist)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		[CLSCompliant(false)]
		unsafe public ArgIterator(RuntimeArgumentHandle arglist,
					  void *ptr)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void End()
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object o)
		{
			throw new NotSupportedException("This operation is not supported for this type");
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		[CLSCompliant(false)]
		public TypedReference GetNextArg()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		[CLSCompliant(false)]
		public TypedReference GetNextArg(RuntimeTypeHandle rth)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public RuntimeTypeHandle GetNextArgType()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int GetRemainingCount()
		{
			throw new NotImplementedException();
		}
	}
}
