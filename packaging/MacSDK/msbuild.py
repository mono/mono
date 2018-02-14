import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'Therzok', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = '1840b6732294690a3aa92ab86a21c619c563f486')

	def build (self):
		self.sh ('./cibuild.sh --scope Compile --target Mono --host Mono --config Release')

	def install (self):
                self.sh ('./install-mono-prefix.sh %s' % self.staged_prefix)

MSBuild ()
