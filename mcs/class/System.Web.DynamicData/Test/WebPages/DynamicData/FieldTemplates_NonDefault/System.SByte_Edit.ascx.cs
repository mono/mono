﻿using System;
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

public partial class SByte_EditField : System.Web.DynamicData.FieldTemplateUserControl {
    protected void Page_Load(object sender, EventArgs e) {
        TextBox1.MaxLength = Column.MaxLength;
        if (Column.MaxLength < 20)
            TextBox1.Columns = Column.MaxLength;
        TextBox1.ToolTip = Column.Description;

        SetUpValidator(RequiredFieldValidator1);
        SetUpValidator(RegularExpressionValidator1);
        SetUpValidator(DynamicValidator1);
    }
    
    protected override void ExtractValues(IOrderedDictionary dictionary) {
        dictionary[Column.Name] = ConvertEditedValue(TextBox1.Text);
    }

    public override Control DataControl {
        get {
            return TextBox1;
        }
    }
}
