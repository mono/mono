
class MonoBasicPackage (GitHubTarballPackage):

    def __init__(self):
        GitHubTarballPackage.__init__(self, 'mono', 'mono-basic', '4.8', 'e31cb702937a0adcc853250a0989c5f43565f9b8',
                                      configure='./configure --prefix="%{staged_profile}"')

    def install(self):
        self.sh('./configure --prefix="%{staged_prefix}"')
        self.sh('DEPRECATED=1 make install')

MonoBasicPackage()
