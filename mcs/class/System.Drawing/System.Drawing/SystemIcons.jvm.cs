//
// System.Drawing.systemIcons.cs
//
// Authors:
//  Vladimir Krasnov (vladimirk@mainsoft.com)
//  Konstantin Triger (kostat@mainsoft.com)
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
using System.Reflection;

namespace System.Drawing {
	public sealed class SystemIcons {
		static readonly Icon[] systemIcons;

		enum IconName {
			Application, Asterisk, Error, 
			Exclamation, Hand, Information, 
			Question, Warning, WinLogo
		}

		static SystemIcons() {
			
			Type nameType = typeof(IconName);
			string [] iconNames = Enum.GetNames(nameType);
			systemIcons = new Icon[iconNames.Length];
			Assembly assembly = Assembly.GetExecutingAssembly();
			for (int i = 0; i < iconNames.Length; i++)
				systemIcons[(int)(IconName)Enum.Parse(nameType, iconNames[i])] = 
					new Icon(assembly.GetManifestResourceStream(String.Format("System.Drawing.Assembly.{0}.ico", iconNames[i])));
		}

		private SystemIcons() {
		}

		public static Icon Application { get { return systemIcons[(int)IconName.Application];} }
		public static Icon Asterisk { get { return systemIcons[(int)IconName.Asterisk];} }
		public static Icon Error { get { return systemIcons[(int)IconName.Error];} }
		public static Icon Exclamation { get { return systemIcons[(int)IconName.Exclamation];} }
		public static Icon Hand { get { return systemIcons[(int)IconName.Hand];} }
		public static Icon Information { get { return systemIcons[(int)IconName.Information];} }
		public static Icon Question { get { return systemIcons[(int)IconName.Question];} }
		public static Icon Warning { get { return systemIcons[(int)IconName.Warning];} }
		public static Icon WinLogo { get { return systemIcons[(int)IconName.WinLogo];} }
	}
}
