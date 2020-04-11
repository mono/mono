param ($filepath, $url, $runtime)
$PACKAGE_PATH="~\\.nuget\\packages\\microsoft.aspnetcore.components.webassembly.runtime\\$runtime\\tools\\dotnetwasm"

if ($null -eq $filepath) {
	Invoke-WebRequest -Uri $url -OutFile wasm-package.zip -UseBasicParsing

	Expand-Archive wasm-package.zip 
	cd wasm-package
}
else {
	cd $filepath
}

rm -r $PACKAGE_PATH\bcl
cp wasm-bcl $PACKAGE_PATH\bcl

rm -r $PACKAGE_PATH\framework
cp framework $PACKAGE_PATH\framework

rm -r $PACKAGE_PATH\wasm\*
cp builds\release\dotnet.$runtime.js $PACKAGE_PATH\wasm\
cp builds\release\dotnet.wasm $PACKAGE_PATH\wasm\

cd ..
rm wasm-package.zip
rm -r wasm-package