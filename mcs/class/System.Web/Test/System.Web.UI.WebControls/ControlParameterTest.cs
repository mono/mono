//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using NUnit.Framework;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	public class ControlParameterPoker : ControlParameter
	{
		public ControlParameterPoker (ControlParameter control)
			: base (control)
		{
		}
	    
		public ControlParameterPoker (string name,TypeCode type, string controlID,string propertyName)
			:base(name,type,controlID,propertyName)
		{
		}

		public ControlParameterPoker (string name, string controlId, string propertyName)
			: base (name, controlId, propertyName)
		{
		}

	
		public ControlParameterPoker (string name, string controlId)
			: base (name, controlId)	
		{	
		}
       
		public ControlParameterPoker() // constructor       
		{        
			TrackViewState ();       
		}

		public object DoEvaluate (HttpContext context,Control control)
		{
			return base.Evaluate (context,control);
		}

		public Parameter DoClone ()
		{
			return base.Clone ();
		}
	
		public object SaveState ()		
		{	
			return SaveViewState ();		
		}

       
		public void LoadState (object o)       
		{       
			LoadViewState (o);      
		}
       
		public StateBag StateBag       
		{       
			get { return base.ViewState; }      
		}
       
	}

	class TestControl : Control 
	{
		DataKey _values;
		
		public DataKey Values {
			get { return _values; }
		}

		public TestControl (DataKey values)
		{
			this._values = values;
		}
	}
	
	[TestFixture]
	public class ControlParameterTest
	{
		[Test]
		public void ControlParameter_DefaultProperties ()
		{
			ControlParameterPoker ctrlParam1 = new ControlParameterPoker ();
			Assert.AreEqual ("",ctrlParam1.ControlID, "ControlIdDefault");
			Assert.AreEqual ("", ctrlParam1.PropertyName, "PropertyNameDefault");
			ControlParameterPoker ctrlParam2 = new ControlParameterPoker ("ControlTest","ControlID");
			Assert.AreEqual ("ControlID", ctrlParam2.ControlID, "OverloadConstructorControlId1");
			Assert.AreEqual ("ControlTest", ctrlParam2.Name, "OverloadConstructorPropertyName1");
			ControlParameterPoker ctrlParam3 = new ControlParameterPoker ("Age",TypeCode.Int32,"Label1","Text");
			Assert.AreEqual ("Age", ctrlParam3.Name, "OverloadConstructorName2");
			Assert.AreEqual (TypeCode.Int32, ctrlParam3.Type, "OverloadConstructorType2");
			Assert.AreEqual ("Label1", ctrlParam3.ControlID, "OverloadConstructorControlID2");
			Assert.AreEqual ("Text", ctrlParam3.PropertyName, "OverloadConstructorPropertyName2");
			ControlParameterPoker ctrlParam4 = new ControlParameterPoker ("Age","Label1","Text");
			Assert.AreEqual ("Age", ctrlParam4.Name, "OverloadConstructorName3");			
			Assert.AreEqual ("Label1", ctrlParam4.ControlID, "OverloadConstructorControlID3");
			Assert.AreEqual ("Text", ctrlParam4.PropertyName, "OverloadConstructorPropertyName3");
			ControlParameterPoker ctrlParam5 = new ControlParameterPoker (ctrlParam3);
			Assert.AreEqual ("Age", ctrlParam3.Name, "OverloadConstructorName4");
			Assert.AreEqual (TypeCode.Int32, ctrlParam3.Type, "OverloadConstructorType4");
			Assert.AreEqual ("Label1", ctrlParam3.ControlID, "OverloadConstructorControlID4");
			Assert.AreEqual ("Text", ctrlParam3.PropertyName, "OverloadConstructorPropertyName4");			
		 }

		[Test]
		public void ControlParameter_AssignToDefaultProperties ()
		{
			ControlParameterPoker ctrlParam = new ControlParameterPoker ();
			ctrlParam.PropertyName = "Text";
			Assert.AreEqual ("Text",ctrlParam.PropertyName ,"AssignToPropertyName");
			ctrlParam.ControlID ="Label";
			Assert.AreEqual ("Label",ctrlParam.ControlID ,"AssignToPropertyName"); 
		 }

	    //Protected Methods

		[Test]
		public void ControlParameter_Clone ()
		{
			ControlParameterPoker ctrlParam = new ControlParameterPoker ("Salary", TypeCode.Int64, "TextBox1", "Text");
			ControlParameter clonedParam = (ControlParameter)ctrlParam.DoClone ();
			Assert.AreEqual ("Salary", clonedParam.Name, "ClonedParamName");
			Assert.AreEqual (TypeCode.Int64, clonedParam.Type, "ClonedParamType");
			Assert.AreEqual ("TextBox1", clonedParam.ControlID, "ClonedParamControlID");
			Assert.AreEqual ("Text", clonedParam.PropertyName, "ClonedParamPropertyName");

		}
		[Test]
		public void ControlParameter_Evaluate ()
		{
			ControlParameterPoker ctrlParam = new ControlParameterPoker ("Salary",TypeCode.Int64,"Label1","Text");
			Page page = new Page ();
			Label label1 = new Label ();
			label1.ID = "Label1";
			label1.Text = "2000";
			page.Controls.Add (label1);			
			string value=(string)ctrlParam.DoEvaluate (HttpContext.Current,label1);
			Assert.AreEqual ("2000", value, "EvaluateValue1");
			label1.Text = "TestNewValue";
			ctrlParam.Type = TypeCode.String;
			value = (string) ctrlParam.DoEvaluate (HttpContext.Current, label1);
			Assert.AreEqual ("TestNewValue", value, "EvaluateValue2");
		}

		[Test]
		public void ControlParameter_EvaluateComplex ()
		{
			ControlParameterPoker ctrlParam = new ControlParameterPoker ("Test", "TestControl1", "Values['one']");
			Page page = new Page ();
			
			OrderedDictionary dict = new OrderedDictionary ();
			dict.Add ("one", "1");
			
			DataKey values = new DataKey (dict);
			TestControl test = new TestControl (values);
			test.ID = "TestControl1";
			page.Controls.Add (test);
			string value = ctrlParam.DoEvaluate (HttpContext.Current, test) as string;
			Assert.AreEqual ("1", value, "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void EvaluateArgumemtException ()
		{
			ControlParameterPoker ctrlParam = new ControlParameterPoker ();
			TextBox textBox1 = new TextBox ();
			textBox1.ID = "textbox1";			
			Page page = new Page ();
			page.Controls.Add (textBox1); 
			ctrlParam.DoEvaluate (HttpContext.Current, textBox1); 
		}
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EvaluateInvalidOperationException ()
		{
			ControlParameterPoker ctrlParam = new ControlParameterPoker ("age", "Button", "parameter");
			Button b = new Button ();
			Page page = new Page ();
			page.Controls.Add (b);
			ctrlParam.DoEvaluate (HttpContext.Current, b);  
		}


	}
}
#endif
