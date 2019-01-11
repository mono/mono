//
// LoggerInfo.cs: Contains information about logger parameters.
//
// Authors:
//   Craig Sutherland (cj.sutherland(at)xtra.co.nz)
//   Daniel Nauck (dna(at)mono-project.de)
//
// (C) 2009 Craig Sutherland, Daniel Nauck
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


using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.XBuild.Framework;

namespace Mono.XBuild.CommandLine
{
	internal class LoggerInfo : AssemblyLoadInfo
	{
		static readonly Regex assemblyInfoRegEx = new Regex(@"(?<assemblyName>[\w\.]+)(,\s?Version=(?<assemblyVersion>\d+\.\d+\.\d+\.\d+))?(,\s?Culture=(?<assemblyCulture>\w+))?(,\s?PublicKeyToken=(?<publicKeyToken>\w+))?",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		string loggerAssemblyName;
		string loggerType;
		string loggerArgs;

		string assemblyInfoName;
		string assemblyInfoVersion;
		string assemblyInfoCulture;
		string assemblyInfoPublicKeyToken;

		internal LoggerInfo (string value)
		{
			if (!Parse (value))
				return;

			if (string.IsNullOrEmpty (loggerType))
				loggerType = GetLoggerTypeName (loggerAssemblyName);

			if (assemblyInfoName != null)
				SetAssemblyName (LoadInfoType.AssemblyName, null, assemblyInfoName, assemblyInfoVersion, assemblyInfoCulture, assemblyInfoPublicKeyToken, loggerType);

			else
				SetAssemblyName (LoadInfoType.AssemblyFilename, loggerAssemblyName, null, null, null, null, loggerType);
		}

		internal string Parameters {
			get { return loggerArgs; }
		}

		static string GetLoggerTypeName (string assemblyName)
		{
			Assembly loggerAssembly = null;

			// try to load assembly that contains the logger
			if (HasAssemblyInfo (assemblyName))
				loggerAssembly = Assembly.Load (assemblyName);
			else if (File.Exists (assemblyName))
				loggerAssembly = Assembly.LoadFrom (assemblyName);

			if (loggerAssembly == null)
				return null;

			// search for a class thats implement ILogger
			var loggerClass = (from t in loggerAssembly.GetTypes ()
						where t.IsClass &&
						t.GetInterface ("Microsoft.Build.Framework.ILogger") != null &&
						t.IsPublic
						select t).FirstOrDefault ();

			if (loggerClass != null)
				return loggerClass.FullName;

			return null;
		}

		bool Parse (string arg)
		{
			// Wipe all the existing values, just in case
			loggerAssemblyName = null;
			loggerType = null;
			loggerArgs = null;
			assemblyInfoName = null;
			assemblyInfoVersion = null;
			assemblyInfoCulture = null;
			assemblyInfoPublicKeyToken = null;

			if (string.IsNullOrEmpty (arg))
				return false;

			string [] parts = arg.Split (new char [] {':'}, 2);
			if (parts.Length != 2)
				return false;

			if (string.Compare ("/l", parts [0], StringComparison.OrdinalIgnoreCase) != 0 &&
				string.Compare ("/logger", parts [0], StringComparison.OrdinalIgnoreCase) != 0)
				return false;

			arg = parts [1];

			// We have a logger arg, now get the various parts
			parts = arg.Split (new char [] {';'}, 2);
			string firstPart = parts [0];
			if (parts.Length > 1)
				loggerArgs = parts [1];

			// Next see if there is a type name
			parts = firstPart.Split (new char [] {','}, 2);
			if (parts.Length == 1) {
				loggerAssemblyName = firstPart;
			} else {
				if (HasAssemblyInfo (parts [1])) {
					loggerAssemblyName = firstPart;
					GetAssemblyInfo (loggerAssemblyName);
				} else {
					loggerType = parts [0];
					parts [0] = string.Empty;
					loggerAssemblyName = string.Join (",", parts).Substring (1).Trim ();
				}
			}
			
			return true;
		}

		static bool HasAssemblyInfo (string part)
		{
			var containsInfo = (part.IndexOf ("version=", StringComparison.OrdinalIgnoreCase) >= 0) ||
				(part.IndexOf ("culture=", StringComparison.OrdinalIgnoreCase) >= 0) ||
				(part.IndexOf ("publickeytoken=", StringComparison.OrdinalIgnoreCase) >= 0);

			return containsInfo;
		}

		void GetAssemblyInfo (string assemblyName)
		{
			var match = assemblyInfoRegEx.Match (assemblyName);

			if(match == null)
				return;

			assemblyInfoName = match.Groups ["assemblyName"].Value;
			assemblyInfoVersion = match.Groups ["assemblyVersion"].Value;
			assemblyInfoCulture = match.Groups ["assemblyCulture"].Value;
			assemblyInfoPublicKeyToken = match.Groups ["publicKeyToken"].Value;
		}
	}
}
