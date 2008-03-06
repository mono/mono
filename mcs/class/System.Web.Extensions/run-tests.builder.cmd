echo on
setlocal
set JRE_HOME=%VMW_HOME%\jre
set CATALINA_HOME=%VMW_HOME%\jakarta-tomcat
set JAVA_OPTS=-client -Xmx512m -XX:MaxPermSize=256m -Djava.awt.headless=true

call %CATALINA_HOME%\bin\startup.bat

echo Waiting 10 sec for tomcat to start....
@ping 127.0.0.1 -n 10 -w 1000 > nul

call run-tests.cmd 

%CATALINA_HOME%\bin\shutdown.bat
endlocal
