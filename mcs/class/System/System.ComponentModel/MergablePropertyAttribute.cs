//
// System.ComponentModel.MergablePropertyAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
        public sealed class MergablePropertyAttribute : Attribute
	{
		public static readonly MergablePropertyAttribute No;
		public static readonly MergablePropertyAttribute Yes;

		[MonoTODO]
		public MergablePropertyAttribute (bool allowMerge)
		{
		}

		public bool AllowMerge {
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
		~MergablePropertyAttribute()
		{
		}
	}
}
