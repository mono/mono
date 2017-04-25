MONO_RUNTIME_ROOT = MONO_ROOT .. "mono/"

function SetupSGen()
	local f = filter()

	defines
	{
		"HAVE_SGEN_GC",
		"HAVE_MOVING_COLLECTOR",
		"HAVE_WRITE_BARRIERS",
		"MONO_DLL_EXPORT"
	}

	files
	{
		MONO_RUNTIME_ROOT .. "metadata/sgen-*.*"
	}

	filter "system:windows"
		files { MONO_RUNTIME_ROOT .. "metadata/sgen-os-win*.c" }

	filter "system:not windows"
		files { MONO_RUNTIME_ROOT .. "metadata/sgen-os-posix.c" }

	filter "system:macosx"
		files { MONO_RUNTIME_ROOT .. "metadata/sgen-os-mach.c" }

	filter(f)
end

function SetupConfigDefines()
	defines
	{
		"HAVE_DECL_INTERLOCKEDCOMPAREEXCHANGE64=1",
		"HAVE_DECL_INTERLOCKEDEXCHANGE64=1",
		"HAVE_DECL_INTERLOCKEDINCREMENT64=1",
		"HAVE_DECL_INTERLOCKEDDECREMENT64=1",
		"HAVE_DECL_INTERLOCKEDADD=1",
		"HAVE_DECL_INTERLOCKEDADD64=1",
		"HAVE_DECL_INTERLOCKEDCOMPAREEXCHANGE64=1",
		"HAVE_DECL___READFSDWORD=1",
	}
end

function GenerateConfig()
	if not os.is("windows") then
		return
	end

	print('Generating Windows config.h')
	os.copyfile(MONO_ROOT .. "winconfig.h", gendir .. "/config.h")
end

function GenerateVersion()
	print('Generating version.h')

	local contents
	if os.isdir(MONO_ROOT .. ".git") then
		local branches = os.outputof("set LANG=C && git branch")
		local branch = string.gmatch(branches, "^\* (%w+)")()
		local version = os.outputof("set LANG=C && git log --no-color --first-parent -n1 --pretty=format:%h")
		contents = string.format("#define FULL_VERSION \"%s/%s\"", branch, version)
	else
		contents = "#define FULL_VERSION \"tarball\""
	end

	file = io.open(path.getabsolute(gendir .. "/version.h"), "w+")
	file:write(contents)
	file:close()
end

function SetupMonoIncludes(monoRuntimeRoot)
	includedirs
	{
		gendir,
		monoRuntimeRoot,
		monoRuntimeRoot .. "..",
		monoRuntimeRoot .. "../eglib/src",
		monoRuntimeRoot .. "utils/",
	}
	print(monoRuntimeRoot .. "../eglib/src")
end

function SetupMonoLinks()
	local f = filter()

	links
	{
		"eglib",
		"libmonoruntime",
		"libmonoutils"
	}

	filter "system:windows"

		links
		{
			"Mswsock",
			"ws2_32",
			"psapi",
			"version",
			"winmm",
		}

	filter()
end

function GenerateMachineDescription(arch)
	local prj = premake.api.scope.project.location
	local abs = path.getabsolute(MONO_RUNTIME_ROOT .. 'mini/cpu-' .. arch .. '.md')
	local input = path.getrelative(prj, abs)	
	local out = gendir .. '/cpu-' .. arch .. '.h'
	local desc = arch .. '_desc'

	prebuildcommands
	{
		'"%{cfg.targetdir}/genmdesc" ' .. out .. ' ' .. desc .. ' ' .. input
	}
end

GenerateConfig()
GenerateVersion()

project "mono"
	
	kind "ConsoleApp"
	language "C"

	files
	{
		MONO_RUNTIME_ROOT .. "mini/main.c",
	}

	includedirs
	{
		gendir,
		MONO_RUNTIME_ROOT .. "../",
		MONO_RUNTIME_ROOT .. "../eglib/src/"
	}	
	
	links
	{
		"eglib",
		"libmono",
		"libmonoruntime",
		"libmonoutils"
	}

	filter "action:vs*"
		defines { "_CRT_SECURE_NO_WARNINGS", "_CRT_NONSTDC_NO_DEPRECATE" }
		SetupConfigDefines()


