builddir = path.getabsolute("./" .. _ACTION or "")
libdir = path.join(builddir, "lib")
gendir = path.join(builddir, "gen")

MONO_ROOT = "../"
MONO_TOOLS_ROOT = MONO_ROOT .. "tools/"

include("helpers.lua")

GenerateBuildVersion("buildver-sgen.h")
os.mkdir(gendir)

workspace "Mono"

	configurations
	{
		"Debug_SGen",
		"Release_SGen",
		"Debug_Boehm",
		"Release_Boehm"
	}

	platforms { "x32", "x64" }

  	filter "configurations:Release"
    	defines { "NDEBUG" }	

	filter "platforms:x32"
		architecture "x86"

	filter "platforms:x64"
		architecture "x86_64"

	filter { "system:windows", "language:C or C++" }
		defines { "WIN32", "_WINDOWS" }
		defines { "WINVER=0x0600", "_WIN32_WINNT=0x0600" }	

	filter {}
	
	location (builddir)
	objdir (builddir .. "/obj/")
	targetdir (libdir)
	libdirs { libdir }

	framework "4.5"
	flags { "Unicode", "Symbols" }

	group "Compiler"
		include("mcs.lua")

	group "Class Libraries"
		include("classlibs.lua")

	group "Profilers"
		include("profilers.lua")

	group "Runtime"
		include("eglib.lua")
		include("mono.lua")

	group "Tools"
		include("tools.lua")
