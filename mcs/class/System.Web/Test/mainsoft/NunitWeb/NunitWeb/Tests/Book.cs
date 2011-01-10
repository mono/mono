// Book.cs
using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Samples.AspNet.CS.Controls
{
    [
    AspNetHostingPermission(SecurityAction.Demand,
        Level = AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, 
        Level=AspNetHostingPermissionLevel.Minimal),
    DefaultProperty("Title"),
    ToolboxData("<{0}:Book runat=\"server\"> </{0}:Book>")
    ]
    public class Book : WebControl
    {
        private Author authorValue;
        private String initialAuthorString;

        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Description("The name of the author."),
        DesignerSerializationVisibility(
            DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        ]
        public virtual Author Author
        {
            get
            {
                if (authorValue == null)
                {
                    authorValue = new Author();
                }
                return authorValue;
            }

        }

        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(BookType.NotDefined),
        Description("Fiction or Not"),
        ]
        public virtual BookType BookType
        {
            get
            {
                object t = ViewState["BookType"];
                return (t == null) ? BookType.NotDefined : (BookType)t;
            }
            set
            {
                ViewState["BookType"] = value;
            }
        }

        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Description("The symbol for the currency."),
        Localizable(true)
        ]
        public virtual string CurrencySymbol
        {
            get
            {
                string s = (string)ViewState["CurrencySymbol"];
                return (s == null) ? String.Empty : s;
            }
            set
            {
                ViewState["CurrencySymbol"] = value;
            }
        }


        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue("0.00"),
        Description("The price of the book."),
        Localizable(true)
        ]
        public virtual Decimal Price
        {
            get
            {
                object price = ViewState["Price"];
                return (price  == null) ? Decimal.Zero : (Decimal)price;
            }
            set
            {
                ViewState["Price"] = value;
            }
        }

        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Description("The title of the book."),
        Localizable(true)
        ]
        public virtual string Title
        {
            get
            {
                string s = (string)ViewState["Title"];
                return (s == null) ? String.Empty : s;
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.WriteEncodedText(Title);
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.WriteEncodedText(Author.ToString());
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.WriteEncodedText(BookType.ToString());
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(CurrencySymbol);
            writer.Write("&nbsp;");
            writer.Write(String.Format("{0:F2}", Price));
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            Author auth = (Author)ViewState["Author"];
            if (auth != null)
            {
                authorValue = auth;
            }
        }

        protected override object SaveViewState()
        {
            if (authorValue != null)
            {
                String currentAuthorString = authorValue.ToString();
                if (!(currentAuthorString.Equals(initialAuthorString)))
                {
                    ViewState["Author"] = authorValue;
                }
            }

            return base.SaveViewState();
        }

        protected override void TrackViewState()
        {
            if (authorValue != null)
            {
                initialAuthorString = authorValue.ToString();
            }
            base.TrackViewState();
        }

    }
}
