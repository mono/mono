using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;

namespace test
{
  public class SimplePage : System.Web.UI.Page
  {
    protected DataGrid testGrid;

    public SimplePage()
    {
      this.Load += new System.EventHandler(this.Page_Load);
    }

    private void Page_Load(object o, EventArgs e)
    {
      IDataReader reader = new DummyReader ();
      testGrid.DataSource = reader;
      testGrid.DataBind();
    }
  }

  class DummyReader : IDataReader, IEnumerable {
  	IEnumerator IEnumerable.GetEnumerator ()
	{
		return new EnumThis ();
	}

	class EnumThis : IEnumerator {
		public bool MoveNext ()
		{
			return false;
		}

		public void Reset ()
		{
		}

		public object Current {
			get { return null; }
		}
	}

	public void Close ()
	{
		Console.WriteLine ("Close");
	}
		
	public DataTable GetSchemaTable ()
	{
		Console.WriteLine ("GetSchemaTable");
		return null;
	}
		
	public bool NextResult ()
	{
		Console.WriteLine ("NextResult");
		return false;
	}

	public bool Read ()
	{
		Console.WriteLine ("Read");
		return false;
	}

	public int Depth {
		get {
			Console.WriteLine ("Depth");
			return 0;
		}
	}

	public bool IsClosed {
		get {
			Console.WriteLine ("IsClosed");
			return false;
		}
	}

	public int RecordsAffected {
		get {
			Console.WriteLine ("RecordsAffected");
			return -1;
		}
	}

	public void Dispose ()
	{
		Console.WriteLine ("Dispose");
	}

	public bool GetBoolean(int i)
	{
		return false;
	}

	public  byte GetByte(int i)
	{
		return 0;
	}
	public  long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
	{
		return 0;
	}


	public  char GetChar(int i)
	{
		return 'A';
	}


	public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
	{
		return 0;
	}


	public  IDataReader GetData(int i)
	{
		return null;
	}


	public  string GetDataTypeName(int i)
	{
		return null;
	}


	public  DateTime GetDateTime(int i)
	{
		return DateTime.MinValue;
	}


	public  Decimal GetDecimal(int i)
	{
		return 0;
	}


	public  double GetDouble(int i)
	{
		return 0;
	}


	public  Type GetFieldType(int i)
	{
		return null;
	}


	public  float GetFloat(int i)
	{
		return 0;
	}


	public  Guid GetGuid(int i)
	{
		return new Guid ();
	}


	public  short GetInt16(int i)
	{
		return 0;
	}


	public  int GetInt32(int i)
	{
		return 0;
	}


	public  long GetInt64(int i)
	{
		return 0;
	}


	public  string GetName(int i)
	{
		return null;
	}


	public  int GetOrdinal(string name)
	{
		return 0;
	}


	public  string GetString(int i)
	{
		return null;
	}


	public  object GetValue(int i)
	{
		return null;
	}


	public  int GetValues(object[] values)
	{
		return 0;
	}


	public  bool IsDBNull(int i)
	{
		return false;
	}


	public int FieldCount {
		get { return 0; }
	}

	public object this [string name] {
		get { return null; }
	}

	public object this [int i] {
		get { return null; }
	}
  }
}
