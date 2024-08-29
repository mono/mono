//
// Tests for System.Web.UI.WebControls.BaseValidator 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Alternate;

namespace Alternate {
	[ValidationProperty ("SomeProperty")]
	class MyTextBox : ICustomTypeDescriptor {
		public string SomeProperty {
			get { return null; }
			set {}
		}

		public System.ComponentModel.AttributeCollection GetAttributes ()
		{
			// This one is called and then GetProperties
			//Console.WriteLine ("GetAttributes ");
			return TypeDescriptor.GetAttributes  (this, true);
		}

		public string GetClassName()
		{
			//Console.WriteLine ("GetClassName");
			return TypeDescriptor.GetClassName (this, true);
		}

		public string GetComponentName()
		{
			//Console.WriteLine ("GetComponentName");
			return TypeDescriptor.GetComponentName (this, true);
		}

		public TypeConverter GetConverter()
		{
			//Console.WriteLine ("GetConverter");
			return TypeDescriptor.GetConverter (this, true);
		}

		public EventDescriptor GetDefaultEvent()
		{
			//Console.WriteLine ("GetDefaultEvent");
			return TypeDescriptor.GetDefaultEvent (this, true);
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			//Console.WriteLine ("GetDefaultProperty");
			return TypeDescriptor.GetDefaultProperty (this, true);
		}

		public object GetEditor(Type editorBaseType)
		{
			//Console.WriteLine ("GetEditor (editorBaseType");
			return null;
		}

		public EventDescriptorCollection GetEvents()
		{
			//Console.WriteLine ("GetEvents");
			return TypeDescriptor.GetEvents (this, true);
		}

		public EventDescriptorCollection GetEvents(Attribute[] arr)
		{
			//Console.WriteLine ("GetEvents");
			return TypeDescriptor.GetEvents (arr, true);
		}

		public PropertyDescriptorCollection GetProperties()
		{
			//Console.WriteLine ("GetProperties");
			return TypeDescriptor.GetProperties (this, true);
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] arr)
		{
			//Console.WriteLine ("GetProperties");
			return TypeDescriptor.GetProperties (this, arr, true);
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			//Console.WriteLine ("GetPropertyOwner (pd)");
			return null;
		}
	}
}

namespace MonoTests.System.Web.UI.WebControls
{
	class BaseValidatorPoker : BaseValidator {
		public BaseValidatorPoker ()
		{
			TrackViewState (); 
		}
		
		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public void CheckControlProperties ()
		{
			ControlPropertiesValid ();
		}

		protected override bool EvaluateIsValid ()
		{
			return IsValid;
		}

		public new PropertyDescriptor GetValidationProperty (object o)
		{
			return BaseValidator.GetValidationProperty (o);
		}

		public string DoGetControlValidationValue (string name)
		{
			return GetControlValidationValue (name);
		}

		public virtual bool DoControlPropertiesValid ()
		{
			return ControlPropertiesValid ();
		}
	  
		public void DoCheckControlValidationProperty (string name, string propertyName)
		{
			CheckControlValidationProperty (name, propertyName);
		}
	}

	[TestFixture]	
	public class BaseValidatorTest : ValidatorTest {

		[Test]
		public void ViewState ()
		{
			BaseValidatorPoker p = new BaseValidatorPoker ();

			/* defaults */
			Assert.AreEqual (String.Empty, p.ControlToValidate, "D1");
			Assert.AreEqual (ValidatorDisplay.Static, p.Display, "D2");
			Assert.AreEqual (true, p.EnableClientScript, "D3");
			Assert.AreEqual (true, p.Enabled, "D4");
			Assert.AreEqual (String.Empty, p.ErrorMessage, "D5");
			Assert.AreEqual (Color.Empty, p.ForeColor, "D6");
			Assert.AreEqual (true, p.IsValid, "D7");

			/* get/set */
			p.ControlToValidate = "foo";
			Assert.AreEqual ("foo", p.ControlToValidate, "D8");

			p.Display = ValidatorDisplay.Dynamic;
			Assert.AreEqual (ValidatorDisplay.Dynamic, p.Display, "D9");

			p.EnableClientScript = false;
			Assert.AreEqual (false, p.EnableClientScript, "D9");

			p.Enabled = false;
			Assert.AreEqual (false, p.Enabled, "D10");

			p.ErrorMessage = "stupid monkey";
			Assert.AreEqual ("stupid monkey", p.ErrorMessage, "D11");

			p.ForeColor = Color.Blue;
			Assert.AreEqual (Color.Blue, p.ForeColor, "D12");
			//XXX add check to see if setting the color alters the style at all.

			p.IsValid = false;
			Assert.AreEqual (false, p.IsValid, "D13");
		}

