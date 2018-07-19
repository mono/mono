using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {
    /// <summary>
    /// Walk a tree and return the set of unique aliases it produces.
    /// </summary>
    class SqlGatherProducedAliases {
        internal static HashSet<SqlAlias> Gather(SqlNode node) {
            Gatherer g = new Gatherer();
            g.Visit(node);
            return g.Produced;
        }

		private class Gatherer : SqlVisitor {
			internal HashSet<SqlAlias> Produced = new HashSet<SqlAlias>();
			internal override SqlAlias VisitAlias(SqlAlias a) {
				Produced.Add(a);
				return base.VisitAlias(a);
			}
		}

    }
}
