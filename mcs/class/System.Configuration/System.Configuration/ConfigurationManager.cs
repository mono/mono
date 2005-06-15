//
// System.Configuration.ConfigurationManager.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
// 	Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.IO;

namespace System.Configuration {

	public abstract class ConfigurationManager
	{
		ConfigurationManager ()
		{
		}
		
		[MonoTODO ("userLevel")]
		public static Configuration OpenExeConfiguration (ConfigurationUserLevel userLevel, string exePath)
		{
			if (exePath == null) {
				exePath = Assembly.GetCallingAssembly ().Location;
			} else if (!File.Exists (exePath)) {
				throw new ArgumentException ("File not found or not readable.", "exePath");
			}

			return new Configuration (exePath + ".config", OpenMachineConfiguration ());
		}

		public static Configuration OpenMachineConfiguration ()
		{
			return new Configuration (System.Runtime.InteropServices.RuntimeEnvironment.SystemConfigurationFile);
		}
	}
}

#endif
