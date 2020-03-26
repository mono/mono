from bockbuild.package import Package


class MonoExtensionsPackage(Package):

    def __init__(self):
        Package.__init__(self, 'mono-extensions', None,
                         sources=['git@github.com:xamarin/mono-extensions.git'],
                         revision='a72d7d064515cab3f0ecc67807de6fcaf6f8db01'
                         )
        self.source_dir_name = 'mono-extensions'

    def build(self):
        pass

    def install(self):
        pass

MonoExtensionsPackage()
