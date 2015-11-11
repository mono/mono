local CPPSHARP_DIR = "CppSharp/"
local NEWTONSOFT_DIR = "Newtonsoft.Json.6.0.8/lib/net45/",

solution "MonoChecker"

  configurations { "Debug", "Release" }
  platforms { "x32", "x64" }
  flags { "Symbols" }

  project "MonoChecker"

    kind  "ConsoleApp"
    language "C#"

    files { "*.cs" }
    links
    {
      CPPSHARP_DIR .. "CppSharp",
      CPPSHARP_DIR .. "CppSharp.AST",
      CPPSHARP_DIR .. "CppSharp.Parser.CSharp",
      CPPSHARP_DIR .. "CppSharp.Generator",
      NEWTONSOFT_DIR .. "Newtonsoft.Json.dll"
    }
