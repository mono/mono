namespace System.Web.Mvc {
    using System;
    using System.Collections.Specialized;
    using System.Web;

    // Used for mocking the UnvalidatedRequestValues type in System.Web.WebPages

    internal interface IUnvalidatedRequestValues {
        NameValueCollection Form { get; }
        NameValueCollection QueryString { get; }
        string this[string key] { get; }
    }
}
