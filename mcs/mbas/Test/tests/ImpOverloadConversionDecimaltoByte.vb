'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoByte
	Function fun(Byval i as Byte)
		if i <> 10 then
			Throw new System.Exception("Implicit Conversion of Decimal to Byte not working. Expected 10 but got " &i)
		End if
	End Function
	Sub Main()
		Dim i as Decimal = 10
		fun(i)
		
	End Sub
End Module
