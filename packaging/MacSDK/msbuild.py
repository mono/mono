import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = 'db0fb4f6c1210aae62b431deaa6673431eb678de')

	def build (self):
		self.sh ('./build.sh -host mono -configuration Release -skipTests')

	def install (self):
		# use the bootstrap msbuild as the system might not have one available!
                self.sh ('./artifacts/msbuild/msbuild mono/build/install.proj /p:MonoInstallPrefix=%s /p:Configuration=Release-MONO /p:IgnoreDiffFailure=true' % self.staged_prefix)

MSBuild ()
