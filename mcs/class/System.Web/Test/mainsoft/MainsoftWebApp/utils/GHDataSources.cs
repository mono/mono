using System;
using System.Data;
using System.Collections;

namespace GHTTests
{
	/// <summary>
	/// Summary description for GHDataSources.
	/// </summary>
	public class GHDataSources
	{
		public GHDataSources()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public static Array DSArray()
		{
			return new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" } ;
		}
		
		public static ArrayList DSArrayHash()
		{
			ArrayList list1 = new ArrayList();
			ArrayList list3 = DSArrayList();
			IEnumerator enumerator1 = list3.GetEnumerator();
			while (enumerator1.MoveNext())
			{
				Hashtable hashtable1 = new Hashtable();
				hashtable1.Add("number", enumerator1.Current.ToString());
				hashtable1.Add("number_up", enumerator1.Current.ToString().ToUpper());
				hashtable1.Add("number_number", enumerator1.Current.ToString() + "_" + enumerator1.Current.ToString());
				list1.Add(hashtable1);
			}
			return list1;
		}
		
		public static ArrayList DSArrayList()
		{
			ArrayList list1 = new ArrayList();
			list1.Add("one");
			list1.Add("two");
			list1.Add("three");
			list1.Add("four");
			list1.Add("five");
			list1.Add("six");
			list1.Add("seven");
			list1.Add("eight");
			list1.Add("nine");
			list1.Add("ten");
			return list1;
		}

		public static DataTable DSDataTable()
		{
			return DSDataTable(0, 1);
		}
		public static DataTable DSDataTable(int startPage, int pages)
		{
			return DSDataTable(startPage, pages, "");
		}
		public static DataTable DSDataTable(int startPage, int pages, string text)
		{
			return DSDataTable(startPage, pages, text, true);
		}
		public static DataTable DSDataTable(int startPage, int pages, string text, bool numPages)
		{
			DataTable table2 = new DataTable("Customers");
			DataColumn column1 = table2.Columns.Add("ID", typeof(int));
			column1.AllowDBNull = false;
			column1.AutoIncrement = true;
			column1.AutoIncrementSeed = 1;
			column1.AutoIncrementStep = 1;
			column1.Unique = true;
			DataColumn[] columnArray1 = new DataColumn[] { column1 } ;
			table2.PrimaryKey = columnArray1;
			column1 = table2.Columns.Add("Name", typeof(string));
			column1.MaxLength = 100;
			column1.DefaultValue = "nobody";
			column1 = table2.Columns.Add("Company", typeof(string));
			column1.MaxLength = 100;
			column1.DefaultValue = "nonexistent";
			ArrayList list1 = DSArrayList();
			IEnumerator enumerator1 = list1.GetEnumerator();
			for (int i = startPage; i < startPage+pages; i++)
			{
				enumerator1.Reset();
				while (enumerator1.MoveNext())
				{
					string page_num = "";
					if (numPages)
						page_num = i.ToString() + "_";

					DataRow row1 = table2.NewRow();
					row1["Name"] = text + "n_" + page_num + (string)(enumerator1.Current);
					row1["Company"] = text + "c_" + page_num + (string)(enumerator1.Current);
					table2.Rows.Add(row1);
				}
			}
			return table2;
		}

		public static DataSet DSDataSet()
		{
			DataSet set1 = new DataSet("CustOrdersDS");
			DataTable table1 = new DataTable("Customers");
			set1.Tables.Add(table1);
			DataTable table2 = new DataTable("Orders");
			set1.Tables.Add(table2);
			DataColumn column1 = table1.Columns.Add("ID", typeof(int));
			column1.AllowDBNull = false;
			column1.AutoIncrement = true;
			column1.AutoIncrementSeed = 1;
			column1.AutoIncrementStep = 1;
			column1.Unique = true;
			DataColumn[] columnArray1 = new DataColumn[] { column1 } ;
			table1.PrimaryKey = columnArray1;
			column1 = table1.Columns.Add("Name", typeof(string));
			column1.MaxLength = 14;
			column1.DefaultValue = "nobody";
			column1 = table1.Columns.Add("Company", typeof(string));
			column1.MaxLength = 14;
			column1.DefaultValue = "nonexistent";
			ArrayList list1 = DSArrayList();
			IEnumerator enumerator1 = list1.GetEnumerator();
			while (enumerator1.MoveNext())
			{
				DataRow row1 = table1.NewRow();
				row1["Name"] = "n_" + (string)(enumerator1.Current);
				row1["Company"] = "c_" + (string)(enumerator1.Current);
				table1.Rows.Add(row1);
			}
			column1 = table2.Columns.Add("ID", typeof(int));
			column1.AllowDBNull = false;
			column1.AutoIncrement = true;
			column1.AutoIncrementSeed = 1;
			column1.AutoIncrementStep = 1;
			column1.Unique = true;
			table2.Columns.Add("CustID", typeof(int));
			table2.Columns.Add("Date", typeof(DateTime));
			table2.Columns.Add("Total", typeof(decimal));
			int num1 = 1;
			do
			{
				DataRow row2 = table2.NewRow();
				row2["CustID"] = num1;
				row2["Date"] = new DateTime(0x3aac5ed800);
				row2["Total"] = num1 * num1;
				table2.Rows.Add(row2);
				num1++;
			}
			while (num1 <= 10);
			columnArray1 = new DataColumn[] { column1 } ;
			table2.PrimaryKey = columnArray1;
			return set1;
		}


	}
}
