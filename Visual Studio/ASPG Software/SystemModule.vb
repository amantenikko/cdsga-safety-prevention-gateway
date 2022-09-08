Imports MySql.Data.MySqlClient
Module SystemModule
    Public con As New MySqlConnection
    Public cmd As New MySqlCommand
    Public mb As New MySqlBackup
    Public dtAdptr As New MySqlDataAdapter
    Public dtTbl As New DataTable
    Public dtSet As New DataSet

    Public DeviceIsOnline As Boolean

    Sub AppInit()
        If con.State = ConnectionState.Open Then con.Close()
        Try
            con.ConnectionString = "server='" & My.Settings.conStrgServer & "';username='" & My.Settings.conStrgUsername & "';password='" & My.Settings.conStrgPassword & "';database="
            con.Open()
            cmd.Connection = con
            cmd.CommandText = "CREATE DATABASE If Not EXISTS `db_cdsga_spgm`;"
            cmd.ExecuteNonQuery()
            My.Settings.conStrgDatabase = "db_cdsga_spgm"
            My.Settings.Save()
        Catch ex As Exception
            'MsgBox(ex.ToString)
        Finally
            con.Close()
        End Try
    End Sub

    Sub OpenDBCon()
        If con.State = ConnectionState.Open Then con.Close()
        Try
            If My.Settings.conStrgDatabase.Trim() = "" Then
                AppInit()
            End If
            con.ConnectionString = "server='" & My.Settings.conStrgServer & "';username='" & My.Settings.conStrgUsername & "';password='" & My.Settings.conStrgPassword & "';database='" & My.Settings.conStrgDatabase & "'"
            con.Open()
            'MsgBox("Success")
        Catch ex As Exception
            'MsgBox(ex.ToString)
        End Try
    End Sub

    Sub CheckDBnTblIfExist(willCreate As Boolean)
        AdapterDBQuery("SELECT * From information_schema.tables
                        Where table_schema = '" & My.Settings.conStrgDatabase & "'
                        And table_name = 'tbl_logbook' LIMIT 1;", "checkTbl")

        If dtSet.Tables("checkTbl").Rows.Count <= 0 And willCreate Then
            CommandDBQuery("CREATE TABLE `" & My.Settings.conStrgDatabase & "`.`tbl_logbook` ( `visitor_id` INT NOT NULL AUTO_INCREMENT ,
                        `visitor_name` VARCHAR(50) NOT NULL , `visitor_program` VARCHAR(50) NOT NULL , 
                        `visitor_contact` VARCHAR(50) NOT NULL , `visitor_address` VARCHAR(50) NOT NULL , 
                        `visitor_purpose` VARCHAR(50) NOT NULL , `visitor_temp` VARCHAR(10) NOT NULL , 
                        `visitor_datetimein` DATETIME NOT NULL , PRIMARY KEY (`visitor_id`));")
        Else
            'MsgBox("Table Exists")
        End If
    End Sub

    Sub CheckTblWtngIfExist(willCreate As Boolean)
        AdapterDBQuery("SELECT * From information_schema.tables
                        Where table_schema = '" & My.Settings.conStrgDatabase & "'
                        And table_name = 'tbl_waitinglist' LIMIT 1;", "checkTblWtng")

        If dtSet.Tables("checkTblWtng").Rows.Count <= 0 And willCreate Then
            CommandDBQuery("CREATE TABLE `" & My.Settings.conStrgDatabase & "`.`tbl_waitinglist` ( `visitor_id` INT NOT NULL AUTO_INCREMENT ,
                        `visitor_name` VARCHAR(50) NOT NULL , `visitor_program` VARCHAR(50) NOT NULL , 
                        `visitor_contact` VARCHAR(50) NOT NULL , `visitor_address` VARCHAR(50) NOT NULL , 
                        `visitor_purpose` VARCHAR(50) NOT NULL , `visitor_temp` VARCHAR(10) NOT NULL , 
                        `visitor_datetimein` DATETIME NOT NULL , PRIMARY KEY (`visitor_id`));")
        Else
            'MsgBox("Table Exists")
        End If
    End Sub

    Sub CommandDBQuery(queryStr As String)
        OpenDBCon()
        Try
            cmd.Connection = con
            cmd.CommandText = queryStr
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            'MsgBox(ex.ToString)
        Finally
            con.Close()
        End Try
    End Sub

    Sub AdapterDBQuery(queryStr As String, tblName As String)
        OpenDBCon()
        Try
            cmd.Connection = con
            cmd.CommandText = queryStr
            dtAdptr.SelectCommand = cmd
            dtSet.Tables(tblName).Clear()
            dtAdptr.Fill(dtSet, tblName)
        Catch ex As Exception
            'MsgBox(ex.ToString)
        Finally
            con.Close()
        End Try
    End Sub

    Sub AdapterDBQuery(queryStr As String, scrollVal As Integer, maxVal As Integer, tblName As String)
        OpenDBCon()
        Try
            cmd.Connection = con
            cmd.CommandText = queryStr
            dtAdptr.SelectCommand = cmd
            dtSet.Tables(tblName).Clear()
            dtAdptr.Fill(dtSet, scrollVal, maxVal, tblName)
        Catch ex As Exception
            AppInit()
            'MsgBox(ex.ToString)
            con.Close()
        End Try
    End Sub

    Sub DatabaseBackup(location As String, isRestore As Boolean)
        OpenDBCon()
        Try
            cmd.Connection = con
            mb.Command = cmd
            If isRestore Then
                mb.ImportFromFile(location)
            Else
                mb.ExportToFile(location)
            End If
        Catch ex As Exception
            'MsgBox(ex.ToString)
        Finally
            con.Close()
        End Try
    End Sub
End Module