//
// System.ComponentModel.InheritanceAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
	public sealed class InheritanceAttribute : Attribute
	{
		private InheritanceLevel level;

		public static readonly InheritanceAttribute Default = new InheritanceAttribute ();
		public static readonly InheritanceAttribute Inherited = new InheritanceAttribute (InheritanceLevel.Inherited);
		public static readonly InheritanceAttribute InheritedReadOnly = new InheritanceAttribute (InheritanceLevel.InheritedReadOnly);
		public static readonly InheritanceAttribute NotInherited = new InheritanceAttribute (InheritanceLevel.NotInherited);


		public InheritanceAttribute()
		{
			this.level = InheritanceLevel.NotInherited;
		}


		public InheritanceAttribute (InheritanceLevel inheritanceLevel)
		{
			this.level = inheritanceLevel;
		}

		public InheritanceLevel InheritanceLevel {
			get { return level; }
		}


		public override bool Equals (object obj)
		{
			if (!(obj is InheritanceAttribute))
				return false;
			if (obj == this)
				return true;
			return ((InheritanceAttribute) obj).InheritanceLevel == level;
		}


		public override int GetHashCode()
		{
			return level.GetHashCode ();
		}


		public override bool IsDefaultAttribute()
		{
			return level == InheritanceAttribute.Default.InheritanceLevel;
		}


		public override string ToString()
		{
			return this.level.ToString ();
		}
	}
}

