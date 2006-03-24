//
// doc-bootstrap.cs: Stub support for XML documentation.
//
// Author:
//	Raja R Harinath <rharinath@novell.com>
//
// Licensed under the terms of the GNU GPL
//
// (C) 2004 Novell, Inc.
//
//

#if BOOTSTRAP_WITH_OLDLIB

using XmlElement = System.Object;

namespace Mono.CSharp {
	public class DocUtil
	{
		internal static void GenerateTypeDocComment (TypeContainer t, DeclSpace ds)
		{
		}

		internal static void GenerateDocComment (MemberCore mc, DeclSpace ds)
		{
		}

		public static string GetMethodDocCommentName (MethodCore mc, DeclSpace ds)
		{
			return "";
		}

		internal static void OnMethodGenerateDocComment (MethodCore mc, DeclSpace ds, XmlElement el)
		{
		}

		public static void GenerateEnumDocComment (Enum e, DeclSpace ds)
		{
		}
	}

	public class Documentation
	{
		public Documentation (string xml_output_filename)
		{
		}

		public bool OutputDocComment (string asmfilename)
		{
			return true;
		}

		public void GenerateDocComment ()
		{
		}
	}
}

#endif
