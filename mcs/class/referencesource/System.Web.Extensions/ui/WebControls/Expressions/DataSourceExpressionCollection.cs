namespace System.Web.UI.WebControls.Expressions {
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;

    public class DataSourceExpressionCollection : StateManagedCollection {
        private IQueryableDataSource _dataSource;

        private static readonly Type[] knownTypes = new Type[] {
            typeof(SearchExpression),
            typeof(MethodExpression),
            typeof(OrderByExpression),
            typeof(RangeExpression),
            typeof(PropertyExpression),
            typeof(CustomExpression),
        };

        public HttpContext Context {
            get;
            private set;
        }

        public Control Owner {
            get;
            private set;
        }

        public DataSourceExpression this[int index] {
            get {
                return (DataSourceExpression)((IList)this)[index];
            }
            set {
                ((IList)this)[index] = value;
            }
        }

        // Allows for nested expression blocks to be initilaized after the fact
        internal void SetContext(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            Owner = owner;
            Context = context;
            _dataSource = dataSource;

            foreach (DataSourceExpression expression in this) {
                expression.SetContext(owner, context, _dataSource);
            }
        }

        public void Add(DataSourceExpression expression) {
            ((IList)this).Add(expression);
        }

        protected override object CreateKnownType(int index) {
            switch (index) {
                case 0: 
                    return new SearchExpression();
                case 1: 
                    return new MethodExpression();
                case 2: 
                    return new OrderByExpression();
                case 3: 
                    return new RangeExpression();
                case 4: 
                    return new PropertyExpression();
                case 5: 
                    return new CustomExpression();
                default:
                    throw new ArgumentOutOfRangeException("index");
            }
        }

        public void CopyTo(DataSourceExpression[] expressionArray, int index) {
            base.CopyTo(expressionArray, index);
        }

        public void Contains(DataSourceExpression expression) {
            ((IList)this).Contains(expression);
        }

        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }

        public int IndexOf(DataSourceExpression expression) {
            return ((IList)this).IndexOf(expression);
        }

        public void Insert(int index, DataSourceExpression expression) {
            ((IList)this).Insert(index, expression);
        }

        public void Remove(DataSourceExpression expression) {
            ((IList)this).Remove(expression);
        }

        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o) {
            ((DataSourceExpression)o).SetDirty();
        }
    }
}
