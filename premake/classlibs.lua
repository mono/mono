MCS_CLASS_LIBS_ROOT = MONO_ROOT .. "mcs/class/"

if false then

local dirs = os.matchdirs(MCS_CLASS_LIBS_ROOT .. "**")

for k,v in ipairs(dirs) do
	print(v)
end

end