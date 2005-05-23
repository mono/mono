'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	Public Function fun(ByRef i as integer, ByRef j As Integer)
		i = 9
		j = 10
	End Function
End Class

Module M
        Sub Main()
		dim o as Object = new C()
		dim a as integer = 1
		dim err As String = ""
		o.fun(a, a)
		if  a <> 9 then
			err += "#A1 binding not proper. Expected '9' but got '" & a & "'" & vbCrLf
		End If
		o.fun(i:=a, j:=a)
		if  a <> 9 then
			err += "#A2 binding not proper. Expected '9' but got '" & a & "'" & vbCrLf
		End If
		o.fun(j:=a, i:=a)
		if  a <> 10 then
			err += "#A3 binding not proper. Expected '10' but got '" & a & "'" & vbCrLf
		End If
		if (err <> "")
			throw new System.Exception (err)
		End IF
		
        End Sub
End Module
