Public Structure MAT44
    Public M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 As Double

    Private Shared ReadOnly m_id As New MAT44(1.0, 0.0, 0.0, 0.0,
                                              0.0, 1.0, 0.0, 0.0,
                                              0.0, 0.0, 1.0, 0.0,
                                              0.0, 0.0, 0.0, 1.0)

    Public Shared ReadOnly Property Identity() As MAT44
        Get
            Return m_id
        End Get
    End Property

    Public Property Offset() As VEC3
        Get
            Return New VEC3(M14, M24, M34)
        End Get
        Set(ByVal value As VEC3)
            M14 = value.X
            M24 = value.Y
            M34 = value.Z
        End Set
    End Property

    Sub New(ByVal _m11 As Double, ByVal _m12 As Double, ByVal _m13 As Double, ByVal _m14 As Double,
            ByVal _m21 As Double, ByVal _m22 As Double, ByVal _m23 As Double, ByVal _m24 As Double,
            ByVal _m31 As Double, ByVal _m32 As Double, ByVal _m33 As Double, ByVal _m34 As Double,
            ByVal _m41 As Double, ByVal _m42 As Double, ByVal _m43 As Double, ByVal _m44 As Double)
        M11 = _m11 : M12 = _m12 : M13 = _m13 : M14 = _m14
        M21 = _m21 : M22 = _m22 : M23 = _m23 : M24 = _m24
        M31 = _m31 : M32 = _m32 : M33 = _m33 : M34 = _m34
        M41 = _m41 : M42 = _m42 : M43 = _m43 : M44 = _m44
    End Sub

    ''' <summary> Multiplies a vec3 (x,y,z) Vertex_Textured by the matrix (if the vector is treated as a coordinate point w=1).</summary>
    Function MPoint(ByVal p As VEC3) As VEC3
        Return New VEC3(M11 * p.X + M12 * p.Y + M13 * p.Z + M14,
                        M21 * p.X + M22 * p.Y + M23 * p.Z + M24,
                        M31 * p.X + M32 * p.Y + M33 * p.Z + M34)
    End Function

    ''' <summary> Multiplies a vec3 (x,y,z) Vertex_Textured by the matrix (if the vector is treated as a displacement vector w=0).</summary>
    Function MVector(ByVal v As VEC3) As VEC3
        Return New VEC3(M11 * v.X + M12 * v.Y + M13 * v.Z,
                        M21 * v.X + M22 * v.Y + M23 * v.Z,
                        M31 * v.X + M32 * v.Y + M33 * v.Z)
    End Function

    Shared Operator *(ByVal m1 As MAT44, ByVal m2 As MAT44) As MAT44
        Return New MAT44(m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41,
                         m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42,
                         m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43,
                         m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44,
                         m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41,
                         m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42,
                         m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43,
                         m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44,
                         m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41,
                         m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42,
                         m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43,
                         m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44,
                         m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41,
                         m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42,
                         m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43,
                         m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44)
    End Operator

    Shared Function RotationMatrixXAxis(ByVal agl As Double, ByVal ctr As VEC3) As MAT44
        Dim nm As MAT44 = Identity
        Dim s, c As Double
        s = Math.Sin(agl) : c = Math.Cos(agl)
        With nm
            .M22 = c
            .M33 = c
            .M23 = -s
            .M32 = s
        End With
        Return nm
    End Function

    Shared Function RotationMatrixYAxis(ByVal agl As Double, ByVal ctr As VEC3) As MAT44
        Dim nm As MAT44 = Identity
        Dim s, c As Double
        s = Math.Sin(agl) : c = Math.Cos(agl)
        With nm
            .M11 = c
            .M13 = s
            .M31 = -s
            .M33 = c
        End With
        Return nm
    End Function

    Shared Function RotationMatrixZAxis(ByVal agl As Double, ByVal ctr As VEC3) As MAT44
        Dim nm As MAT44 = Identity
        Dim s, c As Double
        s = Math.Sin(agl) : c = Math.Cos(agl)
        With nm
            .M11 = c
            .M12 = -s
            .M21 = s
            .M22 = c
        End With
        Return nm
    End Function

End Structure