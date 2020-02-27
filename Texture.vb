Public Structure Texture
    Public Name As String
    Dim Bitmap As Memory_Bitmap
    Dim Width, Height As Integer

    Public Sub New(ByVal texture_name As String,
                   ByVal texture_bitmap As Memory_Bitmap)
        Name = texture_name
        Bitmap = texture_bitmap
        Width = Bitmap.Width
        Height = Bitmap.Height
    End Sub

    Public ReadOnly Property P(ByVal u As Single,
                               ByVal v As Single) As Color
        Get
            Return Bitmap.GetPixel(u * (Width - 2) + 1, v * (Height - 2) + 1)
        End Get
    End Property

End Structure