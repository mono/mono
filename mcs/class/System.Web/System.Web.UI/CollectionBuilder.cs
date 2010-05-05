//
// System.Web.UI.CollectionBuilder.cs
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections;
using System.Reflection;
using System.Text;

namespace System.Web.UI
{
	sealed class CollectionBuilder : ControlBuilder
	{
		Type[] possibleElementTypes;

		internal CollectionBuilder ()
		{
		}

		public override void AppendLiteralString (string s)
		{
			if (s != null && s.Trim ().Length > 0)
				throw new HttpException ("Literal content not allowed for " + ControlType);
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs)
		{
			Type t = Root.GetChildControlType (tagName, attribs);
			if (possibleElementTypes != null) {
                               bool foundMatchingType = false;
                               for (int i = 0; i < possibleElementTypes.Length && !foundMatchingType; ++i)
                                       foundMatchingType = possibleElementTypes[i].IsAssignableFrom (t);
                               
                               if (!foundMatchingType) {
                                       StringBuilder possibleTypesString = new StringBuilder ();
                                       for (int i = 0; i < possibleElementTypes.Length; i++) {
                                               if (i != 0)
                                                       possibleTypesString.Append (", ");
                                               possibleTypesString.Append (possibleElementTypes[i]);
                                       }
                                       throw new HttpException ("Cannot add a " + t + " to " + possibleTypesString);
                               }
			}

			return t;
		}

		public override void Init (TemplateParser parser,
					   ControlBuilder parentBuilder,
					   Type type,
					   string tagName,
					   string id,
					   IDictionary attribs)
		{			
			base.Init (parser, parentBuilder, type, tagName, id, attribs);

			PropertyInfo prop = parentBuilder.ControlType.GetProperty (tagName, FlagsNoCase);
			SetControlType (prop.PropertyType);

			MemberInfo[] mems = ControlType.GetMember ("Item", MemberTypes.Property, FlagsNoCase & ~BindingFlags.IgnoreCase);
			if (mems.Length > 0) {
                               possibleElementTypes = new Type [mems.Length];
                               for (int i = 0; i < mems.Length; ++i)
                                       possibleElementTypes [i] = ((PropertyInfo)mems [i]).PropertyType;
			} else
				throw new HttpException ("Collection of type '" + ControlType + "' does not have an indexer.");
		}
	}
}

