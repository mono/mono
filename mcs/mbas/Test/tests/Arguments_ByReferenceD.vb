'==============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Reference:
'APR-1.3.0: If the variable elements is of reference type i.e. it contain a pointers to a class
'		then procedure can change the members of instance to which it points  
'==============================================================================================

Imports System
Imports System.Array
Module APR_1_3_0

	Public Sub Increase(ByRef A() As Long)
    		Dim J As Integer
    		For J = 0 To 3
       		A(J) = A(J) + 1
    		Next J
	End Sub
   ' ...
	Public Sub Replace(ByRef A() As Long)
   		Dim J As Integer
   		Dim K() As Long = {100, 200, 300,400}
   		A = K
   		For J = 0 To 3
      		A(J) = A(J) + 1
   		Next J
	End Sub
 ' ...
 	
   Sub Main()
      Dim N() As Long = {10, 20, 30, 40}
	Dim N1() As Long = {11, 21, 31, 41}
	Dim N2() As Long = {101, 201, 301, 401}
	Dim i As Integer
	Increase(N)
	For i = 0 to 3
	if (N(i) <> N1(i))
		Throw New System.Exception("#A1, Unexception Behaviour in Increase Function")
	end if
	Next i
	Replace(N)
	For i= 0 to 3
	if ( N(i) <> N2(i))
		Throw New System.Exception("#A2, Unexception Behaviour in Increase Function")
	end if
	Next i

   End Sub 
End Module

'==============================================================================================