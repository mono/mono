//
// System.ComponentModel.ComponentEditor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public abstract class ComponentEditor
	{
		[MonoTODO]
		protected ComponentEditor()
		{
		}

		[MonoTODO]
		public bool EditComponent (object component)
		{
			throw new NotImplementedException();
		}
		
		public abstract bool EditComponent (ITypeDescriptorContext context,
						    object component);

		[MonoTODO]
		~ComponentEditor()
		{
		}
	}
}
