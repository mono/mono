// LoggerDescription.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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
using System.Reflection;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Logging
{
	public class LoggerDescription
	{
		public LoggerDescription (string loggerClassName, string loggerAssemblyName,
				string loggerAssemblyFile, string loggerSwitchParameters,
				LoggerVerbosity verbosity)
		{
			if (loggerAssemblyName != null && loggerAssemblyFile != null)
				throw new InvalidOperationException ("Cannot specify both loggerAssemblyName and loggerAssemblyFile at the same time.");
			if (loggerAssemblyName == null && loggerAssemblyFile == null)
				throw new InvalidOperationException ("Either loggerAssemblyName or loggerAssemblyFile must be specified");
			class_name = loggerClassName;
			assembly_name = loggerAssemblyName;
			assembly_file = loggerAssemblyFile;
			LoggerSwitchParameters = loggerSwitchParameters;
			Verbosity = verbosity;
		}

		string class_name, assembly_name, assembly_file;

		public string LoggerSwitchParameters { get; private set; }
		public LoggerVerbosity Verbosity { get; private set; }

		public ILogger CreateLogger ()
		{
			var assembly = assembly_name != null ? AppDomain.CurrentDomain.Load (assembly_name) : Assembly.LoadFile (assembly_file);
			var type = assembly.GetType (class_name);
			return (ILogger) Activator.CreateInstance (type, Verbosity);
		}
	}
}

