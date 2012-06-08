
# git clone git://github.com/IronLanguages/main.git

rsync -r /home/marek/git/dlr/main/Runtime/Microsoft.Dynamic/ Runtime/Microsoft.Dynamic --exclude=".*/" --exclude="*.snk"
rsync -r /home/marek/git/dlr/main/Runtime/Microsoft.Scripting.Core/ Runtime/Microsoft.Scripting.Core --exclude=".*/" --exclude="*.snk"
