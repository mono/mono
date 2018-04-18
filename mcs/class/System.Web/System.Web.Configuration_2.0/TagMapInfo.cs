//
// System.Web.Configuration.TagMapInfo
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
using System.Xml;

namespace System.Web.Configuration
{
	public sealed class TagMapInfo : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty mappedTagTypeProp;
		static ConfigurationProperty tagTypeProp;


		static TagMapInfo ()
		{
			mappedTagTypeProp = new ConfigurationProperty ("mappedTagType", typeof (string), null,
								       TypeDescriptor.GetConverter (typeof (string)),
								       PropertyHelper.NonEmptyStringValidator,
								       ConfigurationPropertyOptions.None);
			tagTypeProp = new ConfigurationProperty ("tagType", typeof (string), "",
								 TypeDescriptor.GetConverter (typeof (string)),
								 PropertyHelper.NonEmptyStringValidator,
								 ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (mappedTagTypeProp);
			properties.Add (tagTypeProp);
		}

		internal TagMapInfo ()
		{
		}
		
		public TagMapInfo (string tagTypeName, string mappedTagTypeName)
		{
			this.TagType = tagTypeName;
			this.MappedTagType = mappedTagTypeName;
		}

		public override bool Equals (object o)
		{
			TagMapInfo info = o as TagMapInfo;
			if (info == null)
				return false;

			return (MappedTagType == info.MappedTagType
				&& TagType == info.TagType);
		}

		public override int GetHashCode ()
		{
			return MappedTagType.GetHashCode() + TagType.GetHashCode();
		}

		protected internal override bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			bool ret = base.SerializeElement (writer, serializeCollectionKey);

			/* XXX more here? .. */

			return ret;
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("mappedTagType")]
		public string MappedTagType {
			get { return (string) base[mappedTagTypeProp]; }
			set { base[mappedTagTypeProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("tagType", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string TagType {
			get { return (string) base[tagTypeProp]; }
			set { base[tagTypeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

