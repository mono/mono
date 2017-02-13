class FsharpPackage(GitHubTarballPackage):

    def __init__(self):
        GitHubTarballPackage.__init__(self,
                                      'fsharp', 'fsharp',
                                      '4.0.1.20',
                                      '9bd7c2420e06c1597ef5a37b6cb6e0f8d2911b10',
                                      configure='./configure --prefix="%{package_prefix}"')

        self.extra_stage_files = [
            'lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp/Microsoft.FSharp.Targets']

    def prep(self):
        Package.prep(self)

        for p in range(1, len(self.sources)):
            self.sh('patch -p1 < "%{local_sources[' + str(p) + ']}"')

    def build(self):
        self.sh('autoreconf')
        Package.configure(self)
        Package.make(self)

FsharpPackage()
