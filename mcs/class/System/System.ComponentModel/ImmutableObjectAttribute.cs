//
// System.ComponentModel.ImmutableObjectAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
        public sealed class ImmutableObjectAttribute : Attribute
	{
		public static readonly ImmutableObjectAttribute No;
		public static readonly ImmutableObjectAttribute Yes;

		[MonoTODO]
		public ImmutableObjectAttribute (bool immutable)
		{
		}

		public bool Immutable {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool IsDefaultAttribute()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ImmutableObjectAttribute()
		{
		}
	}
}
