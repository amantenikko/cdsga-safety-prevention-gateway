Imports System.ComponentModel
Imports System.Globalization
Imports System.IO.Ports
Imports System.Threading
Imports Bunifu.UI.WinForms

Public Class Form_Main
    Dim visitorName As String
    Dim visitorProgram As String

    Dim visitorTemp As Double = 0D
    Dim tempCounter As Double = 0D
    Dim visitorCount As Integer = 0

    Dim SerialDataValueRead As String
    Dim SerialDataValueWrite As String

    Dim AutoRecordCount As Integer = -1

    Private Sub Form_Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dtSet.Tables.Add("dataTbl")
        dtSet.Tables.Add("dataTblCount")
        dtSet.Tables.Add("waitTbl")
        dtSet.Tables.Add("waitTblSelected")
        dtSet.Tables.Add("checkTbl")
        dtSet.Tables.Add("ExportTbl")
        dtSet.Tables.Add("checkTblWtng")
        dtSet.Tables.Add("dataTbldateday")
        dtSet.Tables.Add("dataTbldateweek")
        dtSet.Tables.Add("dataTbldatemonth")
        dtSet.Tables.Add("dataTbldateyear")

        CheckTblWtngIfExist(True)
        AdapterDBQuery("SELECT `visitor_id`, `visitor_name`, `visitor_temp`, `visitor_datetimein` FROM `tbl_waitinglist`", "waitTbl")
        DataGridView1.AutoGenerateColumns = True
        DataGridView1.DataMember = "waitTbl"
        DataGridView1.DataSource = dtSet
        DataGridView1.Refresh()

        If String.IsNullOrWhiteSpace(My.Settings.conStrgDatabase) Then AppInit()
        LayoutUpdater()
        FormUpdater()
        TableUpdater()
        ClearForm()
        TimeUpdater.Start()
    End Sub

    Private Sub TimeUpdater_Tick(sender As Object, e As EventArgs) Handles TimeUpdater.Tick
        LayoutUpdater()

        If AutoRecordCount > 0 Then
            BunifuButton_CheckRecord.Text = "Auto record in " & AutoRecordCount
            AutoRecordCount -= 1
        ElseIf AutoRecordCount = 0 Then
            BunifuButton_CheckRecord.PerformClick()
            AutoRecordCount -= 1
        End If

        If My.Settings.autoConnectDevice And DeviceIsOnline = False Then
            Form_Settings.BunifuButton21.PerformClick()
        End If
    End Sub

    Private Sub TempUpdater_Tick(sender As Object, e As EventArgs) Handles TempUpdater.Tick
        tempCounter += visitorTemp / 10
        BunifuLabel_TempVal.Text = tempCounter.ToString("N2") & "°C"

        If tempCounter >= visitorTemp Then
            BunifuLabel_TempVal.Text = visitorTemp.ToString("N2") & "°C"
            TempUpdater.Stop()
        End If
    End Sub

    Private Sub BunifuImageButton_DataView_Click(sender As Object, e As EventArgs) Handles BunifuImageButton_DataView.Click
        If Form_DataView.Opacity = 0 Then
            If My.Settings.usePassword = True And My.Settings.passwordDataView = True Then
                Dim i As String = Form_InputBox.Show("Password for DataView", "Password", "", "", False)
                If i <> My.Settings.password Then Return
                Form_DataView.LayoutLoad()
                Form_DataView.Opacity = 100
            Else
                Form_DataView.LayoutLoad()
                Form_DataView.Opacity = 100
            End If
        End If
        Form_DataView.Focus()
    End Sub

    Private Sub BunifuImageButton_Settings_Click(sender As Object, e As EventArgs) Handles BunifuImageButton_Settings.Click
        If Form_Settings.Opacity = 0 Then
            If My.Settings.usePassword = True Then
                Dim i As String = Form_InputBox.Show("Password for Settings", "Password", "", "", False)
                'Dim i As String = InputBox("Enter a Password to access the settings: ", "User Input")
                If i <> My.Settings.password Then Return
                Form_Settings.LayoutLoad()
                Form_Settings.Opacity = 100
            Else
                Form_Settings.LayoutLoad()
                Form_Settings.Opacity = 100
            End If
        End If
        Form_Settings.Focus()
    End Sub

    Private Sub BunifuImageButton_Exit_Click(sender As Object, e As EventArgs) Handles BunifuImageButton_Exit.Click
        Form_Settings.Opacity = 0
        Form_DataView.Opacity = 0
        If My.Settings.usePassword = True And My.Settings.passwordExit = True Then
            Dim i As String = Form_InputBox.Show("Password for Exit", "Password", "", "", False)
            If i <> My.Settings.password Then Return
            End
        Else
            Dim i As String = Form_InputBox.Show("Are you sure to Exit?", "Exit", "", "", True)
            If i = "Confirm" Then End
        End If
    End Sub

    Sub FormUpdater()
        If My.Settings.useFullName <> False Then
            BunifuLabel_FirstName.Text = "Full Name:"
            BunifuTextBox_FirstName.Text = ""
            BunifuTextBox_LastName.Text = ""
            BunifuTextBox_FullName.BringToFront()
            BunifuTextBox_FullName.Show()
        Else
            BunifuLabel_FirstName.Text = "First Name:"
            BunifuTextBox_FullName.Text = ""
            BunifuTextBox_FullName.SendToBack()
            BunifuTextBox_FullName.Hide()
        End If
    End Sub

    Sub LayoutUpdater()
        If My.Settings.displaySeconds Then
            GLabel1.Text = DateTime.Now.ToString("h:mm:ss tt").ToUpper
        Else
            GLabel1.Text = DateTime.Now.ToString("h:mm tt").ToUpper
        End If
        'If (GLabel1.Width - GLabel1.Height) <> 470 Then
        '    GLabel1.Height = GLabel1.Width - 470
        'End If
        GLabel1.Left = (GLabel1.Parent.Width / 2) - (GLabel1.Width / 2)
        GLabel2.Text = DateTime.Now.ToString("MMMM dd, yyyy - dddd")
        GLabel2.Left = (GLabel2.Parent.Width / 2) - (GLabel2.Width / 2)
        If My.Settings.useVisitorCapacity Then
            BunifuLabel_VisitCount.Text = visitorCount & "/" & My.Settings.visitorCapacity
        Else
            BunifuLabel_VisitCount.Text = visitorCount
        End If
        BunifuLabel_VisitCount.Left = ((BunifuLabel_VisitCount.Parent.Width / 2) - (BunifuLabel_VisitCount.Width / 2)) - (BunifuLabel_VisitCount.Width / 2) + 90
        'BunifuPictureBox_VisitorIcon.Left = ((BunifuPictureBox_VisitorIcon.Parent.Width / 2) - (BunifuPictureBox_VisitorIcon.Width / 2)) + (BunifuPictureBox_VisitorIcon.Width / 2) + 47
    End Sub

    Sub TableUpdater()
        Try
            For column = 0 To DataGridView1.Columns.Count - 1
                DataGridView1.Columns.Item(column).SortMode = DataGridViewColumnSortMode.Programmatic
                DataGridView1.Columns(column).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft
            Next column

            With DataGridView1
                .ColumnHeadersDefaultCellStyle.Padding = New Padding(0, 0, 0, 0)
                .Columns(0).HeaderText = "No."
                .Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(1).HeaderText = "Waiting List"
                .Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                .Columns(2).HeaderText = "Temperature"
                .Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
                .Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(3).HeaderText = "Time In"
                If My.Settings.displaySeconds Then
                    .Columns(3).DefaultCellStyle.Format = "h:mm:ss tt"
                Else
                    .Columns(3).DefaultCellStyle.Format = "h:mm tt"
                End If
                .Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                .ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
                .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            End With

            If DataGridView1.ColumnCount < 5 Then
                Dim btncolumn As New DataGridViewButtonColumn
                'DataGridView1.Columns.Insert(0, btncolumn)
                DataGridView1.Columns.Add(btncolumn)
                btncolumn.FlatStyle = FlatStyle.Flat
                btncolumn.Text = "Edit"
                btncolumn.ToolTipText = "Edit"
                btncolumn.Width = 35

                Dim btncolumn2 As New DataGridViewButtonColumn
                DataGridView1.Columns.Add(btncolumn2)
                btncolumn2.FlatStyle = FlatStyle.Flat
                btncolumn2.Text = "Delete"
                btncolumn2.ToolTipText = "Delete"
                btncolumn2.Width = 35
            End If

            For row = 0 To DataGridView1.Rows.Count - 1
                DataGridView1.Rows(row).Height = 35
            Next row

            'Dim height As Integer = 0
            'For Each row As DataGridViewRow In DataGridView1.Rows
            '    height += row.Height
            'Next
            'height += DataGridView1.ColumnHeadersHeight
            'If DataGridView1.Rows.Count > 5 Then

            'Else
            '    DataGridView1.ClientSize = New Size(DataGridView1.Width, height + 2)
            'End If

        Catch ex As Exception
            'MsgBox(ex.ToString)
        End Try

        DataGridView1.Refresh()
    End Sub

    Sub ClearForm()
        BunifuTextBox_Notification.FillColor = Color.DarkGray
        BunifuTextBox_Notification.IconLeft = My.Resources.noticealert_icon
        BunifuTextBox_Notification.Text = "Notice: Please Fill up the following."
        BunifuTextBox_FullName.Text = ""
        BunifuTextBox_FullName.IconRight = Nothing
        BunifuTextBox_FirstName.Text = ""
        BunifuTextBox_FirstName.IconRight = Nothing
        BunifuTextBox_LastName.Text = ""
        BunifuTextBox_LastName.IconRight = Nothing
        BunifuDropdown_Program.Items.Insert(0, "")
        BunifuDropdown_Program.SelectedIndex = 0
        BunifuDropdown_Program.Text = ""
        BunifuDropdown_Program.Items.RemoveAt(0)
        PictureBox_AlertProgram.Visible = False
        BunifuDropdown_Program.SelectedItem = "1st year"
        DomainUpDown_Year.Hide()
        BunifuTextBox_ContactNum.Text = ""
        BunifuTextBox_ContactNum.IconRight = Nothing
        BunifuTextBox_Address.Text = ""
        BunifuTextBox_Address.IconRight = Nothing
        BunifuTextBox_Purpose.Text = ""
        BunifuTextBox_Purpose.IconRight = Nothing
        BunifuLabel_TempVal.Text = "METER"
        BunifuRadialGauge1.Value = 0
        BunifuTextBox_TimeIn.Text = ""
        BunifuTextBox_DateIn.Text = ""
        BunifuButton_ClearForm.Enabled = True
        DataGridView1.Enabled = True

        If My.Settings.isAutoRecord And DeviceIsOnline Then
            BunifuButton_CheckRecord.Text = "Save and Record"
            AutoRecordCount = -1
        End If
    End Sub

    Private Sub BunifuButton_ClearForm_Click(sender As Object, e As EventArgs) Handles BunifuButton_ClearForm.Click
        ClearForm()
    End Sub

    Private Sub BunifuButton_Click(sender As Object, e As EventArgs) Handles BunifuButton_CheckRecord.Click, BunifuButton_WaitRecord.Click
        BunifuTextBox_FullName.Text = BunifuTextBox_FullName.Text.TrimStart().TrimEnd()
        BunifuTextBox_FirstName.Text = BunifuTextBox_FirstName.Text.TrimStart().TrimEnd()
        BunifuTextBox_LastName.Text = BunifuTextBox_LastName.Text.TrimStart().TrimEnd()
        BunifuTextBox_ContactNum.Text = BunifuTextBox_ContactNum.Text.TrimStart().TrimEnd()
        BunifuTextBox_Address.Text = BunifuTextBox_Address.Text.TrimStart().TrimEnd()
        BunifuTextBox_Purpose.Text = BunifuTextBox_Purpose.Text.TrimStart().TrimEnd()

        If My.Settings.useFullName <> False Then
            visitorName = BunifuTextBox_FullName.Text
        Else
            If BunifuTextBox_FirstName.Text.Trim <> "" And BunifuTextBox_LastName.Text.Trim <> "" Then
                visitorName = BunifuTextBox_LastName.Text & ", " & BunifuTextBox_FirstName.Text
            Else
                visitorName = ""
            End If
        End If

        If BunifuDropdown_Program.SelectedItem <> "" Then
            visitorProgram = BunifuDropdown_Program.SelectedItem
            If DomainUpDown_Year.Visible = True Then visitorProgram &= " " & DomainUpDown_Year.Text
        Else
            visitorProgram = ""
        End If

        If BunifuTextBox_FullName.Text = "" Then BunifuTextBox_FullName.IconRight = My.Resources.alert_icon
        If BunifuTextBox_FirstName.Text = "" Then BunifuTextBox_FirstName.IconRight = My.Resources.alert_icon
        If BunifuTextBox_LastName.Text = "" Then BunifuTextBox_LastName.IconRight = My.Resources.alert_icon

        If sender.Equals(BunifuButton_CheckRecord) Then
            BunifuButton_CheckRecord.Text = "Save and Record"
            AutoRecordCount = -1

            If BunifuDropdown_Program.Text = "" Then PictureBox_AlertProgram.Visible = True
            If BunifuTextBox_ContactNum.Text = "" Then BunifuTextBox_ContactNum.IconRight = My.Resources.alert_icon
            If BunifuTextBox_Address.Text = "" Then BunifuTextBox_Address.IconRight = My.Resources.alert_icon
            If BunifuTextBox_Purpose.Text = "" Then BunifuTextBox_Purpose.IconRight = My.Resources.alert_icon

            If visitorName.Trim = "" Or
            visitorProgram.Trim = "" Or
            BunifuTextBox_ContactNum.Text.Trim = "" Or
            BunifuTextBox_Address.Text.Trim = "" Or
            BunifuTextBox_Purpose.Text.Trim = "" Then
                BunifuTextBox_Notification.Visible = True
                BunifuTextBox_Notification.FillColor = Color.FromArgb(255, 128, 0)
                BunifuTextBox_Notification.IconLeft = My.Resources.noticealert_icon
                BunifuTextBox_Notification.Text = "Notice: Please Fill up all neccessary requirements!"
                Return
            End If

            BunifuButton_CheckRecord.Enabled = False
            BackgroundWorker1.RunWorkerAsync()
        End If

        If sender.Equals(BunifuButton_WaitRecord) Then
            If BunifuDropdown_Program.Text = "" Then PictureBox_AlertProgram.Visible = True

            If visitorName.Trim = "" Or visitorProgram.Trim = "" Then
                BunifuTextBox_Notification.Visible = True
                BunifuTextBox_Notification.FillColor = Color.FromArgb(255, 128, 0)
                BunifuTextBox_Notification.IconLeft = My.Resources.noticealert_icon
                BunifuTextBox_Notification.Text = "Notice: Please Fill up all neccessary requirements!"
                Return
            End If

            CheckTblWtngIfExist(True)

            CommandDBQuery("INSERT INTO `tbl_waitinglist` (`visitor_id`, 
                        `visitor_name`, `visitor_program`, `visitor_contact`, 
                        `visitor_address`, `visitor_purpose`, `visitor_temp`, 
                        `visitor_datetimein`) VALUES (NULL, 
                        '" & visitorName & "', '" & visitorProgram & "',
                        '" & BunifuTextBox_ContactNum.Text & "', '" & BunifuTextBox_Address.Text & "', 
                        '" & BunifuTextBox_Purpose.Text & "', '" & visitorTemp & "', 
                        '" & DateTime.Now.ToString("yy-MM-dd HH:mm:ss") & "');")

            AdapterDBQuery("SELECT `visitor_id`, `visitor_name`, `visitor_temp`, `visitor_datetimein` FROM `tbl_waitinglist`", "waitTbl")

            TableUpdater()
            ClearForm()
        End If
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        If e.ColumnIndex = 4 Then
            AdapterDBQuery("SELECT `visitor_id`, `visitor_name`, `visitor_program`, `visitor_contact`, 
                        `visitor_address`, `visitor_purpose`, `visitor_temp`, 
                        `visitor_datetimein` FROM `tbl_waitinglist` WHERE `tbl_waitinglist`.`visitor_id` = " & DataGridView1.Item(0, e.RowIndex).Value, "waitTblSelected")
            If My.Settings.useFullName Then
                'BunifuTextBox_FullName.Text = DataGridView1.Item(1, e.RowIndex).Value
                BunifuTextBox_FullName.Text = dtSet.Tables("waitTblSelected").Rows(0).Item(1).ToString
                BunifuTextBox_FullName.Enabled = False
            Else
                'Dim Names() As String = Split(DataGridView1.Item(1, e.RowIndex).Value, ",")
                Dim Names() As String = Split(dtSet.Tables("waitTblSelected").Rows(0).Item(1).ToString, ",")
                BunifuTextBox_FirstName.Text = Names(1).TrimStart.TrimEnd
                BunifuTextBox_LastName.Text = Names(0).TrimStart.TrimEnd
                BunifuTextBox_FirstName.Enabled = False
                BunifuTextBox_LastName.Enabled = False
            End If
            'BunifuDropdown_Program.SelectedText = "BSCpE" 'dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString.Remove(dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString.Length - 5)
            'BunifuDropdown_Program.SelectedValue = "BSCpE"
            'BunifuLabel8.Text = dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString.Remove(0, (dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString.Length - 9)).Remove(1)
            'DomainUpDown_Year.SelectedIndex = dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString.Remove(0, (dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString.Length - 5).ToString.Count)
            If BunifuDropdown_Program.SelectedItem = "Not a Student" Or
               BunifuDropdown_Program.SelectedItem = "Staff" Then
                DomainUpDown_Year.Show()
            Else
                DomainUpDown_Year.Hide()
            End If
            BunifuTextBox_ContactNum.Text = dtSet.Tables("waitTblSelected").Rows(0).Item(3).ToString
            BunifuTextBox_Address.Text = dtSet.Tables("waitTblSelected").Rows(0).Item(4).ToString
            BunifuTextBox_Purpose.Text = dtSet.Tables("waitTblSelected").Rows(0).Item(5).ToString
            DomainUpDown_Year.Enabled = False
            BunifuTextBox_ContactNum.Enabled = False
            BunifuTextBox_Address.Enabled = False
            BunifuTextBox_Purpose.Enabled = False
            'If My.Settings.displaySeconds Then
            '    BunifuTextBox_TimeIn.Text = DateTime.Parse(dtSet.Tables("waitTblSelected").Rows(0).Item(7).ToString).ToString("h:mm:ss tt").ToUpper
            'Else
            '    BunifuTextBox_TimeIn.Text = DateTime.Parse(dtSet.Tables("waitTblSelected").Rows(0).Item(7).ToString).ToString("h:mm tt").ToUpper
            'End If
            'BunifuTextBox_DateIn.Text = DateTime.Parse(dtSet.Tables("waitTblSelected").Rows(0).Item(7).ToString).ToString("MM/dd/yy")
            CommandDBQuery("DELETE FROM `tbl_waitinglist` WHERE `tbl_waitinglist`.`visitor_id` = " & DataGridView1.Item(0, e.RowIndex).Value & ";")
            DataGridView1.Rows.RemoveAt(e.RowIndex)
            BunifuButton_ClearForm.Enabled = False
            DataGridView1.Enabled = False
        End If
        If e.ColumnIndex = 5 Then
            AdapterDBQuery("SELECT `visitor_id`, `visitor_name`, `visitor_program`, `visitor_contact`, 
                        `visitor_address`, `visitor_purpose`, `visitor_temp`, 
                        `visitor_datetimein` FROM `tbl_waitinglist` WHERE `tbl_waitinglist`.`visitor_id` = " & DataGridView1.Item(0, e.RowIndex).Value, "waitTblSelected")

            CommandDBQuery("INSERT INTO `tbl_logbook` (`visitor_id`, 
                        `visitor_name`, `visitor_program`, `visitor_contact`, 
                        `visitor_address`, `visitor_purpose`, `visitor_temp`, 
                        `visitor_datetimein`) VALUES (NULL, 
                        '" & dtSet.Tables("waitTblSelected").Rows(0).Item(1).ToString & "', '" & dtSet.Tables("waitTblSelected").Rows(0).Item(2).ToString & "',
                        '" & dtSet.Tables("waitTblSelected").Rows(0).Item(3).ToString & "', '" & dtSet.Tables("waitTblSelected").Rows(0).Item(4).ToString & "', 
                        '" & dtSet.Tables("waitTblSelected").Rows(0).Item(5).ToString & "', '" & dtSet.Tables("waitTblSelected").Rows(0).Item(6).ToString & "', 
                        '" & Date.Parse(dtSet.Tables("waitTblSelected").Rows(0).Item(7).ToString).ToString("yy-MM-dd HH:mm:ss") & "');")

            CommandDBQuery("DELETE FROM `tbl_waitinglist` WHERE `tbl_waitinglist`.`visitor_id` = " & DataGridView1.Item(0, e.RowIndex).Value & ";")
            DataGridView1.Rows.RemoveAt(e.RowIndex)

            'CommandDBQuery("ALTER TABLE `tbl_waitinglist` AUTO_INCREMENT = 1")
        End If
    End Sub

    'Private Sub DataGridView1_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellMouseEnter
    '    If e.ColumnIndex >= 4 Then
    '        DataGridView1.Cursor = Cursors.Hand
    '    Else
    '        DataGridView1.Cursor = Cursors.Default
    '    End If
    'End Sub

    Private Sub DataGridView1_LostFocus(sender As Object, e As EventArgs) Handles DataGridView1.LostFocus, DataGridView1.MouseLeave
        DataGridView1.ClearSelection()
    End Sub

    Private Sub DataGridView1_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles DataGridView1.CellPainting
        If e.RowIndex >= 0 AndAlso e.ColumnIndex = 4 Then

            e.PaintBackground(e.ClipBounds, True)

            'If e.CellBounds.Contains(DataGridView1.PointToClient(MousePosition)) Then
            '    Dim backColor As Color

            '    If MouseButtons = MouseButtons.Left Then
            '        backColor = Color.FromArgb(104, 104, 104)
            '    Else
            '        backColor = Color.FromArgb(84, 84, 84)
            '    End If

            '    Using br As New SolidBrush(backColor)
            '        e.Graphics.FillRectangle(br, e.CellBounds)
            '    End Using
            'End If

            e.Graphics.DrawImage(My.Resources.arrow_up_icon, New Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 5, e.CellBounds.Width - 10, e.CellBounds.Height - 10))
            e.Handled = True
        End If
        If e.RowIndex >= 0 AndAlso e.ColumnIndex = 5 Then

            e.PaintBackground(e.ClipBounds, True)

            'If e.CellBounds.Contains(DataGridView1.PointToClient(MousePosition)) Then
            '    Dim backColor As Color

            '    If MouseButtons = MouseButtons.Left Then
            '        backColor = Color.FromArgb(104, 104, 104)
            '    Else
            '        backColor = Color.FromArgb(84, 84, 84)
            '    End If

            '    Using br As New SolidBrush(backColor)
            '        e.Graphics.FillRectangle(br, e.CellBounds)
            '    End Using
            'End If

            e.Graphics.DrawImage(My.Resources.arrow_right_icon, New Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 5, e.CellBounds.Width - 10, e.CellBounds.Height - 10))
            e.Handled = True
        End If
    End Sub

    Private Sub BunifuTextBox_Name_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles BunifuTextBox_FirstName.KeyPress, BunifuTextBox_LastName.KeyPress, BunifuTextBox_FullName.KeyPress
        If e.KeyChar <> ControlChars.Back AndAlso e.KeyChar <> " " Then
            If Not Char.IsLetter(e.KeyChar) Then
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub BunifuTextBox_TextChanged(sender As Object, e As EventArgs) Handles BunifuTextBox_FirstName.TextChanged,
        BunifuTextBox_LastName.TextChanged, BunifuTextBox_FullName.TextChanged,
        BunifuTextBox_ContactNum.TextChanged, BunifuTextBox_Address.TextChanged, BunifuTextBox_Purpose.TextChanged

        If sender.Equals(BunifuTextBox_FirstName) Then
            BunifuTextBox_FirstName.IconRight = Nothing
            BunifuTextBox_FullName.IconRight = Nothing
        End If
        If sender.Equals(BunifuTextBox_LastName) Then
            BunifuTextBox_LastName.IconRight = Nothing
            BunifuTextBox_FullName.IconRight = Nothing
        End If
        If sender.Equals(BunifuTextBox_FullName) Then
            BunifuTextBox_FullName.IconRight = Nothing
            BunifuTextBox_FirstName.IconRight = Nothing
            BunifuTextBox_LastName.IconRight = Nothing
        End If
        If sender.Equals(BunifuTextBox_ContactNum) Then BunifuTextBox_ContactNum.IconRight = Nothing
        If sender.Equals(BunifuTextBox_Address) Then BunifuTextBox_Address.IconRight = Nothing
        If sender.Equals(BunifuTextBox_Purpose) Then BunifuTextBox_Purpose.IconRight = Nothing
        If (BunifuTextBox_FullName.Text.Trim <> "" Or
            (BunifuTextBox_FirstName.Text.Trim <> "" And
            BunifuTextBox_LastName.Text.Trim <> "")) And
            BunifuDropdown_Program.Text <> "" And
            BunifuTextBox_ContactNum.Text.Trim <> "" And
            BunifuTextBox_Address.Text.Trim <> "" And
            BunifuTextBox_Purpose.Text.Trim <> "" Then
            BunifuTextBox_Notification.FillColor = Color.DarkGray
            BunifuTextBox_Notification.IconLeft = My.Resources.noticealert_icon
            BunifuTextBox_Notification.Text = "Notice: Press green button to submit."
        End If
        If (BunifuTextBox_FullName.Text.Trim = "" AndAlso
            (BunifuTextBox_FirstName.Text.Trim = "" And
            BunifuTextBox_LastName.Text.Trim = "")) And
            BunifuDropdown_Program.Text = "" And
            BunifuTextBox_ContactNum.Text.Trim = "" And
            BunifuTextBox_Address.Text.Trim = "" And
            BunifuTextBox_Purpose.Text.Trim = "" Then
            If BunifuTextBox_TimeIn.Text.Trim <> "" Then BunifuTextBox_TimeIn.Text = ""
            If BunifuTextBox_DateIn.Text.Trim <> "" Then BunifuTextBox_DateIn.Text = ""
        Else
            If BunifuTextBox_TimeIn.Text = "" Or BunifuTextBox_DateIn.Text = "" Then
                If My.Settings.displaySeconds Then
                    BunifuTextBox_TimeIn.Text = DateTime.Now.ToString("h:mm:ss tt").ToUpper
                Else
                    BunifuTextBox_TimeIn.Text = DateTime.Now.ToString("h:mm tt").ToUpper
                End If
                BunifuTextBox_DateIn.Text = DateTime.Now.ToString("MM/dd/yy")
            End If
            If BunifuTextBox_Notification.FillColor = Color.MediumSeaGreen Then
                BunifuTextBox_Notification.FillColor = Color.DarkGray
                BunifuTextBox_Notification.IconLeft = My.Resources.noticealert_icon
                BunifuTextBox_Notification.Text = "Notice: Please Fill up the following."
            End If
        End If
    End Sub

    Private Sub BunifuTextBox_Purpose_LostFocus(sender As Object, e As EventArgs) Handles BunifuTextBox_FullName.LostFocus,
        BunifuTextBox_FirstName.LostFocus, BunifuTextBox_LastName.LostFocus
        BunifuTextBox_FullName.Text = StrConv(BunifuTextBox_FullName.Text, vbProperCase)
        BunifuTextBox_FirstName.Text = StrConv(BunifuTextBox_FirstName.Text, vbProperCase)
        BunifuTextBox_LastName.Text = StrConv(BunifuTextBox_LastName.Text, vbProperCase)
    End Sub

    Private Sub BunifuTextBox_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles BunifuTextBox_ContactNum.KeyPress
        If InStr("1234567890", e.KeyChar) = 0 And Asc(e.KeyChar) <> 8 Then
            e.KeyChar = Chr(0)
            e.Handled = True
        End If
    End Sub

    Private Sub BunifuDropdown_SelectedIndexChanged(sender As Object, e As EventArgs) Handles BunifuDropdown_Program.SelectedIndexChanged
        If sender.Equals(BunifuDropdown_Program) Then
            PictureBox_AlertProgram.Visible = False
            BunifuTextBox_TextChanged(sender, e)
            If BunifuDropdown_Program.SelectedItem = "BSCpE" Or
               BunifuDropdown_Program.SelectedItem = "BSEE" Or
               BunifuDropdown_Program.SelectedItem = "BSECE" Then
                DomainUpDown_Year.Items.Clear()
                If Int32.Parse(DateTime.Now.ToString("yyyyMM")) < 202206 Then DomainUpDown_Year.Items.Add("5th year")
                DomainUpDown_Year.Items.AddRange(New String() {"4th year", "3rd year", "2nd year", "1st year"})
                DomainUpDown_Year.Show()
            ElseIf BunifuDropdown_Program.SelectedItem = "Others" Or
               BunifuDropdown_Program.SelectedItem = "Staff" Then
                DomainUpDown_Year.Hide()
            Else
                If DomainUpDown_Year.Items.Count <> 4 Then
                    DomainUpDown_Year.Items.Clear()
                    DomainUpDown_Year.Items.AddRange(New String() {"4th year", "3rd year", "2nd year", "1st year"})
                    If DomainUpDown_Year.Text = "5th year" Then DomainUpDown_Year.Text = "4th year"
                End If
                DomainUpDown_Year.Show()
            End If
        End If
    End Sub

    Private Sub BackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        CheckDBnTblIfExist(True)

        CommandDBQuery("INSERT INTO `tbl_logbook` (`visitor_id`, 
                        `visitor_name`, `visitor_program`, `visitor_contact`, 
                        `visitor_address`, `visitor_purpose`, `visitor_temp`, 
                        `visitor_datetimein`) VALUES (NULL, 
                        '" & visitorName & "', '" & visitorProgram & "',
                        '" & BunifuTextBox_ContactNum.Text & "', '" & BunifuTextBox_Address.Text & "', 
                        '" & BunifuTextBox_Purpose.Text & "', '" & visitorTemp & "', 
                        '" & DateTime.Now.ToString("yy-MM-dd HH:mm:ss") & "');")
    End Sub

    Private Sub BackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If SerialPort1.IsOpen And visitorTemp <= My.Settings.tempFeverVal Then
            SerialPort1.WriteLine("bmOpen")
        End If
        ClearForm()
        BunifuTextBox_Notification.FillColor = Color.MediumSeaGreen
        BunifuTextBox_Notification.IconLeft = My.Resources.correctalert_icon
        BunifuTextBox_Notification.Text = "Notice: Visitor Information has been Recorded successfully!"
        BunifuButton_CheckRecord.Enabled = True
    End Sub

    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        SerialDataValueRead = SerialPort1.ReadLine
        ReceivedSerialData(SerialDataValueRead)
    End Sub

    Delegate Sub SerialDataCallback(ByVal [text] As String)
    Private Sub ReceivedSerialData(ByVal [text] As String)
        'Dim dataString() As String = [text].Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim dataString() As String = [text].Split({";"c})

        Try
            If InvokeRequired Then
                Invoke(New SerialDataCallback(AddressOf ReceivedSerialData), New Object() {[text]})
            Else
                If dataString(0) = "2" Then
                    visitorCount += Integer.Parse(dataString(1))
                    If visitorCount < 0 Then visitorCount = 0
                    If My.Settings.useVisitorCapacity Then
                        If visitorCount >= My.Settings.visitorCapacity Then
                            visitorCount = My.Settings.visitorCapacity
                            SerialPort1.WriteLine("vCountf1")
                        Else
                            SerialPort1.WriteLine("vCountf0")
                        End If
                        BunifuLabel_VisitCount.Text = visitorCount & "/" & My.Settings.visitorCapacity
                    Else
                            BunifuLabel_VisitCount.Text = visitorCount
                    End If
                    BunifuLabel_VisitCount.Left = ((BunifuLabel_VisitCount.Parent.Width / 2) - (BunifuLabel_VisitCount.Width / 2)) - (BunifuLabel_VisitCount.Width / 2) + 90
                    'BunifuPictureBox_VisitorIcon.Left = ((BunifuPictureBox_VisitorIcon.Parent.Width / 2) - (BunifuPictureBox_VisitorIcon.Width / 2)) + (BunifuPictureBox_VisitorIcon.Width / 2) + 47
                End If
                If dataString(0) = "1" Then
                    visitorTemp = Double.Parse(dataString(1))
                    BunifuRadialGauge1.ValueByTransition = Double.Parse(dataString(1)).ToString("N0")
                    'visitorTemp = Double.Parse(dataString(1)) + My.Settings.tempAdjustment
                    'BunifuRadialGauge1.ValueByTransition = (Double.Parse(dataString(1)) + My.Settings.tempAdjustment).ToString("N0")
                    tempCounter = 0
                    TempUpdater.Start()
                    'BunifuLabel_TempVal.Text = (Double.Parse(dataString(1)) + My.Settings.tempAdjustment).ToString("N1") & "°C"
                    'If Double.Parse(Double.Parse(dataString(1)) + My.Settings.tempAdjustment).ToString("N1") < My.Settings.tempFeverVal Then
                    '    BunifuButton_CheckRecord.Enabled = True
                    'Else
                    '    BunifuButton_CheckRecord.Enabled = False
                    'End If
                End If
                If dataString(0) = "3" Then
                    If My.Settings.useFullName Then
                        BunifuTextBox_FullName.Text = dataString(2) & ", " & dataString(1)
                    Else
                        BunifuTextBox_FirstName.Text = dataString(1)
                        BunifuTextBox_LastName.Text = dataString(2)
                    End If
                    BunifuDropdown_Program.SelectedIndex = Integer.Parse(dataString(3))
                    DomainUpDown_Year.SelectedIndex = Integer.Parse(dataString(4))
                    BunifuTextBox_ContactNum.Text = dataString(5)
                    BunifuTextBox_Address.Text = dataString(6)
                    BunifuTextBox_Purpose.Text = dataString(7)
                    If My.Settings.isAutoRecord And DeviceIsOnline Then AutoRecordCount = My.Settings.autoRecordCount
                End If
            End If
        Catch ex As Exception
            'MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub Form_Main_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        End
    End Sub

    Private Sub Form_Main_Closing(sender As Object, e As FormClosingEventArgs) Handles Me.Closing
        If (e.CloseReason = CloseReason.UserClosing Or e.CloseReason = CloseReason.TaskManagerClosing) Then
            e.Cancel = True
        End If
    End Sub
End Class