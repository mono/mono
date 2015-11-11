using System;
using CustomAttributes;

partial class A
{
	// Partial methods w/o attributes.
	partial void PartialMethodWith_NoAttr_NoDefn(string s);
	partial void PartialMethodWith_NoAttr_Decl(string s);

	// Partial methods w/o a definition.
	[AttributeA("ANoDef")]
	partial void PartialMethodWith_AAttr_NoDefn(string s);
	partial void PartialMethodWith_BAttr_NoDefn([AttributeB("BNoDef")]string s);

	// Attributes only on declaration.
	[AttributeA("ADecl")]
	partial void PartialMethodWith_AAttr_Decl(string s);
	partial void PartialMethodWith_BAttr_Decl([AttributeB("BDecl")]string s);

	// Attributes only on definition.
	partial void PartialMethodWith_AAttr_Defn(string s);
	partial void PartialMethodWith_BAttr_Defn(string s);

	// Different Attribute on definition.
	[AttributeA("WithABAttr")]
	partial void PartialMethodWith_ABAttr(string s);
	partial void PartialMethodWith_BAAttr([AttributeB("WithBAAttr")]string s);
}

partial class A
{
	// Partial methods w/o attributes.
	partial void PartialMethodWith_NoAttr_Decl(string s) { }

	// Attributes only on declaration.
	partial void PartialMethodWith_AAttr_Decl(string s) { }
	partial void PartialMethodWith_BAttr_Decl(string s) { }

	// Attributes only on definition.
	[AttributeA("ADefn")]
	partial void PartialMethodWith_AAttr_Defn(string s) { }
	partial void PartialMethodWith_BAttr_Defn([AttributeB("BDefn")]string s)
	{
	}

	// Different Attribute on definition.
	[AttributeB("ABAttr")]
	partial void PartialMethodWith_ABAttr(string s) { }
	partial void PartialMethodWith_BAAttr([AttributeA("BAAttr")]string s) { }
}

namespace CustomAttributes {
	[AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
	public class AttributeA : Attribute {
		public AttributeA(String a) {}
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
	public class AttributeB : Attribute {
		public AttributeB(String a) {}
	}
}

class X
{
	public static void Main ()
	{
	}
}