
// useful grep
// grep -h "#if" /svn/mono/external/rx/Rx/NET/Source/System.Reactive.*/*.cs /svn/mono/external/rx/Rx/NET/Source/System.Reactive.*/*/*.cs /svn/mono/external/rx/Rx/NET/Source/System.Reactive.*/*/*/*.cs | sort | uniq

using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

var template_android = File.ReadAllText ("project_template_android.txt");
var template_ios = File.ReadAllText ("project_template_ios.txt");

var guids_android = new string [] { "4fa878dc-6e88-43c4-b37b-8c1151cec56f", "fef62c23-10cd-4def-a5ae-54a8b309e970", "d263c126-9d3c-4299-a0c1-f25c703d07c9", "ec704077-ea31-4852-ad24-6732244174c0", "9360e845-d79d-4288-9100-63a80fad2bf0", "00dc3654-e373-4e3f-80fe-109f795adf1f", "e662282b-4341-4f86-aaaa-a942335b47fb", "a153a379-670c-42c2-9018-fc0d933a4f7f", "b29d45a6-0b8c-49c5-82a2-457e4d3cbc33", "3a3b5e76-029f-46b0-9ccf-fefe06eb61e7", "cb2ab716-bfcb-43bc-a03b-a3bda427746c", "73c5260f-8972-4e7c-822b-1a3a0358fa0b" };
var guids_ios = new string [] { "6f2675f5-fcc7-4a28-9dc3-657b4613dcc5", "a67f34b5-75c1-4319-a93e-93df87e728a4", "79a43ceb-1a18-49ea-aac4-b72b9c90bf5a", "0a977063-0796-4cd4-84b8-aedb2d648b26", "b41cb61a-dca0-4539-8f99-7b3499e18e6d", "24f995bd-7075-489c-b7a5-7fde08c304b6", "894021ec-14fb-430a-8572-bea9569ae435", "92857c8e-0e83-4d02-a831-8af3fed43336", "912e14a2-7bdf-4600-8d55-e8c4f33a2063", "0f6c2933-8d0c-41e6-9f77-e8714ab8c4ab", "47d85a91-e8e2-4088-bf5a-68a161754d48", "45377009-1425-47fc-985e-05f98022f9e3" };

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

var excluded_android_asses = new string [] {
	"System.Reactive.Windows.Forms",
	"System.Reactive.Windows.Threading",
	};
var excluded_ios_asses = new string [] {
	"System.Reactive.Providers",
	"System.Reactive.Windows.Forms",
	"System.Reactive.Windows.Threading",
	}

var blacklist = new string [] {
	// WPF Dispatcher.Invoke() is not implemented.
	"DispatcherSchedulerTest.cs",
	// This is not limited to Dispatcher, but many of them are relevant to it, or Winforms (we filter it out by not defining HAS_WINFORMS)
	"ObservableConcurrencyTest.cs",
	};

var dstAndroid = "../../external/rx/Rx/NET/Source/Rx_Xamarin/android";
var dstIOS = "../../external/rx/Rx/NET/Source/Rx_Xamarin/iOS";

