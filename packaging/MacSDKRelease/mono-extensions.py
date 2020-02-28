from bockbuild.package import Package


class MonoExtensionsPackage(Package):

    def __init__(self):
        Package.__init__(self, 'mono-extensions', None,
                         sources=['git@github.com:xamarin/mono-extensions.git'],
                         revision='7043820e923b3863218c8118a01601aec20551b6'
                         )
        self.source_dir_name = 'mono-extensions'

    def build(self):
        pass

    def install(self):
        pass

MonoExtensionsPackage()
