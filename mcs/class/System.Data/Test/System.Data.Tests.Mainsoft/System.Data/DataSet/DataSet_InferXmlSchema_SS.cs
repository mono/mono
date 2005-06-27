// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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

using System;
using System.Data;
using NUnit.Framework;
using GHTUtils;
using GHTUtils.Base;
using System.Text;
using System.IO;

namespace tests.system_data_dll.System_Data
{
[TestFixture]
public class DataSet_InferXmlSchema_SS : GHTBase
{
	public static void Main()
	{
		DataSet_InferXmlSchema_SS tc = new DataSet_InferXmlSchema_SS();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_InferXmlSchema_SS");
			tc.run();
		}
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			tc.EndTest(exp);
		}
		
	}

	//Activate This Construntor to log All To Standard output
	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}


	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

	public void run()
	{
		Exception exp = null;
		try
		{
			BeginCase("Test inferXmlSchema full schema 1 uri");
			test();
			
		} 
		catch(Exception ex)
		{exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Test inferXmlSchema full schema 1 uri that dont exists");
			test1();
			
		} 
		catch(Exception ex)
		{exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Test inferXmlSchema missing schema 2 uri");
			test2();
			
		} 
		catch(Exception ex)
		{exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Test inferXmlSchema mixed schema 1 uri");
			test5();
			
		} 
		catch(Exception ex)
		{exp = ex;}
		finally{EndCase(exp); exp = null;}

		inferingTables1();
		inferingTables2();
		inferingTables3();
		inferingTables4();
		inferingTables5();

		inferringColumns1();
		inferringColumns2();

		inferringRelationships1();

		elementText1();
		elementText2();
		
	}

	#region test namespaces 

	[Test]
	public void test()
	{
		StringBuilder sb  = new StringBuilder();
		sb.Append("<NewDataSet xmlns:od='urn:schemas-microsoft-com:officedata'>");
		sb.Append("<Categories>");
		sb.Append("<CategoryID od:adotype='3'>1</CategoryID>");
		sb.Append("<CategoryName od:maxLength='15' od:adotype='130'>Beverages</CategoryName>");
		sb.Append("<Description od:adotype='203'>Soft drinks and teas</Description>");
		sb.Append("</Categories>");
		sb.Append("<Products>");
		sb.Append("<ProductID od:adotype='20'>1</ProductID>");
		sb.Append("<ReorderLevel od:adotype='3'>10</ReorderLevel>");
		sb.Append("<Discontinued od:adotype='11'>0</Discontinued>");
		sb.Append("</Products>");
		sb.Append("</NewDataSet>");

		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));

		DataSet ds = new DataSet();
	//	ds.ReadXml(myStream);
		ds.InferXmlSchema(myStream, new string[] {"urn:schemas-microsoft-com:officedata"});
		Compare(ds.Tables.Count,2);
		Compare(ds.Tables[0].Columns[0].ColumnName,"CategoryID");
		Compare(ds.Tables[0].Columns[1].ColumnName,"CategoryName");
		Compare(ds.Tables[0].Columns[2].ColumnName,"Description");

		Compare(ds.Tables[1].Columns[0].ColumnName,"ProductID");
		Compare(ds.Tables[1].Columns[1].ColumnName,"ReorderLevel");
		Compare(ds.Tables[1].Columns[2].ColumnName,"Discontinued");
	}

	[Test]
	public void test1()
	{
		StringBuilder sb  = new StringBuilder();
		sb.Append("<NewDataSet xmlns:od='urn:schemas-microsoft-com:officedata'>");
		sb.Append("<Categories>");
		sb.Append("<CategoryID od:adotype='3'>1</CategoryID>");
		sb.Append("<CategoryName od:maxLength='15' od:adotype='130'>Beverages</CategoryName>");
		sb.Append("<Description od:adotype='203'>Soft drinks and teas</Description>");
		sb.Append("</Categories>");
		sb.Append("<Products>");
		sb.Append("<ProductID od:adotype='20'>1</ProductID>");
		sb.Append("<ReorderLevel od:adotype='3'>10</ReorderLevel>");
		sb.Append("<Discontinued od:adotype='11'>0</Discontinued>");
		sb.Append("</Products>");
		sb.Append("</NewDataSet>");

		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));

		DataSet ds = new DataSet();
		//ds.ReadXml(myStream);
		ds.InferXmlSchema(myStream,new string[] {"urn:schemas-microsoft-com:officedata1"});
		Compare(ds.Tables.Count,8);
	
	}

	[Test]
	public void test5()
	{
		StringBuilder sb  = new StringBuilder();
		sb.Append("<NewDataSet xmlns:od='urn:schemas-microsoft-com:officedata'>");
		sb.Append("<Categories>");
		sb.Append("<CategoryID od:adotype='3'>1</CategoryID>");
		sb.Append("<CategoryName od:maxLength='15' adotype='130'>Beverages</CategoryName>");
		sb.Append("<Description od:adotype='203'>Soft drinks and teas</Description>");
		sb.Append("</Categories>");
		sb.Append("<Products>");
		sb.Append("<ProductID od:adotype='20'>1</ProductID>");
		sb.Append("<ReorderLevel od:adotype='3'>10</ReorderLevel>");
		sb.Append("<Discontinued od:adotype='11'>0</Discontinued>");
		sb.Append("</Products>");
		sb.Append("</NewDataSet>");

		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));

		DataSet ds = new DataSet();
		//	ds.ReadXml(myStream);
		ds.InferXmlSchema(myStream, new string[] {"urn:schemas-microsoft-com:officedata"});
		Compare(ds.Tables.Count,3);

		Compare(ds.Tables[0].Columns.Count,3);
		Compare(ds.Tables[0].Columns["CategoryID"].ColumnName,"CategoryID"); 
		Compare(ds.Tables[0].Columns["Categories_Id"].ColumnName,"Categories_Id");//Hidden
		Compare(ds.Tables[0].Columns["Description"].ColumnName,"Description");
		

		Compare(ds.Tables[1].Columns.Count,3);
		Compare(ds.Tables[1].Columns["adotype"].ColumnName,"adotype");
		Compare(ds.Tables[1].Columns["CategoryName_Text"].ColumnName,"CategoryName_Text");
		Compare(ds.Tables[1].Columns["Categories_Id"].ColumnName,"Categories_Id");//Hidden

		Compare(ds.Tables[2].Columns.Count,3);
		Compare(ds.Tables[2].Columns["ProductID"].ColumnName,"ProductID");
		Compare(ds.Tables[2].Columns["ReorderLevel"].ColumnName,"ReorderLevel");
		Compare(ds.Tables[2].Columns["Discontinued"].ColumnName,"Discontinued");
	}


	[Test]
	public void test2() //Ignoring 2 namespaces
	{
		Exception exp = null;
		try
		{
			StringBuilder sb  = new StringBuilder();
			sb.Append("<h:html xmlns:xdc='http://www.xml.com/books' xmlns:h='http://www.w3.org/HTML/1998/html4'>");
			sb.Append("<h:head><h:title>Book Review</h:title></h:head>");
			sb.Append("<h:body>");
			sb.Append("<xdc:bookreview>");
			sb.Append("<xdc:title h:attrib1='1' xdc:attrib2='2' >XML: A Primer</xdc:title>");
			sb.Append("</xdc:bookreview>");
			sb.Append("</h:body>");
			sb.Append("</h:html>");

			MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
			DataSet tempDs = new DataSet();
			tempDs.ReadXml(myStream);
			myStream.Seek(0,SeekOrigin.Begin);
			DataSet ds = new DataSet();
			ds.InferXmlSchema(myStream, new string[] {"http://www.xml.com/books","http://www.w3.org/HTML/1998/html4"});
			//Compare(ds.Tables.Count,8);

//			string str1 = tempDs.GetXmlSchema(); //DataProvider.GetDSSchema(tempDs);
//			string str2 = ds.GetXmlSchema(); //DataProvider.GetDSSchema(ds);
			Compare(ds.Tables.Count,3);
			Compare(ds.Tables[2].TableName,"bookreview");
			Compare(ds.Tables[2].Columns.Count,2);
		}
		catch(Exception ex)
		{exp = ex;}
	
	}
	#endregion

	#region inferingTables
	[Test]
	public void inferingTables1()  
	{
		//Acroding to the msdn documantaion : 
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringtables.htm
		//Elements that have attributes specified in them will result in inferred tables

		BeginCase("inferingTables1");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1 attr1='value1'/>");
		sb.Append("<Element1 attr1='value2'>Text1</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["attr1"].ColumnName,"attr1");
			Compare(ds.Tables[0].Columns["Element1_Text"].ColumnName,"Element1_Text");
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}
	}

	[Test]
	public void inferingTables2()  
	{
		//Acroding to the msdn documantaion : 
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringtables.htm
		//Elements that have child elements will result in inferred tables

		BeginCase("inferingTables2");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1>");
		sb.Append("<ChildElement1>Text1</ChildElement1>");
		sb.Append("</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["ChildElement1"].ColumnName,"ChildElement1");
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}
	}

	[Test]
	public void inferingTables3()  
	{
		//Acroding to the msdn documantaion : 
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringtables.htm
		//The document, or root, element will result in an inferred table if it has attributes
		//or child elements that will be inferred as columns. 
		//If the document element has no attributes and no child elements that would be inferred as columns, the element will be inferred as a DataSet

		BeginCase("inferingTables3");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1>Text1</Element1>");
		sb.Append("<Element2>Text2</Element2>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"NewDataSet");
			Compare(ds.Tables[0].TableName,"DocumentElement");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["Element1"].ColumnName,"Element1");
			Compare(ds.Tables[0].Columns["Element2"].ColumnName,"Element2");
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}
	}

	[Test]
	public void inferingTables4()  
	{
		//Acroding to the msdn documantaion : 
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringtables.htm
		//The document, or root, element will result in an inferred table if it has attributes
		//or child elements that will be inferred as columns. 
		//If the document element has no attributes and no child elements that would be inferred as columns, the element will be inferred as a DataSet

		BeginCase("inferingTables4");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1 attr1='value1' attr2='value2'/>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["attr1"].ColumnName,"attr1");
			Compare(ds.Tables[0].Columns["attr2"].ColumnName,"attr2");
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}
	}

	[Test]
	[Category ("NotWorking")]
	public void inferingTables5()  
	{
		//Acroding to the msdn documantaion : 
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringtables.htm
		//Elements that repeat will result in a single inferred table

		BeginCase("inferingTables5");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1>Text1</Element1>");
		sb.Append("<Element1>Text2</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["Element1_Text"].ColumnName,"Element1_Text");
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}
	}
	#endregion

	#region inferringColumns
	[Test]
	public void inferringColumns1()
	{
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringcolumns.htm
		BeginCase("inferringColumns1");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1 attr1='value1' attr2='value2'/>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["attr1"].ColumnName,"attr1");
			Compare(ds.Tables[0].Columns["attr2"].ColumnName,"attr2");
			Compare(ds.Tables[0].Columns["attr1"].ColumnMapping ,MappingType.Attribute);
			Compare(ds.Tables[0].Columns["attr2"].ColumnMapping ,MappingType.Attribute);
			Compare(ds.Tables[0].Columns["attr1"].DataType  ,typeof(string));
			Compare(ds.Tables[0].Columns["attr2"].DataType  ,typeof(string));
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}


	}

	[Test]
	public void inferringColumns2()
	{
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringcolumns.htm
		//If an element has no child elements or attributes, it will be inferred as a column. 
		//The ColumnMapping property of the column will be set to MappingType.Element. 
		//The text for child elements is stored in a row in the table

		BeginCase("inferringColumns2");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1>");
		sb.Append("<ChildElement1>Text1</ChildElement1>");
		sb.Append("<ChildElement2>Text2</ChildElement2>");
		sb.Append("</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);
			Compare(ds.Tables[0].Columns["ChildElement1"].ColumnName,"ChildElement1");
			Compare(ds.Tables[0].Columns["ChildElement2"].ColumnName,"ChildElement2");
			Compare(ds.Tables[0].Columns["ChildElement1"].ColumnMapping ,MappingType.Element);
			Compare(ds.Tables[0].Columns["ChildElement2"].ColumnMapping ,MappingType.Element);
			Compare(ds.Tables[0].Columns["ChildElement1"].DataType  ,typeof(string));
			Compare(ds.Tables[0].Columns["ChildElement2"].DataType  ,typeof(string));
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}


	}

	#endregion

	#region Inferring Relationships

	[Test]
	public void inferringRelationships1()
	{
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringrelationships.htm

		BeginCase("inferringRelationships1");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1>");
		sb.Append("<ChildElement1 attr1='value1' attr2='value2'/>");
		sb.Append("<ChildElement2>Text2</ChildElement2>");
		sb.Append("</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);
			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables[1].TableName,"ChildElement1");
			Compare(ds.Tables.Count,2);

			Compare(ds.Tables["Element1"].Columns["Element1_Id"].ColumnName,"Element1_Id");
			Compare(ds.Tables["Element1"].Columns["Element1_Id"].ColumnMapping ,MappingType.Hidden);
			Compare(ds.Tables["Element1"].Columns["Element1_Id"].DataType  ,typeof(Int32));


			Compare(ds.Tables["Element1"].Columns["ChildElement2"].ColumnName,"ChildElement2");
			Compare(ds.Tables["Element1"].Columns["ChildElement2"].ColumnMapping ,MappingType.Element);
			Compare(ds.Tables["Element1"].Columns["ChildElement2"].DataType  ,typeof(string));


			Compare(ds.Tables["ChildElement1"].Columns["attr1"].ColumnName,"attr1");
			Compare(ds.Tables["ChildElement1"].Columns["attr1"].ColumnMapping ,MappingType.Attribute);
			Compare(ds.Tables["ChildElement1"].Columns["attr1"].DataType  ,typeof(string));

			Compare(ds.Tables["ChildElement1"].Columns["attr2"].ColumnName,"attr2");
			Compare(ds.Tables["ChildElement1"].Columns["attr2"].ColumnMapping ,MappingType.Attribute);
			Compare(ds.Tables["ChildElement1"].Columns["attr2"].DataType  ,typeof(string));

			Compare(ds.Tables["ChildElement1"].Columns["Element1_Id"].ColumnName,"Element1_Id");
			Compare(ds.Tables["ChildElement1"].Columns["Element1_Id"].ColumnMapping ,MappingType.Hidden);
			Compare(ds.Tables["ChildElement1"].Columns["Element1_Id"].DataType  ,typeof(Int32));

			//Checking dataRelation :
			Compare(ds.Relations["Element1_ChildElement1"].ParentTable.TableName,"Element1");
			Compare(ds.Relations["Element1_ChildElement1"].ParentColumns[0].ColumnName,"Element1_Id");
			Compare(ds.Relations["Element1_ChildElement1"].ChildTable.TableName,"ChildElement1");
			Compare(ds.Relations["Element1_ChildElement1"].ChildColumns[0].ColumnName,"Element1_Id");
			Compare(ds.Relations["Element1_ChildElement1"].Nested,true);

			//Checking ForeignKeyConstraint

			ForeignKeyConstraint con = (ForeignKeyConstraint)ds.Tables["ChildElement1"].Constraints["Element1_ChildElement1"];

			Compare(con.Columns[0].ColumnName,"Element1_Id");
			Compare(con.DeleteRule,Rule.Cascade);
			Compare(con.AcceptRejectRule,AcceptRejectRule.None);
			Compare(con.RelatedTable.TableName,"Element1");
			Compare(con.Table.TableName,"ChildElement1");

		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}

	}

	#endregion

	#region Inferring Element Text

	[Test]
	public void elementText1()
	{
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringelementtext.htm
		
		BeginCase("elementText1");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1 attr1='value1'>Text1</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);

			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);

			Compare(ds.Tables["Element1"].Columns["attr1"].ColumnName,"attr1");
			Compare(ds.Tables["Element1"].Columns["attr1"].ColumnMapping ,MappingType.Attribute);
			Compare(ds.Tables["Element1"].Columns["attr1"].DataType  ,typeof(string));

			Compare(ds.Tables["Element1"].Columns["Element1_Text"].ColumnName,"Element1_Text");
			Compare(ds.Tables["Element1"].Columns["Element1_Text"].ColumnMapping ,MappingType.SimpleContent);
			Compare(ds.Tables["Element1"].Columns["Element1_Text"].DataType  ,typeof(string));

		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}

	}

	[Test]
	public void elementText2()
	{
		//ms-help://MS.MSDNQTR.2003FEB.1033/cpguide/html/cpconinferringelementtext.htm
		
		BeginCase("elementText1");
		Exception exp=null;
		StringBuilder sb  = new StringBuilder();

		sb.Append("<DocumentElement>");
		sb.Append("<Element1>");
		sb.Append("Text1");
		sb.Append("<ChildElement1>Text2</ChildElement1>");
		sb.Append("Text3");
		sb.Append("</Element1>");
		sb.Append("</DocumentElement>");
		DataSet ds = new DataSet();
		MemoryStream myStream = new MemoryStream(new ASCIIEncoding().GetBytes(sb.ToString()));
		try
		{
			ds.InferXmlSchema(myStream,null);

			Compare(ds.DataSetName,"DocumentElement");
			Compare(ds.Tables[0].TableName,"Element1");
			Compare(ds.Tables.Count,1);

			Compare(ds.Tables["Element1"].Columns["ChildElement1"].ColumnName,"ChildElement1");
			Compare(ds.Tables["Element1"].Columns["ChildElement1"].ColumnMapping ,MappingType.Element);
			Compare(ds.Tables["Element1"].Columns["ChildElement1"].DataType  ,typeof(string));
			Compare(ds.Tables["Element1"].Columns.Count,1);

			
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
		}

	}

	#endregion


}
}
