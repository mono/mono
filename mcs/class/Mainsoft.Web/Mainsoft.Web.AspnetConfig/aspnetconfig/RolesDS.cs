// Mainsoft.Web.AspnetConfig - Site AspnetConfig utility
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
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


using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

namespace Mainsoft.Web.AspnetConfig
{
    public class RolesDS
    {
        public static DataTable Select()
        {
            return CreateDataTable();
        }

        public static DataTable SelectUser()
        {
            return CreateUserDataTable();
        }

        public static DataTable SelectUser(string searchtag, string searchby)
        {
            return CreateUserDataTable(searchtag, searchby);
        }

        public static DataTable SelectUsersRole(string user)
        {
            return CreateUsersRoles(user);
        }

        public static DataTable CreateUsersRoles(string user)
        {
            DataTable aTable = new DataTable("A");
            DataColumn dtCol;
            DataColumn dtCol1;
            DataRow dtRow;


            // Create User Name column and add to the table
            dtCol = new DataColumn();
            dtCol.DataType = Type.GetType("System.String");
            dtCol.ColumnName = "Role";
            dtCol.AutoIncrement = false;
            dtCol.ReadOnly = false;
            dtCol.Unique = true;
            aTable.Columns.Add(dtCol);

            dtCol1 = new DataColumn();
            dtCol1.DataType = Type.GetType("System.Boolean");
            dtCol1.ColumnName = "IsInRole";
            dtCol1.AutoIncrement = false;
            dtCol1.ReadOnly = false;
            dtCol1.Unique = false;
            aTable.Columns.Add(dtCol1);

	    if (Roles.Enabled) {
		    // Create rows to the table
		    foreach (String role in Roles.GetAllRoles ()) {
			    dtRow = aTable.NewRow ();
			    dtRow["Role"] = role;
			    dtRow["IsInRole"] = Roles.IsUserInRole (user, role);
			    aTable.Rows.Add (dtRow);
		    }
	    }
            aTable.PrimaryKey = new DataColumn[] { aTable.Columns["Role"] };
            return aTable;
        }

        public static DataTable Delete(string Role)
        {
            if (Roles.RoleExists(Role))
            {
                Roles.DeleteRole(Role,false);
            }
            return Select();
        }

        public static DataTable DeleteUser(string User)
        {
            Membership.DeleteUser(User, true);
            return SelectUser();
        }

        public static DataTable CreateUserDataTable()
        {
            DataTable aTable = new DataTable("A");
            DataColumn dtCol;
            DataColumn dtCol1;
            DataRow dtRow;

            // Create User Name column and add to the table
            dtCol = new DataColumn();
            dtCol.DataType = Type.GetType("System.String");
            dtCol.ColumnName = "User";
            dtCol.AutoIncrement = false;
            dtCol.ReadOnly = false;
            dtCol.Unique = true;
            aTable.Columns.Add(dtCol);

            dtCol1 = new DataColumn();
            dtCol1.DataType = Type.GetType("System.Boolean");
            dtCol1.ColumnName = "Active";
            dtCol1.AutoIncrement = false;
            dtCol1.ReadOnly = false;
            dtCol1.Unique = false;
            aTable.Columns.Add(dtCol1);

            // Create rows to the table
            foreach (MembershipUser user in Membership.GetAllUsers())
            {
                dtRow = aTable.NewRow();
                dtRow["User"] = user.UserName;
                dtRow["Active"] = user.IsApproved;
                aTable.Rows.Add(dtRow);
            }

            aTable.PrimaryKey = new DataColumn[] { aTable.Columns["User"] };
            return aTable;
        }

