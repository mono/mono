//
// System.ComponentModel.PropertyTabAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
        public class PropertyTabAttribute : Attribute
	{
		[MonoTODO]
		public PropertyTabAttribute()
		{
		}

		[MonoTODO]
		public PropertyTabAttribute (string tabClassName)
		{
		}

		[MonoTODO]
		public PropertyTabAttribute (Type tabClass)
		{
		}

		[MonoTODO]
		public PropertyTabAttribute (string tabClassName, 
					     PropertyTabScope tabScope)
		{
		}

		[MonoTODO]
		public PropertyTabAttribute (Type tabClass,
					     PropertyTabScope tabScope)
		{
		}

		public Type[] TabClasses {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public PropertyTabScope[] TabScopes {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override bool Equals (object other)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public bool Equals (PropertyTabAttribute other)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		protected string[] TabClassNames {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		protected void InitializeArrays (string[] tabClassNames,
						 PropertyTabScope[] tabScopes)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected void InitializeArrays (Type[] tabClasses,
						 PropertyTabScope[] tabScopes)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~PropertyTabAttribute()
		{
		}
	}
}
