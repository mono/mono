//
// System.ComponentModel.ComponentEditor
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
	public abstract class ComponentEditor
	{
		protected ComponentEditor()
		{
		}

		public bool EditComponent (object component)
		{
			return EditComponent (null, component);
		}
		
		public abstract bool EditComponent (ITypeDescriptorContext context, object component);

	}
}

