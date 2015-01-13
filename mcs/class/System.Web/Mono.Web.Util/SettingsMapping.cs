//
// Mono.Web.Util.SettingsMapping
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Web.Util
{
	public enum SettingsMappingPlatform
	{
		Windows,
		Unix
	};
  
	internal class SettingsMapping
	{
		string _sectionTypeName;
		Type _sectionType;
		string _mapperTypeName;
		Type _mapperType;
		SettingsMappingPlatform _platform;
		List <SettingsMappingWhat> _whats;
    
		public Type SectionType {
			get {
				if (_sectionType == null)
					_sectionType = Type.GetType (_sectionTypeName, false);
				return _sectionType;
			}
		}

		public Type MapperType {
			get {
				if (_mapperType == null) {
					_mapperType = Type.GetType (_mapperTypeName, true);
					if (!typeof (ISectionSettingsMapper).IsAssignableFrom (_mapperType)) {
						_mapperType = null;
						throw new InvalidOperationException ("Mapper type does not implement the ISectionSettingsMapper interface");
					}
				}

				return _mapperType;
			}
		}

		public SettingsMappingPlatform Platform {
			get { return _platform; }
		}
    
		public SettingsMapping (XPathNavigator nav)
		{
			_sectionTypeName = nav.GetAttribute ("sectionType", String.Empty);
			_mapperTypeName = nav.GetAttribute ("mapperType", String.Empty);

			EnumConverter cvt = new EnumConverter (typeof (SettingsMappingPlatform));
			_platform = (SettingsMappingPlatform) cvt.ConvertFromInvariantString (nav.GetAttribute ("platform", String.Empty));

			LoadContents (nav);
		}

		public object MapSection (object input, Type type)
		{
			if (type != SectionType)
				throw new ArgumentException ("type", "Invalid section type for this mapper");

			ISectionSettingsMapper mapper = Activator.CreateInstance (MapperType) as ISectionSettingsMapper;
			if (mapper == null)
				return input;
      
			return mapper.MapSection (input, _whats);
		}
    
		void LoadContents (XPathNavigator nav)
		{
			XPathNodeIterator iter = nav.Select ("./what[string-length (@value) > 0]");
			_whats = new List <SettingsMappingWhat> ();
			while (iter.MoveNext ())
				_whats.Add (new SettingsMappingWhat (iter.Current));
		}
	}
}
