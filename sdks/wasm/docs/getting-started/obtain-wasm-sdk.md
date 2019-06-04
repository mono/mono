# Obtaining the WebAssembly SDK

Right now the WebAssembly sdk is packaged in a zip file and can be downloaded and unzipped ready to go from the build server.  We are working on distribution and easier setup in future steps.

&nbsp;&nbsp;&nbsp;&nbsp;[Requirements](#requirements)  
&nbsp;&nbsp;&nbsp;&nbsp;[Download WebAssembly SDK](#download-webassembly-sdk)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Latest Successful Build](#latest-successful-build)   
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Specific Build](#specific-build)  
&nbsp;&nbsp;&nbsp;&nbsp;[Extraction](#extraction)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Mac extraction](#mac-extraction)   
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Windows extraction](#windows-extraction)

# Requirements

_None_

## Download WebAssembly SDK

### Latest successful build

1. Go to the [WebAssembly Last Successful Build](https://jenkins.mono-project.com/job/test-mono-mainline-wasm/label=ubuntu-1804-amd64/lastSuccessfulBuild/Azure/)

1. Download the .zip file designated with `sdks/wasm/mono-wasm-###########.zip`.

### Specific build

1. Go to the [WebAssembly Builds](https://jenkins.mono-project.com/job/test-mono-mainline-wasm/label=ubuntu-1804-amd64/)

1. Pick the build you want from the *Build History* pane (e.g., most recent green build).

1. Underneath __Build History Pane__ on the left select the appropriate distribution build that is marked with a __green__ checkmark.

1. Once selected click on the *Azure Artifacts* link on the left.

1. Download the .zip file designated with `sdks/wasm/mono-wasm-###########.zip`. 

## Extraction

### Mac extraction

1. Extract the contents of the zip file, using `unzip <filename> -d ./sdks`.

1. Refer to the top-level extracted directory as: `WASM_SDK`

### Windows extraction

1. Right-click the downloaded file, click Extract All.

1. Change directory to `sdks`.

1. Refer to the top-level extracted directory as: `WASM_SDK`

