-- This module checks for the all the project dependencies.

function SetupManagedProject()
  language "C#"
  location (path.join(builddir, "projects"))
end

function SetupWindowsDefines()
    defines
    {
      "WIN32_THREADS",
      "UNICODE",
      "_UNICODE",
      "FD_SETSIZE=1024",
      "HOST_WIN32",
      "WIN32_LEAN_AND_MEAN"
    }
end

function SetupMSVCWarnings()
  buildoptions
  {
    "/wd4018", -- signed/unsigned mismatch
    "/wd4244", -- conversion from 'x' to 'y', possible loss of data
    "/wd4133", -- incompatible types - from 'x *' to 'y *'
    "/wd4715", -- not all control paths return a value
    "/wd4047", -- 'x' differs in levels of indirection from 'y'
    "/wd4116", -- unnamed type definition in parentheses
  }

  -- clang-cl specific warnings
  -- filter { "toolset:clang", "action:vs*" }
  if useClangCl then
    buildoptions
    {      
      "-Wno-implicit-int",
      "-Wno-implicit-function-declaration",
      "-Wno-return-type",
      "-Wno-unused-variable",
      "-Wno-deprecated-declarations",
      "-Wno-parentheses",
      "-Wno-incompatible-pointer-types",
      "-Wno-missing-braces",
      "-Wno-unused-function",
    }
  end
end

function IncludeDir(dir)
  local files = os.matchfiles(path.join(dir, "**.lua"))
  
  for i,file in pairs(files) do
    print(string.format(" including %s", file))
    include(file)
  end
end

function WriteToFile(path, content)
  file = io.open(path, "w")
  file:write(content)
  file:close()
end

function GenerateBuildVersion(file)
  print("Generating build version file: " .. file)
  local contents = "const char *build_date = \"\";"
  WriteToFile(gendir .. '/' .. file, contents)
end

-- Premake hacks

local p = premake
local m = p.vstudio.vc2010

premake.override(premake.vstudio.vc2010, "platformToolset", function(base, cfg)
    local tool, version = p.config.toolset(cfg)
    if version then
      version = "v" .. version
    else
      local action = p.action.current()
      version = action.vstudio.platformToolset
    end
    m.element("PlatformToolset", nil, version)
end)