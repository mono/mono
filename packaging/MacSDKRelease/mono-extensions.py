from bockbuild.package import Package


class MonoExtensionsPackage(Package):

    def __init__(self):
        Package.__init__(self, 'mono-extensions', None,
                         sources=['git@github.com:xamarin/mono-extensions.git'],
                         revision='3cc5e2e1870b35f15b1540f835a370d2b011bacd'
                         )
        self.source_dir_name = 'mono-extensions'

    def build(self):
        pass

    def install(self):
        pass

MonoExtensionsPackage()
