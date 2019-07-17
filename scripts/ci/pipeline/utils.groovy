def reportGitHubStatus(commitHash, context, backref, statusResult, statusResultMessage) {
    node("master") {
        step([
            $class: 'GitHubCommitStatusSetter',
            commitShaSource: [$class: "ManuallyEnteredShaSource", sha: commitHash],
            contextSource: [$class: 'ManuallyEnteredCommitContextSource', context: context],
            statusBackrefSource: [$class: 'ManuallyEnteredBackrefSource', backref: backref],
            statusResultSource: [$class: 'ConditionalStatusResultSource', results: [[$class: 'AnyBuildResult', state: statusResult, message: statusResultMessage]]]
        ])
    }
}

return this
