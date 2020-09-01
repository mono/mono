isPrivate = (env.JENKINS_URL ==~ /.*jenkins\.internalx\.com.*/ ? true : false)
isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
isReleaseJob = (!isPr && monoBranch ==~ /20\d\d-\d\d/) // check if we're on a 2017-xx branch, i.e. release
jobName = (isPr ? "build-package-osx-mono-pullrequest" : isPrivate ? "build-package-osx-mono-private" : "build-package-osx-mono")
windowsJobName = (isPr ? "build-package-win-mono-pullrequest" : isPrivate ? "build-package-win-mono-private/${monoBranch}" : "build-package-win-mono/${monoBranch}")
isWindowsPrBuild = (isPr && env.ghprbCommentBody.contains("@monojenkins build pkg and msi"))
packageFileName = null
commitHash = null
utils = null

if (monoBranch == 'master') {
    properties([ /* compressBuildLog() */  // compression is incompatible with JEP-210 right now
                pipelineTriggers([cron('0 3 * * *')])
    ])

    // multi-branch pipelines still get triggered for each commit, skip these builds on master by checking whether this build was timer-triggered
    if (currentBuild.getBuildCauses('hudson.triggers.TimerTrigger$TimerTriggerCause').size() == 0) {
        echo "Skipping per-commit build on master."
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
                    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, 'PKG-mono', env.BUILD_URL, 'PENDING', 'Building...')

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
                                virtualPath: "${monoBranch}/${env.BUILD_NUMBER}/${commitHash}/",
                                filesPath: "${packageFileName},mac-entitlements.plist",
                                allowAnonymousAccess: (isPrivate ? false : true),
                                pubAccessible: (isPrivate ? false : true),
                                doNotWaitForPreviousBuild: true,
                                uploadArtifactsOnlyIfSuccessful: true)
                }

                sh 'git clean -xdff'
            }
        }

        if (isReleaseJob) {
            stage("Signing") {
                timeout(time: 90, unit: 'MINUTES') {
                    // waits until the signing job posts completion signal to this pipeline input
                    input id: 'FinishedSigning', message: 'Waiting for signing to finish (please be patient)...', submitter: 'monojenkins'
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
