MCS_ROOT = MONO_ROOT .. 'mcs/'

function GenerateConsts()
  print('Generating Consts.cs')

  local consts = 'build/common/Consts.cs.in'
  local src = MCS_ROOT .. consts
  local dest = gendir .. '/Consts.cs'

  local file = io.open(path.getabsolute(src))
  if not file then
    error(consts .. " was not found")
  end

  local contents = file:read("*all")
  file:close()

  local token = '@MONO_VERSION@'
  local version = '3.12'

  local contents, matches = string.gsub(contents, token, version)
  if matches == 0 then
    print("Error replacing token " .. token .. ' with version')
    return
  end

  file = io.open(path.getabsolute(dest), "w+")
  file:write(contents)
  file:close()
end

GenerateConsts()

project "mcs-gen"

  kind "ConsoleApp"
  dependson { "jay" }

  files { MCS_ROOT .. "mcs/*.jay" }

  filter 'files:**.jay'
    -- A message to display while this build step is running (optional)
    buildmessage 'Generating %{file.relpath}'

    -- One or more commands to run (required)
    local prj = premake.api.scope.project.location
    local input = path.getrelative(prj, path.getabsolute(MCS_ROOT .. 'jay/skeleton.cs'))
    local out = 'gen/%{file.basename}.cs'
    buildcommands { '"lib\\jay" -cv < "' .. input .. '" "%{file.relpath}" > ' .. out }

   -- One or more outputs resulting from the build (required)
   buildoutputs { out }

project "mcs"
  SetupManagedProject()

  kind "ConsoleApp"
  dependson { "mcs-gen" }

  defines { 'NET_4_5' }
  files
  {
    MCS_ROOT .. "mcs/*.cs",
    MCS_ROOT .. "mcs/*.jay",
    MCS_ROOT .. "class/Mono.CompilerServices.SymbolWriter/*.cs",
    MCS_ROOT .. "class/Mono.Security/Mono.Security.Cryptography/CryptoConvert.cs",
    MCS_ROOT .. "tools/monop/outline.cs",
    gendir .. "/cs-parser.cs",
    gendir .. "/Consts.cs"
  }

  excludes { MCS_ROOT .. "mcs/ikvm.cs" }
  links { "System", "System.Xml" }
