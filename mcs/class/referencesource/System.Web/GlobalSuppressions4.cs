using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", Scope = "resource", Target = "System.Web.resources", MessageId = "aspnet", Justification = @"By design")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", Scope = "resource", Target = "System.Web.resources", MessageId = "jquery", Justification = @"By design")]
[module: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", Scope = "resource", Target = "System.Web.resources", MessageId = "whitespace", Justification = @"whitespace is appropriate to indicate space\tab etc.")]

[module: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "System.Web.UI.ImageClickEventArgs.#X", Justification = @"Breaking change")]
[module: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "System.Web.UI.ImageClickEventArgs.#XRaw", Justification = @"Though not a breaking change, better to keep it consistent with X.")]
[module: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "System.Web.UI.ImageClickEventArgs.#Y", Justification = @"Breaking change")]
[module: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "System.Web.UI.ImageClickEventArgs.#YRaw", Justification = @"Though not a breaking change, better to keep it consistent with Y.")]

[module: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Scope = "member", Target = "System.Web.UI.Control.#BeginRenderTracing(System.IO.TextWriter,System.Object)", MessageId = "object", Justification = @"By design - okay.")]
[module: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Scope = "member", Target = "System.Web.UI.Control.#EndRenderTracing(System.IO.TextWriter,System.Object)", MessageId = "object", Justification = @"By design - okay.")]
[module: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Scope = "member", Target = "System.Web.UI.Control.#SetTraceData(System.Object,System.Object,System.Object)", MessageId = "object", Justification = @"By design - okay.")]
[module: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Scope = "member", Target = "System.Web.UI.RenderTraceListener.#BeginRendering(System.IO.TextWriter,System.Object)", MessageId = "object", Justification = @"By design - okay.")]
[module: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Scope = "member", Target = "System.Web.UI.RenderTraceListener.#EndRendering(System.IO.TextWriter,System.Object)", MessageId = "object", Justification = @"By design - okay.")]
[module: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Scope = "member", Target = "System.Web.UI.RenderTraceListener.#SetTraceData(System.Object,System.Object,System.Object)", MessageId = "object", Justification = @"By design - okay.")]

#region CA2116 False Positives
// CA2116 is firing false positives due to DevDiv #342582 (http://vstfdevdiv:8080/DevDiv2/web/wi.aspx?id=342582).
// We should remove these suppressions when that bug is fixed.
[module: SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Scope = "member", Target = "System.Web.Configuration.RegexWorker.#.cctor()", Justification = "False positive.")]
[module: SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Scope = "member", Target = "System.Web.Handlers.AssemblyResourceLoader.#.cctor()", Justification = "False positive.")]
[module: SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Scope = "member", Target = "System.Web.UI.BaseParser.#.cctor()", Justification = "False positive.")]
[module: SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Scope = "member", Target = "System.Web.UI.ControlBuilder.#.cctor()", Justification = "False positive.")]
[module: SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Scope = "member", Target = "System.Web.UI.Page.#ExportWebPart(System.String)", Justification = "False positive.")]
[module: SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Scope = "member", Target = "System.Web.UI.SimpleWebHandlerParser.#.cctor()", Justification = "False positive.")]
#endregion
