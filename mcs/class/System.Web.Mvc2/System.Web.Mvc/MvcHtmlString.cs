/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Web;

    // In ASP.NET 4, a new syntax <%: %> is being introduced in WebForms pages, where <%: expression %> is equivalent to
    // <%= HttpUtility.HtmlEncode(expression) %>. The intent of this is to reduce common causes of XSS vulnerabilities
    // in WebForms pages (WebForms views in the case of MVC). This involves the addition of an interface
    // System.Web.IHtmlString and a static method overload System.Web.HttpUtility::HtmlEncode(object).  The interface
    // definition is roughly:
    //   public interface IHtmlString {
    //     string ToHtmlString();
    //   }
    // And the HtmlEncode(object) logic is roughly:
    //   - If the input argument is an IHtmlString, return argument.ToHtmlString(),
    //   - Otherwise, return HtmlEncode(Convert.ToString(argument)).
    //
    // Unfortunately this has the effect that calling <%: Html.SomeHelper() %> in an MVC application running on .NET 4
    // will end up encoding output that is already HTML-safe. As a result, we're changing out HTML helpers to return
    // MvcHtmlString where appropriate. <%= Html.SomeHelper() %> will continue to work in both .NET 3.5 and .NET 4, but
    // changing the return types to MvcHtmlString has the added benefit that <%: Html.SomeHelper() %> will also work
    // properly in .NET 4 rather than resulting in a double-encoded output. MVC developers in .NET 4 will then be able
    // to use the <%: %> syntax almost everywhere instead of having to remember where to use <%= %> and where to use
    // <%: %>. This should help developers craft more secure web applications by default.
    //
    // To create an MvcHtmlString, use the static Create() method instead of calling the protected constructor.

    public class MvcHtmlString {

        private delegate MvcHtmlString MvcHtmlStringCreator(string value);
        private static readonly MvcHtmlStringCreator _creator = GetCreator();

        // imporant: this declaration must occur after the _creator declaration
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "MvcHtmlString is immutable")]
        public static readonly MvcHtmlString Empty = Create(String.Empty);

        private readonly string _value;

        // This constructor is only protected so that we can subclass it in a dynamic module. In practice,
        // nobody should ever call this constructor, and it is likely to be removed in a future version
        // of the framework. Use the static Create() method instead.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("The recommended alternative is the static MvcHtmlString.Create(String value) method.")]
        protected MvcHtmlString(string value) {
            _value = value ?? String.Empty;
        }

        public static MvcHtmlString Create(string value) {
            return _creator(value);
        }

        // in .NET 4, we dynamically create a type that subclasses MvcHtmlString and implements IHtmlString
        private static MvcHtmlStringCreator GetCreator() {
            Type iHtmlStringType = typeof(HttpContext).Assembly.GetType("System.Web.IHtmlString");
            if (iHtmlStringType != null) {
                // first, create the dynamic type
                Type dynamicType = DynamicTypeGenerator.GenerateType("DynamicMvcHtmlString", typeof(MvcHtmlString), new Type[] { iHtmlStringType });

                // then, create the delegate to instantiate the dynamic type
                ParameterExpression valueParamExpr = Expression.Parameter(typeof(string), "value");
                NewExpression newObjExpr = Expression.New(dynamicType.GetConstructor(new Type[] { typeof(string) }), valueParamExpr);
                Expression<MvcHtmlStringCreator> lambdaExpr = Expression.Lambda<MvcHtmlStringCreator>(newObjExpr, valueParamExpr);
                return lambdaExpr.Compile();
            }
            else {
                // disabling 0618 allows us to call the MvcHtmlString() constructor
#pragma warning disable 0618
                return value => new MvcHtmlString(value);
#pragma warning restore 0618
            }
        }

        public static bool IsNullOrEmpty(MvcHtmlString value) {
            return (value == null || value._value.Length == 0);
        }

        // IHtmlString.ToHtmlString()
        public string ToHtmlString() {
            return _value;
        }

        public override string ToString() {
            return _value;
        }

    }
}
