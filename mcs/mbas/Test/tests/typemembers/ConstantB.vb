Class A
	public const c as integer = 10
End Class

Class B
	Inherits A

	public shadows Const c as integer = 20
end class

Module M
	Sub Main()
	End Sub
End Module

