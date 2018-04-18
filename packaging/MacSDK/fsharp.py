class FsharpPackage(GitHubTarballPackage):
    def __init__(self):
        GitHubTarballPackage.__init__(self,
            'fsharp', 'fsharp',
            '4.1.33',
            '561af8ba705fdbd84274702bc8073b9a94ba0a7d',
            configure='./configure --prefix="%{package_prefix}"',
            override_properties={ 'make': 'make' })

        self.extra_stage_files = ['lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp/Microsoft.FSharp.Targets']
        self.sources.extend(['patches/fsharp-portable-pdb.patch', 'patches/fsharp-string-switchName.patch'])

    def prep(self):
        Package.prep(self)

        for p in range(1, len(self.sources)):
            self.sh('patch -p1 < "%{local_sources[' + str(p) + ']}"')

    def build(self):
        self.sh('autoreconf') 
        Package.configure(self)
        Package.make(self)

FsharpPackage()
