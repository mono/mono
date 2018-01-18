import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',  # note: fix scripts/ci/run-test-mac-sdk.sh when bumping the version number
			revision = 'ca830585a017dcf55083f9eb6b0cdb6fbc61ed84')

	def build (self):
		self.sh ('./cibuild.sh --scope Compile --target Mono --host Mono --config Release')

	def install (self):
                self.sh ('./install-mono-prefix.sh %s' % self.staged_prefix)

MSBuild ()
