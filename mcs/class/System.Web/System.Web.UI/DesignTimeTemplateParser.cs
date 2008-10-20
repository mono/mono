//
// System.Web.UI.DesignTimeTemplateParser.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	public static class DesignTimeTemplateParser {
#else
	public sealed class DesignTimeTemplateParser {

		DesignTimeTemplateParser ()
		{
		}
#endif
		[SecurityPermission (SecurityAction.Demand, ControlThread = true, UnmanagedCode = true)]
		public static Control ParseControl (DesignTimeParseData data)
		{
			TemplateParser NewParser = InitParser (data);
			NewParser.RootBuilder.Text = data.ParseText;
			if (NewParser.RootBuilder.Children == null)
				return null;
			foreach (ControlBuilder builder in NewParser.RootBuilder.Children)
				return (Control) builder.CreateInstance ();
			return null;
		}

		[SecurityPermission (SecurityAction.Demand, ControlThread = true, UnmanagedCode = true)]
		public static ITemplate ParseTemplate (DesignTimeParseData data)
		{
			TemplateParser NewParser = InitParser (data);
			NewParser.RootBuilder.Text = data.ParseText;
			return NewParser.RootBuilder;
		}

		[MonoTODO]
		static TemplateParser InitParser (DesignTimeParseData data)
		{
			// TODO create the parser and set data
			TemplateParser NewParser = new PageParser(); // see FIXME in PageParser
			// = data.DesignerHost;
			// = data.DataBindingHandler;
			// = data.ParseText;
			// Parse data
			return NewParser;
		}
#if NET_2_0
		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlThread = true, UnmanagedCode = true)]
		public static Control[] ParseControls (DesignTimeParseData data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlThread = true)]
		public static ControlBuilder ParseTheme (IDesignerHost host, string theme, string themePath)
		{
			throw new NotImplementedException ();
		}
#endif
	} 
}
