// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013 Xamarin Inc. http://www.xamarin.com
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
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Mono.ApiTools {

	class MethodComparer : ConstructorComparer {

		public MethodComparer (State state)
			: base (state)
		{
		}

		public override string GroupName {
			get { return "methods"; }
		}

		public override string ElementName {
			get { return "method"; }
		}

		// operators have identical names but vary by return types
		public override bool Find (XElement e)
		{
			if (e.GetAttribute ("name") != Source.GetAttribute ("name"))
				return false;

			if (e.GetAttribute ("returntype") != Source.GetAttribute ("returntype"))
				return false;

			var eGP = e.Element ("generic-parameters");
			var sGP = Source.Element ("generic-parameters");

			if (eGP == null && sGP == null)
				return true;
			else if (eGP == null ^ sGP == null)
				return false;
			else {
				var eGPs = eGP.Elements ("generic-parameter");
				var sGPs = sGP.Elements ("generic-parameter");
				return eGPs.Count () == sGPs.Count ();
			}
		}

		protected override bool IsBreakingRemoval (XElement e)
		{
			// Removing virtual methods that override another method is not a breaking change.
			var is_override = e.Attribute ("is-override");
			if (is_override != null)
				return is_override.Value != "true";
			
			return true; // all other removals are breaking changes
		}
	}
}