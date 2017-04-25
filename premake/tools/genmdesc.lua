project "genmdesc"

  kind "ConsoleApp"
  language "C"

  files
  {
    MONO_ROOT .. "../mono/mini/genmdesc.c",
    MONO_ROOT .. "../mono/mini/helpers.c",
    MONO_ROOT .. "../mono/utils/monobitset.c",
    MONO_ROOT .. "../mono/metadata/opcodes.c"
  }

  removedefines { "DEBUG" }

  SetupMonoIncludes(MONO_ROOT)

  links
  {
    "eglib",
  }

  filter "action:vs*"

    defines { "_CRT_SECURE_NO_WARNINGS", "_CRT_NONSTDC_NO_DEPRECATE" }
    SetupConfigDefines()
    SetupMSVCWarnings()

    buildoptions
    {
      "/wd4273", -- 'function' : inconsistent DLL linkage
      "/wd4197"
    }

    linkoptions
    {
      "/ignore:4049", -- locally defined symbol imported
      "/ignore:4217", -- locally defined symbol imported in function
    }
