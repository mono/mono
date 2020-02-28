from bockbuild.package import Package


class MonoExtensionsPackage(Package):

    def __init__(self):
        Package.__init__(self, 'mono-extensions', None,
                         sources=['git@github.com:xamarin/mono-extensions.git'],
                         revision='ceae34ed8c7c6d1134f9cf643167d540a6204f86'
                         )
        self.source_dir_name = 'mono-extensions'

    def build(self):
        pass

    def install(self):
        pass

MonoExtensionsPackage()
