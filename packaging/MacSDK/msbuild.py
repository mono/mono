import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = 'b83c1ab3895c516cfe6de08b741c52dd6a514f6c')

	def build (self):
		self.sh ('./build.sh -host mono -configuration Release -skipTests')

	def install (self):
		# use the bootstrap msbuild as the system might not have one available!
                self.sh ('./artifacts/msbuild/msbuild mono/build/install.proj /p:MonoInstallPrefix=%s /p:Configuration=Release-MONO /p:IgnoreDiffFailure=true' % self.staged_prefix)

MSBuild ()
