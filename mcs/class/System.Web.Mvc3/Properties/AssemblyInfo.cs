using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Web;

[assembly: AssemblyTitle("System.Web.Mvc.dll")]
[assembly: AssemblyDescription("System.Web.Mvc.dll")]
[assembly: ComVisible(false)]
[assembly: Guid("4b5f4208-c6b0-4c37-9a41-63325ffa52ad")]
#if !CODE_COVERAGE
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
#endif
[assembly: CLSCompliant(true)]
#if !MONO
[assembly: InternalsVisibleTo("System.Web.Mvc.Test")]
#endif

[assembly: PreApplicationStartMethod(typeof(System.Web.Mvc.PreApplicationStartCode), "Start")]

[assembly: TypeForwardedTo(typeof(System.Web.Mvc.TagBuilder))]
[assembly: TypeForwardedTo(typeof(System.Web.Mvc.TagRenderMode))]
[assembly: TypeForwardedTo(typeof(System.Web.Mvc.HttpAntiForgeryException))]