project "libmono"

	kind "SharedLib"
	language "C"

	defines
	{
		"__default_codegen__",
		"HAVE_CONFIG_H",
		"MONO_DLL_EXPORT"
	}
	
	SetupMonoIncludes(MONO_RUNTIME_ROOT)
	SetupMonoLinks()

	files
	{
		MONO_RUNTIME_ROOT .. "mini/*.c",
		MONO_RUNTIME_ROOT .. "mini/*.h",
	}

	excludes
	{
		-- Archicture-specific files
		MONO_RUNTIME_ROOT .. "mini/*-alpha.*",
		MONO_RUNTIME_ROOT .. "mini/*-amd64.*",
		MONO_RUNTIME_ROOT .. "mini/*-arm.*",
		MONO_RUNTIME_ROOT .. "mini/*-arm64.*",
		MONO_RUNTIME_ROOT .. "mini/*-hppa.*",
		MONO_RUNTIME_ROOT .. "mini/*-ia64.*",
		MONO_RUNTIME_ROOT .. "mini/*-llvm.*",
		MONO_RUNTIME_ROOT .. "mini/*-mips.*",
		MONO_RUNTIME_ROOT .. "mini/*-ppc.*",
		MONO_RUNTIME_ROOT .. "mini/*-s390*.*",
		MONO_RUNTIME_ROOT .. "mini/*-sparc.*",
		MONO_RUNTIME_ROOT .. "mini/*-x86.*",

		-- Platform-specific files
		MONO_RUNTIME_ROOT .. "mini/*-windows.*",
		MONO_RUNTIME_ROOT .. "mini/*-darwin.*",
		MONO_RUNTIME_ROOT .. "mini/*-posix.*",

		-- Tools
		MONO_RUNTIME_ROOT .. "mini/fsacheck.c",
		MONO_RUNTIME_ROOT .. "mini/genmdesc.c",
		MONO_RUNTIME_ROOT .. "mini/main.c",
	}

	dependson { "genmdesc" }
	
	filter "platforms:x32"
		GenerateMachineDescription('x86')
		files
		{
			MONO_RUNTIME_ROOT .. "mini/*-x86.c",
		}

	filter "platforms:x64"
		GenerateMachineDescription('amd64')
		files
		{
			MONO_RUNTIME_ROOT .. "mini/*-amd64.c",
		}	

	filter "system:windows"
		SetupWindowsDefines()
		files
		{
			MONO_RUNTIME_ROOT .. "mini/*-windows.c"
		}

	filter "system:not windows"
		files
		{
			MONO_RUNTIME_ROOT .. "mini/*-posix.c"
		}

	filter "system:macosx"
		files
		{
			MONO_RUNTIME_ROOT .. "mini/*-darwin.c"
		}

	filter "action:vs*"
		defines { "_CRT_SECURE_NO_WARNINGS", "_CRT_NONSTDC_NO_DEPRECATE" }
		SetupConfigDefines()
		SetupMSVCWarnings()
		buildoptions
		{
			"/wd4018", -- signed/unsigned mismatch
			"/wd4244", -- conversion from 'x' to 'y', possible loss of data
			"/wd4133", -- incompatible types - from 'x *' to 'y *'
			"/wd4715", -- not all control paths return a value
			"/wd4047", -- 'x' differs in levels of indirection from 'y'
		}
		linkoptions
		{
			"/ignore:4049", -- locally defined symbol imported
			"/ignore:4217", -- locally defined symbol imported in function
		}

