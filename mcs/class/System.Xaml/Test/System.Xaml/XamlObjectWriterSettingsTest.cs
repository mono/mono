//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlObjectWriterSettingsTest
	{
		[Test]
		public void DefaultValues ()
		{
			var s = new XamlObjectWriterSettings ();
			Assert.IsNull (s.AccessLevel, "#1");
			Assert.IsNull (s.AfterBeginInitHandler, "#2");
			Assert.IsNull (s.AfterEndInitHandler, "#3");
			Assert.IsNull (s.AfterPropertiesHandler, "#4");
			Assert.IsNull (s.BeforePropertiesHandler, "#5");
			Assert.IsNull (s.ExternalNameScope, "#6");
			Assert.IsFalse (s.IgnoreCanConvert, "#7");
			Assert.IsFalse (s.PreferUnconvertedDictionaryKeys, "#8");
			Assert.IsFalse (s.RegisterNamesOnExternalNamescope, "#9");
			Assert.IsNull (s.RootObjectInstance, "#10");
			Assert.IsFalse (s.SkipDuplicatePropertyCheck, "#11");
			Assert.IsFalse (s.SkipProvideValueOnRoot, "#12");
			Assert.IsNull (s.XamlSetValueHandler, "#13");
		}

		[Test]
		public void RootObjectInstance ()
		{
			// bug #689548
			var obj = new RootObjectInstanceTestClass ();
			RootObjectInstanceTestClass result;
			
			var rsettings = new XamlXmlReaderSettings ();
			
			var xml = String.Format (@"<RootObjectInstanceTestClass Property=""Test"" xmlns=""clr-namespace:MonoTests.System.Xaml;assembly={0}""></RootObjectInstanceTestClass>", GetType ().Assembly.GetName ().Name);
			using (var reader = new XamlXmlReader (new StringReader (xml), rsettings)) {
				var wsettings = new XamlObjectWriterSettings ();
				wsettings.RootObjectInstance = obj;
				using (var writer = new XamlObjectWriter (reader.SchemaContext, wsettings)) {
					XamlServices.Transform (reader, writer, false);
					result = (RootObjectInstanceTestClass) writer.Result;
				}
			}
			
			Assert.AreEqual (obj, result, "#1");
			Assert.AreEqual ("Test", obj.Property, "#2");
		}
	}

	public class RootObjectInstanceTestClass
	{
		public String Property { get; set; }
	}
}