		[Test]
		public void ValidationProperty ()
		{
			BaseValidatorPoker p = new BaseValidatorPoker ();
			PropertyDescriptor d;

			StartValidationTest (p);

			TextBox box = AddTextBox ("textbox", "hello world");

			d = p.GetValidationProperty (box);
			Assert.AreEqual ("Text", d.Name, "A1");
			Assert.AreEqual ("hello world", p.DoGetControlValidationValue ("textbox"), "A2");

			StopValidationTest ();
		}

		[Test]
		public void ControlPropertiesValid1 ()
		{
			BaseValidatorPoker p = new BaseValidatorPoker ();

			StartValidationTest (p);
			TextBox box = SetValidationTextBox ("textbox", "hello world");

			Assert.IsTrue (p.DoControlPropertiesValid (), "B1");

			StopValidationTest ();
		}

		[Test]
		public void NullValidationProperty ()
		{
			BaseValidatorPoker v = new BaseValidatorPoker ();

			Page p = new Page ();
			p.Controls.Add (v);
			RadioButtonList l = new RadioButtonList ();
			p.Controls.Add (l);
			l.ID = "XXX";
			v.ControlToValidate = "XXX";

			Assert.AreEqual (String.Empty, v.DoGetControlValidationValue ("XXX"), "#A1");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ControlPropertiesValid2 ()
		{
			BaseValidatorPoker p = new BaseValidatorPoker ();

			StartValidationTest (p);
			TextBox box = AddTextBox ("textbox", "hello world");

			/* successful */
			p.DoCheckControlValidationProperty ("textbox", "Text");

			/* failure (exception) due to unknown control */
			p.DoCheckControlValidationProperty ("textbox2", "Text");

			StopValidationTest ();
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ControlPropertiesValid3 ()
		{
			BaseValidatorPoker p = new BaseValidatorPoker ();

			StartValidationTest (p);

			p.ControlToValidate = "textbox";

			/* failure (exception) due to unknown control */
			p.DoCheckControlValidationProperty ("textbox", "Text2");

			StopValidationTest ();
		}


		[Test]
		[ExpectedException(typeof(HttpException))]
		public void BasicExceptionTest () {
			BaseValidatorPoker p = new BaseValidatorPoker ();

			p.CheckControlProperties();
		}


		[Test]
		public void GetControlValidationValue ()
		{
			BaseValidatorPoker p = new BaseValidatorPoker ();

			StartValidationTest (p);
			TextBox box = AddTextBox ("textbox", "hello world");
			Label label = new Label ();

			label.ID = "label";

			Page.Controls.Add (label);

			/* successful */
			Assert.AreEqual ("hello world", p.DoGetControlValidationValue ("textbox"), "C1");

			/* failure (non-existant control)*/
			Assert.IsNull (p.DoGetControlValidationValue ("textbox2"), "C2");

			/* failure (control without a ValidationProperty */
			Assert.IsNull (p.DoGetControlValidationValue ("label"), "C3");

			StopValidationTest ();
		}

		[Test]
		public void CustomDescriptor ()
		{
			PropertyDescriptor pd = BaseValidator.GetValidationProperty (new MyTextBox ());
			Assert.AreEqual ("SomeProperty", pd.Name);
		}

		[Test]
		public void NoCustomDescriptor ()
		{
			PropertyDescriptor pd = BaseValidator.GetValidationProperty (new TextBox ());
			Assert.AreEqual ("Text", pd.Name);
		}
	}
}
