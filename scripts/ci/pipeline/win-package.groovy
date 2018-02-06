def isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
def monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
def isReleaseJob = (!isPr && monoBranch ==~ /201\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
def jobName = (isPr ? "build-package-win-mono-pullrequest" : "build-package-win-mono")
def macJobName = (isPr ? "build-package-osx-mono-pullrequest" : "build-package-osx-mono/${monoBranch}")
def packageFileNameX86 = null
def packageFileNameX64 = null
def commitHash = null
def utils = null

node ("w64") {
    ws ("workspace/${jobName}/${monoBranch}") {
        timestamps {
            stage('Checkout') {
                // clone and checkout repo
                checkout scm

                // get current commit sha
                commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                currentBuild.displayName = "${commitHash.substring(0,7)}"

                utils = load "scripts/ci/pipeline/utils.groovy"
            }
            stage('Download Mac .pkg from Azure') {
                step([
                    $class: 'AzureStorageBuilder',
                    downloadType: [value: 'project', containerName: '', projectName: "${macJobName}",
                                buildSelector: [$class: 'TriggeredBuildSelector', upstreamFilterStrategy: 'UseGlobalSetting', allowUpstreamDependencies: false, fallbackToLastSuccessful: false]],
                    includeFilesPattern: '**/*.pkg',
                    excludeFilesPattern: '',
                    downloadDirLoc: '',
                    flattenDirectories: true,
                    includeArchiveZips: false,
                    strAccName: 'credential for xamjenkinsartifact',
                    storageCredentialId: 'fbd29020e8166fbede5518e038544343'
                ])
            }
            try {
                stage('Build') {
                    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x86', env.BUILD_URL, 'PENDING', 'Building...')
                    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x64', env.BUILD_URL, 'PENDING', 'Building...')

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
                    step([
                        $class: 'WAStoragePublisher',
                        allowAnonymousAccess: true,
                        cleanUpContainer: false,
                        cntPubAccess: true,
                        containerName: "${jobName}",
                        doNotFailIfArchivingReturnsNothing: false,
                        doNotUploadIndividualFiles: false,
                        doNotWaitForPreviousBuild: true,
                        excludeFilesPath: '',
                        filesPath: "${packageFileNameX86},${packageFileNameX64}",
                        storageAccName: 'credential for xamjenkinsartifact',
                        storageCredentialId: 'fbd29020e8166fbede5518e038544343',
                        uploadArtifactsOnlyIfSuccessful: true,
                        uploadZips: false,
                        virtualPath: "${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/"
                    ])
                }

                if (isReleaseJob) {
                    stage("Signing") {
                        timeout(time: 30, unit: 'MINUTES') {
                            // waits until the signing job posts completion signal to this pipeline input
                            input id: 'FinishedSigning', message: 'Waiting for signing to finish...', submitter: 'monojenkins'
                            echo "Signing done."
                        }
                    }
                }
                else {
                    echo "Not a release job, skipping signing."
                }

                def packageUrlX86 = "https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/${packageFileNameX86}"
                def packageUrlX64 = "https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/${packageFileNameX64}";

                currentBuild.description = "<hr/><h2>DOWNLOAD: <a href=\"${packageUrlX86}\">${packageFileNameX86}</a> -- <a href=\"${packageUrlX64}\">${packageFileNameX64}</a></h2><hr/>"
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x86', packageUrlX86, 'SUCCESS', packageFileNameX86)
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x64', packageUrlX64, 'SUCCESS', packageFileNameX64)
            }
            catch (Exception e) {
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x86', env.BUILD_URL, 'FAILURE', "Build failed.")
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'MSI-mono_x64', env.BUILD_URL, 'FAILURE', "Build failed.")
                throw e
            }
        }
    }
}
