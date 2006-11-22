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

using NUnit.Framework;
using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Collections.Specialized ;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	public class ParameterPoker:Parameter
	{
		public bool parameterChanged = false;

		public ParameterPoker () // constructor
		{
			TrackViewState ();
		 }
		public ParameterPoker (Parameter p)
			: base (p)
		{
		}

		public ParameterPoker (String name)
			: base (name)
		{
		}

		public ParameterPoker (string name, TypeCode type)
			: base (name, type)
		{
		}

		public ParameterPoker (string name,TypeCode type, string defaultValue):base(name,type,defaultValue)
		{
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

		public Parameter DoClone()
		{
			return base.Clone ();
		}
		public object DoEvaluate(HttpContext context, Control control)
		{
			return base.Evaluate (context,control );
		}
		//protected override void OnParameterChanged ()
		//{
		//        parameterChanged = true;
		//        base.OnParameterChanged ();
		//}
		public void DoSetDirty ()
		{
			base.SetDirty ();
		}

		public bool IsTrackingViewStatePoker 
		{
			get {return base.IsTrackingViewState;}
		}
	}

	[TestFixture]
	public class ParameterTest
	{
		[Test]
		public void Parameter_DefaultProperties ()
		{
			ParameterPoker param = new ParameterPoker ();
			Assert.AreEqual (0, param.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (true, param.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.AreEqual (null, param.DefaultValue, "DefaultValue");
			Assert.AreEqual (ParameterDirection.Input,param.Direction,"Direction");
			Assert.AreEqual ("", param.Name, "Name");
			Assert.AreEqual (0, param.Size, "Size");
			Assert.AreEqual (TypeCode.Empty, param.Type, "Type");	

		}

		[Test]
		public void Parameter_AssignToDefaultProperties ()
		{
			ParameterPoker param = new ParameterPoker ();
			param.ConvertEmptyStringToNull = false;
			Assert.AreEqual (false, param.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			param.DefaultValue = "LName";
			Assert.AreEqual ("LName", param.DefaultValue, "DefaultValue");
			param.Direction = ParameterDirection.InputOutput;
			Assert.AreEqual (ParameterDirection.InputOutput, param.Direction, "DirectionInputOutput");
			param.Direction = ParameterDirection.Output;
			Assert.AreEqual (ParameterDirection.Output, param.Direction, "DirectionOutput");
			param.Direction = ParameterDirection.ReturnValue;
			Assert.AreEqual (ParameterDirection.ReturnValue, param.Direction, "DirectionReturnValue");
			param.Name = "paramName";
			Assert.AreEqual ("paramName", param.Name, "Name");
			param.Size = 10;
			Assert.AreEqual (10, param.Size, "Size");
			param.Type = TypeCode.Boolean;
			Assert.AreEqual (TypeCode.Boolean, param.Type, "BooleanType");
			param.Type = TypeCode.Byte;
			Assert.AreEqual (TypeCode.Byte, param.Type, "ByteType");
			param.Type = TypeCode.Char ;
			Assert.AreEqual (TypeCode.Char, param.Type, "CharType");
			param.Type = TypeCode.DateTime ;
			Assert.AreEqual (TypeCode.DateTime, param.Type, "DateTimeType");
			param.Type = TypeCode.DBNull ;
			Assert.AreEqual (TypeCode.DBNull, param.Type, "DBNullType");
			param.Type = TypeCode.Decimal;
			Assert.AreEqual (TypeCode.Decimal, param.Type, "DecimalType");
			param.Type = TypeCode.Double;
			Assert.AreEqual (TypeCode.Double, param.Type, "DoubleType");
			param.Type = TypeCode.Int16;
			Assert.AreEqual (TypeCode.Int16, param.Type, "Int16Type");
			param.Type = TypeCode.Int32;
			Assert.AreEqual (TypeCode.Int32, param.Type, "Int32Type");
			param.Type = TypeCode.Int64;
			Assert.AreEqual (TypeCode.Int64, param.Type, "Int64Type");
			param.Type = TypeCode.Object;
			Assert.AreEqual (TypeCode.Object, param.Type, "ObjectType");
			param.Type = TypeCode.SByte ;
			Assert.AreEqual (TypeCode.SByte, param.Type, "SByteType");
			param.Type = TypeCode.Single;
			Assert.AreEqual (TypeCode.Single, param.Type, "SingleType");
			param.Type = TypeCode.String;
			Assert.AreEqual (TypeCode.String, param.Type, "StringType");
			param.Type = TypeCode.UInt16;
			Assert.AreEqual (TypeCode.UInt16, param.Type, "UInt16Type");
			param.Type = TypeCode.UInt32;
			Assert.AreEqual (TypeCode.UInt32, param.Type, "UInt32Type");
			param.Type = TypeCode.UInt64;
			Assert.AreEqual (TypeCode.UInt64, param.Type, "UInt64Type");
			Assert.AreEqual (6, param.StateBag.Count, "ViewStateCount");
		}

		[Test]
		public void Parameter_ConvertEmptyStringToNull ()
		{
			ParameterPoker param = new ParameterPoker ("ID", TypeCode.String, "1001");
			param.ConvertEmptyStringToNull = false;
		
					 
		}

		[Test]
		public void Parameter_DefaultProtectedProperties ()
		{
			ParameterPoker param = new ParameterPoker ();
			Assert.AreEqual (true, param.IsTrackingViewStatePoker, "IsTrackingViewState");
		}

		//Public methods

		[Test]
		public void Parameter_ToString ()
		{
			ParameterPoker param = new ParameterPoker ("ID",TypeCode.String ,"1001");
			Assert.AreEqual ("ID", param.ToString (), "ToString");

		}

		//Protected Methods

		[Test]
		public void Parameter_Clone ()
		{
			ParameterPoker param = new ParameterPoker ("ID", TypeCode.String, "1001");
			param.Size = 10;
			param.Direction = ParameterDirection.Output; 
			Parameter p = param.DoClone ();
			Assert.AreEqual ("ID", p.Name, "Clone1");
			Assert.AreEqual (TypeCode.String, p.Type, "Clone2");
			Assert.AreEqual ("1001", p.DefaultValue, "Clone3");
			Assert.AreEqual (10, param.Size, "Clone4");
			Assert.AreEqual (ParameterDirection.Output, param.Direction, "Clone5");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Parameter_Evaluate ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (Evaluate))).Run ();
			WebTest.Unload ();
		}		
		
		public static void Evaluate (Page p)
		{
			ParameterPoker param = new ParameterPoker ("ID", TypeCode.String, "1001");
			TextBox tb = new TextBox ();			
			p.Controls.Add (tb); 
			Object eval = param.DoEvaluate (HttpContext.Current, tb);
			//The default Evaluate method  returns  null in all cases
			Assert.AreEqual (null, eval, "ParameterEvaluate");
			 
		}

		[Test]
		public void Parameter_DoSetDirty ()
		{
			ParameterPoker param = new ParameterPoker ("ID", TypeCode.String, "1001");
			object state = param.SaveState ();
			Assert.AreEqual (null, state, "BeforeSetDirtyMethod");
			param.DoSetDirty ();
			state = param.SaveState ();
			Assert.IsTrue (null != state, "AfterSetDirtyMethod1");
		}

		[Test]
		public void Parameter_ViewState ()
		{
			ParameterPoker param = new ParameterPoker ("ID", TypeCode.String, "1001");
			ParameterPoker copy = new ParameterPoker ();
			param.Size = 100;
			param.Direction = ParameterDirection.InputOutput;
			param.DoSetDirty ();
			object state = param.SaveState ();
			copy.LoadState (state);
			Assert.AreEqual ("1001", copy.DefaultValue, "DefaultValue");
			Assert.AreEqual (ParameterDirection.InputOutput, copy.Direction, "Direction");
			Assert.AreEqual (100, copy.Size, "Size");
			Assert.AreEqual (TypeCode.String, copy.Type, "Type");
			Assert.AreEqual ("ID", copy.Name, "Name");
			
		}
	}
}
#endif 
