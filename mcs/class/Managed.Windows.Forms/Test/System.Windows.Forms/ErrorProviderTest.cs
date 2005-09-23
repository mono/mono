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
	public class ErrorProviderTest 
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
			ToolBar myToolBar = new ToolBar ();
			ErrorProvider myErrorProvider = new ErrorProvider ();
			Assert.AreEqual (myErrorProvider.CanExtend (myControl), true, "#ext1");
			Assert.AreEqual (myErrorProvider.CanExtend (myToolBar), false, "#ext2");
			Assert.AreEqual (myErrorProvider.CanExtend (myForm), false, "#ext3");
		}
	
		[Test]
		public void GetandSetErrorTest ()
		{
			Form myForm = new Form ();
			Label myLabel = new Label ();
			ErrorProvider myErrorProvider = new ErrorProvider ();
			myErrorProvider.SetError(myLabel, "New Error msg for Label");
			Assert.AreEqual ("New Error msg for Label", myErrorProvider.GetError (myLabel), "#getset1");
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
			ErrorProvider myErrorProvider = new ErrorProvider ();
			myErrorProvider.SetIconPadding (myForm, 2);
			Assert.AreEqual (2, myErrorProvider.GetIconPadding (myForm), "#getset3");
		}
	}
}