//
// IXmlCompilerInclude.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

#if NET_2_0
using System.Xml;
using MS.Internal.Xml.Query;

namespace System.Xml.Query
{
	public interface IXmlCompilerInclude
	{
		XmlExpression ResolveContextDocument ();

		XmlExpression ResolveFunction (XmlQualifiedName name, object [] parameters);

		XmlExpression ResolveVariable (XmlQualifiedName varName);
	}
}

// FIXME: This class should be in System.Xml in the future, but MS still keeps
// this class in MS.Internal

namespace MS.Internal.Xml.Query
{
	public class XmlExpression
	{
	}
}
#endif
