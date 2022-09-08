Imports System.ComponentModel
Imports System.Globalization

Public Class Form_Settings
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

    Public Sub LayoutLoad()
        BunifuToggleSwitch_UseFullName.Checked = My.Settings.useFullName
        BunifuToggleSwitch_DisplaySeconds.Checked = My.Settings.displaySeconds
        BunifuTextBox_Psswrd.Text = My.Settings.password
        BunifuToggleSwitch_VstrCapacity.Checked = My.Settings.useVisitorCapacity
        BunifuTextBox_VstrCapacity.Text = My.Settings.visitorCapacity
        BunifuTextBox_Server.Text = My.Settings.conStrgServer
        BunifuTextBox_Username.Text = My.Settings.conStrgUsername
        BunifuTextBox_Password.Text = My.Settings.conStrgPassword
        BunifuTextBox_Database.Text = My.Settings.conStrgDatabase
        BunifuTextBox_FeverValue.Text = My.Settings.tempFeverVal.ToString("N2")
        BunifuTextBox_TempAdjust.Text = My.Settings.tempAdjustment.ToString("N2")
        BunifuTextBox_SanitizerDuration.Text = My.Settings.sanitizerDuration
        BunifuToggleSwitch_AutoRecord.Checked = My.Settings.isAutoRecord
        BunifuTextBox_AutoRecord.Text = My.Settings.autoRecordCount
        BunifuToggleSwitch_AutoConnect.Checked = My.Settings.autoConnectDevice
        BunifuTextBox1.Text = My.Settings.tempMaxd
        BunifuTextBox2.Text = My.Settings.tempMind

        BunifuCheckBox2.Enabled = True
        BunifuCheckBox3.Enabled = True
        BunifuCheckBox4.Enabled = True
        BunifuCheckBox2.Checked = My.Settings.passwordDataView
        BunifuCheckBox3.Checked = My.Settings.passwordExit
        BunifuCheckBox4.Checked = My.Settings.passwordSave
        BunifuToggleSwitch_SetPassword.Checked = My.Settings.usePassword

        BunifuTextBox_Password.IconRight = My.Resources.showpass_icon
        BunifuTextBox_Password.PasswordChar = "●"
        BunifuTextBox_Password.UseSystemPasswordChar = True

        BunifuButton_TestCon.Text = "Test Connection"
        BunifuButton_TestCon.IdleIconRightImage = Nothing
        BunifuPanel_Loading.Visible = False
        BunifuPanel2_Loading.Visible = False
        BunifuLabel_ApplyNotice.Visible = False
        BunifuButton_Apply.Enabled = False
    End Sub

    Private Sub Form_Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LayoutLoad()
    End Sub

    Private Sub BunifuButton_Close_Click(sender As Object, e As EventArgs) Handles BunifuButton_Close.Click
        Me.Opacity = 0
        LayoutLoad()
    End Sub

    Private Sub BunifuButton_Apply_Click(sender As Object, e As EventArgs) Handles BunifuButton_Apply.Click
        My.Settings.useFullName = BunifuToggleSwitch_UseFullName.Checked
        My.Settings.displaySeconds = BunifuToggleSwitch_DisplaySeconds.Checked
        My.Settings.usePassword = BunifuToggleSwitch_SetPassword.Checked
        My.Settings.password = BunifuTextBox_Psswrd.Text
        My.Settings.deactivateSanitizer = BunifuToggleSwitch_DeactivateSanitizer.Checked
        My.Settings.useVisitorCapacity = BunifuToggleSwitch_VstrCapacity.Checked
        My.Settings.visitorCapacity = BunifuTextBox_VstrCapacity.Text
        My.Settings.tempFeverVal = BunifuTextBox_FeverValue.Text
        My.Settings.tempAdjustment = BunifuTextBox_TempAdjust.Text
        My.Settings.sanitizerDuration = BunifuTextBox_SanitizerDuration.Text
        My.Settings.passwordDataView = BunifuCheckBox2.Checked
        My.Settings.passwordExit = BunifuCheckBox3.Checked
        My.Settings.passwordSave = BunifuCheckBox4.Checked
        My.Settings.isAutoRecord = BunifuToggleSwitch_AutoRecord.Checked
        My.Settings.autoRecordCount = BunifuTextBox_AutoRecord.Text
        My.Settings.autoConnectDevice = BunifuToggleSwitch_AutoConnect.Checked
        My.Settings.tempMaxd = BunifuTextBox1.Text
        My.Settings.tempMind = BunifuTextBox2.Text

        If con.State = ConnectionState.Open Then con.Close()
        Try
            con.ConnectionString = "server='" & BunifuTextBox_Server.Text & "';username='" & BunifuTextBox_Username.Text & "';password='" & BunifuTextBox_Password.Text & "';database='" & BunifuTextBox_Database.Text & "'"
            con.Open()
            My.Settings.conStrgServer = BunifuTextBox_Server.Text
            My.Settings.conStrgUsername = BunifuTextBox_Username.Text
            My.Settings.conStrgPassword = BunifuTextBox_Password.Text
            My.Settings.conStrgDatabase = BunifuTextBox_Database.Text
        Catch ex As Exception
            'MsgBox(ex.ToString)
        Finally
            con.Close()
        End Try

        If DeviceIsOnline Then
            BunifuButton23.Enabled = True
            BunifuButton24.Enabled = True
            BunifuButton25.Enabled = True
            BunifuButton26.Enabled = True
            BunifuButton27.Enabled = True
        End If

        My.Settings.Save()
        Form_Main.FormUpdater()
        Form_Main.TableUpdater()

        If Form_Main.BunifuTextBox_TimeIn.Text.Trim <> "" Then
            If My.Settings.displaySeconds Then
                Form_Main.BunifuTextBox_TimeIn.Text = DateTime.Parse(Form_Main.BunifuTextBox_TimeIn.Text).ToString("h:mm:ss tt").ToUpper
            Else
                Form_Main.BunifuTextBox_TimeIn.Text = DateTime.Parse(Form_Main.BunifuTextBox_TimeIn.Text).ToString("h:mm tt").ToUpper
            End If
        End If

        If Form_DataView.DataGridView1.RowCount > 0 Then
            If My.Settings.displaySeconds Then
                Form_DataView.DataGridView1.Columns(6).DefaultCellStyle.Format = "h:mm:ss tt"
            Else
                Form_DataView.DataGridView1.Columns(6).DefaultCellStyle.Format = "h:mm tt"
            End If
        End If

        BunifuLabel_ApplyNotice.Visible = False
        BunifuButton_Apply.Enabled = False
        LayoutLoad()
    End Sub

    Private Sub BunifuTextBox_FeverValue_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles BunifuTextBox_FeverValue.KeyPress, BunifuTextBox_TempAdjust.KeyPress, BunifuTextBox2.KeyPress, BunifuTextBox1.KeyPress
        If InStr("1234567890.-", e.KeyChar) = 0 And Asc(e.KeyChar) <> 8 Or (e.KeyChar = "." And InStr(sender.Text, ".") > 0) Then
            e.KeyChar = Chr(0)
            e.Handled = True
        End If
    End Sub

    Private Sub BunifuTextBox_Splash_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles BunifuTextBox_VstrCapacity.KeyPress, BunifuTextBox_SanitizerDuration.KeyPress, BunifuTextBox_AutoRecord.KeyPress
        If InStr("1234567890", e.KeyChar) = 0 And Asc(e.KeyChar) <> 8 Then
            e.KeyChar = Chr(0)
            e.Handled = True
        End If
    End Sub

    Private Sub BunifuToggleSwitch_CheckedChanged(sender As Object, e As Bunifu.UI.WinForms.BunifuToggleSwitch.CheckedChangedEventArgs) Handles BunifuToggleSwitch_SetPassword.CheckedChanged, BunifuToggleSwitch_VstrCapacity.CheckedChanged, BunifuToggleSwitch_AutoRecord.CheckedChanged, BunifuToggleSwitch_DeactivateSanitizer.CheckedChanged
        If sender.Equals(BunifuToggleSwitch_SetPassword) Then
            If BunifuToggleSwitch_SetPassword.Checked = True Then
                BunifuTextBox_Psswrd.Enabled = True
                BunifuTextBox_Psswrd.IconRight = My.Resources.showpass_icon
                BunifuCheckBox2.Enabled = True
                BunifuCheckBox2.OnCheck.CheckBoxColor = Color.Maroon
                If BunifuCheckBox2.Checked = True Then
                    BunifuCheckBox2.OnDisable.CheckBoxColor = Color.FromArgb(191, 191, 191)
                Else
                    BunifuCheckBox2.OnDisable.CheckBoxColor = Color.FromArgb(240, 240, 240)
                End If
                BunifuCheckBox3.Enabled = True
                BunifuCheckBox3.OnCheck.CheckBoxColor = Color.Maroon
                If BunifuCheckBox3.Checked = True Then
                    BunifuCheckBox3.OnDisable.CheckBoxColor = Color.FromArgb(191, 191, 191)
                Else
                    BunifuCheckBox3.OnDisable.CheckBoxColor = Color.FromArgb(240, 240, 240)
                End If
                BunifuCheckBox4.Enabled = True
                BunifuCheckBox4.OnCheck.CheckBoxColor = Color.Maroon
                If BunifuCheckBox4.Checked = True Then
                    BunifuCheckBox4.OnDisable.CheckBoxColor = Color.FromArgb(191, 191, 191)
                Else
                    BunifuCheckBox4.OnDisable.CheckBoxColor = Color.FromArgb(240, 240, 240)
                End If
                BunifuLabel16.ForeColor = Color.Black
                BunifuLabel17.ForeColor = Color.Black
                BunifuLabel18.ForeColor = Color.Black
                BunifuLabel19.ForeColor = Color.Black
            Else
                BunifuTextBox_Psswrd.Enabled = False
                BunifuTextBox_Psswrd.IconRight = Nothing
                BunifuTextBox_Psswrd.PasswordChar = "●"
                BunifuTextBox_Psswrd.UseSystemPasswordChar = True
                BunifuCheckBox2.Enabled = False
                If BunifuCheckBox2.Checked = True Then
                    BunifuCheckBox2.OnCheck.CheckBoxColor = Color.FromArgb(191, 191, 191)
                Else
                    BunifuCheckBox2.OnCheck.CheckBoxColor = Color.FromArgb(240, 240, 240)
                End If
                BunifuCheckBox3.Enabled = False
                If BunifuCheckBox3.Checked = True Then
                    BunifuCheckBox3.OnCheck.CheckBoxColor = Color.FromArgb(191, 191, 191)
                Else
                    BunifuCheckBox3.OnCheck.CheckBoxColor = Color.FromArgb(240, 240, 240)
                End If
                BunifuCheckBox4.Enabled = False
                If BunifuCheckBox4.Checked = True Then
                    BunifuCheckBox4.OnCheck.CheckBoxColor = Color.FromArgb(191, 191, 191)
                Else
                    BunifuCheckBox4.OnCheck.CheckBoxColor = Color.FromArgb(240, 240, 240)
                End If
                BunifuLabel16.ForeColor = Color.Gray
                BunifuLabel17.ForeColor = Color.Gray
                BunifuLabel18.ForeColor = Color.Gray
                BunifuLabel19.ForeColor = Color.Gray
            End If
        End If
        If sender.Equals(BunifuToggleSwitch_VstrCapacity) Then
            If BunifuToggleSwitch_VstrCapacity.Checked = True Then
                BunifuTextBox_VstrCapacity.Enabled = True
            Else
                BunifuTextBox_VstrCapacity.Enabled = False
            End If
        End If
        If sender.Equals(BunifuToggleSwitch_AutoRecord) Then
            If BunifuToggleSwitch_AutoRecord.Checked = True And BunifuToggleSwitch_AutoRecord.Enabled Then
                BunifuTextBox_AutoRecord.Enabled = True
            Else
                BunifuTextBox_AutoRecord.Enabled = False
            End If
        End If
        If sender.Equals(BunifuToggleSwitch_DeactivateSanitizer) Then
            If BunifuToggleSwitch_DeactivateSanitizer.Checked = True And BunifuToggleSwitch_DeactivateSanitizer.Enabled Then
                BunifuTextBox_SanitizerDuration.Enabled = False
            Else
                BunifuTextBox_SanitizerDuration.Enabled = True
            End If
        End If
    End Sub

    Private Sub BunifuButton_WipeDB_Click(sender As Object, e As EventArgs) Handles BunifuButton_WipeDB.Click
        Dim i As String = Form_InputBox.Show("Are you sure?", "Delete data", "", "", True)
        If i = "Confirm" Then

        End If
    End Sub

    Private Sub BunifuButton_TestCon_Click(sender As Object, e As EventArgs) Handles BunifuButton_TestCon.Click
        If Not DatabaseConWorker.IsBusy Then
            BunifuPanel_Loading.Visible = True
            BunifuTextBox_Server.ReadOnly = True
            BunifuTextBox_Username.ReadOnly = True
            BunifuTextBox_Password.ReadOnly = True
            BunifuTextBox_Database.ReadOnly = True
            BunifuButton_Import.Enabled = False
            BunifuButton_Export.Enabled = False
            BunifuButton_WipeDB.Enabled = False
            BunifuButton_TestCon.Text = "Connecting..."
            BunifuButton_TestCon.IdleIconRightImage = Nothing
            DatabaseConWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub BunifuButton21_Click(sender As Object, e As EventArgs) Handles BunifuButton21.Click
        BunifuDropdown1.Items.Clear()
        BunifuDropdown1.Items.AddRange(IO.Ports.SerialPort.GetPortNames())
        If BunifuDropdown1.Items.Count > 0 Then
            BunifuDropdown1.SelectedIndex = BunifuDropdown1.Items.Count - 1
            BunifuButton22.Enabled = True
            BunifuButton22.Text = "Connect Device"
            If My.Settings.autoConnectDevice Then
                BunifuButton22.PerformClick()
            Else
                BunifuDropdown1.DroppedDown = True
            End If
        Else
            BunifuDropdown1.Text = "N/A"
            BunifuButton22.Text = "No COM Port Detected!"
        End If
    End Sub

    Private Sub BunifuButton22_Click(sender As Object, e As EventArgs) Handles BunifuButton22.Click
        If BunifuButton22.Text = "Disconnect Device" Then
            Form_Main.SerialPort1.Close()
            BunifuButton22.Text = "Connect Device"
            BunifuButton22.IdleIconRightImage = Nothing
            BunifuPanel2_Loading.Visible = False
            BunifuDropdown1.Enabled = True
            'BunifuDropdown2.Enabled = True
            BunifuButton21.Enabled = True
            BunifuButton23.Enabled = False
            BunifuButton24.Enabled = False
            BunifuButton25.Enabled = False
            BunifuButton26.Enabled = False
            BunifuButton27.Enabled = False
            BunifuToggleSwitch_AutoRecord.Enabled = False
            BunifuTextBox_AutoRecord.Enabled = False
            BunifuLabel_AutoRecord1.ForeColor = Color.Gray
            BunifuLabel_AutoRecord2.ForeColor = Color.Gray
            BunifuTextBox_FeverValue.Enabled = False
            BunifuTextBox_FeverValue.IconRight = Nothing
            BunifuLabel_FeverValue.ForeColor = Color.Gray
            BunifuTextBox_TempAdjust.Enabled = False
            BunifuTextBox_TempAdjust.IconRight = Nothing
            BunifuLabel_TempAdjust.ForeColor = Color.Gray
            BunifuToggleSwitch_DeactivateSanitizer.Checked = False
            BunifuToggleSwitch_DeactivateSanitizer.Enabled = False
            BunifuLabel_DeactivateSanitizer.ForeColor = Color.Gray
            BunifuTextBox_SanitizerDuration.Enabled = False
            BunifuLabel_SanitizerDuration.ForeColor = Color.Gray
            BunifuLabel_SanitizerDuration2.ForeColor = Color.Gray
            BunifuTextBox1.Enabled = False
            BunifuLabel3.ForeColor = Color.Gray
            BunifuTextBox2.Enabled = False
            BunifuLabel6.ForeColor = Color.Gray
            DeviceIsOnline = False
            SerialPort1.Close() '<--------------------------------------to be delete
        ElseIf Not DeviceConWorker.IsBusy Then
            BunifuPanel2_Loading.Visible = True
            BunifuButton22.Text = "Connecting..."
            BunifuButton22.IdleIconRightImage = Nothing
            DeviceConWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub BunifuTextBox_Psswrd_OnIconRightClick(sender As Object, e As EventArgs) Handles BunifuTextBox_Psswrd.OnIconRightClick
        If BunifuTextBox_Psswrd.UseSystemPasswordChar = True Then
            BunifuTextBox_Psswrd.IconRight = My.Resources.hidepass_icon
            BunifuTextBox_Psswrd.PasswordChar = ""
            BunifuTextBox_Psswrd.UseSystemPasswordChar = False
        Else
            BunifuTextBox_Psswrd.IconRight = My.Resources.showpass_icon
            BunifuTextBox_Psswrd.PasswordChar = "●"
            BunifuTextBox_Psswrd.UseSystemPasswordChar = True
        End If
    End Sub

    Private Sub BunifuTextBox_Password_OnIconRightClick(sender As Object, e As EventArgs) Handles BunifuTextBox_Password.OnIconRightClick
        If BunifuTextBox_Password.UseSystemPasswordChar = True Then
            BunifuTextBox_Password.IconRight = My.Resources.hidepass_icon
            BunifuTextBox_Password.PasswordChar = ""
            BunifuTextBox_Password.UseSystemPasswordChar = False
        Else
            BunifuTextBox_Password.IconRight = My.Resources.showpass_icon
            BunifuTextBox_Password.PasswordChar = "●"
            BunifuTextBox_Password.UseSystemPasswordChar = True
        End If
    End Sub

    Private Sub DatabaseConWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles DatabaseConWorker.DoWork
        Threading.Thread.Sleep(2000)
        If con.State = ConnectionState.Open Then con.Close()
        Try
            con.ConnectionString = "server='" & BunifuTextBox_Server.Text & "';username='" & BunifuTextBox_Username.Text & "';password='" & BunifuTextBox_Password.Text & "';database='" & BunifuTextBox_Database.Text & "'"
            con.Open()
        Catch ex As Exception
            e.Cancel = True
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub DatabaseConWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles DatabaseConWorker.RunWorkerCompleted
        BunifuPanel_Loading.Visible = False
        BunifuTextBox_Server.ReadOnly = False
        BunifuTextBox_Username.ReadOnly = False
        BunifuTextBox_Password.ReadOnly = False
        BunifuTextBox_Database.ReadOnly = False
        BunifuButton_Import.Enabled = True
        BunifuButton_Export.Enabled = True
        BunifuButton_WipeDB.Enabled = True
        If Not e.Cancelled Then
            BunifuButton_TestCon.Text = "Connected"
            BunifuButton_TestCon.IdleIconRightImage = My.Resources.check_icon
            BunifuButton_Export.Text = "Export"
        Else
            BunifuButton_TestCon.Text = "Try Again"
            BunifuButton_TestCon.IdleIconRightImage = My.Resources.close_icon
            BunifuButton_Export.Text = "Create"
        End If
    End Sub

    Private Sub DatabaseConWorker_TextChanged(sender As Object, e As EventArgs) Handles BunifuTextBox_Server.TextChanged, BunifuTextBox_Username.TextChanged,
             BunifuTextBox_Password.TextChanged, BunifuTextBox_Database.TextChanged
        BunifuLabel_ApplyNotice.Visible = True
        BunifuButton_Apply.Enabled = True
        BunifuButton_TestCon.Text = "Test Connection"
        BunifuButton_TestCon.IdleIconRightImage = Nothing
    End Sub

    Private Sub DeviceConWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles DeviceConWorker.DoWork
        Threading.Thread.Sleep(2000)
    End Sub

    Private Sub DeviceConWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles DeviceConWorker.RunWorkerCompleted
        BunifuPanel2_Loading.Visible = False
        Try
            Form_Main.SerialPort1.PortName = BunifuDropdown1.Text
            Form_Main.SerialPort1.BaudRate = Integer.Parse(BunifuDropdown2.Text)
            Form_Main.SerialPort1.Open()
            BunifuButton22.Text = "Disconnect Device"
            BunifuButton22.IdleIconRightImage = My.Resources.check_icon
            BunifuDropdown1.Enabled = False
            'BunifuDropdown2.Enabled = False
            BunifuButton21.Enabled = False
            BunifuButton23.Enabled = True
            BunifuButton24.Enabled = True
            BunifuButton25.Enabled = True
            BunifuButton26.Enabled = True
            BunifuButton27.Enabled = True
            BunifuToggleSwitch_AutoRecord.Enabled = True
            If BunifuToggleSwitch_AutoRecord.Checked = True Then BunifuTextBox_AutoRecord.Enabled = True
            BunifuLabel_AutoRecord1.ForeColor = Color.Black
            BunifuLabel_AutoRecord2.ForeColor = Color.Black
            BunifuTextBox_FeverValue.Enabled = True
            BunifuTextBox_FeverValue.IconRight = My.Resources.degree_icon
            BunifuLabel_FeverValue.ForeColor = Color.Black
            BunifuTextBox_TempAdjust.Enabled = True
            BunifuTextBox_TempAdjust.IconRight = My.Resources.degree_icon
            BunifuLabel_TempAdjust.ForeColor = Color.Black
            BunifuToggleSwitch_DeactivateSanitizer.Enabled = True
            BunifuToggleSwitch_DeactivateSanitizer.Checked = My.Settings.deactivateSanitizer
            BunifuLabel_DeactivateSanitizer.ForeColor = Color.Black
            If BunifuToggleSwitch_DeactivateSanitizer.Checked = False Then BunifuTextBox_SanitizerDuration.Enabled = True
            BunifuLabel_SanitizerDuration.ForeColor = Color.Black
            BunifuLabel_SanitizerDuration2.ForeColor = Color.Black
            BunifuTextBox1.Enabled = True
            BunifuLabel3.ForeColor = Color.Black
            BunifuTextBox2.Enabled = True
            BunifuLabel6.ForeColor = Color.Black
            DeviceIsOnline = True
            SerialPort1.Open()  '<--------------------------------------to be delete
        Catch ex As Exception
            'MsgBox(ex.ToString)
            Form_Main.SerialPort1.Close()
            BunifuButton22.Text = "Try Again"
            BunifuButton22.IdleIconRightImage = My.Resources.close_icon
            BunifuDropdown1.Enabled = True
            'BunifuDropdown2.Enabled = True
            BunifuButton21.Enabled = True
            DeviceIsOnline = False
        End Try
    End Sub

    Private Sub BunifuDropdown_SelectedIndexChanged(sender As Object, e As EventArgs) Handles BunifuDropdown1.SelectedIndexChanged, BunifuDropdown2.SelectedIndexChanged
        If BunifuDropdown1.Text <> "N/A" Then
            BunifuButton22.Text = "Connect Device"
            BunifuButton22.IdleIconRightImage = Nothing
        End If
    End Sub

    Private Sub BunifuPanel1_MouseUp(sender As Object, e As MouseEventArgs) Handles BunifuPanel1.MouseUp
        If Me.Location.Y < 0 Then
            Me.Location = New Point(Me.Location.X, 0)
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SerialPort1.WriteLine(TextBox1.Text)
    End Sub

    Private Sub Apply_Button_Enabler(sender As Object, e As EventArgs) Handles BunifuTextBox_VstrCapacity.TextChanged,
           BunifuTextBox_TempAdjust.TextChanged, BunifuTextBox_Psswrd.TextChanged, BunifuTextBox_FeverValue.TextChanged, BunifuTextBox_SanitizerDuration.TextChanged, BunifuTextBox_AutoRecord.TextChanged,
           BunifuCheckBox2.Click, BunifuCheckBox3.Click, BunifuCheckBox4.Click, BunifuToggleSwitch_UseFullName.Click, BunifuToggleSwitch_SetPassword.Click, BunifuToggleSwitch_VstrCapacity.Click,
           BunifuToggleSwitch_DisplaySeconds.Click, BunifuToggleSwitch_AutoRecord.Click, BunifuToggleSwitch_AutoConnect.Click, BunifuToggleSwitch_DeactivateSanitizer.Click, BunifuTextBox2.TextChanged, BunifuTextBox1.TextChanged
        BunifuLabel_ApplyNotice.Visible = True
        BunifuButton_Apply.Enabled = True
    End Sub

    Private Sub Form_Settings_Closing(sender As Object, e As FormClosingEventArgs) Handles Me.Closing
        If (e.CloseReason = CloseReason.UserClosing) Then
            e.Cancel = True
        End If
    End Sub

    Private Sub BunifuButton_ExportImport_Click(sender As Object, e As EventArgs) Handles BunifuButton_Export.Click, BunifuButton_Import.Click
        If sender.Equals(BunifuButton_Export) Then
            If BunifuButton_Export.Text = "Create" Then
                If con.State = ConnectionState.Open Then con.Close()
                Try
                    con.ConnectionString = "server='" & BunifuTextBox_Server.Text & "';username='" & BunifuTextBox_Username.Text & "';password='" & BunifuTextBox_Password.Text & "';database="
                    con.Open()
                    cmd.Connection = con
                    cmd.CommandText = "CREATE DATABASE If Not EXISTS `" & BunifuTextBox_Database.Text & "`;"
                    cmd.ExecuteNonQuery()
                    My.Settings.conStrgDatabase = BunifuTextBox_Database.Text
                    My.Settings.Save()
                    BunifuButton_TestCon.PerformClick()
                Catch ex As Exception
                    MsgBox(ex.ToString)
                Finally
                    con.Close()
                End Try
            Else
                SaveFileDialog1.FileName = "Backup-" & My.Settings.conStrgDatabase & "-" & DateTime.Now.ToString("M_d_yy", CultureInfo.InvariantCulture) & ".sql"
                If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                    DatabaseBackup(SaveFileDialog1.FileName.ToString, False)
                End If
            End If
        Else
            If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                DatabaseBackup(OpenFileDialog1.FileName.ToString, True)
            End If
        End If
        BunifuTextBox_Database.Text = My.Settings.conStrgDatabase
    End Sub

    Private Sub BunifuCheckBox2_CheckedChanged(sender As Object, e As Bunifu.UI.WinForms.BunifuCheckBox.CheckedChangedEventArgs) Handles BunifuCheckBox2.CheckedChanged
        If BunifuCheckBox2.Checked Then
            BunifuCheckBox4.Checked = False
            BunifuCheckBox4.Enabled = False
            BunifuLabel18.ForeColor = Color.Gray
        Else
            BunifuCheckBox4.Enabled = True
            BunifuLabel18.ForeColor = Color.Black
        End If
    End Sub

    Private Sub BunifuButton23_Click(sender As Object, e As EventArgs) Handles BunifuButton23.Click, BunifuButton24.Click, BunifuButton25.Click, BunifuButton27.Click, BunifuButton26.Click
        If sender.Equals(BunifuButton23) Then
            BunifuButton23.Enabled = False
            My.Settings.tempFeverVal = BunifuTextBox_FeverValue.Text
            Form_Main.SerialPort1.WriteLine("tempF" & BunifuTextBox_FeverValue.Text)
        ElseIf sender.Equals(BunifuButton24) Then
            BunifuButton24.Enabled = False
            My.Settings.tempAdjustment = BunifuTextBox_TempAdjust.Text
            Form_Main.SerialPort1.WriteLine("tempA" & BunifuTextBox_TempAdjust.Text)
        ElseIf sender.Equals(BunifuButton25) Then
            BunifuButton25.Enabled = False
            My.Settings.sanitizerDuration = BunifuTextBox_SanitizerDuration.Text
            Form_Main.SerialPort1.WriteLine("pumpD" & Integer.Parse(BunifuTextBox_SanitizerDuration.Text) * 1000)
        ElseIf sender.Equals(BunifuButton26) Then
            BunifuButton26.Enabled = False
            My.Settings.sanitizerDuration = BunifuTextBox1.Text
            Form_Main.SerialPort1.WriteLine("tempH" & BunifuTextBox1.Text)
        ElseIf sender.Equals(BunifuButton27) Then
            BunifuButton27.Enabled = False
            My.Settings.sanitizerDuration = BunifuTextBox2.Text
            Form_Main.SerialPort1.WriteLine("tempL" & BunifuTextBox2.Text)
        End If
        My.Settings.Save()
    End Sub
End Class