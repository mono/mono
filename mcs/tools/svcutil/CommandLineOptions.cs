using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using Mono.Options;

[assembly: AssemblyTitle ("Mono service contract conversion tool")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyVersion ("0.1.0")]
[assembly: AssemblyCopyright ("Copyright (C) 2006 Novell, Inc.")]

namespace Mono.ServiceContractTool
{
	public enum OutputType
	{
		None,
		[EnumMember (Value = "code")]
		Code,
		[EnumMember (Value = "metadata")]
		Metadata,
		[EnumMember (Value = "xmlSerializer")]
		XmlSerializer,
	}

	public class CommandLineOptions
	{
		public CommandLineOptions ()
		{
			options = CreateOptions ();
		}

		public bool Help, Usage, Version;
		OptionSet options;

		public OptionSet CreateOptions ()
		{
			return new OptionSet {
				{ "a|async",
					"Generate async methods.",
					v => GenerateAsync = v != null },
				{ "config=",
					"Configuration file names to generate.",
					v => ConfigFiles.AddRange (v.Split (',')) },
				{ "i|internal",
					"Generate types as internal.",
					v => GenerateTypesAsInternal = v != null },
				{ "l|language=",
					"Specify target code {LANGUAGE}. Default is 'csharp'.",
					v => Language = v },
				{ "monotouch",
					"Generate MonoTouch client. (This option may vanish)",
					v => GenerateMonoTouchProxy = v != null },
				{ "moonlight",
					"Generate moonlight client. (This option may vanish)",
					v => GenerateMoonlightProxy = v != null },
				{ "n|namespace=",
					"Code namespace name to generate.",
					v => Namespace = v },
				{ "noConfig",
					"Do not generate config file.",
					v => NoConfig = v != null },
				{ "noLogo",
					"Do not show tool logo.",
					v => NoLogo = v != null },
				{ "o|out=",
					"Output code filename.",
					v => OutputFilename = v },
				{ "r|reference=",
					"Referenced assembly files.",
					v => ReferencedAssemblies.AddRange (v.Split (',')) },
				{ "tcv|targetClientVersion:",
					"Indicate target client version. Valid values:\n" +
					"  Version35",
					v => {
						if (v == null)
							return;
						switch (v.ToLowerInvariant ()) {
							case "version35":
								TargetClientVersion35 = true;
								break;
						}
					} },
				{ "tm|typedMessage",
					"Generate typed messages.",
					v => GenerateTypedMessages = v != null },
				{ "usage",
					"Show usage syntax and exit.",
					v => Usage = v != null },
				{ "V|version",
					"Display version and licensing information.",
					v=> Version = v != null },
				{ "h|?|help",
					"Show this help list.",
					v => Help = v != null },
			};
		}

		public void ProcessArgs (string[] args)
		{
			RemainingArguments = options.Parse (args);
		}

		public void DoHelp ()
		{
			ShowBanner ();
			Console.WriteLine ();
			DoUsage ();
			Console.WriteLine ("Options:");
			options.WriteOptionDescriptions (Console.Out);
			Console.WriteLine ();
			Console.WriteLine ("metadataPath : ws-mex file path.");
			Console.WriteLine ("metadataUrl: URL to ws-mex");
			Console.WriteLine ("assemblyPath: path to an assembly");
		}

		public void DoUsage ()
		{
			Console.WriteLine ("Usage: svcutil [options] [metadataPath* | metadataUrl* | assemblyPath*]");
		}

		public void DoVersion ()
		{
			ShowBanner ();
		}

		public void ShowBanner ()
		{
			Console.WriteLine ("Mono service contract conversion tool  {0} - Copyright (C) 2006 Novell, Inc.",
					Assembly.GetExecutingAssembly ().GetName ().Version);
		}

		public List<string> RemainingArguments;

		//[Option ("Target directory to create files", 'd', "directory")]
		public string TargetDirectory;

		public string OutputFilename;

		//[Option ("Target output type", 't', "target")]
		public OutputType OutputType = OutputType.Code;

		//[Option ("Validate all service endpoints", 'v', "validate")]
		public bool Validate;

		public List<string> ConfigFiles = new List<string> ();

		// FIXME: support it
		public bool ChannelInterface;

		// FIXME: support it
		public bool GenerateProxy;

		public bool GenerateAsync;

		public bool GenerateTypedMessages;

		public bool TargetClientVersion35;

		bool generate_moonlight_proxy, generate_monotouch_proxy;

		public bool GenerateMoonlightProxy {
			get { return generate_moonlight_proxy; }
			set {
				if (!value)
					return;
				generate_moonlight_proxy = true;
				GenerateAsync = true;
			}
		}

		public bool GenerateMonoTouchProxy {
			// this is a hack. It does not differentiate from GenerateMoonlightProxy on getter.
			get { return generate_monotouch_proxy; }
			set {
				if (!value)
					return;
				GenerateMoonlightProxy = true;
				generate_monotouch_proxy = true;
			}
		}

		public bool GenerateTypesAsInternal;

		public bool NoConfig;

		public string Language = "csharp";

		public string Namespace = String.Empty;

		public List<string> ReferencedAssemblies = new List<string> ();

		public bool NoLogo;
	}
}

