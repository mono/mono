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
using System.Text;
using System.Xml.Linq;

namespace Mono.ApiTools {

	class EventComparer : MemberComparer {

		public EventComparer (State state)
			: base (state)
		{
		}

		public override string GroupName {
			get { return "events"; }
		}

		public override string ElementName {
			get { return "event"; }
		}

		public override bool Equals (XElement source, XElement target, ApiChanges changes)
		{
			if (base.Equals (source, target, changes))
				return true;

			var change = new ApiChange (GetDescription (source), State);
			change.Header = "Modified " + GroupName;
			change.Append ("public event ");

			var srcEventType = source.GetTypeName ("eventtype", State);
			var tgtEventType = target.GetTypeName ("eventtype", State);

			if (srcEventType != tgtEventType) {
				change.AppendModified (srcEventType, tgtEventType, true);
			} else {
				change.Append (srcEventType);
			}
			change.Append (" ");
			change.Append (source.GetAttribute ("name")).Append (";");
			return false;
		}

		public override string GetDescription (XElement e)
		{
			StringBuilder sb = new StringBuilder ();
			// TODO: attribs
			sb.Append ("public event ");
			sb.Append (e.GetTypeName ("eventtype", State)).Append (' ');
			sb.Append (e.GetAttribute ("name")).Append (';');
			return sb.ToString ();
		}
	}
}