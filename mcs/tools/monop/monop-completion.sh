# function for monop bash completion, source it from other scripts like .bashrc

completion_monop()
{
    local sw
    cur=${COMP_WORDS[COMP_CWORD]}
    [ "$cur" ] && COMPREPLY=($( compgen -W "`monop -c ${cur}`" -- $cur ))
}
complete -F completion_monop monop
