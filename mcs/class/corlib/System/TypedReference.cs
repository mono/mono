//
// System.TypedReference.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;

namespace System 
{
	[CLSCompliant(false)]
	public struct TypedReference 
	{
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
		public static Type GetTargetType(TypedReference value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static TypedReference MakeTypedReference(object target, FieldInfo[] flds) 
		{
			if(target==null) {
				throw new ArgumentNullException("target is null");
			}
			if(flds==null) {
				throw new ArgumentNullException("flds is null");
			}
			if(flds.Length==0) {
				throw new ArgumentException("flds has no elements");
			}
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static void SetTypedReference(TypedReference target,
						     object value) 
		{
			if(value==null) {
				throw new ArgumentNullException("value is null");
			}
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public static RuntimeTypeHandle TargetTypeToken(TypedReference value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static object ToObject(TypedReference value)
		{
			throw new NotImplementedException();
		}
	}
}
