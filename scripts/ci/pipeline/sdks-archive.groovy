// compression is incompatible with JEP-210 right now
properties([/* compressBuildLog() */])

parallel (
    "Android Darwin (Debug)": {
        throttle(['provisions-android-toolchain']) {
            node ("osx-devices") {
                archive ("android", "debug", "Darwin")
            }
        }
    },
    "Android Darwin (Release)": {
        throttle(['provisions-android-toolchain']) {
            node ("osx-devices") {
                archive ("android", "release", "Darwin")
            }
        }
    },
    "Android Windows (Release)": {
        throttle(['provisions-android-toolchain']) {
            node ("w64") {
                archive ("android", "release", "Windows")
            }
        }
    },
    "Android Linux (Debug)": {
        throttle(['provisions-android-toolchain']) {
            node ("debian-9-amd64-exclusive") {
                archive ("android", "debug", "Linux", "debian-9-amd64multiarchi386-preview", "g++-mingw-w64 gcc-mingw-w64 lib32stdc++6 lib32z1 libz-mingw-w64-dev linux-libc-dev:i386 zlib1g-dev zlib1g-dev:i386", "${env.HOME}")
            }
        }
    },
    "Android Linux (Release)": {
        throttle(['provisions-android-toolchain']) {
            node ("debian-9-amd64-exclusive") {
                archive ("android", "release", "Linux", "debian-9-amd64multiarchi386-preview", "g++-mingw-w64 gcc-mingw-w64 lib32stdc++6 lib32z1 libz-mingw-w64-dev linux-libc-dev:i386 zlib1g-dev zlib1g-dev:i386", "${env.HOME}")
            }
        }
    },
    "iOS": {
        throttle(['provisions-ios-toolchain']) {
            node ("osx-devices") {
                archive ("ios", "release", "Darwin")
            }
        }
    },
    "Mac": {
        throttle(['provisions-mac-toolchain']) {
            node ("osx-devices") {
                archive ("mac", "release", "Darwin")
            }
        }
    },
    "WASM Linux": {
        throttle(['provisions-wasm-toolchain']) {
            node ("ubuntu-1804-amd64") {
                archive ("wasm", "release", "Linux", "ubuntu-1804-amd64-preview", "npm dotnet-sdk-2.1 nuget")
            }
        }
    }
)

def archive (product, configuration, platform, chrootname = "", chrootadditionalpackages = "", chrootBindMounts = "") {
    def isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
    def monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
    def jobName = (isPr ? "archive-mono-pullrequest" : "archive-mono")
    def packageFileName = null
    def commitHash = null
    def utils = null

    ws ("workspace/${jobName}/${monoBranch}/${product}/${configuration}") {
        timestamps {
            stage('Checkout') {
                echo "Running on ${env.NODE_NAME}"

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
                        if (platform == "Darwin") {
                            def brewpackages = "autoconf automake ccache cmake coreutils gdk-pixbuf gettext glib gnu-sed gnu-tar intltool ios-deploy jpeg libffi libidn2 libpng libtiff libtool libunistring ninja openssl p7zip pcre pkg-config scons wget xz mingw-w64 make xamarin/xamarin-android-windeps/mingw-zlib"
                            sh "brew tap xamarin/xamarin-android-windeps"
                            sh "brew install ${brewpackages} || brew upgrade ${brewpackages}"
                            sh "CI_TAGS=sdks-${product},no-tests,${configuration} scripts/ci/run-jenkins.sh"
                        } else if (platform == "Linux") {
                            chroot chrootName: chrootname,
                                command: "CI_TAGS=sdks-${product},no-tests,${configuration} scripts/ci/run-jenkins.sh",
                                bindMounts: chrootBindMounts,
                                additionalPackages: "xvfb xauth mono-devel git python wget bc build-essential libtool autoconf automake gettext iputils-ping cmake lsof libkrb5-dev curl p7zip-full ninja-build zip unzip gcc-multilib g++-multilib mingw-w64 binutils-mingw-w64 openjdk-8-jre ${chrootadditionalpackages}"
                        } else if (platform == "Windows") {
                            sh "PATH=\"/usr/bin:/usr/local/bin:$PATH\" CI_TAGS=sdks-${product},win-amd64,no-tests,${configuration} scripts/ci/run-jenkins.sh"
                        } else {
                            throw new Exception("Unknown platform \"${platform}\"")
                        }
                    }
                    // find Archive in the workspace root
                    packageFileName = findFiles (glob: "${product}-${configuration}-${platform}-${commitHash}.*")[0].name
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
