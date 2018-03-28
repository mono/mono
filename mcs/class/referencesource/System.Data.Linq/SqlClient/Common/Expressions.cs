using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {

    // SQL Client extensions to ExpressionType
    internal enum InternalExpressionType {
        Known = 2000,
        LinkedTable = 2001
    }

    abstract internal class InternalExpression : Expression {
#pragma warning disable 618 // Disable the 'obsolete' warning.
        internal InternalExpression(InternalExpressionType nt, Type type)
            : base ((ExpressionType)nt, type) {
        }
#pragma warning restore 618
        internal static KnownExpression Known(SqlExpression expr) {
            return new KnownExpression(expr, expr.ClrType);
        }
        internal static KnownExpression Known(SqlNode node, Type type) {
            return new KnownExpression(node, type);
        }
    }

    internal sealed class KnownExpression : InternalExpression {
        SqlNode node;
        internal KnownExpression(SqlNode node, Type type)
            : base(InternalExpressionType.Known, type) {
            this.node = node;
        }
        internal SqlNode Node {
            get { return this.node; }
        }
    }

    internal sealed class LinkedTableExpression : InternalExpression {
        private SqlLink link; 
        private ITable table; 
        internal LinkedTableExpression(SqlLink link, ITable table, Type type) 
            : base(InternalExpressionType.LinkedTable, type) {
            this.link = link;
            this.table = table;
        }
        internal SqlLink Link {
            get {return this.link;}
        }
        internal ITable Table {
            get {return this.table;}
        }
    }

}
