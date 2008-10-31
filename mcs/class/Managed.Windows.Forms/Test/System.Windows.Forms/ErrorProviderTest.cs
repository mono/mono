//
// ErrorProviderTest.cs: Test cases for ErrorProvider.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ErrorProviderTest : TestHelper 
	{
		[Test]
		public void ErrorProviderPropertyTest ()
		{
			ErrorProvider myErrorProvider = new ErrorProvider ();

			// B
			Assert.AreEqual (250, myErrorProvider.BlinkRate, "#B1");
			Assert.AreEqual (ErrorBlinkStyle.BlinkIfDifferentError, myErrorProvider.BlinkStyle, "#B2");

			// C
			Assert.AreEqual (null, myErrorProvider.ContainerControl, "#C1");

			// D 
			Assert.AreEqual (null, myErrorProvider.DataMember, "#D1");
			Assert.AreEqual (null, myErrorProvider.DataSource, "#D2");

			// I 
			Assert.AreEqual (16, myErrorProvider.Icon.Height, "#I1");
			Assert.AreEqual (16, myErrorProvider.Icon.Width, "#I2");

			// S
			Assert.AreEqual (null, myErrorProvider.Site, "#S1");
		}

		[Test]
		public void BindToDateAndErrorsTest ()
		{
			ErrorProvider myErrorProvider = new ErrorProvider ();
			DataSet myDataSet= new DataSet();
			myErrorProvider.DataSource = myDataSet;
			myErrorProvider.DataMember = "Customers";
			Assert.AreEqual (myDataSet, myErrorProvider.DataSource, "#Bind1");
			Assert.AreEqual ("Customers", myErrorProvider.DataMember, "#Bind2");
		}

		[Test]
		public void CanExtendTest ()
		{
			Control myControl = new Control ();
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			ToolBar myToolBar = new ToolBar ();
			ErrorProvider myErrorProvider = new ErrorProvider ();
			Assert.AreEqual (myErrorProvider.CanExtend (myControl), true, "#ext1");
			Assert.AreEqual (myErrorProvider.CanExtend (myToolBar), false, "#ext2");
			Assert.AreEqual (myErrorProvider.CanExtend (myForm), false, "#ext3");
			myForm.Dispose ();
		}

		[Test]
		public void GetandSetErrorTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			Label label1 = new Label ();
			Label label2 = new Label ();
			ErrorProvider myErrorProvider = new ErrorProvider ();
			Assert.AreEqual (string.Empty, myErrorProvider.GetError (label1), "#1");
			myErrorProvider.SetError (label1, "ErrorMsg1");
			Assert.AreEqual ("ErrorMsg1", myErrorProvider.GetError (label1), "#2");
			Assert.AreEqual (string.Empty, myErrorProvider.GetError (label2), "#3");
			myErrorProvider.SetError (label2, "ErrorMsg2");
			Assert.AreEqual ("ErrorMsg2", myErrorProvider.GetError (label2), "#4");
			myErrorProvider.SetError (label2, null);
			Assert.AreEqual ("ErrorMsg1", myErrorProvider.GetError (label1), "#5");
			Assert.AreEqual (string.Empty, myErrorProvider.GetError (label2), "#6");
			myForm.Dispose ();
		}

		[Test]
		public void GetandSetIconAlignmentTest ()
		{
			TextBox myTextBox = new TextBox ();
			ErrorProvider myErrorProvider = new ErrorProvider ();
			myErrorProvider.SetIconAlignment (myTextBox, ErrorIconAlignment.MiddleRight);
			Assert.AreEqual (ErrorIconAlignment.MiddleRight, myErrorProvider.GetIconAlignment (myTextBox), "#getset2");
		}

		[Test]
		public void GetandSetIconPaddingTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			ErrorProvider myErrorProvider = new ErrorProvider ();
			myErrorProvider.SetIconPadding (myForm, 2);
			Assert.AreEqual (2, myErrorProvider.GetIconPadding (myForm), "#getset3");
			myForm.Dispose ();
		}

		[Test]
		public void Bug420305 ()
		{
			// Should not throw an NRE
			Form f = new Form ();
			TextBox tb = new TextBox ();

			ErrorProvider ep = new ErrorProvider ();
			ep.ContainerControl = f;

			ep.SetIconAlignment (tb, ErrorIconAlignment.MiddleRight);
			ep.SetIconPadding (tb, 2);

			f.Controls.Add (tb);

			ep.SetError (tb, "arggggh");
		}
		
#if NET_2_0
		[Test]
		public void ErrorProviderPropertyTag ()
		{
			ErrorProvider md = new ErrorProvider ();
			object s = "MyString";

			Assert.AreEqual (null, md.Tag, "A1");

			md.Tag = s;
			Assert.AreSame (s, md.Tag, "A2");
		}

		[Test]
		public void MethodClear ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			Label label1 = new Label ();
			Label label2 = new Label ();
			ErrorProvider myErrorProvider = new ErrorProvider ();

			myErrorProvider.SetError (label1, "ErrorMsg1");
			myErrorProvider.SetError (label2, "ErrorMsg2");
			
			Assert.AreEqual ("ErrorMsg1", myErrorProvider.GetError (label1), "#1");
			Assert.AreEqual ("ErrorMsg2", myErrorProvider.GetError (label2), "#2");
			
			myErrorProvider.Clear ();

			Assert.AreEqual (string.Empty, myErrorProvider.GetError (label1), "#3");
			Assert.AreEqual (string.Empty, myErrorProvider.GetError (label2), "#4");
			
			myForm.Dispose ();
		}
#endif
	}
}
