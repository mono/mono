//
// System.Web.UI.DesignTimeTemplateParser.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Web.UI
{

	public sealed class DesignTimeTemplateParser
	{
		private DesignTimeTemplateParser ()
		{
		}

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

		public static ITemplate ParseTemplate (DesignTimeParseData data)
		{
			TemplateParser NewParser = InitParser (data);
			NewParser.RootBuilder.Text = data.ParseText;
			return NewParser.RootBuilder;
		}

		[MonoTODO]
		private static TemplateParser InitParser (DesignTimeParseData data)
		{
			// TODO create the parser and set data
			TemplateParser NewParser = new PageParser();
			// = data.DesignerHost;
			// = data.DataBindingHandler;
			// = data.ParseText;
			// Parse data
			return NewParser;
		}
	} 
}
