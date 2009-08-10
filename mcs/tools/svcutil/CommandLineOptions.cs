using System;
using System.Reflection;
using System.Runtime.Serialization;
using Mono.GetOptions;

[assembly: AssemblyTitle ("Mono service contract conversion tool")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyVersion ("0.1.0")]
[assembly: AssemblyCopyright ("Copyright (C) 2006 Novell, Inc.")]
[assembly: Mono.UsageComplement("[metadataPath* | metadataUrl* | assemblyPath*]")]
[assembly: Mono.AdditionalInfo (@"
metadataPath : ws-mex file path.
metadataUrl: URL to ws-mex
assemblyPath: path to an assembly")]

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

	public class CommandLineOptions : Options
	{
		public CommandLineOptions ()
		{
		}

		//[Option ("Target directory to create files", 'd', "directory")]
		public string TargetDirectory;

		[Option ("Output code filename", 'o', "out")]
		public string OutputFilename;

		//[Option ("Target output type", 't', "target")]
		public OutputType OutputType = OutputType.Code;

		//[Option ("Validate all service endpoints", 'v', "validate")]
		public bool Validate;

		[Option ("Configuration file names to generate", "config", MaxOccurs = -1)]
		public string [] ConfigFiles;

		// FIXME: support it
		public bool ChannelInterface;

		// FIXME: support it
		public bool GenerateProxy;

		[Option ("Generate async methods.", 'a', "async")]
		public bool GenerateAsync;

		[Option ("Generate typed messages.", "typedMessage", "tm")]
		public bool GenerateTypedMessages;

		bool generate_moonlight_proxy;

		[Option ("Generate moonlight client. (This option may vanish.)", "moonlight")]
		public bool GenerateMoonlightProxy {
			get { return generate_moonlight_proxy; }
			set {
				if (!value)
					return;
				generate_moonlight_proxy = true;
				GenerateAsync = true;
			}
		}

		[Option ("Generate types as internal.", 'i', "internal")]
		public bool GenerateTypesAsInternal;

		[Option ("Do not generate config file.", "noConfig")]
		public bool NoConfig;

		[Option ("Specify target code language. 'csharp' By default.", 'l', "language")]
		public string Language = "csharp";

		[Option ("Code namespace name to generate.", 'n', "namespace")]
		public string Namespace = String.Empty;

		[Option ("Referenced assembly files", 'r', "reference", MaxOccurs = -1)]
		public string [] ReferencedAssemblies;

		[Option ("Do not show tool logo", "noLogo")]
		public bool NoLogo;
	}
}

