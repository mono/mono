//
// System.Management.Instrumentation.IgnoreMemberAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{

        [AttributeUsage (AttributeTargets.Method | 
			 AttributeTargets.Property | 
			 AttributeTargets.Field)]
        public class IgnoreMemberAttribute : Attribute {

		[MonoTODO]
		public IgnoreMemberAttribute() 
		{
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~IgnoreMemberAttribute()
		{
		}
	}
}
