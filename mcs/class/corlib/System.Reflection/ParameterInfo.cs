// System.Reflection.ParameterInfo
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

namespace System.Reflection
{
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

		[MonoTODO]
		public virtual object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return null;
		}

		[MonoTODO]
		public virtual object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return null;
		}

		[MonoTODO]
		public virtual bool IsDefined( Type attributeType, bool inherit) {
			// FIXME
			return false;
		}
	}
}
