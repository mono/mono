# Authors:
# 	Roopa Wilson (rowilson@novell.com)

#This script executes the exe files and displays the results in a web page

echo "" > exec_log_linux.html 

echo "<html><head><title> Execution Results in Linux</title></head><body><h4>Execution Results in Linux</h4><table border=1><th>S.No</th><th>Test Case</th><th>Result</th>" 2>&1 >> exec_log_linux.html 2>&1

count=1;
for file in *.exe
do
	echo Executing $file...
	echo "<tr><td>$count</td>" 2>&1 >> exec_log_linux.html 2>&1
	mono $file
	if [ $? -eq 0 ] 
		then echo "<td>$file</td><td>Executed OK</td></tr>" 2>&1 >> exec_log_linux.html 2>&1
	else
		echo "<td>$file</td><td>Execution Failed</td></tr>" 2>&1 >> exec_log_linux.html 2>&1
	fi
	count=`expr $count + 1`
done

echo "</table></body></html>" 2>&1 >> exec_log_linux.html 2>&1
