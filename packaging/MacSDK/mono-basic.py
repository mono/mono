
class MonoBasicPackage (GitHubTarballPackage):

    def __init__(self):
        GitHubTarballPackage.__init__(self, 'mono', 'mono-basic', '4.6', '0ebb1bf528dd77842d672ce95e185e7e27ba6db1',
                                      configure='./configure --prefix="%{staged_profile}"')

    def install(self):
        self.sh('./configure --prefix="%{staged_prefix}"')
        self.sh('make install')

MonoBasicPackage()
