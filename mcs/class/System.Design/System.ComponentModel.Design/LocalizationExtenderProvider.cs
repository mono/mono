//
// System.ComponentModel.Design.LocalizationExtenderProvider
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;

namespace System.ComponentModel.Design
{
#if NET_2_0
	[Obsolete ("use CodeDomLocalizationProvider")]
#endif
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

#if NET_2_0
		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}
#endif

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
