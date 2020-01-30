class XspPackage (GitHubTarballPackage):

    def __init__(self):
        GitHubTarballPackage.__init__(self, 'mono', 'xsp', '4.7',
                                      '72b24c04c9246dc1005a3cd1a37398777613880d',
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
