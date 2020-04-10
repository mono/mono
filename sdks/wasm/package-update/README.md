# Instructions for running

Make sure that you have the latest Blazor WebAssembly version:

`dotnet new -i Microsoft.AspNetCore.Components.WebAssembly.Templates::3.2.0-preview2.20160.5`



## To run:

`.\download-packages.ps1 -runtime <blazor webassembly runtime> -url <url to download .zip of wasm build>`

-- or --

`.\download-packages.ps1 -runtime <blazor webassembly runtime> -filepath <filepath to wasm build>`


`download-packages.sh` is the bash equivalent. 