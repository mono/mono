Module ExpConversionBoolToByte
	Sub Main()
		Dim a as Boolean = True
		Dim b as Byte = CByte(a)
		if b <> 255
			Throw new System.Exception("Boolean to Byte Conversion failed. Expected 255 but got " & b)
		End if	
	End Sub
End Module