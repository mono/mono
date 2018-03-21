
node('debian-9-amd64') {
    timestamps {
        stage ('Checkout') {
            git branch: "${sha}", url: 'git://github.com/mono/mono.git'

            sh "git reset --hard ${sha}"
            sh "git clean -xffd"

            sh "git submodule update --recursive --init"
            sh "git submodule foreach --recursive git reset --hard HEAD"
            sh "git submodule foreach --recursive git clean -xffd"
        }
        stage ('Run') {
            chroot chrootName: 'debian-9-amd64-stable', command: 'scripts/ci/run-jenkins.sh', additionalPackages: 'xvfb xauth mono-devel git python wget bc build-essential libtool autoconf automake gettext iputils-ping cmake lsof'
        }
        stage ('NUnit') {
            step([
                $class: 'XUnitBuilder',
                testTimeMargin: '3000',
                thresholdMode: 1,
                thresholds: [
                    [$class: 'FailedThreshold', failureNewThreshold: '99', failureThreshold: '99', unstableNewThreshold: '0', unstableThreshold: '0'],
                    [$class: 'SkippedThreshold', failureNewThreshold: '2000', failureThreshold: '2000', unstableNewThreshold: '2000', unstableThreshold: '2000']],
                tools: [
                    [$class: 'NUnitJunitHudsonTestType', deleteOutputFiles: true, failIfNotNew: true, pattern: 'mcs/class/**/TestResult*.xml, mono/**/TestResult*.xml, runtime/**/TestResult*.xml, acceptance-tests/**/TestResult*.xml', skipNoTestFiles: true, stopProcessingIfError: true]]
            ])
        }
    }
}
