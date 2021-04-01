class XamarinGtkThemePackage (Package):

    def __init__(self):
        Package.__init__(self, 'xamarin-gtk-theme',
                         sources=[
                             'git://github.com/mono/xamarin-gtk-theme.git'],
                         revision='fa8ba3e38edb070eb8b0a70be64f9c10f9b523c2')

    def build(self):
        try:
            self.sh('./autogen.sh --prefix=%{staged_prefix}')
        except:
            pass
        finally:
            #self.sh ('intltoolize --force --copy --debug')
            #self.sh ('./configure --prefix="%{package_prefix}"')
            Package.build(self)


XamarinGtkThemePackage()
