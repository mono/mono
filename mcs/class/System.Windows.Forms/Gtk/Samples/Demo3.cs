//
// System.Windows.Forms Demo2 app
//
//Authors: 
//    Joel Basson (jstrike@mweb.co.za)
//   
//

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using ByteFX.Data.MySqlClient;

namespace demo3
{
	
	public class GtkForm : System.Windows.Forms.Form
	{
		private DataGrid dg1 = new DataGrid();

		private void InitializeWidgets()
		{
			this.dg1.Location = new System.Drawing.Point(30,30);
			this.dg1.Size = new System.Drawing.Size(100,100);
			
			this.Controls.AddRange(new System.Windows.Forms.Control[] { 
									    this.dg1});


			this.Size = new Size(512, 250);

		}

		private void Connect()
		{
			//MySqlConnection dbcon;
			DataSet custDS;
			MySqlDataAdapter custDA;		
			string TableName;

			string connectionString = 
          		"Server=yoursever;" +
          		"Database=yourdatabase;" +
          		"User ID=username;" +
          		"Password=password;";
       			MySqlConnection dbcon;
       			dbcon = new MySqlConnection(connectionString);
       			dbcon.Open();
       			
				custDA = new MySqlDataAdapter();
				custDA.SelectCommand = new MySqlCommand("SELECT * FROM Customer", dbcon);
				MySqlCommandBuilder custCB = new MySqlCommandBuilder(custDA);


				custDS = new DataSet();

				//dbcon.Open();


				custDA.Fill(custDS, "Customer");
			
				//sbananaConn.Close();
			
				//return custDS; 
				
				
       			//dbcmd.Dispose();
       			//dbcmd = null;
				dg1.DataMember = "Customer";
				dg1.DataSource = custDS;
				dg1.SetDataBinding(custDS, "Customer");
       			dbcon.Close();
       			dbcon = null;
		}
		
		public GtkForm()
		{
			InitializeWidgets();
			Connect();
		}


	}
	
	public class GtkMain
	{
		[STAThread]
		public static void Main()
		{
			GtkForm form1 = new GtkForm ();
			form1.Text = "System.Windows.Forms at work!";			
			Application.Run(form1);
		}
	}
}
