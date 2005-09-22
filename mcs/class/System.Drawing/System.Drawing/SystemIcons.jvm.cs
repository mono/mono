//
// System.Drawing.SystemIcons.cs
//
// Authors:
//  Vladimir Krasnov (vladimirk@mainsoft.com)
//
// Copyright (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.IO;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;

namespace System.Drawing
{
	public sealed class SystemIcons
	{
		private static readonly string SYSTEM_ICONS = "IconsResource";

		private SystemIcons()
		{
		}

		public static Icon Application { get { return LoadIcon("Application");} }
		public static Icon Asterisk { get { return LoadIcon("Asterisk");} }
		public static Icon Error { get { return LoadIcon("Error");} }
		public static Icon Exclamation { get { return LoadIcon("Exclamation");} }
		public static Icon Hand { get { return LoadIcon("Hand");} }
		public static Icon Information { get { return LoadIcon("Information");} }
		public static Icon Question { get { return LoadIcon("Question");} }
		public static Icon Warning { get { return LoadIcon("Warning");} }
		public static Icon WinLogo { get { return LoadIcon("WinLogo");} }

		private static Icon LoadIcon (string iconName)
		{
			Hashtable systemIcons = (Hashtable)AppDomain.CurrentDomain.GetData("SYSTEM_ICONS");

			if (systemIcons == null)
			{
				systemIcons = LoadSystemIconsFromResource();
				AppDomain.CurrentDomain.SetData(SYSTEM_ICONS, systemIcons);
			}
		
			return (Icon)systemIcons[iconName];
		}

		private static Hashtable LoadSystemIconsFromResource()
		{
			string [] iconNames = {
									"Application", "Asterisk", "Error", 
									"Exclamation", "Hand", "Information", 
									"Question", "Warning", "WinLogo"
								  };

			NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig ("system.drawing/icons");
			Hashtable icons = new Hashtable(9);

			for (int i = 0; i < iconNames.Length; i++)
				icons.Add(iconNames[i], LoadIconFromResource( config[ iconNames[i] ] ));

			return icons;
		}

		private static Icon LoadIconFromResource(string iconName)
		{
			Stream s;
			try
			{
				java.lang.ClassLoader cl = (java.lang.ClassLoader)
					AppDomain.CurrentDomain.GetData("GH_ContextClassLoader");
				if (cl == null)
					return null;
				java.io.InputStream inputStream = cl.getResourceAsStream(iconName);
				s = (Stream)vmw.common.IOUtils.getStream(inputStream);
			}
			catch (Exception e)
			{
				return null;
			}
			return new Icon(new Bitmap(s));
		}
	}
}
