using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamplesCS
{
    public class CustomUpdatePanel : System.Web.UI.UpdatePanel
    {
        public CustomUpdatePanel()
        {
        }

        private String _groupingText;
        public String GroupingText
        {
            get { return _groupingText; }
            set { _groupingText = value; }
        }

        protected override Control CreateContentTemplateContainer()
        {
            MyContentTemplateContainer myContentTemplateContainer =
                new MyContentTemplateContainer(_groupingText);
            return myContentTemplateContainer;
        }
        private sealed class MyContentTemplateContainer : Control
        {
            private String _displayText;
            public MyContentTemplateContainer(string groupingText)
            {
                _displayText = groupingText;
            }
            protected override void Render(HtmlTextWriter writer)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                writer.Write(_displayText);
                writer.RenderEndTag();
                base.Render(writer);
                writer.RenderEndTag();
            }
        }

    }
}
