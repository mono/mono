isPrivate = false
isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
isReleaseJob = (!isPr && monoBranch ==~ /20\d\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
jobName = (isPr ? "build-package-osx-mono-pullrequest" : isPrivate ? "build-package-osx-mono-private" : "build-package-osx-mono")
windowsJobName = (isPr ? "build-package-win-mono-pullrequest" : isPrivate ? "build-package-win-mono-private/${monoBranch}" : "build-package-win-mono/${monoBranch}")
isWindowsPrBuild = (isPr && env.ghprbCommentBody.contains("@monojenkins build pkg and msi"))
packageFileName = null
commitHash = null
utils = null

if (monoBranch == 'main') {
    properties([ /* compressBuildLog() */  // compression is incompatible with JEP-210 right now
                pipelineTriggers([cron('0 3 * * *')])
    ])

    // multi-branch pipelines still get triggered for each commit, skip these builds on main by checking whether this build was timer-triggered or manually triggered
    if (currentBuild.getBuildCauses('hudson.triggers.TimerTrigger$TimerTriggerCause').size() == 0 && currentBuild.getBuildCauses('hudson.model.Cause$UserIdCause').size() == 0) {
        echo "Skipping per-commit build on main."
        return
    }
}

try {
    timestamps {
        node(isPr ? "mono-package-pr" : "mono-package") {
            ws("workspace/${jobName}/${monoBranch}") {
                stage('Checkout') {
                    echo "Running on ${env.NODE_NAME}"

                    // clone and checkout repo
                    checkout scm

                    // remove old stuff
                    sh 'git clean -xdff'

                    // get current commit sha
                    commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                    currentBuild.displayName = "${commitHash.substring(0,7)}"

                    utils = load "scripts/ci/pipeline/utils.groovy"
                }

                stage('Build') {
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

                    // move mac-entitlements.plist to the workspace root
                    sh 'mv mono/mini/mac-entitlements.plist .'
                }

                stage('Upload .pkg to Azure') {
                    azureUpload(storageCredentialId: (isPrivate ? "bc6a99d18d7d9ca3f6bf6b19e364d564" : "fbd29020e8166fbede5518e038544343"),
                                storageType: "blobstorage",
                                containerName: "${jobName}",
                                virtualPath: "${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/unsigned/",
                                filesPath: "${packageFileName},mac-entitlements.plist",
                                allowAnonymousAccess: (isPrivate ? false : true),
                                pubAccessible: (isPrivate ? false : true),
                                doNotWaitForPreviousBuild: true,
                                uploadArtifactsOnlyIfSuccessful: true)
                }

                sh 'git clean -xdff'
            }
        }

        def downloadHost = (isPrivate ? "dl.internalx.com" : "xamjenkinsartifact.azureedge.net")
        def packageUrl = "https://${downloadHost}/${jobName}/${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/unsigned"
        currentBuild.description = "<hr/><h2>DOWNLOAD: <a href=\"${packageUrl}/${packageFileName}\">${packageFileName}</a></h2><hr/>"
    }
}
catch (Exception e) {
    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'PKG-mono', env.BUILD_URL, 'FAILURE', "Build failed.")
    throw e
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
