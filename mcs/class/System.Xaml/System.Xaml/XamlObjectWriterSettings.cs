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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Permissions;
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlObjectWriterSettings : XamlWriterSettings
	{
		public XamlObjectWriterSettings ()
		{
		}

		public XamlObjectWriterSettings (XamlObjectWriterSettings settings)
			: base (settings)
		{
			var s = settings;
			AccessLevel = s.AccessLevel;
			AfterBeginInitHandler = s.AfterBeginInitHandler;
			AfterEndInitHandler = s.AfterEndInitHandler;
			AfterPropertiesHandler = s.AfterPropertiesHandler;
			BeforePropertiesHandler = s.BeforePropertiesHandler;
			ExternalNameScope = s.ExternalNameScope;
			IgnoreCanConvert = s.IgnoreCanConvert;
			PreferUnconvertedDictionaryKeys = s.PreferUnconvertedDictionaryKeys;
			RegisterNamesOnExternalNamescope = s.RegisterNamesOnExternalNamescope;
			RootObjectInstance = s.RootObjectInstance;
			SkipDuplicatePropertyCheck = s.SkipDuplicatePropertyCheck;
			SkipProvideValueOnRoot = s.SkipProvideValueOnRoot;
			XamlSetValueHandler = s.XamlSetValueHandler;
		}

		public EventHandler<XamlObjectEventArgs> AfterBeginInitHandler { get; set; }
		public EventHandler<XamlObjectEventArgs> AfterEndInitHandler { get; set; }
		public EventHandler<XamlObjectEventArgs> AfterPropertiesHandler { get; set; }
		public EventHandler<XamlObjectEventArgs> BeforePropertiesHandler { get; set; }
		public EventHandler<XamlSetValueEventArgs> XamlSetValueHandler { get; set; }

		[MonoTODO ("Ignored")]
		public XamlAccessLevel AccessLevel { get; set; }
		[MonoTODO ("Ignored")]
		public INameScope ExternalNameScope { get; set; }
		[MonoTODO ("Ignored")]
		public bool IgnoreCanConvert { get; set; }
		[MonoTODO ("Ignored")]
		public bool PreferUnconvertedDictionaryKeys { get; set; }
		[MonoTODO ("Ignored")]
		public bool RegisterNamesOnExternalNamescope { get; set; }
		[MonoTODO ("Ignored")]
		public object RootObjectInstance { get; set; }
		[MonoTODO ("Ignored")]
		public bool SkipDuplicatePropertyCheck { get; set; }
		[MonoTODO ("Ignored")]
		public bool SkipProvideValueOnRoot { get; set; }
	}
}
