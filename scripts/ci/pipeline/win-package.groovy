def monoBranch = env.BRANCH_NAME
def isReleaseJob = (monoBranch ==~ /201\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
def jobName = "build-package-win-mono"
def macJobName = "build-package-osx-mono"
def packageFileNameX86 = null
def packageFileNameX64 = null
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
                packageFileNameX86 = findFiles (glob: "mono-*-win32-0.msi")[0]
                packageFileNameX64 = findFiles (glob: "mono-*-x64-0.msi")[0]
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

            currentBuild.description = "<hr/><h2>DOWNLOAD: <a href=\"https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/${packageFileNameX86}\">${packageFileNameX86}</a> -- <a href=\"https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/${packageFileNameX64}\">${packageFileNameX64}</a></h2><hr/>"
            step([
                $class: 'GitHubCommitStatusSetter',
                commitShaSource: [$class: "ManuallyEnteredShaSource", sha: commitHash],
                contextSource: [$class: 'ManuallyEnteredCommitContextSource', context: 'MSI-mono_x86'],
                statusBackrefSource: [$class: 'ManuallyEnteredBackrefSource', backref: "https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/${packageFileNameX86}"],
                statusResultSource: [$class: 'ConditionalStatusResultSource', results: [[$class: 'AnyBuildResult', state: 'SUCCESS', message: "${packageFileNameX86}"]]]
            ])
            step([
                $class: 'GitHubCommitStatusSetter',
                commitShaSource: [$class: "ManuallyEnteredShaSource", sha: commitHash],
                contextSource: [$class: 'ManuallyEnteredCommitContextSource', context: 'MSI-mono_x64'],
                statusBackrefSource: [$class: 'ManuallyEnteredBackrefSource', backref: "https://xamjenkinsartifact.azureedge.net/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/${packageFileNameX64}"],
                statusResultSource: [$class: 'ConditionalStatusResultSource', results: [[$class: 'AnyBuildResult', state: 'SUCCESS', message: "${packageFileNameX64}"]]]
            ])
        }
    }
}
