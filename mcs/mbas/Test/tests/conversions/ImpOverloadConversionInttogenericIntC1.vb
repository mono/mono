'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Expected to call Long

Module ImpConversionInttogenericInt
	Function fun(Byval i as Short)
		return 2
	End Function
	Function fun(Byval i as Long)
		return 3
	End Function
	Function fun(Byval i as Byte)
		return 4
	End Function
	Sub Main()
		Dim i as Integer = 10
		i=fun(i)
		if i <> 3 then
			Throw new System.Exception("Implicit Conversion not working. Expected 3 but got " &i)
		End if
	
	End Sub
End Module
