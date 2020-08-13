param ($url, $runtime, $asp_working_dir, $mono_working_dir, $asp_remote_name, $mono_remote_name)

if ($null -eq $asp_remote_name) {
	$asp_remote_name="upstream"
} 

if ($null -eq $mono_remote_name) {
	$mono_remote_name="upstream"
}

if ($null -eq $asp_branch_name) {
	$asp_branch_name="blazor-wasm"
}

if ($null -eq $mono_branch_name) {
	$mono_branch_name="master"
}

if ($null -eq $runtime) {
	echo "Error: runtime version required. Use -runtime"
	exit 1
}

if ($null -eq $asp_working_dir) {
	echo "Error: aspnetcore working directory required. Use -asp_working_dir"
	exit 1
}

if ($null -eq $mono_working_dir) {
	echo "Error: mono working directory required. Use -mono_working_dir"
	exit 1
}

if (($null -eq $filepath) -and ($null -eq $url)) {
	echo "Error: either path to the wasm package file required, or the url to download from."
	exit 1
}

$NUGET_HOME="~\\.nuget"
$PACKAGE_PATH="$NUGET_HOME\\packages\\microsoft.aspnetcore.components.webassembly.runtime\\$runtime\\tools\\dotnetwasm"
$PROXY_PACKAGE_PATH="$NUGET_HOME\\packages\\microsoft.aspnetcore.components.webassembly.devserver\\$runtime\\tools\\BlazorDebugProxy"
$ASP_PROXY_PATH="$asp_working_dir\\src\\Components\\WebAssembly\\DebugProxy\\src"
$MONO_PROXY_PATH="$mono_working_dir\\sdks\\wasm\\BrowserDebugProxy"
$TMP_DIR=(mktemp -d)
$TMP_PKG_DIR="$TMP_DIR\\wasm-package"
mkdir $TMP_DIR

if ($null -ne $url) {
	Invoke-WebRequest -Uri $url -OutFile $TMP_DIR\wasm-package.zip -UseBasicParsing
	Expand-Archive "$TMP_DIR\\wasm-package.zip" -d $TMP_PKG_DIR
} else {
	cp -r "$mono_working_dir/sdks/out/wasm-bcl" $TMP_PKG_DIR
	cp -r "$mono_working_dir/sdks/wasm/framework" $TMP_PKG_DIR
	cp -r "$mono_working_dir/sdks/wasm/builds" $TMP_PKG_DIR
}

rm -r $PACKAGE_PATH\bcl
cp $TMP_PKG_DIR\wasm-bcl\wasm $PACKAGE_PATH\bcl

rm -r $PACKAGE_PATH\framework
cp $TMP_PKG_DIR\framework $PACKAGE_PATH\framework

rm -r $PACKAGE_PATH\wasm\*
cp $TMP_PKG_DIR\builds\release\dotnet.js $PACKAGE_PATH\wasm\dotnet.$runtime.js
cp $TMP_PKG_DIR\builds\release\dotnet.wasm $PACKAGE_PATH\wasm\

echo "Replacing DebuggerProxy"

cd $mono_working_dir
git checkout $mono_branch_name
# git fetch $mono_remote_name
# git reset --hard $mono_remote_name/$mono_branch_name

cd $asp_working_dir
git checkout $asp_branch_name
git fetch $asp_remote_name
git reset --hard $asp_remote_name/$asp_branch_name

./clean.ps1

cp $MONO_PROXY_PATH\*.cs $ASP_PROXY_PATH\MonoDebugProxy\ws-proxy\
rm $ASP_PROXY_PATH\MonoDebugProxy\ws-proxy\AssemblyInfo.cs

./build.ps1

cp $ASP_PROXY_PATH\bin\Debug\netcoreapp3.1\Microsoft.AspNetCore.Components.WebAssembly.DebugProxy.dll $PROXY_PACKAGE_PATH

echo "Replacement finished"
