using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq;

	/// <summary>
	/// Turn CROSS APPLY into CROSS JOIN when the right side 
	/// of the apply doesn't reference anything on the left side.
	/// 
	/// Any query which has a CROSS APPLY which cannot be converted to
	/// a CROSS JOIN is annotated so that we can give a meaningful
	/// error message later for SQL2K.
	/// </summary>
	internal class SqlCrossApplyToCrossJoin { 
		internal static SqlNode Reduce(SqlNode node, SqlNodeAnnotations annotations) {
			Reducer r = new Reducer();
			r.Annotations = annotations;
			return r.Visit(node);			
		}

		class Reducer : SqlVisitor {
			internal SqlNodeAnnotations Annotations;

			internal override SqlSource VisitJoin(SqlJoin join) {
				if (join.JoinType == SqlJoinType.CrossApply) {
					// Look down the left side to see what table aliases are produced.
					HashSet<SqlAlias> p = SqlGatherProducedAliases.Gather(join.Left);
					// Look down the right side to see what table aliases are consumed.
					HashSet<SqlAlias> c = SqlGatherConsumedAliases.Gather(join.Right);
					// Look at each consumed alias and see if they are mentioned in produced.
                    if (p.Overlaps(c)) {
						Annotations.Add(join, new SqlServerCompatibilityAnnotation(Strings.SourceExpressionAnnotation(join.SourceExpression), SqlProvider.ProviderMode.Sql2000));
						// Can't reduce because this consumed alias is produced on the left.
						return base.VisitJoin(join);
					}

					// Can turn this into a CROSS JOIN
					join.JoinType = SqlJoinType.Cross;
					return VisitJoin(join);
				}
				return base.VisitJoin(join);
			}
		}
	}
}
