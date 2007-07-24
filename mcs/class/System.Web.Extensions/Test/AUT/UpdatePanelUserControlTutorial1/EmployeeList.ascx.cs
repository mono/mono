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
    public partial class EmployeeList : System.Web.UI.UserControl
    {
        public int EmployeeID
        {
            get
            {
                if (EmployeesGridView.SelectedIndex != -1)
                    return (int)EmployeesGridView.DataKeys[EmployeesGridView.SelectedIndex].Value;
                else
                    return -1;
            }
        }

        public UpdatePanelUpdateMode UpdateMode
        {
            get { return this.EmployeeListUpdatePanel.UpdateMode; }
            set { this.EmployeeListUpdatePanel.UpdateMode = value; }
        }

        public void Update()
        {
            this.EmployeeListUpdatePanel.Update();
        }

        public int SelectedIndex
        {
            get { return this.EmployeesGridView.SelectedIndex; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.LastUpdatedLabel.Text = DateTime.Now.ToString();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EmployeesGridView.SelectedIndexChanged += this.EmployeesGridView_SelectedIndexChanged;
            this.EmployeesGridView.PageIndexChanged += this.EmployeesGridView_PageIndexChanged;
            this.EmployeesGridView.DataBound += this.EmployeesGridView_DataBound;
            this.EmployeesGridView.Sorted += this.EmployeesGridView_Sorted;
        }

        public event EventHandler SelectedIndexChanged;

        protected void OnSelectedIndexChanged(EventArgs e)
        {
            if (SelectedIndexChanged != null)
            {
                SelectedIndexChanged(this, e);
            }
        }

        private void EmployeesGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedIndexChanged(this, e);

            ViewState["SelectedEmployeeID"] = this.EmployeeID;
        }

        private void EmployeesGridView_PageIndexChanged(object sender, EventArgs e)
        {
            if (EmployeesGridView.SelectedIndex != -1)
            {
                this.EmployeesGridView.SelectedIndex = -1;
                this.SelectedIndexChanged(this, e);
            }
        }

        private void EmployeesGridView_Sorted(object sender, EventArgs e)
        {
            if (EmployeesGridView.SelectedIndex != -1)
            {
                this.EmployeesGridView.SelectedIndex = -1;
                this.SelectedIndexChanged(this, e);
            }
        }

        protected void EmployeesGridView_DataBound(object sender, EventArgs e)
        {
            int selectedEmployeeID =
              ViewState["SelectedEmployeeID"] == null ? -1 : (int)ViewState["SelectedEmployeeID"];

            for (int i = 0; i < EmployeesGridView.Rows.Count; i++)
                if ((int)EmployeesGridView.DataKeys[i].Value == selectedEmployeeID)
                {
                    EmployeesGridView.SelectedIndex = i;
                    this.SelectedIndexChanged(this, e);
                }
        }
    }
}