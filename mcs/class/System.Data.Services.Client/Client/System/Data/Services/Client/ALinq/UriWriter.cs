//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    #endregion Namespaces.

    internal class UriWriter : DataServiceExpressionVisitor
    {
        private readonly DataServiceContext context;

        private readonly StringBuilder uriBuilder;
        
        private Version uriVersion;

        private ResourceSetExpression leafResourceSet;
        
        private UriWriter(DataServiceContext context)
        {
            Debug.Assert(context != null, "context != null");
            this.context = context;
            this.uriBuilder = new StringBuilder();
            this.uriVersion = Util.DataServiceVersion1;
        }

        internal static void Translate(DataServiceContext context, bool addTrailingParens, Expression e, out Uri uri, out Version version)
        {
            var writer = new UriWriter(context);
            writer.leafResourceSet = addTrailingParens ? (e as ResourceSetExpression) : null;
            writer.Visit(e);
            uri = Util.CreateUri(context.BaseUriWithSlash, Util.CreateUri(writer.uriBuilder.ToString(), UriKind.Relative));
            version = writer.uriVersion;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            throw Error.MethodNotSupported(m);
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            throw new NotSupportedException(Strings.ALinq_UnaryNotSupported(u.NodeType.ToString()));
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            throw new NotSupportedException(Strings.ALinq_BinaryNotSupported(b.NodeType.ToString()));
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            throw new NotSupportedException(Strings.ALinq_ConstantNotSupported(c.Value));
        }

        internal override Expression VisitTypeIs(TypeBinaryExpression b)
        {
            throw new NotSupportedException(Strings.ALinq_TypeBinaryNotSupported);
        }

        internal override Expression VisitConditional(ConditionalExpression c)
        {
            throw new NotSupportedException(Strings.ALinq_ConditionalNotSupported);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            throw new NotSupportedException(Strings.ALinq_ParameterNotSupported);
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            throw new NotSupportedException(Strings.ALinq_MemberAccessNotSupported(m.Member.Name));
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            throw new NotSupportedException(Strings.ALinq_LambdaNotSupported);
        }

        internal override NewExpression VisitNew(NewExpression nex)
        {
            throw new NotSupportedException(Strings.ALinq_NewNotSupported);
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            throw new NotSupportedException(Strings.ALinq_MemberInitNotSupported);
        }

        internal override Expression VisitListInit(ListInitExpression init)
        {
            throw new NotSupportedException(Strings.ALinq_ListInitNotSupported);
        }

        internal override Expression VisitNewArray(NewArrayExpression na)
        {
            throw new NotSupportedException(Strings.ALinq_NewArrayNotSupported);
        }

        internal override Expression VisitInvocation(InvocationExpression iv)
        {
            throw new NotSupportedException(Strings.ALinq_InvocationNotSupported);
        }

        internal override Expression VisitNavigationPropertySingletonExpression(NavigationPropertySingletonExpression npse)
        {
            this.Visit(npse.Source);
            this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(this.ExpressionToString(npse.MemberExpression));
            this.VisitQueryOptions(npse);
            return npse;
        }

        internal override Expression VisitResourceSetExpression(ResourceSetExpression rse)
        {
            if ((ResourceExpressionType)rse.NodeType == ResourceExpressionType.ResourceNavigationProperty)
            {
                this.Visit(rse.Source);
                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(this.ExpressionToString(rse.MemberExpression));
            }
            else
            {
                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append((string)((ConstantExpression)rse.MemberExpression).Value);
            }

            if (rse.KeyPredicate != null)
            {
                this.uriBuilder.Append(UriHelper.LEFTPAREN);
                if (rse.KeyPredicate.Count == 1)
                {
                    this.uriBuilder.Append(this.ExpressionToString(rse.KeyPredicate.Values.First()));
                }
                else
                {
                    bool addComma = false;
                    foreach (var kvp in rse.KeyPredicate)
                    {
                        if (addComma)
                        {
                            this.uriBuilder.Append(UriHelper.COMMA);
                        }

                        this.uriBuilder.Append(kvp.Key.Name);
                        this.uriBuilder.Append(UriHelper.EQUALSSIGN);
                        this.uriBuilder.Append(this.ExpressionToString(kvp.Value));
                        addComma = true;
                    }
                }

                this.uriBuilder.Append(UriHelper.RIGHTPAREN);
            }
            else if (rse == this.leafResourceSet)
            {
                this.uriBuilder.Append(UriHelper.LEFTPAREN);
                this.uriBuilder.Append(UriHelper.RIGHTPAREN);
            }

            if (rse.CountOption == CountOption.ValueOnly)
            {
                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(UriHelper.DOLLARSIGN).Append(UriHelper.COUNT);
                this.EnsureMinimumVersion(2, 0);
            }

            this.VisitQueryOptions(rse);
            return rse;
        }

        internal void VisitQueryOptions(ResourceExpression re)
        {
            bool needAmpersand = false;

            if (re.HasQueryOptions)
            {
                this.uriBuilder.Append(UriHelper.QUESTIONMARK);

                ResourceSetExpression rse = re as ResourceSetExpression;
                if (rse != null)
                {
                    IEnumerator options = rse.SequenceQueryOptions.GetEnumerator();
                    while (options.MoveNext())
                    {
                        if (needAmpersand)
                        {
                            this.uriBuilder.Append(UriHelper.AMPERSAND);
                        }

                        Expression e = ((Expression)options.Current);
                        ResourceExpressionType et = (ResourceExpressionType)e.NodeType;
                        switch (et)
                        {
                            case ResourceExpressionType.SkipQueryOption:
                                this.VisitQueryOptionExpression((SkipQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.TakeQueryOption:
                                this.VisitQueryOptionExpression((TakeQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.OrderByQueryOption:
                                this.VisitQueryOptionExpression((OrderByQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.FilterQueryOption:
                                this.VisitQueryOptionExpression((FilterQueryOptionExpression)e);
                                break;
                            default:
                                Debug.Assert(false, "Unexpected expression type " + (int)et);
                                break;
                        }

                        needAmpersand = true;
                    }
                }

                if (re.ExpandPaths.Count > 0)
                {
                    if (needAmpersand)
                    {
                        this.uriBuilder.Append(UriHelper.AMPERSAND);
                    }

                    this.VisitExpandOptions(re.ExpandPaths);
                    needAmpersand = true;
                }

                if (re.Projection != null && re.Projection.Paths.Count > 0)
                {
                    if (needAmpersand)
                    {
                        this.uriBuilder.Append(UriHelper.AMPERSAND);
                    }

                    this.VisitProjectionPaths(re.Projection.Paths);
                    needAmpersand = true;
                }

                if (re.CountOption == CountOption.InlineAll)
                {
                    if (needAmpersand)
                    {
                        this.uriBuilder.Append(UriHelper.AMPERSAND);
                    }

                    this.VisitCountOptions();
                    needAmpersand = true;
                }

                if (re.CustomQueryOptions.Count > 0)
                {
                    if (needAmpersand)
                    {
                        this.uriBuilder.Append(UriHelper.AMPERSAND);
                    }

                    this.VisitCustomQueryOptions(re.CustomQueryOptions);
                    needAmpersand = true;
                }
            }
        }

        internal void VisitQueryOptionExpression(SkipQueryOptionExpression sqoe)
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONSKIP);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);
            this.uriBuilder.Append(this.ExpressionToString(sqoe.SkipAmount));
        }

        internal void VisitQueryOptionExpression(TakeQueryOptionExpression tqoe)
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONTOP);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);
            this.uriBuilder.Append(this.ExpressionToString(tqoe.TakeAmount));
        }

        internal void VisitQueryOptionExpression(FilterQueryOptionExpression fqoe)
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONFILTER);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);
            this.uriBuilder.Append(this.ExpressionToString(fqoe.Predicate));
        }

        internal void VisitQueryOptionExpression(OrderByQueryOptionExpression oboe)
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONORDERBY);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);

            int ii = 0;
            while (true)
            {
                var selector = oboe.Selectors[ii];

                this.uriBuilder.Append(this.ExpressionToString(selector.Expression));
                if (selector.Descending)
                {
                    this.uriBuilder.Append(UriHelper.SPACE);
                    this.uriBuilder.Append(UriHelper.OPTIONDESC);
                }

                if (++ii == oboe.Selectors.Count)
                {
                    break;
                }

                this.uriBuilder.Append(UriHelper.COMMA);
            }
        }

        internal void VisitExpandOptions(List<string> paths)
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONEXPAND);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);

            int ii = 0;
            while (true)
            {
                this.uriBuilder.Append(paths[ii]);

                if (++ii == paths.Count)
                {
                    break;
                }

                this.uriBuilder.Append(UriHelper.COMMA);
            }
        }

        internal void VisitProjectionPaths(List<string> paths)
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONSELECT);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);

            int ii = 0;
            while (true)
            {
                string path = paths[ii];

                this.uriBuilder.Append(path);

                if (++ii == paths.Count)
                {
                    break;
                }

                this.uriBuilder.Append(UriHelper.COMMA);
            }

            this.EnsureMinimumVersion(2, 0);
        }

        internal void VisitCountOptions()
        {
            this.uriBuilder.Append(UriHelper.DOLLARSIGN);
            this.uriBuilder.Append(UriHelper.OPTIONCOUNT);
            this.uriBuilder.Append(UriHelper.EQUALSSIGN);
            this.uriBuilder.Append(UriHelper.COUNTALL);
            this.EnsureMinimumVersion(2, 0);
        }

        internal void VisitCustomQueryOptions(Dictionary<ConstantExpression, ConstantExpression> options)
        {
            List<ConstantExpression> keys = options.Keys.ToList();
            List<ConstantExpression> values = options.Values.ToList();

            int ii = 0;
            while (true)
            {
                this.uriBuilder.Append(keys[ii].Value);
                this.uriBuilder.Append(UriHelper.EQUALSSIGN);
                this.uriBuilder.Append(values[ii].Value);

                if (keys[ii].Value.ToString().Equals(UriHelper.DOLLARSIGN + UriHelper.OPTIONCOUNT, StringComparison.OrdinalIgnoreCase))
                {
                    this.EnsureMinimumVersion(2, 0);
                }

                if (++ii == keys.Count)
                {
                    break;
                }

                this.uriBuilder.Append(UriHelper.AMPERSAND);
            }
        }

        private string ExpressionToString(Expression expression)
        {
            return ExpressionWriter.ExpressionToString(this.context, expression);
        }

        private void EnsureMinimumVersion(int major, int minor)
        {
            if (major > this.uriVersion.Major ||
                (major == this.uriVersion.Major && minor > this.uriVersion.Minor))
            {
                this.uriVersion = new Version(major, minor);
            }
        }
    }
}
