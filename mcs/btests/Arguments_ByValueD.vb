'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Value:
'APV-1.0.0: If the variable elements is of reference type i.e. it contain a pointers to a class
'		then procedure can change the members of instance to which it points  
'==============================================================================================

Imports System
Imports System.Array
Module APV1_0

	Public Sub Increase(ByVal A() As Long)
    		Dim J As Integer
    		For J = 0 To 3
       		A(J) = A(J) + 1
    		Next J
	End Sub
   ' ...
	Public Sub Replace(ByVal A() As Long)
   		Dim J As Integer
   		Dim K() As Long = {100, 200, 300, 400}
   		A = K
   		For J = 0 To 3
      		A(J) = A(J) + 1
   		Next J
	End Sub
 ' ...
 	
   Sub Main()
      Dim N() As Long = {10, 20, 30, 40}
	Dim N1() As Long = {11, 21, 31, 41}
	Dim N2() As Long = {100, 200, 300, 400}
	Dim i As Integer
	Increase(N)
	For i=0 To 3
		if(N(i)<>N1(i))
		Throw new System.Exception ("#A1, Unexpected behavior in Increase function")
		end if
	Next i
	i=0	
	Replace(N)
	For i=0 To 3
		if(N(i)=N2(i))
		Throw new System.Exception ("#A2, Unexpected behavior in Replace function")
		end if
	Next i
   End Sub 
End Module
'============================================================================================