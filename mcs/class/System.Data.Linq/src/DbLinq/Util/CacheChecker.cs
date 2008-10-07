using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using System.Text;

#if MONO_STRICT
using System.Data.Linq;
using System.Data.Linq.Identity;
#else
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Identity;
#endif

using DbLinq.Linq;
using DbLinq.Util;

namespace DbLinq.Util
{
    internal class CacheChecker
    {
        /// <summary>
        /// Quote from MSDN:
        /// If the object requested by the query is easily identifiable as one
        /// already retrieved, no query is executed. The identity table acts as a cache
        /// of all previously retrieved objects

        /// From Matt Warren: http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=345635&SiteID=1
        /// The cache is checked when the query is a simple table.Where(pred) or table.First(pred) where the 
        /// predicate refers only to the primary key.  Otherwise the query is always sent and the cache only checked 
        /// after the results are retrieved. 
        /// The DLINQ cache is not distributed or shared, it is local and contained within the context.  It is only a 
        /// referential identity cache used to guarantee that two reads of the same entity return the same instance. 
        /// You are not expected to hold the cache for an extended duration (except possibly for a client scenario), 
        /// or share it across threads, processes, or machines in a cluster. 
        /// </summary>
        public static bool TryRetrieveFromCache<S>(SessionVarsParsed vars, Expression scalarExpr, out S cachedRow)
        {
            cachedRow = default(S);
            int count1 = vars.ExpressionChain.Count;
            MethodCallExpression scalarMethodCall = scalarExpr.XMethodCall();

            bool isSingleWhere = false;
            LambdaExpression whereLambda = null;

            if (count1 == 1 && scalarMethodCall.Arguments.Count == 1)
            {
                MethodCallExpression call0 = vars.ExpressionChain[0];
                if (call0.Method.Name == "Where" && call0.Arguments.Count == 2)
                {
                    //db.Customers.Where(id==1).Single()
                    isSingleWhere = true;
                    whereLambda = call0.Arguments[1].XLambda();
                }
            }
            else if (count1 == 0 && scalarMethodCall.Arguments.Count == 2)
            {
                //db.Customers.Single(id==1)
                isSingleWhere = true;
                whereLambda = scalarMethodCall.Arguments[1].XLambda();
            }

            if ((!isSingleWhere) || whereLambda == null)
                return false;
            if (whereLambda.Parameters.Count != 1 || whereLambda.Parameters[0].NodeType != ExpressionType.Parameter)
                return false;
            if (whereLambda.Body.NodeType != ExpressionType.Equal)
                return false;
            BinaryExpression equals = (BinaryExpression)whereLambda.Body;
            Expression left = equals.Left;
            Expression right = equals.Right;

            MemberExpression member;
            ConstantExpression consta;
            if (left.NodeType == ExpressionType.MemberAccess && right.NodeType == ExpressionType.Constant)
            {
                member = left.XMember();
                consta = right.XConstant();
            }
            else if (left.NodeType == ExpressionType.Constant && right.NodeType == ExpressionType.Parameter)
            {
                member = right.XMember();
                consta = left.XConstant();
            }
            else
            {
                return false;
            }

            if (member.Expression.NodeType != ExpressionType.Parameter)
                return false;

            //check that it's a primary key field
            ColumnAttribute colAttrib = AttribHelper.GetColumnAttribute(member.Member);
            if (colAttrib == null)
                return false;
            if (!colAttrib.IsPrimaryKey)
                return false;

            IdentityKey key = new IdentityKey(typeof(S), consta.Value);
            DataContext context = vars.Context;
            object cachedEntity = context.GetRegisteredEntityByKey(key);
            if (cachedEntity == null)
                return false;
            cachedRow = (S)cachedEntity;
            return true;
        }
    }
}
