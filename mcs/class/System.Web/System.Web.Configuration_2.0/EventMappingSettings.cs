//
// System.Web.Configuration.EventMappingSettings
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


namespace System.Web.Configuration {

	public sealed class EventMappingSettings : ConfigurationElement
	{
		static ConfigurationProperty endEventCodeProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty startEventCodeProp;
		static ConfigurationProperty typeProp;
		static ConfigurationPropertyCollection properties;

		static EventMappingSettings ()
		{
			endEventCodeProp = new ConfigurationProperty ("endEventCode", typeof (int), Int32.MaxValue,
								      TypeDescriptor.GetConverter (typeof (int)),
								      PropertyHelper.IntFromZeroToMaxValidator,
								      ConfigurationPropertyOptions.None);
			nameProp = new ConfigurationProperty ("name", typeof (string), "",
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			startEventCodeProp = new ConfigurationProperty ("startEventCode", typeof (int), 0,
									TypeDescriptor.GetConverter (typeof (int)),
									PropertyHelper.IntFromZeroToMaxValidator,
									ConfigurationPropertyOptions.None);
			typeProp = new ConfigurationProperty ("type", typeof (string), "", ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (endEventCodeProp);
			properties.Add (nameProp);
			properties.Add (startEventCodeProp);
			properties.Add (typeProp);
		}

		internal EventMappingSettings ()
		{
		}

		public EventMappingSettings (string name, string type)
		{
			this.Name = name;
			this.Type = type;
		}

		public EventMappingSettings (string name, string type, int startEventCode, int endEventCode)
		{
			this.Name = name;
			this.Type = type;
			this.StartEventCode = startEventCode;
			this.EndEventCode = endEventCode;
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("endEventCode", DefaultValue = "2147483647")]
		public int EndEventCode {
			get { return (int) base [endEventCodeProp];}
			set { base[endEventCodeProp] = value; }
		}

		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public string Name {
			get { return (string) base [nameProp];}
			set { base[nameProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("startEventCode", DefaultValue = "0")]
		public int StartEventCode {
			get { return (int) base [startEventCodeProp];}
			set { base[startEventCodeProp] = value; }
		}

		[ConfigurationProperty ("type", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Type {
			get { return (string) base [typeProp];}
			set { base[typeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}


