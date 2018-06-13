import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = 'd375bb6ec5b2157275e2a8a83c318a3f4e28ea93')

	def build (self):
		self.sh ('./build.sh -host mono -configuration Release -skipTests')
		self.sh ('zip msbuild-bin-logs.zip artifacts/Release-MONO/log/*')

	def install (self):
                # use the bootstrap msbuild as the system might not have one available!
                self.sh ('./artifacts/mono-msbuild/msbuild mono/build/install.proj /p:MonoInstallPrefix=%s /p:Configuration=Release-MONO /p:IgnoreDiffFailure=true' % self.staged_prefix)

MSBuild ()
