<%@ WebService Language="c#" Class="DataService" %>
using System.Data;
using System.Web.Services;

public class DataService
{
	[WebMethod]
	public DataSet QueryData (string query)
	{
		DataSet ds = new DataSet();
		DataTable dt = ds.Tables.Add("PhoneNumbers");
		dt.Columns.Add("name");
		dt.Columns.Add("home");
		DataRow newRow;
		newRow = dt.NewRow();
		newRow["name"] = "Lluis";
		newRow["home"] = "23452345";
		dt.Rows.Add (newRow);
		newRow = dt.NewRow();
		newRow["name"] = "Pep";
		newRow["home"] = "435345";
		dt.Rows.Add (newRow);
		return ds;
	}
	
	[WebMethod]
	public int SaveData (DataSet dset)
	{
		DataTable t = dset.Tables["PhoneNumbers"];
		return t.Rows.Count;
	}
}
