#!/usr/bin/env ruby

require 'ftools'

$gac = 'C:/WINDOWS/assembly/GAC_MSIL'
$fx =  'C:/WINDOWS/Microsoft.NET/Framework'

$fx1_1 = File.join $fx, "v1.1.4322"
$fx2_0 = File.join $fx, "v2.0.50727"
$fx3_0 = 'C:/Program Files/Reference Assemblies/Microsoft/Framework/v3.0'
$fx3_5 = 'C:/Program Files/Reference Assemblies/Microsoft/Framework/v3.5'
$fx4_0 = 'C:/Program Files/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.0'
$sl2_0 = 'C:/Program Files/Microsoft Silverlight/2.0.40115.0'
$sl2_0sdk = 'C:/Program Files/Microsoft SDKs/Silverlight/v2.0/Libraries/Client/'

$net_1_1 = [
	"mscorlib",
	"System",
	"System.Data",
	"System.Data.OracleClient",
	"System.DirectoryServices",
	"System.Drawing",
	"System.Runtime.Remoting",
	"System.Runtime.Serialization.Formatters.Soap",
	"System.Security",
	"System.ServiceProcess",
	"System.Web",
	"System.Web.Services",
	"System.Windows.Forms",
	"System.Xml",
	"cscompmgd",
	"Microsoft.VisualBasic",
	"",
	"System.Configuration.Install",
	"System.Design",
	"System.Drawing.Design",
	"System.EnterpriseServices",
	"System.Management",
	"System.Messaging"
]

$net_2_0 = [
	"mscorlib",
	"System",
	"System.Configuration",
	"System.Data",
	"System.Data.OracleClient",
	"System.DirectoryServices",
	"System.Drawing",
	"System.Runtime.Remoting",
	"System.Runtime.Serialization.Formatters.Soap",
	"System.Security",
	"System.ServiceProcess",
	"System.Transactions",
	"System.Web",
	"System.Web.Services",
	"System.Windows.Forms",
	"System.Xml",
	"cscompmgd",
	"Microsoft.VisualBasic",
	"",
	"Microsoft.Build.Engine",
	"Microsoft.Build.Framework",
	"Microsoft.Build.Tasks",
	"Microsoft.Build.Utilities",
	"",
	"System.Configuration.Install",
	"System.Design",
	"System.Drawing.Design",
	"System.EnterpriseServices",
	"System.Management",
	"System.Messaging",
]

$net_3_0 = [
	"PresentationCore",

	"PresentationFramework",
	"System.Speech",
	"WindowsBase",
	"",
	"System.IdentityModel",
	"System.IdentityModel.Selectors",
	"System.IO.Log",
	"System.Runtime.Serialization",
	"System.ServiceModel",
	"",
	"System.Workflow.Activities",
	"System.Workflow.ComponentModel",
	"System.Workflow.Runtime",
	"",
	"PresentationBuildTasks",
	"",
	"PresentationFramework.Aero",
	"PresentationFramework.Classic",
	"PresentationFramework.Luna",
	"PresentationFramework.Royale",
	"ReachFramework",
	"",
	"System.Printing",
]

$net_3_5 = [
	"mscorlib",
	"System",
	"System.AddIn",
	"System.AddIn.Contract",
	"System.Configuration",
	"System.Core",
	"System.Configuration.Install",
	"System.Data",
	"System.Data.Linq",
	"System.Data.OracleClient",
	"System.DirectoryServices",
	# "System.DirectoryServices.AccountManagement",
	# "System.DirectoryServices.Protocols",
	"System.Drawing",
	"System.Net",
	"System.Runtime.Remoting",
	"System.Security",
	"System.ServiceProcess",
	"System.Transactions",
	"System.Web",
	"System.Web.Extensions",
	"System.Web.Extensions.Design",
	"System.Web.Mobile",
	"System.Web.RegularExpressions",
	"System.Web.Services",
	"System.Windows.Forms",
	"System.Xml",
	"System.Xml.Linq",
	"",
	"System.Runtime.Serialization.Formatters.Soap",
	"cscompmgd",
	"Microsoft.VisualBasic",
	"",
	"Microsoft.Build.Engine",
	"Microsoft.Build.Framework",
	"Microsoft.Build.Tasks",
	"Microsoft.Build.Utilities",
	"",
	"System.Configuration.Install",
	"System.Design",
	"System.Drawing.Design",
	"System.EnterpriseServices",
	"System.Management",
	"System.Management.Instrumentation",
	"System.Messaging",
]

