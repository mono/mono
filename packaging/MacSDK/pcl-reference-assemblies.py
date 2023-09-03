import glob
import os
import shutil


class PCLReferenceAssembliesPackage(Package):

    def __init__(self):
        Package.__init__(self,
                         name='PortableReferenceAssemblies',
                         version='2014-04-14',
                         sources=['https://github.com/mono/reference-assemblies/releases/download/v4.8/PortableReferenceAssemblies-2014-04-14.zip'])

    def build(self):
        pass

    # A bunch of shell script written inside python literals ;(
    def install(self):
        dest = os.path.join(
            self.staged_prefix,
            "lib",
            "mono",
            "xbuild-frameworks",
            ".NETPortable")
        if not os.path.exists(dest):
            os.makedirs(dest)

        shutil.rmtree(dest, ignore_errors=True)

        self.sh(f"rsync -abv -q {self.workspace}/* {dest}")

        for f in glob.glob(f"{dest}/*/Profile/*/SupportedFrameworks"):
            self.write_xml(f)

    def write_xml(self, directory):
        # print "Writing iOS/Android/Mac listings for " + directory
        data = {
            os.path.join(directory, "MonoTouch.xml"):
            """<Framework Identifier="MonoTouch" MinimumVersion="1.0" Profile="*" DisplayName="Xamarin.iOS Classic"/>""",
            os.path.join(directory, "Xamarin.iOS.xml"):
            """<Framework Identifier="Xamarin.iOS" MinimumVersion="1.0" Profile="*" DisplayName="Xamarin.iOS Unified"/>""",
            os.path.join(directory, "Xamarin.Android.xml"):
            """<Framework Identifier="MonoAndroid" MinimumVersion="1.0" Profile="*" DisplayName="Xamarin.Android"/>""",
            os.path.join(directory, "Xamarin.Mac.xml"):
            """<Framework Identifier="Xamarin.Mac" MinimumVersion="2.0" Profile="*" DisplayName="Xamarin.Mac Unified"/>""",
            os.path.join(directory, "Xamarin.TVOS.xml"):
            """<Framework Identifier="Xamarin.TVOS" MinimumVersion="1.0" Profile="*" DisplayName="Xamarin.TVOS"/>""",
            os.path.join(directory, "Xamarin.WatchOS.xml"):
            """<Framework Identifier="Xamarin.WatchOS" MinimumVersion="1.0" Profile="*" DisplayName="Xamarin.WatchOS"/>""",
        }
        for filename, content in data.iteritems():
            with open(filename, "w") as f:
                f.write(content + "\n")


PCLReferenceAssembliesPackage()
