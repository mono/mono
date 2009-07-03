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

public partial class Boolean_EditField : System.Web.DynamicData.FieldTemplateUserControl {
    protected override void OnDataBinding(EventArgs e) {
        base.OnDataBinding(e);

        object val = FieldValue;
        if (val != null)
            CheckBox1.Checked = (bool) val;
    }

    protected override void ExtractValues(IOrderedDictionary dictionary) {
        dictionary[Column.Name] = CheckBox1.Checked;
    }

    public override Control DataControl {
        get {
            return CheckBox1;
        }
    }
}
