// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;

namespace Moonlight.SecurityModel {

	static public class PlatformCode {

		// Assemblies normally found in "C:\Program Files\Microsoft Silverlight\2.0.31005.0"
		// and, if you have the SDK in "C:\Program Files\Microsoft SDKs\Silverlight\v2.0\Reference Assemblies"

		// [1] does not contain any [SecurityCritical] or [SecuritySafeCritical] attribute
		// [2] has a different public key than the other assemblies

		// Both [1] and [2] may be considered platform code - but since they don't 
		// (but, I guess, eventually could) use [SecurityCritical] nor [SecuritySafeCritical]
		// they are in effect totally transparent (like application code).

		static string [] platform_code_assemblies = {
			"mscorlib",
			"Microsoft.VisualBasic",	// [1][2]
			"System",
			"System.Core",
			"System.Net",
			"System.Runtime.Serialization",
			"System.ServiceModel",		// [1][2]
			"System.ServiceModel.Web",	// [1]
			"System.Windows",
			"System.Windows.Browser",
			"System.Xml"
		};

		static public IEnumerable<string> Assemblies {
			get { return platform_code_assemblies; }
		}
	}
}
