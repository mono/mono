using System;
using System.IO;
using System.Xml.Serialization;
using System.Data;

public class Test 
{
	public static void Main()
	{
		Test t=new Test();
		t.Create("dataset.xml");
		t.Read("dataset.xml");
	}
	
	private void Create(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(DataSet));

		/* Create a new DataSet; add a table, column and ten rows */
		DataSet ds=new DataSet("myDataSet");
		DataTable t=new DataTable("table1");
		DataColumn c=new DataColumn("thing");

		t.Columns.Add(c);
		ds.Tables.Add(t);

		DataRow r;
		for(int i=0; i<10; i++) {
			r=t.NewRow();
			r[0]="Thing "+i;
			t.Rows.Add(r);
		}

		TextWriter writer=new StreamWriter(filename);
		ser.Serialize(writer, ds);
		writer.Close();
	}

	private void Read(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(DataSet));
		FileStream fs=new FileStream(filename, FileMode.Open);
		DataSet ds;

		ds=(DataSet)ser.Deserialize(fs);
		fs.Close();
		
		Console.WriteLine("DataSet name: "+ds.DataSetName);
		Console.WriteLine("DataSet locale: "+ds.Locale.Name);

		foreach(DataTable t in ds.Tables) 
		{
			Console.WriteLine("Table name: "+t.TableName);
			Console.WriteLine("Table locale: "+t.Locale.Name);

			foreach(DataColumn c in t.Columns) 
			{
				Console.WriteLine("Column name: "+c.ColumnName);
				Console.WriteLine("Null allowed? "+c.AllowDBNull);
				
			}

			foreach(DataRow r in t.Rows)
			{
				Console.WriteLine("Row: "+(string)r[0]);
			}
		}
	}
}

