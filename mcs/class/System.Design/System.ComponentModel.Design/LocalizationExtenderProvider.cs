//
// System.ComponentModel.Design.LocalizationExtenderProvider
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Globalization;

namespace System.ComponentModel.Design
{
	[ProvideProperty("Localizable", typeof(object))]
	[ProvideProperty("Language", typeof(object))]
	[ProvideProperty("LoadLanguage", typeof(object))]
	public class LocalizationExtenderProvider : IExtenderProvider, IDisposable
	{
		[MonoTODO]
		public LocalizationExtenderProvider (ISite serviceProvider,
						     IComponent baseComponent)
		{
		}

		[MonoTODO]
		public bool CanExtend (object o)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		[Localizable (true)]
		[DesignOnly (true)]
		public CultureInfo GetLanguage (object o)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		[DesignOnly (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public CultureInfo GetLoadLanguage (object o)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		[Localizable (true)]
		[DesignOnly (true)]
		public bool GetLocalizable (object o)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ResetLanguage (object o)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetLanguage (object o, CultureInfo language)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetLocalizable (object o, bool localizable)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public bool ShouldSerializeLanguage (object o)
		{
			throw new NotImplementedException();
		}
	}
}
