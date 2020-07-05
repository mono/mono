# Instructions for running

Make sure that you have the latest Blazor WebAssembly version:

`dotnet new -i Microsoft.AspNetCore.Components.WebAssembly.Templates::3.2.0-preview2.20160.5`



## To run:

`.\download-packages.ps1 -r <blazor webassembly runtime> -u <url to download .zip of wasm build> -asp_working_dir <path to aspnetcore working dir> -mono_working_dir <path to mono working dir>`

if no url is specified, the script will use a local build from the specified mono working directory. 

`download-packages.sh` is the bash equivalent. 