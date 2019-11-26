class FsharpPackage(GitHubTarballPackage):
    def __init__(self):
        GitHubTarballPackage.__init__(self,
            'mono', 'fsharp',
            '4.7.0',
            '758e81e608fd836c8e91231c86da0d7275e611fd',
            configure='',
            override_properties={ 'make': 'make all install PREFIX="%{package_prefix}" DESTDIR=%{stage_root}' })

        self.extra_stage_files = ['lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp/Microsoft.FSharp.Targets']
        self.sources.extend(['patches/fsharp-portable-pdb.patch'])

    def prep(self):
        Package.prep(self)

        for p in range(1, len(self.sources)):
            self.sh('patch -p1 < "%{local_sources[' + str(p) + ']}"')

    def build(self):
        Package.make(self)

    def install(self):
        pass

FsharpPackage()
