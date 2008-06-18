//
// System.ComponentModel.Design.ExtenderService
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006 Ivan N. Zlatev

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

#if NET_2_0
using System;
using System.Collections;
using System.ComponentModel;

namespace System.ComponentModel.Design
{

	internal sealed class ExtenderService : IExtenderProviderService, IExtenderListService, IDisposable
	{

		private ArrayList _extenderProviders;

		public ExtenderService ()
		{
			_extenderProviders = new ArrayList ();
		}
		
		public void AddExtenderProvider (IExtenderProvider provider)
		{
			if (_extenderProviders != null) {
				if (!_extenderProviders.Contains (provider))
					_extenderProviders.Add (provider);
			}
		}

		public void RemoveExtenderProvider (IExtenderProvider provider)
		{
			if (_extenderProviders != null) {
				if (_extenderProviders.Contains (provider))
					_extenderProviders.Remove (provider);
			}
		}
		
		public IExtenderProvider[] GetExtenderProviders()
		{
			if (_extenderProviders != null) {
				IExtenderProvider[] result = new IExtenderProvider[_extenderProviders.Count];
				_extenderProviders.CopyTo (result, 0);
				
				return result;
			}
			return null;
		}

		public void Dispose ()
		{
			if (_extenderProviders != null) {
				_extenderProviders.Clear ();
				_extenderProviders = null;
			}
		}
	}
}

#endif
