
class MonoBasicPackage (GitHubTarballPackage):

    def __init__(self):
        GitHubTarballPackage.__init__(self, 'mono', 'mono-basic', '4.6', '6c5b2e2b09fa91338fddd942ec32471c5227a545',
                                      configure='./configure --prefix="%{staged_profile}"')

    def install(self):
        self.sh('./configure --prefix="%{staged_prefix}"')
        self.sh('DEPRECATED=1 make install')

MonoBasicPackage()
