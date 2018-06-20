import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = '9202c914597cf40b2407d11d7356e97c4100617a')

	def build (self):
		self.sh ('./build.sh -hostType mono -configuration Release -skipTests')
		self.sh ('zip msbuild-bin-logs.zip artifacts/Release-MONO/log/*')

	def install (self):
		# use the bootstrap msbuild as the system might not have one available!
		self.sh ('./artifacts/mono-msbuild/msbuild mono/build/install.proj /p:MonoInstallPrefix=%s /p:Configuration=Release-MONO /p:IgnoreDiffFailure=true' % self.staged_prefix)

MSBuild ()
