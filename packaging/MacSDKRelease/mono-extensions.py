from bockbuild.package import Package


class MonoExtensionsPackage(Package):

    def __init__(self):
        Package.__init__(self, 'mono-extensions', None,
                         sources=['git@github.com:xamarin/mono-extensions.git'],
                         revision='07ad37d63e0e9dcf7c879a72bc14c5d6c794f7b6'
                         )
        self.source_dir_name = 'mono-extensions'

    def build(self):
        pass

    def install(self):
        pass

MonoExtensionsPackage()
