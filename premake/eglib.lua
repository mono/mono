EGLIB_ROOT = MONO_ROOT .. 'eglib/'

project "eglib"
	
	kind "StaticLib"
	language "C"
	
	includedirs
	{
		gendir,
		EGLIB_ROOT,
		EGLIB_ROOT .. "src",
		EGLIB_ROOT .. ".."
	}
	
	files
	{
		EGLIB_ROOT .. "src/*.c",
		EGLIB_ROOT .. "src/*.h",
	}

	excludes
	{	
		-- Platform-specific files
		EGLIB_ROOT .. "src/*-unix.c",
		EGLIB_ROOT .. "src/*-win32.c",
	}

	filter "system:windows"
		SetupWindowsDefines()
		files
		{
			EGLIB_ROOT .. "src/*-win32.c"
		}

	filter "system:not windows"
		files { EGLIB_ROOT .. "src/*-unix.c" }

	filter "action:vs*"
		SetupMSVCWarnings()
		buildoptions
		{
			"/wd4018", -- signed/unsigned mismatch
			"/wd4133", -- incompatible types - from 'x *' to 'y *'
			"/wd4267", -- conversion from 'size_t' to 'glong', possible loss of data
			"/wd4273", -- inconsistent dll linkage
			"/wd4996"
		}
