class FsharpPackage(GitHubTarballPackage):
    def __init__(self):
        GitHubTarballPackage.__init__(self,
            'fsharp', 'fsharp',
            '4.1.32',
            'a8ec45a4a83e438ed99dc7721f263cb89a4ebe8b',
            configure='./configure --prefix="%{package_prefix}"',
            override_properties={ 'make': 'make' })

        self.extra_stage_files = ['lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp/Microsoft.FSharp.Targets']
        self.sources.extend(['patches/fsharp-portable-pdb.patch'])

    def prep(self):
        Package.prep(self)

        for p in range(1, len(self.sources)):
            self.sh('patch -p1 < "%{local_sources[' + str(p) + ']}"')

    def build(self):
        self.sh('autoreconf') 
        Package.configure(self)
        Package.make(self)

FsharpPackage()
