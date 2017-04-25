JAY_ROOT = MONO_ROOT .. '../mcs/jay/'

project "jay"

  kind "ConsoleApp"
  language "C"

  files { JAY_ROOT .. "*.c" }

  defines { "SKEL_DIRECTORY=\".\""}
  removedefines { "DEBUG" }

  filter "action:vs*"

    SetupMSVCWarnings()
    buildoptions
    {
        "/wd4033",
        "/wd4013",
        "/wd4996",
        "/wd4267",
        "/wd4273",
        "/wd4113",
        "/wd4244",
        "/wd4715",
        "/wd4716"
    }
