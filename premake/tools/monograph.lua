project "monograph"

	kind "ConsoleApp"
	language "C"

	files
	{
		MONO_ROOT .. "../tools/monograph/*.c",
		MONO_ROOT .. "../tools/monograph/*.h",
		MONO_ROOT .. "../mono/metadata/opcodes.c"
	}

	SetupMonoIncludes(MONO_ROOT)
	SetupMonoLinks()
	defines { "MONO_STATIC_BUILD" }

	filter "action:vs*"
	
		defines { "_CRT_SECURE_NO_WARNINGS", "_CRT_NONSTDC_NO_DEPRECATE" }
		SetupConfigDefines()
		buildoptions
		{
			"/wd4018", -- signed/unsigned mismatch
			"/wd4273", -- inconsistent dll linkage
		}
		linkoptions
		{
			"/ignore:4049", -- locally defined symbol imported
			"/ignore:4217", -- locally defined symbol imported in function
		}
