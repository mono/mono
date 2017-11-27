def monoBranch = env.BRANCH_NAME
def isReleaseJob = (monoBranch ==~ /201\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
def jobName = "build-package-win-mono"
def macJobName = "build-package-osx-mono"
def commitHash = null

node ("w64") {
    ws ("workspace/${jobName}/${monoBranch}") {
        timestamps {
            stage('Checkout') {
                // clone and checkout repo
                checkout scm

                // get current commit sha
                commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                currentBuild.displayName = "${commitHash.substring(0,7)}"
            }
            stage('Download Mac .pkg from Azure') {
                step([
                    $class: 'AzureStorageBuilder',
                    downloadType: [value: 'project', containerName: '', projectName: "${macJobName}/${monoBranch}",
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
            stage('Build') {
                // build the .msi
                timeout (time: 420, unit: 'MINUTES') {
                    def macPackageName = sh (script: "ls MonoFramework-MDK-*.pkg", returnStdout: true).trim()
                    def macPackageNameAbs = sh (script: "realpath ${macPackageName}", returnStdout: true).trim()
                    def threePartVersion =  sh (script: "echo ${macPackageName} | sed 's#.*MDK-##; s#\\.macos10.*##; s#.*-dirty-##' | cut -f1-3 -d'.'", returnStdout: true).trim()

                    sh "sed -i \"s/\\(MONO_VERSION=\\).*/\\1${threePartVersion}/\" packaging/Windows/resources/build.bat"
                    sh "sed -i \"s/\\(MONO_VERSION=\\).*/\\1${threePartVersion}/\" packaging/Windows/resources/build64.bat"
                    sh "sed -i \"s/\\(echo Mono version \\).*/\\1${threePartVersion}/\" packaging/Windows/resources/bat/setmonopath.bat"

                    withEnv (["PATH+TOOLS=/usr/bin:/usr/local/bin:/cygdrive/c/Program Files (x86)/Mono/bin", "mdk=${macPackageNameAbs}"]) {
                        // build x86 MSI
                        dir ('packaging/Windows') { sh "./mono-MDK-windows" }

                        sh "git clean -xdff --exclude MonoForWindows-x86.msi --exclude ${macPackageName}"
                        sh "git submodule foreach git clean -xdff"

                        // build x64 MSI
                        dir ('packaging/Windows') { sh "./mono-MDK-windows-x64" }
                    }
                }

                // move .msi files to the workspace root
                sh 'mv packaging/Windows/resources/bin/Release/MonoForWindows-x86.msi .'
                sh 'mv packaging/Windows/resources/bin/Release/MonoForWindows-x64.msi .'
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
                    filesPath: "MonoForWindows-x86.msi,MonoForWindows-x64.msi",
                    storageAccName: 'credential for xamjenkinsartifact',
                    storageCredentialId: 'fbd29020e8166fbede5518e038544343',
                    uploadArtifactsOnlyIfSuccessful: true,
                    uploadZips: false,
                    virtualPath: "${monoBranch}/${env.BUILD_NUMBER}/"
                ])
                currentBuild.description = "<hr/><h2>DOWNLOAD: <a href=\"https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/MonoForWindows-x86.msi\">MonoForWindows-x86.msi</a> -- <a href=\"https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/MonoForWindows-x64.msi\">MonoForWindows-x64.msi</a></h2><hr/>"
            }
        }
    }
}
