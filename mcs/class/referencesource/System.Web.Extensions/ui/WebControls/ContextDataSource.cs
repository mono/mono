namespace System.Web.UI.WebControls {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;

    public abstract class ContextDataSource : QueryableDataSource {
        private ContextDataSourceView _view;        

        internal ContextDataSource(IPage page)
            : base(page) {

        }        

        internal ContextDataSource(ContextDataSourceView view)
            : base(view) {
        }

        protected ContextDataSource() {
        }

        private ContextDataSourceView View {
            get {
                if (_view == null) {
                    _view = (ContextDataSourceView)GetView("DefaultView");
                }
                return _view;
            }
        }

        public virtual string ContextTypeName {
            get {
                return View.ContextTypeName;
            }
            set {
                View.ContextTypeName = value;
            }
        }

        protected string EntitySetName {
            get {
                return View.EntitySetName;
            }
            set {
                View.EntitySetName = value;
            }
        }

        public virtual string EntityTypeName {
            get {
                return View.EntityTypeName;
            }
            set {
                View.EntityTypeName = value;
            }
        }
    }
}
