//
// System.Web.Compilation.GlobalAsaxCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
//
using System;
using System.Collections;
using System.Web.UI;

namespace System.Web.Compilation
{
	class GlobalAsaxCompiler : BaseCompiler
	{
		ApplicationFileParser parser;
		static ArrayList applicationObjectTags = new ArrayList (1);
		static ArrayList sessionObjectTags = new ArrayList (1);
		static ArrayList imports;
		static ArrayList assemblies;

		public GlobalAsaxCompiler (ApplicationFileParser parser)
			: base (parser)
		{
			applicationObjectTags.Clear ();
			sessionObjectTags.Clear ();
			
			if (imports != null)
				imports.Clear ();

			if (assemblies != null)
				assemblies.Clear ();

			this.parser = parser;
		}

		public static Type CompileApplicationType (ApplicationFileParser parser)
		{
			AspGenerator generator = new AspGenerator (parser);
			Type type = generator.GetCompiledType ();
			imports = parser.Imports;
			assemblies = parser.Assemblies;
			return type;
		}

		protected override void CreateMethods ()
		{
			base.CreateMethods ();

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
				
				if (tag.Scope == "session") {
					sessionObjectTags.Add (tag);
					CreateApplicationOrSessionPropertyForObject (tag.Type, tag.ObjectID,
										     false, false);
				} else if (tag.Scope == "application") {
					applicationObjectTags.Add (tag);
					CreateFieldForObject (tag.Type, tag.ObjectID);
					CreateApplicationOrSessionPropertyForObject (tag.Type, tag.ObjectID,
										     true, false);
				} else {
					throw new ParseException (tag.location, "Invalid scope: " + tag.Scope);
				}
			}
		}
		
		internal static ArrayList ApplicationObjects {
			get { return applicationObjectTags; }
		}

		internal static ArrayList SessionObjects {
			get { return sessionObjectTags; }
		}

		internal static ArrayList Assemblies {
			get { return assemblies; }
		}

		internal static ArrayList Imports {
			get { return imports; }
		}
	}
}