int guid_idx = 0;
foreach (var ass in asses) {

	var monoass = ass == "Microsoft.Reactive.Testing" ?
		"Mono.Reactive.Testing" : ass;
	var basePath = "../../external/rx/Rx/NET/Source";
	var csproj = Path.Combine (basePath, ass, ass + ".csproj");
	var pathPrefix = ass == "Tests.System.Reactive" ? "../" : "";

	var android_dir = Path.GetFullPath (Path.Combine (csproj, "..", "..", "Rx_Xamarin", "android", "rx", monoass));
	var ios_dir = Path.GetFullPath (Path.Combine (csproj, "..", "..", "Rx_Xamarin", "iOS", "rx", monoass));
	var android_proj = Path.Combine (android_dir, "android_" + monoass + ".csproj");
	var ios_proj = Path.Combine (ios_dir, "ios_" + monoass + ".csproj");
	if (!Directory.Exists (android_dir))
		Directory.CreateDirectory (android_dir);
	if (!Directory.Exists (ios_dir))
		Directory.CreateDirectory (ios_dir);

	// tests are built under Mono.Reactive.Testing directory.
	
	var sources =
		monoass == "Tests.System.Reactive" ?
		Path.Combine ("Mono.Reactive.Testing", "Mono.Reactive.Testing_test.dll.sources") :
		Path.Combine (monoass, monoass + ".dll.sources");

	var assdir = Path.Combine (monoass, "Assembly");
	var assinfo = Path.Combine (monoass, "Assembly", "AssemblyInfo.cs");

	var projectRefs = "";

	if (monoass != "Tests.System.Reactive") {
		if (!Directory.Exists (assdir))
			Directory.CreateDirectory (assdir);
		using (var tw = File.CreateText (assinfo)) {
			tw.WriteLine ("[assembly:System.Reflection.AssemblyVersion (\"2.1.30214.0\")]");
			tw.WriteLine ("[assembly:System.Reflection.AssemblyFileVersion (\"2.1.30214.0\")]");
		}
	}

	var sourcesXml = "";
	var projectRefsXml = "";
	var resourcesXml = "";

	var signing_xml_template = "<SignAssembly>True</SignAssembly>\n    <DelaySign>True</DelaySign>\n    <AssemblyOriginatorKeyFile>../../../reactive.pub</AssemblyOriginatorKeyFile>\n";
	var signingXml = ass.StartsWith ("System") ? signing_xml_template : "";
	

	var doc = XDocument.Load (csproj);
	var rootNS = doc.XPathSelectElement ("//*[local-name()='RootNamespace']").Value;
	var guid = doc.XPathSelectElement ("//*[local-name()='ProjectGuid']").Value;

	foreach (var e in doc.XPathSelectElements ("//*[local-name()='ProjectReference']"))
		projectRefsXml += e;

	Console.WriteLine ("Writing " + sources + " ...");
	using (var tw = File.CreateText (sources)) {
		if (monoass != "Tests.System.Reactive")
			tw.WriteLine ("Assembly/AssemblyInfo.cs");
		foreach (var path in doc.XPathSelectElements ("//*[local-name()='Compile']")
			.Select (el => el.Attribute ("Include").Value)
			.Select (s => s.Replace ("\\", "/"))) {
			if (!blacklist.Any (b => path.Contains (b))) {
				var p = Path.Combine ("..", basePath, ass, path);
				tw.WriteLine (Path.Combine (pathPrefix, p));
				sourcesXml += "    <Compile Include='..\\..\\..\\..\\..\\..\\" + p.Replace ('/', '\\') + "'>\n      <Link>" + path + "</Link>\n    </Compile>\n";
			}
		}
	}

	Console.WriteLine ("Writing more_build_args...");
	var argsPath = Path.Combine (Path.GetDirectoryName (sources), "more_build_args");
	using (var tw = File.CreateText (argsPath)) {
		if (ass.StartsWith ("System")) {
			tw.WriteLine ("-d:SIGNED");
			tw.WriteLine ("-delaysign");
			tw.WriteLine ("-keyfile:../reactive.pub");
		}

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
			var p = Path.Combine ("..", basePath, ass, res);
			resourcesXml += "    <EmbeddedResource Include='..\\..\\..\\..\\..\\..\\" + p + "'>\n      <Link>" + res + "</Link>\n    </EmbeddedResource>\n";
		}
	}
	foreach (var f in new string [] { android_proj, ios_proj}) {
		string prj_guid;
		string template, prj_prefix, nunitProjRef, nunitRef;
		var androidNUnit = "<ProjectReference Include=\"..\\..\\Andr.Unit\\Android.NUnitLite\\Android.NUnitLite.csproj\"><Project>{6A005891-A3D6-4398-A729-F645397D573A}</Project><Name>Android.NUnitLite</Name></ProjectReference>";
		if (f == android_proj) {
			prj_guid = guids_android [guid_idx];
			template = template_android;
			prj_prefix ="android_";
			nunitProjRef = ass.Contains ("Test") ? androidNUnit : "";
			nunitRef = "";
		} else {
			prj_guid = guids_ios [guid_idx];
			template = template_ios;
			prj_prefix ="ios_";
			nunitProjRef = "";
			nunitRef = ass.Contains ("Test") ? "<Reference Include='MonoTouch.NUnitLite' />" : "";
		}
		using (var tw = File.CreateText (f)) {
			tw.Write (template
				.Replace ("PROJECT_GUID_GOES_HERE", '{' + prj_guid + '}')
				.Replace ("ASSEMBLY_NAME_GOES_HERE", monoass)
				.Replace ("OPTIONAL_ANDROID_NUNITLITE", nunitProjRef)
				.Replace ("OPTIONAL_MONOTOUCH_NUNITLITE", nunitRef)
				.Replace ("PROJECT_REFERENCES_GO_HERE",
					projectRefsXml
						.Replace ("Microsoft.Reactive.Testing", "Mono.Reactive.Testing")
						.Replace ("System", prj_prefix + "System")
						.Replace ("Mono", prj_prefix + "Mono")
						.Replace ("Include=\"..\\" + prj_prefix, "Include=\"..\\"))
				.Replace ("RESOURCES_GO_HERE", sourcesXml.Replace ('\\', f == ios_proj ? '/' : '\\')) // whoa, BACKSLASH doesn't work only on android on MD/mac...!
				.Replace ("SOURCES_GO_HERE", resourcesXml.Replace ('\\', f == ios_proj ? '/' : '\\')) // whoa, BACKSLASH doesn't work only on android on MD/mac...!
				.Replace ("SIGNING_SPEC_GOES_HERE", signingXml));
		}
	}
	guid_idx++;
}

