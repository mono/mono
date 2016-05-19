namespace System.Web.Handlers {
    using System.IO;
    using System.Web.Util;
    using System.Web;
    using System.Collections;
    using System.Collections.Specialized;

    internal class TraceHandlerErrorFormatter : ErrorFormatter {
        bool _isRemote;

        internal TraceHandlerErrorFormatter(bool isRemote) {
            _isRemote = isRemote;
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Trace_Error_Title);}
        }

        protected override string Description {
            get { 
                if(_isRemote)
                    return SR.GetString(SR.Trace_Error_LocalOnly_Description);
                else
                    return HttpUtility.HtmlEncode(SR.GetString(SR.Trace_Error_Enabled_Description));
            }
        }

        protected override string MiscSectionTitle {
            get { return null; }
        }

        protected override string MiscSectionContent {
            get { return null; }
        }

        protected override string ColoredSquareTitle {
            get {
                string detailsTitle = SR.GetString(SR.Generic_Err_Details_Title);
                AdaptiveMiscContent.Add(detailsTitle);
                return detailsTitle;
            }
        }

        protected override string ColoredSquareDescription {
            get {
                string description;
                if(_isRemote)
                    description = HttpUtility.HtmlEncode(SR.GetString(SR.Trace_Error_LocalOnly_Details_Desc));
                else
                    description = HttpUtility.HtmlEncode(SR.GetString(SR.Trace_Error_Enabled_Details_Desc));

                AdaptiveMiscContent.Add(description);
                return description;
            }
        }

        protected override string ColoredSquareContent {
            get {
                string content;
                if(_isRemote)
                    content = HttpUtility.HtmlEncode(SR.GetString(SR.Trace_Error_LocalOnly_Details_Sample));
                else
                    content = HttpUtility.HtmlEncode(SR.GetString(SR.Trace_Error_Enabled_Details_Sample));

                return WrapWithLeftToRightTextFormatIfNeeded(content);
            }
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }

        internal override bool CanBeShownToAllUsers {
            get { return true;}
        }
    }
}
