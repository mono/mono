#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DbLinq.Util;

#if MONO_STRICT
    namespace System.Data.Linq
#else
    namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// Allows to specify 
    /// </summary>
    public sealed class DataLoadOptions
    {
        /// <summary>
        /// There are the associations to load with a type
        /// </summary>
        private readonly IList<MemberInfo> eagerLoading = new List<MemberInfo>();

        /// <summary>
        /// Criteria to restrict associations
        /// </summary>
        private readonly IDictionary<MemberInfo, LambdaExpression> criteria = new Dictionary<MemberInfo, LambdaExpression>();

        /// <summary>
        /// Filters objects retrieved for a particular relationship. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        public void AssociateWith<T>(Expression<Func<T, object>> expression)
        {
            AssociateWith((LambdaExpression)expression);
        }

        /// <summary>
        /// Filters the objects retrieved for a particular relationship.
        /// </summary>
        /// <param name="expression"></param>
        public void AssociateWith(LambdaExpression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the restrictive criteria related to an association
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        internal LambdaExpression GetAssociationCriteria(MemberInfo memberInfo)
        {
            LambdaExpression associationCriteria;
            criteria.TryGetValue(memberInfo, out associationCriteria);
            return associationCriteria;
        }

        /// <summary>
        /// Specifies which sub-objects to retrieve when a query is submitted for an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        public void LoadWith<T>(Expression<Func<T, Object>> expression)
        {
            LoadWith((LambdaExpression)expression);
        }

        /// <summary>
        /// Retrieves specified data related to the main target by using a lambda expression.
        /// </summary>
        /// <param name="expression"></param>
        public void LoadWith(LambdaExpression expression)
        {
            // TODO: ensure we have an EntitySet<>
            var memberInfo = ReflectionUtility.GetMemberInfo(expression);
            if (memberInfo == null)
                throw new InvalidOperationException("The argument expression must be a property access or a field access where the target object is the parameter");
            if (!eagerLoading.Contains(memberInfo))
            {
                VerifyMemberAccessCycles(memberInfo);
                eagerLoading.Add(memberInfo);
            }
        }

        private void VerifyMemberAccessCycles(MemberInfo member)
        {
            var mt = GetMemberEntityType (member);
            var d = member.DeclaringType;
            foreach (var m in eagerLoading)
            {
                if (m.DeclaringType == mt && GetMemberEntityType (m) == d)
                    throw new InvalidOperationException("Illegal cycles are detected in the argument expression among other eager-loading expressions");
            }
        }

        private Type GetMemberEntityType(MemberInfo member)
        {
            var mt = member.GetMemberType();
            if (mt.IsGenericType)
            {
                if (mt.GetGenericTypeDefinition() == typeof(System.Data.Linq.EntitySet<>))
                    mt = mt.GetGenericArguments()[0];
                else if (mt.GetGenericTypeDefinition() == typeof(System.Data.Linq.EntityRef<>))
                    mt = mt.GetGenericArguments()[0];
            }
            return mt;
        }

        /// <summary>
        /// Tells if we do eager or lazy loading
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns>True on eager (immediate) logging</returns>
        internal bool IsImmediate(MemberInfo memberInfo)
        {
            return eagerLoading.Contains(memberInfo);
        }
    }
}
