Module ExpConversionDoubletoBool
	Sub Main()
		Dim a As Single = -4.940656E-12
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Double to Boolean Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
