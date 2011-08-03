//
// System.Web.Configuration.OutputCacheProfile
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.ComponentModel;
using System.Configuration;
using System.Web.UI;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class OutputCacheProfile : ConfigurationElement
	{
		static ConfigurationProperty durationProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty locationProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty noStoreProp;
		static ConfigurationProperty sqlDependencyProp;
		static ConfigurationProperty varyByContentEncodingProp;
		static ConfigurationProperty varyByControlProp;
		static ConfigurationProperty varyByCustomProp;
		static ConfigurationProperty varyByHeaderProp;
		static ConfigurationProperty varyByParamProp;
		static ConfigurationPropertyCollection properties;

		static OutputCacheProfile ()
		{
			durationProp = new ConfigurationProperty ("duration", typeof (int), -1);
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), true);
			locationProp = new ConfigurationProperty ("location", typeof (OutputCacheLocation), null,
								  new GenericEnumConverter (typeof (OutputCacheLocation)),
								  PropertyHelper.DefaultValidator,
								  ConfigurationPropertyOptions.None);
			nameProp = new ConfigurationProperty ("name", typeof (string), "",
							      PropertyHelper.WhiteSpaceTrimStringConverter,
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			noStoreProp = new ConfigurationProperty ("noStore", typeof (bool), false);
			sqlDependencyProp = new ConfigurationProperty ("sqlDependency", typeof (string));
			varyByContentEncodingProp = new ConfigurationProperty ("varyByContentEncoding", typeof (string));
			varyByControlProp = new ConfigurationProperty ("varyByControl", typeof (string));
			varyByCustomProp = new ConfigurationProperty ("varyByCustom", typeof (string));
			varyByHeaderProp = new ConfigurationProperty ("varyByHeader", typeof (string));
			varyByParamProp = new ConfigurationProperty ("varyByParam", typeof (string));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (durationProp);
			properties.Add (enabledProp);
			properties.Add (locationProp);
			properties.Add (nameProp);
			properties.Add (noStoreProp);
			properties.Add (sqlDependencyProp);
			properties.Add (varyByContentEncodingProp);
			properties.Add (varyByControlProp);
			properties.Add (varyByCustomProp);
			properties.Add (varyByHeaderProp);
			properties.Add (varyByParamProp);
		}

		internal OutputCacheProfile ()
		{
		}

		public OutputCacheProfile (string name)
		{
			this.Name = name;
		}

		[ConfigurationProperty ("duration", DefaultValue = "-1")]
		public int Duration {
			get { return (int) base [durationProp];}
			set { base[durationProp] = value; }
		}

		[ConfigurationProperty ("enabled", DefaultValue = "True")]
		public bool Enabled {
			get { return (bool) base [enabledProp];}
			set { base[enabledProp] = value; }
		}

		[ConfigurationProperty ("location")]
		public OutputCacheLocation Location {
			get { return (OutputCacheLocation) base [locationProp];}
			set { base[locationProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [nameProp];}
			set { base[nameProp] = value; }
		}

		[ConfigurationProperty ("noStore", DefaultValue = "False")]
		public bool NoStore {
			get { return (bool) base [noStoreProp];}
			set { base[noStoreProp] = value; }
		}

		[ConfigurationProperty ("sqlDependency")]
		public string SqlDependency {
			get { return (string) base [sqlDependencyProp];}
			set { base[sqlDependencyProp] = value; }
		}

		[ConfigurationPropertyAttribute("varyByContentEncoding")]
		public string VaryByContentEncoding {
			get { return (string) base [varyByContentEncodingProp]; }
			set { base [varyByContentEncodingProp] = value; }
		}
		
		[ConfigurationProperty ("varyByControl")]
		public string VaryByControl {
			get { return (string) base [varyByControlProp];}
			set { base[varyByControlProp] = value; }
		}

		[ConfigurationProperty ("varyByCustom")]
		public string VaryByCustom {
			get { return (string) base [varyByCustomProp];}
			set { base[varyByCustomProp] = value; }
		}

		[ConfigurationProperty ("varyByHeader")]
		public string VaryByHeader {
			get { return (string) base [varyByHeaderProp];}
			set { base[varyByHeaderProp] = value; }
		}

		[ConfigurationProperty ("varyByParam")]
		public string VaryByParam {
			get { return (string) base [varyByParamProp];}
			set { base[varyByParamProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

