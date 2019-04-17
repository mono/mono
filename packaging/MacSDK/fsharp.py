class FsharpPackage(GitHubTarballPackage):
    def __init__(self):
        GitHubTarballPackage.__init__(self,
            'fsharp', 'fsharp',
            '4.5.0',
            '3de387432de8d11a89f99d1af87aa9ce194fe21b',
            configure='',
            override_properties={ 'make': 'make all install PREFIX="%{package_prefix}" DESTDIR=%{stage_root}' })

        self.extra_stage_files = ['lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp/Microsoft.FSharp.Targets']
        self.sources.extend(['patches/fsharp-IsPathRooted-type-inference.patch',
                             'patches/fsharp-portable-pdb.patch',
                             'patches/fsharp-noinstall.patch',
                             'patches/fsharp-GetFileNameWithoutExtension-type-inference.patch',
                             'patches/fsharp-msbuild-16-0.patch',
                             'patches/fsharp-custom-prefix.patch'])

    def prep(self):
        Package.prep(self)

        for p in range(1, len(self.sources)):
            self.sh('patch -p1 < "%{local_sources[' + str(p) + ']}"')

    def build(self):
        Package.make(self)

    def install(self):
        pass

FsharpPackage()
