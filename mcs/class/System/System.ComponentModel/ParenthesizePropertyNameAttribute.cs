//
// System.ComponentModel.ParenthesizePropertyNameAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
        public sealed class ParenthesizePropertyNameAttribute : Attribute
	{
		public static readonly ParenthesizePropertyNameAttribute Default;

		[MonoTODO]
		public ParenthesizePropertyNameAttribute()
		{
		}

		[MonoTODO]
		public ParenthesizePropertyNameAttribute (bool needParenthesis)
		{
		}

		public bool NeedParenthesis {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override bool Equals (object o)
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
		~ParenthesizePropertyNameAttribute()
		{
		}
	}
}
