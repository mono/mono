// System.Reflection.ParameterInfo
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Reflection.Emit;

namespace System.Reflection
{
	[Serializable]
	public class ParameterInfo : ICustomAttributeProvider
	{
		protected Type ClassImpl;
		protected object DefaultValueImpl;
		protected MemberInfo MemberImpl;
		protected string NameImpl;
		protected int PositionImpl;
		protected ParameterAttributes AttrsImpl;

		protected ParameterInfo () {
		}

		internal ParameterInfo (ParameterBuilder pb, Type type, MemberInfo member, int position) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			if (pb != null) {
				this.NameImpl = pb.Name;
				this.PositionImpl = pb.Position - 1;	// ParameterInfo.Position is zero-based
				this.AttrsImpl = (ParameterAttributes) pb.Attributes;
			} else {
				this.NameImpl = "";
				this.PositionImpl = position - 1;
				this.AttrsImpl = ParameterAttributes.None;
			}
		}

		/* to build a ParameterInfo for the return type of a method */
		internal ParameterInfo (Type type, MemberInfo member) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			this.NameImpl = "";
			this.PositionImpl = -1;	// since parameter positions are zero-based, return type pos is -1
			this.AttrsImpl = ParameterAttributes.Retval;
		}
		
		public virtual Type ParameterType {
			get {return ClassImpl;}
		}
		public virtual ParameterAttributes Attributes {
			get {return AttrsImpl;}
		}
		public virtual object DefaultValue {
			get {return DefaultValueImpl;}
		}

		public bool IsIn {
			get {return (AttrsImpl & ParameterAttributes.In) != 0;}
		}

		public bool IsLcid {
			get {return (AttrsImpl & ParameterAttributes.Lcid) != 0;}
		}

		public bool IsOptional {
			get {return (AttrsImpl & ParameterAttributes.Optional) != 0;}
		}

		public bool IsOut {
			get {return (AttrsImpl & ParameterAttributes.Out) != 0;}
		}

		public bool IsRetval {
			get {return (AttrsImpl & ParameterAttributes.Retval) != 0;}
		}

		public virtual MemberInfo Member {
			get {return MemberImpl;}
		}

		public virtual string Name {
			get {return NameImpl;}
		}

		public virtual int Position {
			get {return PositionImpl;}
		}

		public virtual object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public virtual object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public virtual bool IsDefined( Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}
	}
}
