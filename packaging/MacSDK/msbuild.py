import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = 'a69c2a1ae7c06c3ac4593bf192500279a2e05463')

	def build (self):
		self.sh ('./build.sh -host mono -configuration Release -skipTests')

	def install (self):
		# use the bootstrap msbuild as the system might not have one available!
                self.sh ('./artifacts/msbuild/msbuild mono/build/install.proj /p:MonoInstallPrefix=%s /p:Configuration=Release-MONO' % self.staged_prefix)

MSBuild ()
