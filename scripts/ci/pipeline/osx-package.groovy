def isPrivate = (env.JENKINS_URL ==~ /.*jenkins\.internalx\.com.*/ ? true : false)
def isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
def monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
def isReleaseJob = (!isPr && monoBranch ==~ /201\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
def jobName = (isPr ? "build-package-osx-mono-pullrequest" : isPrivate ? "build-package-osx-mono-private" : "build-package-osx-mono")
def windowsJobName = (isPr ? "build-package-win-mono-pullrequest" : isPrivate ? "build-package-win-mono-private/${monoBranch}" : "build-package-win-mono/${monoBranch}")
def isWindowsPrBuild = (isPr && env.ghprbCommentBody.contains("@monojenkins build pkg and msi"))
def packageFileName = null
def commitHash = null
def utils = null
// compression is incompatible with JEP-210 right now
properties([ /* compressBuildLog() */ ])

node ("mono-package") {
    ws ("workspace/${jobName}/${monoBranch}") {
        timestamps {
            stage('Checkout') {
                // clone and checkout repo
                checkout scm

                // remove old stuff
                sh 'git clean -xdff'

                // get current commit sha
                commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                currentBuild.displayName = "${commitHash.substring(0,7)}"

                utils = load "scripts/ci/pipeline/utils.groovy"
            }
            try {
                stage('Build') {
                    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'PKG-mono', env.BUILD_URL, 'PENDING', 'Building...')

                    // workaround for libtiff issue
                    sh 'make -C external/bockbuild/builds/tiff-4.0.8-x86 clean || true'
                    sh 'make -C external/bockbuild/builds/tiff-4.0.8-x64 clean || true'

                    // build the .pkg
                    timeout (time: 420, unit: 'MINUTES') {
                        withEnv (["MONO_BRANCH=${isPr ? '' : monoBranch}", "MONO_BUILD_REVISION=${commitHash}"]) {
                            sshagent (credentials: ['mono-extensions-ssh']) {
                                sh "external/bockbuild/bb MacSDKRelease --arch darwin-universal --verbose --package ${isReleaseJob ? '--release' : ''}"
                            }
                        }
                    }

                    // move .pkg to the workspace root
                    sh 'mv packaging/MacSDKRelease/MonoFramework-MDK-*.pkg .'
                    packageFileName = findFiles (glob: "MonoFramework-MDK-*.pkg")[0].name
                }
                stage('Upload .pkg to Azure') {
                    azureUpload(storageCredentialId: (isPrivate ? "bc6a99d18d7d9ca3f6bf6b19e364d564" : "fbd29020e8166fbede5518e038544343"),
                                storageType: "blobstorage",
                                containerName: "${jobName}",
                                virtualPath: "${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/",
                                filesPath: "${packageFileName}",
                                allowAnonymousAccess: (isPrivate ? false : true),
                                pubAccessible: (isPrivate ? false : true),
                                doNotWaitForPreviousBuild: true,
                                uploadArtifactsOnlyIfSuccessful: true)
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

                def downloadHost = (isPrivate ? "dl.internalx.com" : "xamjenkinsartifact.azureedge.net")
                def packageUrl = "https://${downloadHost}/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}"
                currentBuild.description = "<hr/><h2>DOWNLOAD: <a href=\"${packageUrl}/${packageFileName}\">${packageFileName}</a></h2><hr/>"

                if (isReleaseJob) { utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'artifacts.json', "${packageUrl}/artifacts.json", 'SUCCESS', '') }
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'PKG-mono', "${packageUrl}/${packageFileName}", 'SUCCESS', packageFileName)
            }
            catch (Exception e) {
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'PKG-mono', env.BUILD_URL, 'FAILURE', "Build failed.")
                throw e
            }
        }
    }
}

if (isPrivate) {
    // skip Windows build on private jobs for now
    return
}

if (!isPr || isWindowsPrBuild) {
    def parameters = [[$class: 'StringParameterValue', name: 'sha1', value: commitHash]]

    if (isWindowsPrBuild) {
        parameters += [$class: 'StringParameterValue', name: 'ghprbPullId', value: env.ghprbPullId]
        parameters += [$class: 'StringParameterValue', name: 'ghprbActualCommit', value: env.ghprbActualCommit]
    }

    // trigger the Windows build
    build(job: "${windowsJobName}", wait: false, parameters: parameters)
}