        public static DataTable CreateUserDataTable(string searchtag, string searchby)
        {
            DataTable aTable = new DataTable("A");
            DataColumn dtCol;
            DataColumn dtCol1;
            DataRow dtRow;

            dtCol = new DataColumn();
            dtCol.DataType = Type.GetType("System.String");
            dtCol.ColumnName = "User";
            dtCol.AutoIncrement = false;
            dtCol.ReadOnly = false;
            dtCol.Unique = true;
            aTable.Columns.Add(dtCol);

            dtCol1 = new DataColumn();
            dtCol1.DataType = Type.GetType("System.Boolean");
            dtCol1.ColumnName = "Active";
            dtCol1.AutoIncrement = false;
            dtCol1.ReadOnly = false;
            dtCol1.Unique = false;
            aTable.Columns.Add(dtCol1);

            string text = searchtag;
            text = text.Replace("*", "%");
            text = text.Replace("?", "_");

            if (text.Trim() == "\"\"")
                text = "%";

            if (searchby == "Name")
            {
                foreach (MembershipUser user in Membership.FindUsersByName(text))
                {
                    dtRow = aTable.NewRow();
                    dtRow["User"] = user.UserName;
                    dtRow["Active"] = user.IsApproved;
                    aTable.Rows.Add(dtRow);
                }
            }
            else // Mail
            {
                foreach (MembershipUser user in Membership.FindUsersByEmail(text))
                {
                    dtRow = aTable.NewRow();
                    dtRow["User"] = user.UserName;
                    dtRow["Active"] = user.IsApproved;
                    aTable.Rows.Add(dtRow);
                }
            }
            
            aTable.PrimaryKey = new DataColumn[] { aTable.Columns["User"] };
            return aTable;
        }

        public static DataTable CreateDataTable()
        {
            DataTable aTable = new DataTable("A");
            DataColumn dtCol;
            DataRow dtRow;

            // Create Name column and add to the table
            dtCol = new DataColumn();
            dtCol.DataType = Type.GetType("System.String");
            dtCol.ColumnName = "Role";
            dtCol.AutoIncrement = false;
            dtCol.Caption = "Role Name";
            dtCol.ReadOnly = false;
            dtCol.Unique = false;
            aTable.Columns.Add(dtCol);

	    if (Roles.Enabled) {
		    // Create rows to the table
		    foreach (string str in Roles.GetAllRoles ()) {
			    dtRow = aTable.NewRow ();
			    dtRow["Role"] = str;
			    aTable.Rows.Add (dtRow);
		    }
	    }

            aTable.PrimaryKey = new DataColumn[] { aTable.Columns["Role"] };
            return aTable;
        }

        public static DataTable CreateManageRoleTable(string role, string searchtag, string searchby)
        {
            DataTable aTable = new DataTable("A");
            DataColumn dtCol;
            DataColumn dtCol1;
            DataRow dtRow;

            // Create UserName column and add to the table
            dtCol = new DataColumn();
            dtCol.DataType = Type.GetType("System.String");
            dtCol.ColumnName = "User";
            dtCol.AutoIncrement = false;
            dtCol.Caption = "User Name";
            dtCol.ReadOnly = false;
            dtCol.Unique = true;
            aTable.Columns.Add(dtCol);

            // Create User in role bool column and add to the table
            dtCol1 = new DataColumn();
            dtCol1.DataType = Type.GetType("System.Boolean");
            dtCol1.ColumnName = "InRole";
            dtCol1.AutoIncrement = false;
            dtCol1.Caption = "User Is In Role";
            dtCol1.ReadOnly = false;
            dtCol1.Unique = false;
            aTable.Columns.Add(dtCol1);

            string text = searchtag;
            text = text.Replace("*", "%");
            text = text.Replace("?", "_");

            if (text.Trim() == "\"\"")
                text = "%";

            if (searchby == "Name")
            {
                foreach (MembershipUser user in Membership.FindUsersByName(text))
                {
                    dtRow = aTable.NewRow();
                    dtRow["User"] = user.UserName;
		    if (Roles.Enabled) {
			    dtRow["InRole"] = Roles.IsUserInRole (user.UserName, role);
		    }
		    else {
			    dtRow["InRole"] = false;
		    }
                    aTable.Rows.Add(dtRow);
                }
            }
            else // Mail
            {
                foreach (MembershipUser user in Membership.FindUsersByEmail(text))
                {
                    dtRow = aTable.NewRow();
                    dtRow["User"] = user.UserName;
		    if (Roles.Enabled) {
			    dtRow["InRole"] = Roles.IsUserInRole (user.UserName, role);
		    }
		    else {
			    dtRow["InRole"] = false;
		    }
                    aTable.Rows.Add(dtRow);
                }
            }
            aTable.PrimaryKey = new DataColumn[] { aTable.Columns["User"] };
            return aTable;
        }
    }
}


