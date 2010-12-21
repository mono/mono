repository = "../../../../cecil"

sources = {
	"Mono.Cecil/*.cs" => "Mono.Cecil",
	"Mono.Cecil.Cil/*.cs" => "Mono.Cecil.Cil",
	"Mono.Cecil.Metadata/*.cs" => "Mono.Cecil.Metadata",
	"Mono.Cecil.PE/*.cs" => "Mono.Cecil.PE",
	"Mono.Collections.Generic/*.cs" => "Mono.Collections.Generic",
	"Mono.Security.Cryptography/*.cs" => "Mono.Security.Cryptography",
	"Mono/*.cs" => "Mono",
	"System.Runtime.CompilerServices/*.cs" => "System.Runtime.CompilerServices",
	"NOTES.txt" => ".",

	"symbols/mdb/Mono.Cecil.Mdb/*.cs" => "../Mono.Cecil.Mdb/Mono.Cecil.Mdb/",
}

require "ftools"

sources.each { |source, destination|
	Dir[File::join(repository, source)].each { |file|
		#puts "copying #{file} to #{destination}"
		File.copy(file, destination)
	}
}

log = IO.popen("git log -n1")

File.open("revision", File::WRONLY|File::TRUNC|File::CREAT, 0644) { |f|
	f << log.gets[("commit ".length)..-1]
}
