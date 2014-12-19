//
// PredefinedPropertyFunctions.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine
{
	static class PredefinedPropertyFunctions
	{
		public static double Add (double a, double b)
		{
			return a + b;
		}

		public static long Add (long a, long b)
		{
			return a + b;
		}

		public static double Subtract (double a, double b)
		{
			return a - b;			
		}

		public static long Subtract (long a, long b)
		{
			return a - b;
		}

		public static double Multiply (double a, double b)
		{
			return a * b;
		}

		public static long Multiply (long a, long b)
		{
			return a * b;
		}

		public static double Divide (double a, double b)
		{
			return a / b;
		}

		public static long Divide (long a, long b)
		{
			return a / b;
		}

		public static double Modulo (double a, double b)
		{
			return a % b;
		}

		public static long Modulo (long a, long b)
		{
			return a % b;
		}

		public static string Escape (string unescaped)
		{
			return MSBuildUtils.Escape (unescaped);
		}

		public static string Unescape (string escaped)
		{
			return MSBuildUtils.Unescape (escaped);
		}

		public static int BitwiseOr (int first, int second)
		{
			return first | second;
		}

		public static int BitwiseAnd (int first, int second)
		{
			return first & second;
		}

		public static int BitwiseXor (int first, int second)
		{
			return first ^ second;
		}

		public static int BitwiseNot (int first)
		{
			return ~first;
		}

		public static bool DoesTaskHostExist (string theRuntime, string theArchitecture)
		{
			// TODO: What is this actually supposed to do?
			return true;
		}

		public static string GetDirectoryNameOfFileAbove (string path, string file)
		{
			string filePath;
			path = Path.GetFullPath (path);

			while (true) {
				filePath = Path.Combine (path, file);

				if (File.Exists (filePath))
					return path;

				path = Path.GetDirectoryName (path);

				if (path == null)  // we traversed up until root without a match, return empty string
					return "";
			}
		}

		public static object GetRegistryValue (string key, string value)
		{
			throw new NotImplementedException ("GetRegistryValue");
		}

		public static object GetRegistryValueFromView (string key, string value, object defaultValue, params object[] views)
		{
			throw new NotImplementedException ("GetRegistryValueFromView");
		}

		public static string MakeRelative (string basePath, string path)
		{
			throw new NotImplementedException ("MakeRelative");
		}

		public static string ValueOrDefault (string value, string defaultValue)
		{
			return string.IsNullOrEmpty (value) ? defaultValue : value;
		}
	}
}
