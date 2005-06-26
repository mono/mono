//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//
using System;
using System.Globalization;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	///
	/// <summary>
	///	Test ICodeGenerator's GenerateCodeFromType, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromTypeTest : CodeGeneratorTestBase
	{
		CodeTypeDeclaration type = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			type = new CodeTypeDeclaration ();
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromType (type, writer, options);
			writer.Close ();
		}
		
		[Test]
		public void DefaultTypeTest ()
		{
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class {0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullTypeTest ()
		{
			type = null;
			Generate ();
		}

		[Test]
		public void SimpleTypeTest ()
		{
			type.Name = "Test1";
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void AttributesAndTypeTest ()
		{
			type.Name = "Test1";

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			type.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			type.CustomAttributes.Add (attrDec);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0} B()>  _{0}Public Class Test1{0}End Class{0}", 
				writer.NewLine), Code);
		}

		[Test]
		public void EventMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			evt.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			evt.CustomAttributes.Add (attrDec);

			type.Members.Add (evt);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    <A(),  _{0}     B()>  _{0}    "
				+ "Private Event  As System.Void{0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void EventMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "OnClick";
			evt.Attributes = MemberAttributes.Public;
			evt.Type = new CodeTypeReference(typeof (int));
			type.Members.Add (evt);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    "
				+ "Public Event OnClick As Integer{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void FieldMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			fld.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			fld.CustomAttributes.Add (attrDec);

			type.Members.Add (fld);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    <A(),  _{0}     B()>  _{0}    "
				+ "Private  As System.Void{0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void FieldMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();
			fld.Name = "Name";
			fld.Attributes = MemberAttributes.Public;
			fld.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (fld);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    "
				+ "Public Name As Integer{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}
	}
}
