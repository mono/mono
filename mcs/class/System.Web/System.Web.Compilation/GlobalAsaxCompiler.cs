//
// System.Web.Compilation.GlobalAsaxCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
using System.Globalization;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	class GlobalAsaxCompiler : BaseCompiler
	{
		ApplicationFileParser parser;
		static ArrayList applicationObjectTags = new ArrayList (1);
		static ArrayList sessionObjectTags = new ArrayList (1);

		public GlobalAsaxCompiler (ApplicationFileParser parser)
			: base (parser)
		{
			applicationObjectTags.Clear ();
			sessionObjectTags.Clear ();
			this.parser = parser;
		}

		public static Type CompileApplicationType (ApplicationFileParser parser)
		{
			AspGenerator generator = new AspGenerator (parser);
			return generator.GetCompiledType ();
		}

		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();
#if NET_2_0
			CreateProfileProperty ();
#endif
			
			ProcessObjects (parser.RootBuilder);
		}

		void ProcessObjects (ControlBuilder builder)
		{
			if (builder.Children == null)
				return;

			foreach (object t in builder.Children) {
				if (!(t is ObjectTagBuilder))
					continue;

				ObjectTagBuilder tag = (ObjectTagBuilder) t;
				if (tag.Scope == null) {
					string fname = CreateFieldForObject (tag.Type, tag.ObjectID);
					CreatePropertyForObject (tag.Type, tag.ObjectID, fname, true);
					continue;
				}
				
				if (String.Compare (tag.Scope, "session", true, Helpers.InvariantCulture) == 0) {
					sessionObjectTags.Add (tag);
					CreateApplicationOrSessionPropertyForObject (tag.Type, tag.ObjectID,
										     false, false);
				} else if (String.Compare (tag.Scope, "application", true, Helpers.InvariantCulture) == 0) {
					applicationObjectTags.Add (tag);
					CreateFieldForObject (tag.Type, tag.ObjectID);
					CreateApplicationOrSessionPropertyForObject (tag.Type, tag.ObjectID,
										     true, false);
				} else {
					throw new ParseException (tag.Location, "Invalid scope: " + tag.Scope);
				}
			}
		}
		
		internal static ArrayList ApplicationObjects {
			get { return applicationObjectTags; }
		}

		internal static ArrayList SessionObjects {
			get { return sessionObjectTags; }
		}
	}
}