project "libmonoruntime"

	kind "StaticLib"
	language "C"
	
	defines
	{
		"HAVE_CONFIG_H",
	}

	SetupMonoIncludes(MONO_RUNTIME_ROOT)
	
	files
	{
		MONO_RUNTIME_ROOT .. "metadata/*.c",
		MONO_RUNTIME_ROOT .. "metadata/*.h",
		MONO_RUNTIME_ROOT .. "sgen/*.c",
		MONO_RUNTIME_ROOT .. "sgen/*.h",
	}

	excludes
	{
		MONO_RUNTIME_ROOT .. "metadata/threadpool-ms-io-*",

		-- GC-specific files
		MONO_RUNTIME_ROOT .. "metadata/boehm-gc.c",
		MONO_RUNTIME_ROOT .. "metadata/null-gc.c",
		MONO_RUNTIME_ROOT .. "metadata/sgen-*.*",
		MONO_RUNTIME_ROOT .. "metadata/sgen-os-*.*",
		
		-- Platform-specific files
		MONO_RUNTIME_ROOT .. "metadata/console-unix.c",
		MONO_RUNTIME_ROOT .. "metadata/console-win32.c",
		MONO_RUNTIME_ROOT .. "metadata/coree.c",

		-- Tools
		MONO_RUNTIME_ROOT .. "metadata/monodiet.c",
		MONO_RUNTIME_ROOT .. "metadata/monosn.c",
		MONO_RUNTIME_ROOT .. "metadata/pedump.c",
		MONO_RUNTIME_ROOT .. "metadata/tpool-*.c",
		MONO_RUNTIME_ROOT .. "metadata/test-*.c",
	}

	SetupSGen()

	filter "system:windows"
		SetupWindowsDefines()
		defines { "_WINSOCK_DEPRECATED_NO_WARNINGS" }
		files
		{
			MONO_RUNTIME_ROOT .. "metadata/coree.c",
		}

	filter "system:linux"
		files
		{
			MONO_RUNTIME_ROOT .. "metadata/tpool-epoll.c"
		}
		
	filter "system:macosx or freebsd"
		files
		{
			MONO_RUNTIME_ROOT .. "metadata/tpool-kqueue.c"
		}

	filter "action:vs*"
		defines { "_CRT_SECURE_NO_WARNINGS", "_CRT_NONSTDC_NO_DEPRECATE" }
		SetupConfigDefines()
		SetupMSVCWarnings()


project "libmonoutils"

	kind "StaticLib"
	language "C"
	
	defines
	{
		"HAVE_CONFIG_H",
	}

	SetupMonoIncludes(MONO_RUNTIME_ROOT)
	
	files
	{
		MONO_RUNTIME_ROOT .. "utils/*.c",
		MONO_RUNTIME_ROOT .. "utils/*.h",
	}

	excludes
	{
		MONO_RUNTIME_ROOT .. "utils/sha1.c",

		-- Platform-specific files
		MONO_RUNTIME_ROOT .. "utils/atomic.c",
		MONO_RUNTIME_ROOT .. "utils/mono-hwcap-*.*",
		MONO_RUNTIME_ROOT .. "utils/mono-threads-*.c",

		-- Tools
		MONO_RUNTIME_ROOT .. "utils/mono-embed.c",
	}

	files { MONO_RUNTIME_ROOT .. "utils/mono-threads-state-machine.c" }

	filter "architecture:arm*"
		files { MONO_RUNTIME_ROOT .. "utils/mono-hwcap-arm.*" }

	filter "architecture:x32 or x64"
		files { MONO_RUNTIME_ROOT .. "utils/mono-hwcap-x86.*" }

	filter "system:windows"
		SetupWindowsDefines()
		files
		{
			MONO_RUNTIME_ROOT .. "metadata/coree.c",
			MONO_RUNTIME_ROOT .. "utils/mono-threads-windows.c",
		}
		links
		{
			"Mswsock",
			"ws2_32",
			"psapi",
			"version",
			"winmm",
			"eglib",
		}

	filter "system:not windows"
		files
		{
			MONO_RUNTIME_ROOT .. "utils/mono-threads-posix.c",
		}

	filter "system:macosx"
		files
		{
			MONO_RUNTIME_ROOT .. "utils/mono-threads-mach.c",
		}

	filter "action:vs*"
		defines { "_CRT_SECURE_NO_WARNINGS", "_CRT_NONSTDC_NO_DEPRECATE" }
		SetupConfigDefines()
		SetupMSVCWarnings()
		buildoptions { "/wd4273", "/wd4197" }