$net_4_0 = [
	"mscorlib",

#	"Microsoft.Build.Conversion.v3.5",
	"Microsoft.Build.Conversion.v4.0",
	"Microsoft.Build",
	"Microsoft.Build.Engine",
	"Microsoft.Build.Framework",
#	"Microsoft.Build.Tasks.v3.5",
	"Microsoft.Build.Tasks.v4.0",
#	"Microsoft.Build.Utilities.v3.5",
	"Microsoft.Build.Utilities.v4.0",
	"Microsoft.CSharp",
	"Microsoft.JScript",
	"Microsoft.VisualBasic.Compatibility.Data",
	"Microsoft.VisualBasic.Compatibility",
	"Microsoft.VisualBasic",
#	"Microsoft.VisualC",
#	"Microsoft.VisualC.STLCLR",

	"PresentationBuildTasks",
	"PresentationCore",
	"PresentationFramework.Aero",
	"PresentationFramework.Classic",
	"PresentationFramework",
	"PresentationFramework.Luna",
	"PresentationFramework.Royale",

	"ReachFramework",
	"ReferenceAssemblyBuildTask",

	"System.Activities.Core.Design",
	"System.Activities.Design.Base",
	"System.Activities.Design",
	"System.Activities",
	"System.Activities.Extended",
	"System.AddIn.Contract",
	"System.AddIn",
	"System.Caching",
	"System.ComponentModel.Composition",
	"System.ComponentModel.DataAnnotations",
	"System.configuration",
	"System.Configuration.Install",
	"System.Core",
	"System.Data.DataSetExtensions",
	"System.Data",
	"System.Data.Entity.Design",
	"System.Data.Entity",
	"System.Data.Linq",
	"System.Data.OracleClient",
	"System.Data.Services.Client",
	"System.Data.Services.Design",
	"System.Data.Services",
	"System.Data.SqlXml",
	"System.Deployment",
	"System.Design",
	"System.DirectoryServices.AccountManagement",
	"System.DirectoryServices",
	"System.DirectoryServices.Protocols",
	"System",
	"System.Drawing.Design",
	"System.Drawing",
	"System.Dynamic",
	"System.EnterpriseServices",
	"System.EnterpriseServices.Thunk",
	"System.EnterpriseServices.Wrapper",
	"System.IdentityModel",
	"System.IdentityModel.Selectors",
	"System.IO.Log",
	"System.Management",
	"System.Management.Instrumentation",
	"System.Messaging",
	"System.Net",
	"System.Printing",
	"System.Runtime",
	"System.Runtime.Remoting",
	"System.Runtime.Serialization",
	"System.Runtime.Serialization.Formatters.Soap",
	"System.Security",
	"System.ServiceModel.Activities",
	"System.ServiceModel.Channels",
	"System.ServiceModel.Discovery",
	"System.ServiceModel",
	"System.ServiceModel.Routing",
	"System.ServiceModel.Web",
	"System.ServiceProcess",
	"System.Speech",
	"System.Transactions",
	"System.Web.Abstractions",
	"System.Web.DataVisualization.Design",
	"System.Web.DataVisualization",
	"System.Web",
	"System.Web.DynamicData.Design",
	"System.Web.DynamicData",
	"System.Web.Entity.Design",
	"System.Web.Entity",
	"System.Web.Extensions.Design",
	"System.Web.Extensions",
	"System.Web.Mobile",
	"System.Web.RegularExpressions",
	"System.Web.Routing",
	"System.Web.Services",
	"System.Windows.Forms.DataVisualization.Design",
	"System.Windows.Forms.DataVisualization",
	"System.Windows.Forms",
	"System.Windows.Presentation",
	"System.Workflow.Activities",
	"System.Workflow.ComponentModel",
	"System.Workflow.Runtime",
	"System.WorkflowServices",
	"System.Xaml",
	"System.Xaml.Hosting",
	"System.XML",
	"System.Xml.Linq",

	"WindowsBase",
	"XamlBuildTask"
]

$sl_2_0 = [
	"mscorlib",
	"System.Windows",
	"Microsoft.VisualBasic",
	"System",
	"System.Core",
	"System.Net",
	"System.Runtime.Serialization",
	"System.ServiceModel",
	"System.Windows.Browser",
	"System.Xml",
	"",
	"System.Xml.Linq",
	"System.Windows.Controls",
	"System.Windows.Controls.Data",
]

def locate(assembly, fxs = nil)
	if fxs
		fxs.each do |fx|
			file = File.join fx, assembly + ".dll"
			return file if File.file?(file)
		end
	end

	gac = File.join $gac, assembly, "**", "*.dll"

	glob = Dir.glob gac

	return glob.first if glob and glob.length > 0
end

def delete(glob)
	Dir.glob(glob).each do |file|
		File.delete file
	end
end

def clean(pattern, allow_create = false)
	if allow_create and not File.directory? "masterinfos"
		Dir.mkdir("masterinfos")
		return
	end

	delete(File.join("masterinfos", pattern))
end

def generate(location, assembly)
	out = File.join "masterinfos", assembly + ".xml"
	system("./mono-api-info.exe \"#{location}\" > #{out}")
end

def process(profile, assemblies, fxs = nil)
	clean("*", true)

	assemblies.each do |assembly|
		if assembly != nil and assembly.length > 0
			#puts assembly
			location = locate(assembly, fxs)
			if location
				generate(location, assembly)
			else
				puts "fail to locate " + assembly
			end
			#puts "   " + location if location
		end
	end

	clean("*.dll")

	file = "masterinfos-#{profile}.tar"

	system("tar -cf #{file} masterinfos")
	system("gzip #{file}")

	clean("*")

	Dir.delete("masterinfos")
end

delete("*.tar.gz")

process("1.1", $net_1_1, [$fx1_1])
process("2.0", $net_2_0, [$fx2_0])
process("3.0", $net_3_0, [$fx3_0, $fx2_0])
process("3.5", $net_3_5, [$fx3_5, $fx2_0])
process("4.0", $net_4_0, [$fx4_0])
process("SL2", $sl_2_0, [$sl2_0, $sl2_0sdk])
