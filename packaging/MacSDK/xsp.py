class XspPackage (GitHubTarballPackage):

    def __init__(self):
        GitHubTarballPackage.__init__(self, 'mono', 'xsp', '4.7.1',
                                      'b7190dd996b2c652630297ed9b1b9907602b0d4f',
                                      configure='./autogen.sh --prefix="%{package_prefix}"')

    def install(self):
        # scoop up some mislocated files
        misdir = '%s%s' % (self.stage_root, self.staged_profile)
        unprotect_dir(self.stage_root)
        Package.install(self)
        if not os.path.exists(misdir):
            for path in iterate_dir(self.stage_root):
                print path
            error('Could not find mislocated files')

        self.sh('rsync -a --ignore-existing %s/* %s' %
                (misdir, self.profile.staged_prefix))
        self.sh('rm -rf %s/*' % misdir)


XspPackage()
