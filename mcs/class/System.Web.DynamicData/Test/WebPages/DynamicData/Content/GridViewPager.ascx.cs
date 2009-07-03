using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Web.DynamicData;

public partial class GridViewPager : System.Web.UI.UserControl {
    private GridView _gridView;

    protected void Page_Load(object sender, EventArgs e) {
        Control c = Parent;
        while (c != null) {
            if (c is GridView) {
                _gridView = (GridView)c;
                break;
            }
            c = c.Parent;
        }
    }

    protected void TextBoxPage_TextChanged(object sender, EventArgs e) {
        if (_gridView == null) {
            return;
        }
        int page;
        if (int.TryParse(TextBoxPage.Text.Trim(), out page)) {
            if (page <= 0) {
                page = 1;
            }
            if (page > _gridView.PageCount) {
                page = _gridView.PageCount;
            }
            _gridView.PageIndex = page - 1;
        }
        TextBoxPage.Text = (_gridView.PageIndex + 1).ToString();
    }

    protected void DropDownListPageSize_SelectedIndexChanged(object sender, EventArgs e) {
        if (_gridView == null) {
            return;
        }
        DropDownList dropdownlistpagersize = (DropDownList)sender;
        _gridView.PageSize = Convert.ToInt32(dropdownlistpagersize.SelectedValue);
        int pageindex = _gridView.PageIndex;
        _gridView.DataBind();
        if (_gridView.PageIndex != pageindex) {
            //if page index changed it means the previous page was not valid and was adjusted. Rebind to fill control with adjusted page
            _gridView.DataBind();
        }
    }

    protected void Page_PreRender(object sender, EventArgs e) {
        if (_gridView != null) {
            LabelNumberOfPages.Text = _gridView.PageCount.ToString();
            TextBoxPage.Text = (_gridView.PageIndex + 1).ToString();
            DropDownListPageSize.SelectedValue = _gridView.PageSize.ToString();
        }
    }
}
