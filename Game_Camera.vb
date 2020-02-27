Public Class Game_Camera
    Public Camera_Object As Game_Object
    Public Height As Double
    Public Distance As Double = 10
    Public Min_Distance As Double = 0
    Public Matrix As MAT44
    Dim Camera_Rotation As VEC3
    Dim U As New VEC3(0, 0, 1)
    Dim R As VEC3
    Dim F As VEC3

    Public Sub New(ByRef physical_object As Game_Object,
                   ByVal Eye_Height As Double)
        Height = Eye_Height
        Camera_Object = physical_object
    End Sub

    Public Property Rotation() As VEC3
        Get
            Return Camera_Rotation
        End Get
        Set(ByVal Rotation As VEC3)
            Camera_Rotation += Rotation
        End Set
    End Property

    Public Sub LookAt(ByVal Target As VEC3)
        Dim ViewVector As VEC3 = Target - Camera_Position()
        ViewVector = VEC3.UnitVector(ViewVector)
        F = ViewVector
        U = New VEC3(0, 1, 0)
        R = VEC3.CrossProduct(F, U)
    End Sub

    Public Sub RotateCameraX(ByVal X As Double)
        Camera_Rotation.X += X
    End Sub

    Public Sub RotateCameraY(ByVal Y As Double)
        Camera_Rotation.Y += Y
    End Sub

    Public Sub RotateCameraZ(ByVal Z As Double)
        Camera_Rotation.Z += Z
    End Sub

    Public Sub RotateX(ByVal X As Double)
        Camera_Rotation.X += X
        Camera_Object.Rotation = New VEC3(Camera_Rotation.X, Camera_Rotation.Y, Camera_Rotation.Z)
    End Sub

    Public Sub RotateY(ByVal Y As Double)
        Camera_Rotation.Y += Y
        Camera_Object.Rotation = New VEC3(Camera_Rotation.X, Camera_Rotation.Y, Camera_Rotation.Z)
    End Sub

    Public Sub RotateZ(ByVal Z As Double)
        Camera_Rotation.Z += Z
        Camera_Object.Rotation = New VEC3(Camera_Rotation.X, Camera_Rotation.Y, Camera_Rotation.Z)
    End Sub

    Public Sub ChangeDistance(ByVal metres As Double)
        Distance += metres
        If Distance < Min_Distance Then Distance = Min_Distance
    End Sub

    Public Sub SetDistance(ByVal metres As Double)
        Distance = metres
    End Sub

    Private Function Camera_Offset() As VEC3
        Dim rmat As MAT44 = MAT44.RotationMatrixYAxis(Camera_Rotation.Y, New VEC3) *
                            MAT44.RotationMatrixXAxis(Camera_Rotation.X, New VEC3)

        Dim offset As VEC3 = rmat.MVector(New VEC3(0, 0, -Distance))
        offset.Y += Height
        Return offset
    End Function

    Public Function Camera_Position() As VEC3
        Return (Camera_Object.Position + Camera_Offset())
    End Function

    Public Sub Prepare_Matrix()
        Dim P As VEC3 = Camera_Position()
        Dim pmat As New MAT44(1, 0, 0, -P.X,
                              0, 1, 0, -P.Y,
                              0, 0, 1, -P.Z,
                              0, 0, 0, 1)
        Dim rmat As MAT44 = MAT44.RotationMatrixXAxis(-Camera_Rotation.X, New VEC3) *
                            MAT44.RotationMatrixYAxis(-Camera_Rotation.Y, New VEC3)
        Matrix = (rmat * pmat)
        'Here is the matrix content of a camera the faces a point.

        'R (Rx, Ry, Rz) : The side direction (right hand) of the camera = U x F
        'U (Ux, Uy, Uz) : The camera's vertical direction
        'F (Fx, Fy, Fz) : The camera's front direction (should be (0, 0, 1))
        'P (Px, Py, Pz) : The camera's position dot R, U and F respectively

        'Matrix = New MAT44(R.X, R.Y, R.Z, -P.X, _
        '                   U.X, U.Y, U.Z, -P.Y, _
        '                   F.X, F.Y, F.Z, -P.Z, _
        '                   0, 0, 0, 1)
    End Sub

End Class