#!/usr/bin/env ruby

require 'ftools'

$gac = 'C:/WINDOWS/assembly/GAC_MSIL'
$fx =  'C:/WINDOWS/Microsoft.NET/Framework'

$fx1 = File.join $fx, "v1.1.4322"
$fx2 = File.join $fx, "v2.0.50727"
$fx3_0 = 'C:/Program Files/Reference Assemblies/Microsoft/Framework/v3.0'
$fx3_5 = 'C:/Program Files/Reference Assemblies/Microsoft/Framework/v3.5'
$sl2_0 = 'C:/Program Files/Microsoft Silverlight/2.0.31005.0'
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

def generate(assembly)
	asm = File.join "masterinfos", assembly + ".dll"
	out = File.join "masterinfos", assembly + ".xml"
	system("./mono-api-info.exe #{asm} > #{out}")
end

def process(profile, assemblies, fxs = nil)
	clean("*", true)

	assemblies.each do |assembly|
		if assembly != nil and assembly.length > 0
			#puts assembly 
			location = locate(assembly, fxs)
			if location
				File.copy(location, "masterinfos")
				generate(assembly)
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

process("1.1", $net_1_1, [$fx1])
process("2.0", $net_2_0, [$fx2])
process("3.0", $net_3_0, [$fx3_0, $fx2])
process("3.5", $net_3_5, [$fx3_5, $fx2])
process("SL2", $sl_2_0, [$sl2_0, $sl2_0sdk])
