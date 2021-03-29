import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = 'd094b49f5b5095efe15685431275b9c38fa6d845')

	def build (self):
		try:
			self.sh ('./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release --skip_tests')
		finally:
			self.sh ('find artifacts stage1 -wholename \'*/log/*\' -type f -exec zip msbuild-bin-logs.zip {} \+')

	def install (self):
		# use the bootstrap msbuild as the system might not have one available!
		self.sh ('./stage1/mono-msbuild/msbuild mono/build/install.proj /p:MonoInstallPrefix=%s /p:Configuration=Release-MONO /p:IgnoreDiffFailure=true' % self.staged_prefix)

MSBuild ()
