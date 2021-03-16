isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
isReleaseJob = (!isPr && monoBranch ==~ /20\d\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
jobName = (isPr ? "build-package-win-mono-pullrequest" : "build-package-win-mono")
macJobName = (isPr ? "build-package-osx-mono-pullrequest" : "build-package-osx-mono/${monoBranch}")
packageFileNameX86 = null
packageFileNameX64 = null
commitHash = null
utils = null

// compression is incompatible with JEP-210 right now
properties([ /* compressBuildLog() */])

try {
    timestamps {
        node("w64") {
            ws("workspace/${jobName}/${monoBranch}") {
                stage('Checkout') {
                    echo "Running on ${env.NODE_NAME}"

                    // clone and checkout repo
                    checkout scm

                    // we need to reset to the commit sha passed to us by the upstream Mac build
                    sh (script: "git reset --hard ${env.sha1} && git submodule update --recursive")

                    // get current commit sha
                    commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                    currentBuild.displayName = "${commitHash.substring(0,7)}"

                    utils = load "scripts/ci/pipeline/utils.groovy"
                }

                stage('Download Mac .pkg from Azure') {
                    azureDownload(storageCredentialId: "fbd29020e8166fbede5518e038544343",
                                downloadType: "project",
                                buildSelector: upstream(),
                                projectName: "${macJobName}",
                                flattenDirectories: true,
                                includeFilesPattern: "**/*.pkg")
                }

                stage('Build') {
                    // build the .msi
                    timeout (time: 420, unit: 'MINUTES') {
                        def macPackageName = sh (script: "ls MonoFramework-MDK-*.pkg", returnStdout: true).trim()
                        def macPackageNameAbs = sh (script: "realpath ${macPackageName}", returnStdout: true).trim()
                        def fourPartVersion =  sh (script: "echo ${macPackageName} | sed 's#.*MDK-##; s#\\.macos10.*##; s#.*-dirty-##' | cut -f1-4 -d'.'", returnStdout: true).trim()
                        def gtksharpVersion = sh (script: "sed -n 's/GTKSHARP_VERSION=\\(.*\\)/\\1/p' packaging/Windows/defs/gtksharp", returnStdout: true).trim()

                        sh "sed -i \"s/\\(GTKSHARP_VERSION=\\).*/\\1${gtksharpVersion}/\" packaging/Windows/resources/build.bat"
                        sh "sed -i \"s/\\(MONO_VERSION=\\).*/\\1${fourPartVersion}/\" packaging/Windows/resources/build.bat"
                        sh "sed -i \"s/\\(MONO_VERSION=\\).*/\\1${fourPartVersion}/\" packaging/Windows/resources/build64.bat"
                        sh "sed -i \"s/\\(echo Mono version \\).*/\\1${fourPartVersion}/\" packaging/Windows/resources/bat/setmonopath.bat"

                        withEnv (["PATH+TOOLS=/usr/bin:/usr/local/bin:/cygdrive/c/Program Files (x86)/Mono/bin", "mdk=${macPackageNameAbs}"]) {
                            // build x86 MSI
                            dir ('packaging/Windows') { sh "./mono-MDK-windows" }

                            sh "git clean -xdff --exclude 'mono-*.msi' --exclude ${macPackageName}"
                            sh "git submodule foreach git clean -xdff"

                            // build x64 MSI
                            dir ('packaging/Windows') { sh "./mono-MDK-windows-x64" }
                        }
                    }

                    // move .msi files to the workspace root
                    sh 'mv packaging/Windows/resources/bin/Release/mono-*.msi .'
                    packageFileNameX86 = findFiles (glob: "mono-*-win32-0.msi")[0].name
                    packageFileNameX64 = findFiles (glob: "mono-*-x64-0.msi")[0].name
                }
                stage('Upload .msi to Azure') {
                    azureUpload(storageCredentialId: "fbd29020e8166fbede5518e038544343",
                                storageType: "blobstorage",
                                containerName: "${jobName}",
                                virtualPath: "${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/unsigned/",
                                filesPath: "${packageFileNameX86},${packageFileNameX64}",
                                allowAnonymousAccess: true,
                                pubAccessible: true,
                                doNotWaitForPreviousBuild: true,
                                uploadArtifactsOnlyIfSuccessful: true)
                }

                sh 'git clean -xdff'
            }
        }

        def packageUrlX86 = "https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/unsigned/${packageFileNameX86}"
        def packageUrlX64 = "https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/unsigned/${packageFileNameX64}";

        currentBuild.description = "<hr/><h2>DOWNLOAD: <a href=\"${packageUrlX86}\">${packageFileNameX86}</a> -- <a href=\"${packageUrlX64}\">${packageFileNameX64}</a></h2><hr/>"
    }
}
catch (Exception e) {
    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x86', env.BUILD_URL, 'FAILURE', "Build failed.")
    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x64', env.BUILD_URL, 'FAILURE', "Build failed.")
    throw e
}
