
Public Class Form_InputBox
    Private AeroEnabled As Boolean

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            CheckAeroEnabled()
            Dim MyCreateParams As CreateParams = MyBase.CreateParams
            MyCreateParams.ExStyle = MyCreateParams.ExStyle Or &H80
            If Not AeroEnabled Then MyCreateParams.ClassStyle = MyCreateParams.ClassStyle Or NativeConstants.CS_DROPSHADOW
            Return MyCreateParams
        End Get
    End Property

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case NativeConstants.WM_NCPAINT
                Dim val = 2
                If AeroEnabled Then
                    NativeMethods.DwmSetWindowAttribute(Handle, 2, val, 4)
                    Dim bla As New NativeStructs.MARGINS()
                    With bla
                        .bottomHeight = 1
                        .leftWidth = 0
                        .rightWidth = 0
                        .topHeight = 0
                    End With
                    NativeMethods.DwmExtendFrameIntoClientArea(Handle, bla)
                End If
                Exit Select
        End Select
        MyBase.WndProc(m)
    End Sub
    Private Sub CheckAeroEnabled()
        If Environment.OSVersion.Version.Major >= 6 Then
            Dim enabled As Integer = 0
            Dim response As Integer = NativeMethods.DwmIsCompositionEnabled(enabled)
            AeroEnabled = (enabled = 1)
        Else
            AeroEnabled = False
        End If
    End Sub

    Protected m_ReturnText As String = ""
    Protected m_NoInput As Boolean = False

    Public Overloads Function ShowDialog(
   TitleText As String,
   PromptText As String,
   DefaultText As String,
   ByRef EnteredText As String,
   NoInput As Boolean) As String
        BunifuLabel_Title.Text = TitleText
        Text = PromptText
        m_NoInput = NoInput
        If NoInput Then
            BunifuTextBox_Txt_TextEntry.Visible = False
            BunifuTextBox_But_Ok.Text = "Yes"
            BunifuTextBox_But_Cancel.Text = "No"
            Size = New Size(228, 84)
        End If
        BunifuTextBox_Txt_TextEntry.Text = DefaultText
        ShowDialog()
        EnteredText = m_ReturnText

        Return m_ReturnText
    End Function

    Private Sub Form_InputBox_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Form_DataView.TopMost = False
        Form_Settings.TopMost = False
        BunifuTextBox_Txt_TextEntry.IconRight = My.Resources.showpass_icon
        BunifuTextBox_Txt_TextEntry.PasswordChar = "●"
        BunifuTextBox_Txt_TextEntry.UseSystemPasswordChar = True
    End Sub

    Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles BunifuTextBox_But_Ok.Click
        If m_NoInput Then
            m_ReturnText = "Confirm"
            DialogResult = DialogResult.OK
            Form_DataView.TopMost = True
            Form_Settings.TopMost = True
            Close()
        Else
            m_ReturnText = BunifuTextBox_Txt_TextEntry.Text
            If m_ReturnText = My.Settings.password Then
                DialogResult = DialogResult.OK
                Form_DataView.TopMost = True
                Form_Settings.TopMost = True
                Close()
            End If
        End If
    End Sub

    Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles BunifuTextBox_But_Cancel.Click
        DialogResult = DialogResult.Cancel
        m_ReturnText = ""
        Form_DataView.TopMost = True
        Form_Settings.TopMost = True
        Close()
    End Sub

    Private Sub Psswrd_OnIconRightClick(sender As Object, e As EventArgs) Handles BunifuTextBox_Txt_TextEntry.OnIconRightClick
        If BunifuTextBox_Txt_TextEntry.UseSystemPasswordChar = True Then
            BunifuTextBox_Txt_TextEntry.IconRight = My.Resources.hidepass_icon
            BunifuTextBox_Txt_TextEntry.PasswordChar = ""
            BunifuTextBox_Txt_TextEntry.UseSystemPasswordChar = False
        Else
            BunifuTextBox_Txt_TextEntry.IconRight = My.Resources.showpass_icon
            BunifuTextBox_Txt_TextEntry.PasswordChar = "●"
            BunifuTextBox_Txt_TextEntry.UseSystemPasswordChar = True
        End If
    End Sub

    Public Overloads Shared Function Show(TitleText As String, PromptText As String, DefaultText As String, ByRef TextInputted As String, NoInput As Boolean) As String
        Dim tmp As New Form_InputBox
        Return tmp.ShowDialog(TitleText, PromptText, DefaultText, TextInputted, NoInput)
    End Function
End Class