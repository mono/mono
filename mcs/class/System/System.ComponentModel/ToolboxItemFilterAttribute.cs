//
// System.ComponentModel.ToolboxItemFilterAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
        public sealed class ToolboxItemFilterAttribute : Attribute
	{
		[MonoTODO]
		public ToolboxItemFilterAttribute (string filterString)
		{
		}

		[MonoTODO]
		public ToolboxItemFilterAttribute (string filterString,
						   ToolboxItemFilterType filterType)
		{
		}

		public string FilterString {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public ToolboxItemFilterType FilterType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public override object TypeId {
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
		public override bool Match (object obj)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ToolboxItemFilterAttribute()
		{
		}
	}
}
