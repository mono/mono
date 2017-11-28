import fileinput

class MSBuild (GitHubPackage):
	def __init__ (self):
		GitHubPackage.__init__ (self, 'mono', 'msbuild', '15',
			revision = '5cf9eb2cc748907924342b32a3eefa78bb325de8')

	def build (self):
		self.sh ('./cibuild.sh --scope Compile --target Mono --host Mono --config Release')

	def install (self):
                self.sh ('./install-mono-prefix.sh %s' % self.staged_prefix)

MSBuild ()
