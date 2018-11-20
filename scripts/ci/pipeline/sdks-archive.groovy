properties([compressBuildLog()])

parallel (
    "Archive-android-debug-Darwin": {
        node ("osx-devices") {
            archive ("android", "debug", "Darwin")
        }
    },
    "Archive-android-release-Darwin": {
        node ("osx-devices") {
            archive ("android", "release", "Darwin")
        }
    },
    // "Archive-android-debug-Linux": {
    //     node ("debian-9-amd64multiarchi386-preview") {
    //         archive ("android", "debug", "Linux",)
    //     }
    // },
    // "Archive-android-release-Linux": {
    //     node ("debian-9-amd64multiarchi386-preview") {
    //         archive ("android", "release", "Linux",)
    //     }
    // },
    "Archive-ios-release-Darwin": {
        node ("osx-devices") {
            archive ("ios", "release", "Darwin")
        }
    },
    "Archive-wasm-release-Linux": {
        node ("ubuntu-1804-amd64") {
            archive ("wasm", "release", "Linux", "ubuntu-1804-amd64-preview", "npm dotnet-sdk-2.1 nuget")
        }
    }
)

def archive (product, configuration, platform, chrootname = "", chrootadditionalpackages = "") {
    def isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
    def monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
    def jobName = (isPr ? "archive-mono-pullrequest" : "archive-mono")
    def packageFileName = null
    def commitHash = null
    def utils = null

    ws ("workspace/${jobName}/${monoBranch}/${product}/${configuration}") {
        timestamps {
            stage('Checkout') {
                // clone and checkout repo
                checkout scm

                utils = load "scripts/ci/pipeline/utils.groovy"

                // remove old stuff
                sh 'git reset --hard HEAD'
                sh 'git submodule foreach --recursive git reset --hard HEAD'
                sh 'git clean -xdff'
                sh 'git submodule foreach --recursive git clean -xdff'

                // get current commit sha
                commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                currentBuild.displayName = "${commitHash.substring(0,7)}"
            }
            try {
                stage('Build') {
                    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, "Archive-${product}-${configuration}-${platform}", env.BUILD_URL, 'PENDING', 'Building...')

                    // build the Archive
                    timeout (time: 300, unit: 'MINUTES') {
                        lock ("${product}-${env.NODE_NAME}") {
                            if (platform == "Darwin") {
                                def brewpackages = "autoconf automake ccache cmake coreutils gdk-pixbuf gettext glib gnu-sed gnu-tar intltool ios-deploy jpeg libffi libidn2 libpng libtiff libtool libunistring ninja openssl p7zip pcre pkg-config scons wget xz"
                                sh "brew install ${brewpackages} || brew upgrade ${brewpackages}"

                                sh "CI_TAGS=sdks-${product},no-tests,${configuration} scripts/ci/run-jenkins.sh"
                            } else if (platform == "Linux") {
                                chroot chrootName: chrootname,
                                    command: "CI_TAGS=sdks-${product},no-tests,${configuration} scripts/ci/run-jenkins.sh",
                                    additionalPackages: "xvfb xauth mono-devel git python wget bc build-essential libtool autoconf automake gettext iputils-ping cmake lsof libkrb5-dev curl p7zip-full ninja-build zip unzip gcc-multilib g++-multilib mingw-w64 binutils-mingw-w64 openjdk-8-jre ${chrootadditionalpackages}"
                            } else {
                                throw new Exception("Unknown platform \"${platform}\"")
                            }
                        }
                    }
                    // move Archive to the workspace root
                    packageFileName = findFiles (glob: "${product}-${configuration}-${platform}-${commitHash}.zip")[0].name
                }
                stage('Upload Archive to Azure') {
                    azureUpload(storageCredentialId: "fbd29020e8166fbede5518e038544343",
                                storageType: "blobstorage",
                                containerName: "mono-sdks",
                                virtualPath: "",
                                filesPath: "${packageFileName}",
                                allowAnonymousAccess: true,
                                pubAccessible: true,
                                doNotWaitForPreviousBuild: true,
                                uploadArtifactsOnlyIfSuccessful: true)
                }

                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, "Archive-${product}-${configuration}-${platform}", "https://xamjenkinsartifact.azureedge.net/mono-sdks/${packageFileName}", 'SUCCESS', packageFileName)
            }
            catch (Exception e) {
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, "Archive-${product}-${configuration}-${platform}", env.BUILD_URL, 'FAILURE', "Build failed.")
                throw e
            }
        }
    }
}
