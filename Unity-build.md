## Build Mono for Unity Uasges

### Clone the repo
- Clone this repo into a directory that follows the structure required by our build scripts
- Create some root directory: mkdir my-mono
- Create the expected directory for the mono checkout : mkdir my-mono\mono
- If you have a github account: git clone git@github.com:Unity-Technologies/mono my-mono\mono
- Otherwise: git clone git://github.com/Unity-Technologies/mono my-mono/mono  You won't be able to push back changes - if you need to do this, follow the instructions here: Unity on GitHub
- Pull the git submodules (this is required after cloning, and is often necessary when changing branches)
  - git submodule update --init --recursive
  - NOTE:  If on windows, run in Git Bash to be able to enter a password

### Build on Windows

#### Runtime
From the root of your cloned mono directory run:
- external\buildscripts\build_runtime_win.pl

or: Open msvc/mono.sln in Visual Studio and build the Runtime/libmono-dynamic project

- If you are on 2020.3 or older you can use VS 2015/VS 2019 as long as you don't upgrade the projects and have VS 2010 installed!

- Don't forget to select x64 platform if you intend to use the DLLs with the 64-bit Player/Editor!

- Artifacts will be in: <mono root>\msvc\build\boehm\x64\bin\Debug

### Build on OSX
You need to install pkg-config. With Homebrew installed run:
- brew install pkg-config

#### Runtime
Optional - Enable debug build: 

- Edit external/buildscripts/build_runtime_osx.pl and add '–debug=1' to the list of arguments passed to build.pl

- From the root of your cloned mono directory run:
  - perl external/buildscripts/build_runtime_osx.pl

Optional: If the build fails with the error "'mach_dep.lo' is not a valid libtool object" - disable parallel build by changing (external/buildscripts/build_runtime_osx.pl) my $jobs = 4 to my $jobs = 1 and try again.

After the building is completed you can verify that you have a good build universal dylib by running : 

- file builds/embedruntimes/osx/libmonobdwgc-2.0.dylib

If you stumble upon the build script not being able to copy the main Mono .dylib symbols file (.dSYM) don't stress, the information is embedded.

#### class libraries

From the root of your cloned mono directory run:
 - perl external/buildscripts/build_classlibs_osx.pl

### Build on other platforms
- Run external/buildscripts/build_runtime_myplatform.pl (Runtime only)
or
- Run ./autogen.sh followed by make. (All platforms (requires cygwin on windows))