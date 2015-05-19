//
// System.Web.Services.Configuration.SoapExtensionTypeElement
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;


namespace System.Web.Services.Configuration {

	public sealed class SoapExtensionTypeElement : ConfigurationElement
	{
		static ConfigurationProperty groupProp;
		static ConfigurationProperty priorityProp;
		static ConfigurationProperty typeProp;
		static ConfigurationPropertyCollection properties;

		static SoapExtensionTypeElement ()
		{
			groupProp = new ConfigurationProperty ("group", typeof (PriorityGroup), PriorityGroup.Low, ConfigurationPropertyOptions.IsKey);
			priorityProp = new ConfigurationProperty ("priority", typeof (int), 0,
								  new Int32Converter(), new IntegerValidator (0, Int32.MaxValue),
								  ConfigurationPropertyOptions.IsKey);
			typeProp = new ConfigurationProperty ("type", typeof (Type), null,
							      new TypeTypeConverter (),
							      null, ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (groupProp);
			properties.Add (priorityProp);
			properties.Add (typeProp);

		}

		public SoapExtensionTypeElement (Type type, int priority, PriorityGroup group)
		{
			this.Type = type;
			this.Priority = priority;
			this.Group = group;
		}

		[MonoTODO]
		public SoapExtensionTypeElement (string type, int priority, PriorityGroup group)
			: this (Type.GetType (type), priority, group)
		{
		}

		public SoapExtensionTypeElement ()
		{
		}
   
		[ConfigurationProperty ("group", DefaultValue = PriorityGroup.Low, Options = ConfigurationPropertyOptions.IsKey)]
		public PriorityGroup Group {
			get { return (PriorityGroup) base [groupProp];}
			set { base[groupProp] = value; }
		}

		[IntegerValidator (MaxValue = int.MaxValue)]
		[ConfigurationProperty ("priority", DefaultValue = 0, Options = ConfigurationPropertyOptions.IsKey)]
		public int Priority {
			get { return (int) base [priorityProp];}
			set { base[priorityProp] = value; }
		}

		[TypeConverter (typeof (TypeTypeConverter))]
		[ConfigurationProperty ("type", Options = ConfigurationPropertyOptions.IsKey)]
		public Type Type {
			get { return (Type) base [typeProp];}
			set { base[typeProp] = value; }
		}

		internal object GetKey ()
		{
			return String.Format ("{0}-{0}-{0}", Type, Priority, Group);
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}
}


