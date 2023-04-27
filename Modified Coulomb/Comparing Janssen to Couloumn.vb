Option Explicit
'This Calculates K for a Silo utilizing Coulomb's Method and taking into account the curvature of the silo
'The inputs are as follows
'Phi = Material Internal Friction Angle
'Beta = Angle of Repose of Material
'Alpha = Backbatter of Retaining Wall
'Delta = Friction Angle Between Wall and Material
'n = Aspect Ratio of the Silo

Function FDeriv_K(Phi As Double, Beta As Double, Alpha As Double, Delta As Double, Rho As Double, n As Double, dia As Double)
    Dim R As Double
    Dim h As Double
    Dim hw As Double
    Dim NUM As Double, DEN As Double
    Dim A1, A2, C1, C2, A, C, x0, v, v0, c0 As Double

'ASSUMPTIONS
'1) n=H/(2*R) -> H=2*n*R
'Right now this is defined as the total silo height. Is this correct? The rest of the sheet doesn't assume that.
'Proposal => Definition: Aspect Ratio is Height on the Wall divided by the diameter
'           This will require that the actual aspect ratio of the pile and equivalent pile be calculated.

'2) Phi <= Beta
'MITCH CHECK THROUGH THIS
'3) Alpha = 90 deg = Pi/2
'4) Delta = m*Phi
'5) Failure Wedge Volume will be less than or equal to centerline
'   i.e. Rho <= atan(H/R) = atan(2*n)

'CONSTANTS'
    Const Pi As Double = 3.14159265358979
        
'Calculate Radius and Height on the Wall
    R = dia / 2
    hw = n * dia
'    h = h + R * Tan(Beta)
	x0 = R * (Tan(Rho) - Tan(Beta) - n) / (Tan(Rho) - Tan(Beta))
	
    If x0 < 0 Then
        x0 = 0
    ElseIf x0 > R Then
		x0=0
	Else
		x0=x0
    End If
    
'Calculate centroids and areas    
    'y1 = -Tan(Beta) * x + R * (Tan(Beta) + n)
    'y2 = -Tan(Rho) * x + R * Tan(Rho)
    A1 = (-1 / 2 * Tan(Beta) * R ^ 2 + R ^ 2 * (Tan(Beta) + n)) - (-1 / 2 * Tan(Beta) * x0 ^ 2 + R * (Tan(Beta) + n) * x0)
    A2 = (-1 / 2 * Tan(Rho) * R ^ 2 + R * Tan(Rho) * R) - (-1 / 2 * Tan(Rho) * R * x0 + R * Tan(Rho) * x0)
    C1 = ((-1 / 3 * Tan(Beta) * R ^ 3 + 1 / 2 * R * (Tan(Beta) + n) * R ^ 2) - (-1 / 3 * Tan(Beta) * R * x0 ^ 2 + 1 / 2 * R * (Tan(Beta) + n) * x0 ^ 2)) / A1
    C2 = ((-1 / 3 * Tan(Rho) * R ^ 3 + 1 / 2 * R * Tan(Rho) * R ^ 2) - (-1 / 3 * Tan(Rho) * x0 ^ 3 + 1 / 2 * R * Tan(Rho) * x0 ^ 2)) / A2
    
    A = A1 - A2
    C = (A1 * C1 - A2 * C2) / (A1 - A2)
    
'Calculate volume
    v = A * 2 * Pi * C
    c0 = 2 * Pi * R
    v0 = v / c0
    
'Calculate the Lateral Pressure Ratio
    NUM = (2 * v0 * Sin(Rho - Phi))
    DEN = hw ^ 2 * Sin(Pi - Alpha - Rho + Phi + Delta)
    FDeriv_K = NUM / DEN
            
End Function

Function K(Phi As Double, Beta As Double, Delta As Double, Alpha As Double, n As Double, Optional dia As Double) As Double
'Dimension some variables
Dim Rho As Double, Rho_Min As Double, Rho_Max As Double, h As Double
Dim Kh As Double, Kmh As Double, FDeriv As Double, m As Double
Dim Loopey As Boolean

'Constants
Const Pi As Double = 3.14159265358979
Const dh As Double = 0.00001

'Check for missing diameter
    If IsMissing(dia) = True Or dia = 0 Then
        dia = 100
    End If

'Define Rho_Min and Rho_Max
    Rho_Min = 0.0001
    Rho_Max = Pi

'Calculations'
            Rho = Rho_Min
            Loopey = True
            Do While Loopey
                K = FDeriv_K(Phi, Beta, Alpha, Delta, Rho, n, dia)
                Kh = FDeriv_K(Phi, Beta, Alpha, Delta, Rho + dh, n, dia)
                Kmh = FDeriv_K(Phi, Beta, Alpha, Delta, Rho - dh, n, dia)
                FDeriv = (Kh - Kmh) / (2 * dh)
                
                If K <= 0 Then
                    FDeriv = 1000000000
                End If
                
                If Rho <= Rho_Max And FDeriv > dh Then
                    Loopey = True
                Else
                    Loopey = False
                End If
                
                If FDeriv > 0.15 Then
                    m = dh * 100
                ElseIf FDeriv > 0.05 Then
                    m = dh * 10
                Else
                    m = dh
                End If
                                
                Rho = Rho + m
                
            Loop
            
            Rho = Rho * 180 / Pi
            
            
            'K = ActiveCell
            'ActiveCell.Offset(1, 0) = Rho

End Function
