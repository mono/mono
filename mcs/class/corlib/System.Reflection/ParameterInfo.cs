// System.Reflection.ParameterInfo
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

namespace System.Reflection
{
	public class ParameterInfo
	{

		public virtual Type ParameterType {
			get {return null;}
		}
		public virtual ParameterAttributes Attributes {get{return(ParameterAttributes)0;}}
		public virtual object DefaultValue {get{return null;}}

		public bool IsIn {get{return false;}}

		public bool IsLcid {get{return false;}}

		public bool IsOptional {get{return false;}}

		public bool IsOut {get{return false;}}

		public bool IsRetval {get{return false;}}

		public virtual MemberInfo Member {get{return null;}}

		public virtual string Name {get{return null;}}

		public virtual int Position {get{return 0;}}

		public virtual object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return null;
		}

		public virtual object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return null;
		}
	}
}
