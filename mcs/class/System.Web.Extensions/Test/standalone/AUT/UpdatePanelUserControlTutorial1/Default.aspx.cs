using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace UpdatePanelUserControl
{

    public partial class Default : System.Web.UI.Page
    {
        protected void EmployeeList1_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (EmployeeList1.SelectedIndex != -1)
                EmployeeInfo1.EmployeeID = EmployeeList1.EmployeeID;
            else
                EmployeeInfo1.EmployeeID = 0;

            EmployeeInfo1.Update();
        }
    }
}
