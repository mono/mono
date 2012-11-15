
// useful grep
// grep -h "#if" /svn/mono/external/rx/Rx.NET/System.Reactive.*/*.cs /svn/mono/external/rx/Rx.NET/System.Reactive.*/*/*.cs /svn/mono/external/rx/Rx.NET/System.Reactive.*/*/*/*.cs | sort | uniq

using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

var asses = new string [] {
	"System.Reactive.Interfaces",
	"System.Reactive.Core",
	"System.Reactive.PlatformServices",
	"System.Reactive.Linq",
	"System.Reactive.Debugger", // maybe needed for testing assembly.
	"System.Reactive.Experimental", // needed for testing assembly.
	"System.Reactive.Providers",
	"System.Reactive.Runtime.Remoting",
	"System.Reactive.Windows.Forms",
	"System.Reactive.Windows.Threading",
	"Microsoft.Reactive.Testing",
	"Tests.System.Reactive",
	};

var blacklist = new string [] {
	// FIXME: this is the only source that we cannot build.
	//Test/../../../../external/rx/Rx.NET/Tests.System.Reactive/Tests/ObservableExTest.cs(1478,27): error CS0411: The type arguments for method `System.Reactive.Linq.ObservableEx.ManySelect<TSource,TResult>(this System.IObservable<TSource>, System.Func<System.IObservable<TSource>,TResult>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
	"ObservableExTest.cs",

	// WPF Dispatcher.Invoke() is not implemented.
	"DispatcherSchedulerTest.cs",
	// This is not limited to Dispatcher, but many of them are relevant to it, or Winforms (we filter it out by not defining HAS_WINFORMS)
	"ObservableConcurrencyTest.cs",
	};

foreach (var ass in asses) {

	var monoass = ass == "Microsoft.Reactive.Testing" ?
		"Mono.Reactive.Testing" : ass;
	var basePath = "../../external/rx/Rx.NET";
	var csproj = Path.Combine (basePath, ass, ass + ".csproj");
	var pathPrefix = ass == "Tests.System.Reactive" ? "../../" : "../";

	// tests are built under Mono.Reactive.Testing directory.
	var sources =
		monoass == "Tests.System.Reactive" ?
		Path.Combine ("Mono.Reactive.Testing", "Mono.Reactive.Testing_test.dll.sources") :
		Path.Combine (monoass, monoass + ".dll.sources");

	var doc = XDocument.Load (csproj);
	var rootNS = doc.XPathSelectElement ("//*[local-name()='RootNamespace']").Value;
	using (var tw = File.CreateText (sources)) {
		tw.WriteLine ("Assembly/MonoAssemblyInfo.cs");
		foreach (var path in doc.XPathSelectElements ("//*[local-name()='Compile']")
			.Select (el => el.Attribute ("Include").Value)
			.Select (s => s.Replace ("\\", "/")))
			if (!blacklist.Any (b => path.Contains (b)))
				tw.WriteLine (Path.Combine (pathPrefix, basePath, ass, path));
	}

	var monoAssInfo = Path.Combine (Path.GetDirectoryName (sources), "Assembly", "MonoAssemblyInfo.cs");
	using (var tw = File.CreateText (monoAssInfo))
		tw.WriteLine (@"
using System.Reflection;
[assembly: AssemblyDelaySign (true)]
[assembly:AssemblyKeyFile (""../reactive.pub"")]
");

	var argsPath = Path.Combine (Path.GetDirectoryName (sources), "more_build_args");
	using (var tw = File.CreateText (argsPath)) {
		tw.WriteLine ("-d:SIGNED");

		foreach (var path in doc.XPathSelectElements ("//*[local-name()='EmbeddedResource']")) {
			var res = path.Attribute ("Include").Value;
			var resx = Path.Combine (basePath, ass, res);
			var resFileName = res.Replace ("resx", "resources");
			var resxDest = Path.Combine (monoass, res);
			var resPath = Path.Combine (monoass, resFileName);
			if (File.Exists (resxDest))
				File.Delete (resxDest);
			File.Copy (resx, resxDest);
			//Process.Start ("resgen", String.Format ("{0} {1}", resx, resPath));
			tw.WriteLine ("-resource:{0},{1}.{2}", resFileName, rootNS, resFileName);
		}
	}
}

