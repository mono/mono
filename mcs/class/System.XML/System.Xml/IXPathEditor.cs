//
// IXPathEditor.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_2_0

using System;

namespace System.Xml
{
	public interface IXPathEditor
	{
		XmlWriter CreateAttributes ();

		XmlWriter CreateFirstChild ();

		XmlWriter CreateNextSibling ();

		void DeleteCurrent ();

		void SetValue (string value);
	}


}
#endif
