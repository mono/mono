//
// System.Web.UI.WebControls.SettingElement.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration
{
	public sealed class SettingElement : ConfigurationElement
	{
		[MonoTODO]
		public SettingElement ()
		{
		}

		[MonoTODO]
		public SettingElement (string name,
				       SettingsSerializeAs serializeAs)
		{
		}

		[MonoTODO]
		[ConfigurationProperty ("name", DefaultValue="",
					Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("value", DefaultValue=null,
					Options = ConfigurationPropertyOptions.IsRequired)]
		public SettingValueElement Value {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("Serialize", DefaultValue=SettingsSerializeAs.String,
					Options = ConfigurationPropertyOptions.IsRequired)]
		public SettingsSerializeAs SerializeAs {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override ConfigurationPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool Equals (object o)
		{
			SettingElement e = o as SettingElement;
			if (e == null)
				return false;

			return e.Name == Name && e.SerializeAs == SerializeAs;
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException();
		}
	}

}

#endif
