//
// System.ComponentModel.Design.InheritanceAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	[AttributeUsage(AttributeTargets.Property | 
			AttributeTargets.Field
			| AttributeTargets.Event)]
        public sealed class InheritanceAttribute : Attribute
	{
		[MonoTODO]
		public InheritanceAttribute()
		{
		}

		[MonoTODO]
		public InheritanceAttribute (InheritanceLevel inheritanceLevel)
		{
		}

		public static readonly InheritanceAttribute Default;
		public static readonly InheritanceAttribute Inherited;
		public static readonly InheritanceAttribute InheritedReadOnly;
		public static readonly InheritanceAttribute NotInherited;
		
		public InheritanceLevel InheritanceLevel {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
	        public override bool Equals (object value)
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
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~InheritanceAttribute()
		{
		}
	}
}
