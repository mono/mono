Module ImpConversionofBytetoBool
	Sub Main()
		Dim a as Byte = 123
		Dim b as Boolean = a
		if b <> True then 
			Throw New System.Exception("Implicit Conversion of Byte to Bool(True) has Failed. Expected True got " & b)
		End if	
	End Sub
End Module
