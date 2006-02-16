cd c:\src\c5\src\C5\docNet\docbuild

copy ..\..\C5\bin\Debug\C5.dll W:\research\c5\current
copy ..\..\C5\bin\Debug\C5.pdb W:\research\c5\current

rem del "W:\research\c5\current\types\*.htm"

xcopy /Q *.htm W:\research\c5\current /S/Y
copy docnet.css W:\research\c5\current
pause
