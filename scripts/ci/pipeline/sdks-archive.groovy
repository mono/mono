isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
monoBranch = (isPr ? "pr" : env.BRANCH_NAME)
jobName = env.JOB_NAME.split('/').first()

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
            node ("debian-10-amd64-exclusive") {
                archive ("android", "debug", "Linux", "debian-10-amd64multiarchi386-preview", "g++-mingw-w64 gcc-mingw-w64 lib32stdc++6 lib32z1 libz-mingw-w64-dev linux-libc-dev:i386 zlib1g-dev zlib1g-dev:i386 zulu-8 rsync python3-pip", "/mnt/scratch")
            }
        }
    },
    "Android Linux (Release)": {
        throttle(['provisions-android-toolchain']) {
            node ("debian-10-amd64-exclusive") {
                archive ("android", "release", "Linux", "debian-10-amd64multiarchi386-preview", "g++-mingw-w64 gcc-mingw-w64 lib32stdc++6 lib32z1 libz-mingw-w64-dev linux-libc-dev:i386 zlib1g-dev zlib1g-dev:i386 zulu-8 rsync python3-pip", "/mnt/scratch")
            }
        }
    },
    "iOS (Xcode 11)": {
        throttle(['provisions-ios-toolchain']) {
            node ("xcode11") {
                archive ("ios", "release", "Darwin", "", "", "", "xcode11")
            }
        }
    },
    "Mac (Xcode 11)": {
        throttle(['provisions-mac-toolchain']) {
            node ("xcode11") {
                archive ("mac", "release", "Darwin", "", "", "", "xcode11")
            }
        }
    },
    "WASM Linux": {
        throttle(['provisions-wasm-toolchain']) {
            node ("ubuntu-1804-amd64") {
                archive ("wasm", "release", "Linux", "ubuntu-1804-amd64-preview", "npm dotnet-sdk-2.1 nuget openjdk-8-jre python3-pip")
            }
        }
    }
)

def archive (product, configuration, platform, chrootname = "", chrootadditionalpackages = "", chrootBindMounts = "", xcodeVersion = "") {
    def packageFileName = null
    def packageFileSha1 = null
    def commitHash = null
    def commitContext = (xcodeVersion == "" ? "Archive-${product}-${configuration}-${platform}" : "Archive-${product}-${configuration}-${platform}-${xcodeVersion}")
    def azureArtifactUrl = null
    def azureContainerName = "mono-sdks"
    def azureVirtualPath = null
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
                // homebrew Git 2.22.0 misparses the submodule command and passes arguments like --hard and -xdff
                // to git-submodule instead of to git-reset or git-clean.  Passing the entire command as a single
                // argument seems to help.
                sh 'git submodule foreach --recursive "git reset --hard HEAD"'
                sh 'git clean -xdff'
                sh 'git submodule foreach --recursive "git clean -xdff"'

                // get current commit sha
                commitHash = sh (script: 'git rev-parse HEAD', returnStdout: true).trim()
                currentBuild.displayName = "${commitHash.substring(0,7)}"
            }
            try {
                stage('Build') {
                    utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, commitContext, env.BUILD_URL, 'PENDING', 'Building...')

                    // build the Archive
                    timeout (time: 300, unit: 'MINUTES') {
                        if (platform == "Darwin") {
                            def brewpackages = "autoconf automake ccache cmake coreutils gdk-pixbuf gettext glib gnu-sed gnu-tar intltool ios-deploy jpeg libffi libidn2 libpng libtiff libtool libunistring ninja openssl p7zip pcre pkg-config scons wget xz mingw-w64 make xamarin/xamarin-android-windeps/mingw-zlib"
                            sh "brew tap xamarin/xamarin-android-windeps"
                            sh "brew install ${brewpackages} || brew upgrade ${brewpackages}"
                            sh "CI_TAGS=sdks-${product},no-tests,${configuration},${xcodeVersion} scripts/ci/run-jenkins.sh"
                        } else if (platform == "Linux") {
                            chroot chrootName: chrootname,
                                command: "CI_TAGS=sdks-${product},no-tests,${configuration} ANDROID_TOOLCHAIN_DIR=/mnt/scratch/android-toolchain ANDROID_TOOLCHAIN_CACHE_DIR=/mnt/scratch/android-archives scripts/ci/run-jenkins.sh",
                                bindMounts: chrootBindMounts,
                                additionalPackages: "xvfb xauth mono-devel git python wget bc build-essential libtool autoconf automake gettext iputils-ping cmake lsof libkrb5-dev curl p7zip-full ninja-build zip unzip gcc-multilib g++-multilib mingw-w64 binutils-mingw-w64 ${chrootadditionalpackages}"
                        } else if (platform == "Windows") {
                            sh "PATH=\"/usr/bin:/usr/local/bin:$PATH\" CI_TAGS=sdks-${product},win-amd64,no-tests,${configuration},${xcodeVersion} scripts/ci/run-jenkins.sh"
                        } else {
                            throw new Exception("Unknown platform \"${platform}\"")
                        }
                    }
                    // find Archive in the workspace root
                    packageFileName = findFiles (glob: "${product}-${configuration}-${platform}-${commitHash}.*")[0].name

                    // compute SHA1 of the Archive
                    packageFileSha1 = sha1 (packageFileName)
                    writeFile (file: "${packageFileName}.sha1", text: "${packageFileSha1}")

                    // include xcode version in virtual path if necessary
                    if (xcodeVersion == "") {
                        azureVirtualPath = ""
                        azureArtifactUrl = "https://xamjenkinsartifact.azureedge.net/${azureContainerName}/${packageFileName}"
                    } else {
                        azureVirtualPath = "xcode-" + readFile ("xcode_version.txt")
                        azureArtifactUrl = "https://xamjenkinsartifact.azureedge.net/${azureContainerName}/${azureVirtualPath}/${packageFileName}"
                    }
                }
                stage('Upload Archive to Azure') {
                    azureUpload(storageCredentialId: "fbd29020e8166fbede5518e038544343",
                                storageType: "blobstorage",
                                containerName: azureContainerName,
                                virtualPath: azureVirtualPath,
                                filesPath: "${packageFileName},${packageFileName}.sha1",
                                allowAnonymousAccess: true,
                                pubAccessible: true,
                                doNotWaitForPreviousBuild: true,
                                uploadArtifactsOnlyIfSuccessful: true)
                }

                sh 'git clean -xdff'

                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, commitContext, azureArtifactUrl, 'SUCCESS', packageFileName)
            }
            catch (Exception e) {
                utils.reportGitHubStatus (isPr ? env.ghprbActualCommit : commitHash, commitContext, env.BUILD_URL, 'FAILURE', "Build failed.")
                throw e
            }
        }
    }
}
