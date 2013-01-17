namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    [FileLevelControlBuilder(typeof(ViewPageControlBuilder))]
    public class ViewPage : Page, IViewDataContainer {

        private DynamicViewDataDictionary _dynamicViewData;
        private string _masterLocation;
        [ThreadStatic]
        private static int _nextId;
        private ViewDataDictionary _viewData;

        public AjaxHelper<object> Ajax {
            get;
            set;
        }

        public HtmlHelper<object> Html {
            get;
            set;
        }

        public string MasterLocation {
            get {
                return _masterLocation ?? String.Empty;
            }
            set {
                _masterLocation = value;
            }
        }

        public object Model {
            get {
                return ViewData.Model;
            }
        }

        public TempDataDictionary TempData {
            get {
                return ViewContext.TempData;
            }
        }

        public UrlHelper Url {
            get;
            set;
        }

        public dynamic ViewBag {
            get {
                if (_dynamicViewData == null) {
                    _dynamicViewData = new DynamicViewDataDictionary(() => ViewData);
                }
                return _dynamicViewData;
            }
        }

        public ViewContext ViewContext {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is the mechanism by which the ViewPage gets its ViewDataDictionary object.")]
        public ViewDataDictionary ViewData {
            get {
                if (_viewData == null) {
                    SetViewData(new ViewDataDictionary());
                }
                return _viewData;
            }
            set {
                SetViewData(value);
            }
        }

        public HtmlTextWriter Writer {
            get;
            private set;
        }

        public virtual void InitHelpers() {
            Ajax = new AjaxHelper<object>(ViewContext, this);
            Html = new HtmlHelper<object>(ViewContext, this);
            Url = new UrlHelper(ViewContext.RequestContext);
        }

        internal static string NextId() {
            return (++_nextId).ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnPreInit(EventArgs e) {
            base.OnPreInit(e);

            if (!String.IsNullOrEmpty(MasterLocation)) {
                MasterPageFile = MasterLocation;
            }
        }

        public override void ProcessRequest(HttpContext context) {
            // Tracing requires IDs to be unique.
            ID = NextId();

            base.ProcessRequest(context);
        }

        protected override void Render(HtmlTextWriter writer) {
            Writer = writer;
            try {
                base.Render(writer);
            }
            finally {
                Writer = null;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is disposed in the finally block of the method")]
        public virtual void RenderView(ViewContext viewContext) {
            ViewContext = viewContext;
            InitHelpers();

            bool createdSwitchWriter = false;
            SwitchWriter switchWriter = viewContext.HttpContext.Response.Output as SwitchWriter;

            try {
                if (switchWriter == null) {
                    switchWriter = new SwitchWriter();
                    createdSwitchWriter = true;
                }

                using (switchWriter.Scope(viewContext.Writer)) {
                    if (createdSwitchWriter) {
                        // It's safe to reset the _nextId within a Server.Execute() since it pushes a new TraceContext onto
                        // the stack, so there won't be an ID conflict.
                        int originalNextId = _nextId;
                        try {
                            _nextId = 0;
                            viewContext.HttpContext.Server.Execute(HttpHandlerUtil.WrapForServerExecute(this), switchWriter, true /* preserveForm */);
                        }
                        finally {
                            // Restore the original _nextId in case this isn't actually the outermost view, since resetting
                            // the _nextId may now cause trace ID conflicts in the outer view.
                            _nextId = originalNextId;
                        }
                    }
                    else {
                        ProcessRequest(HttpContext.Current);
                    }
                }
            }
            finally {
                if (createdSwitchWriter) {
                    switchWriter.Dispose();
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "textWriter", Justification = "This method existed in MVC 1.0 and has been deprecated.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method existed in MVC 1.0 and has been deprecated.")]
        [Obsolete("The TextWriter is now provided by the ViewContext object passed to the RenderView method.", true /* error */)]
        public void SetTextWriter(TextWriter textWriter) {
            // this is now a no-op
        }

        protected virtual void SetViewData(ViewDataDictionary viewData) {
            _viewData = viewData;
        }

        internal class SwitchWriter : TextWriter {
            public SwitchWriter()
                : base(CultureInfo.CurrentCulture) {
            }

            public override Encoding Encoding {
                get {
                    return InnerWriter.Encoding;
                }
            }

            public override IFormatProvider FormatProvider {
                get {
                    return InnerWriter.FormatProvider;
                }
            }

            internal TextWriter InnerWriter {
                get;
                set;
            }

            public override string NewLine {
                get {
                    return InnerWriter.NewLine;
                }
                set {
                    InnerWriter.NewLine = value;
                }
            }

            public override void Close() {
                InnerWriter.Close();
            }

            public override void Flush() {
                InnerWriter.Flush();
            }

            public IDisposable Scope(TextWriter writer) {
                WriterScope scope = new WriterScope(this, InnerWriter);

                try {
                    if (writer != this) {
                        InnerWriter = writer;
                    }

                    return scope;
                }
                catch {
                    scope.Dispose();
                    throw;
                }
            }

            public override void Write(bool value) {
                InnerWriter.Write(value);
            }

            public override void Write(char value) {
                InnerWriter.Write(value);
            }

            public override void Write(char[] buffer) {
                InnerWriter.Write(buffer);
            }

            public override void Write(char[] buffer, int index, int count) {
                InnerWriter.Write(buffer, index, count);
            }

            public override void Write(decimal value) {
                InnerWriter.Write(value);
            }

            public override void Write(double value) {
                InnerWriter.Write(value);
            }

            public override void Write(float value) {
                InnerWriter.Write(value);
            }

            public override void Write(int value) {
                InnerWriter.Write(value);
            }

            public override void Write(long value) {
                InnerWriter.Write(value);
            }

            public override void Write(object value) {
                InnerWriter.Write(value);
            }

            public override void Write(string format, object arg0) {
                InnerWriter.Write(format, arg0);
            }

            public override void Write(string format, object arg0, object arg1) {
                InnerWriter.Write(format, arg0, arg1);
            }

            public override void Write(string format, object arg0, object arg1, object arg2) {
                InnerWriter.Write(format, arg0, arg1, arg2);
            }

            public override void Write(string format, params object[] arg) {
                InnerWriter.Write(format, arg);
            }

            public override void Write(string value) {
                InnerWriter.Write(value);
            }

            public override void Write(uint value) {
                InnerWriter.Write(value);
            }

            public override void Write(ulong value) {
                InnerWriter.Write(value);
            }

            public override void WriteLine() {
                InnerWriter.WriteLine();
            }

            public override void WriteLine(bool value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(char value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(char[] buffer) {
                InnerWriter.WriteLine(buffer);
            }

            public override void WriteLine(char[] buffer, int index, int count) {
                InnerWriter.WriteLine(buffer, index, count);
            }

            public override void WriteLine(decimal value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(double value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(float value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(int value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(long value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(object value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(string format, object arg0) {
                InnerWriter.WriteLine(format, arg0);
            }

            public override void WriteLine(string format, object arg0, object arg1) {
                InnerWriter.WriteLine(format, arg0, arg1);
            }

            public override void WriteLine(string format, object arg0, object arg1, object arg2) {
                InnerWriter.WriteLine(format, arg0, arg1, arg2);
            }

            public override void WriteLine(string format, params object[] arg) {
                InnerWriter.WriteLine(format, arg);
            }

            public override void WriteLine(string value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(uint value) {
                InnerWriter.WriteLine(value);
            }

            public override void WriteLine(ulong value) {
                InnerWriter.WriteLine(value);
            }

            private sealed class WriterScope : IDisposable {
                private SwitchWriter _switchWriter;
                private TextWriter _writerToRestore;

                public WriterScope(SwitchWriter switchWriter, TextWriter writerToRestore) {
                    _switchWriter = switchWriter;
                    _writerToRestore = writerToRestore;
                }

                public void Dispose() {
                    _switchWriter.InnerWriter = _writerToRestore;
                }
            }

        }

    }
}
