namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq.Expressions;
    using System.Web.UI.WebControls;

    [
    PersistChildren(false),
    ParseChildren(true, "Parameters")
    ]
    public abstract class ParameterDataSourceExpression : DataSourceExpression {
        private ParameterCollection _parameters;

        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ParameterCollection Parameters {
            get {
                if (_parameters == null) {
                    _parameters = new ParameterCollection();
                    _parameters.ParametersChanged += new EventHandler(OnParametersChanged);
                }
                return _parameters;
            }
        }

        internal virtual IDictionary<string, object> GetValues() {
            return Parameters.ToDictionary(Context, Owner);
        }

        public override void SetContext(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            base.SetContext(owner, context, dataSource);

            owner.Page.LoadComplete += new EventHandler(OnPageLoadComplete);
        }

        private void OnParametersChanged(object sender, EventArgs e) {
            if (DataSource != null) {
                DataSource.RaiseViewChanged();
            }
        }

        private void OnPageLoadComplete(object sender, System.EventArgs e) {
            Parameters.UpdateValues(Context, Owner);
        }

        protected override object SaveViewState() {
            Pair p = new Pair();
            p.First = base.SaveViewState();
            p.Second = DataSourceHelper.SaveViewState(_parameters);
            return p;
        }

        protected override void LoadViewState(object savedState) {
            Pair p = (Pair)savedState;
            base.LoadViewState(p.First);
            if (p.Second != null) {
                ((IStateManager)Parameters).LoadViewState(p.Second);
            }
        }

        protected override void TrackViewState() {
            base.TrackViewState();
            DataSourceHelper.TrackViewState(_parameters);
        }
    }
}
