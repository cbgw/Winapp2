﻿Option Strict On
'    Copyright (C) 2018-2020 Robbie Ward
' 
'    This file is a part of Winapp2ool
' 
'    Winapp2ool is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    Winapp2ool is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with Winapp2ool.  If not, see <http://www.gnu.org/licenses/>.
Imports System.Globalization
''' <summary>
''' This module contains functions that allow the user to reach online resources. 
''' Its primary user-facing functionality is to present the list of downloads from the GitHub to the user
''' </summary>
Module Downloader
    ''' <summary> The web address of the winapp2.ini GitHub </summary>
    Public ReadOnly Property gitLink As String = "https://github.com/MoscaDotTo/Winapp2/"
    '''<summary> The web address of the CCleaner version of winapp2.ini </summary>
    Public ReadOnly Property wa2Link As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/Winapp2.ini"
    '''<summary> The web address of the Non-CCleaner version of winapp2.ini </summary>
    Public ReadOnly Property nonccLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/Non-CCleaner/Winapp2.ini"
    '''<summary> The web address of winapp2ool.exe </summary>
    Public ReadOnly Property toolLink As String = "https://github.com/MoscaDotTo/Winapp2/raw/master/winapp2ool/bin/Release/winapp2ool.exe"
    '''<summary> The web address of the beta build of winapp2ool.exe </summary>
    Public ReadOnly Property betaToolLink As String = "https://github.com/MoscaDotTo/Winapp2/raw/Branch1/winapp2ool/bin/Release/winapp2ool.exe"
    '''<summary> The web address of version.txt (winapp2ool's public version identifer) </summary>
    Public ReadOnly Property toolVerLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/winapp2ool/version.txt"
    ''' <summary> The web address of version.txt on the beta branch </summary>
    Public ReadOnly Property betaToolVerLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/Branch1/winapp2ool/version.txt"
    '''<summary> The web address of removed entries.ini </summary>
    Public ReadOnly Property removedLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/Non-CCleaner/Removed%20entries.ini"
    '''<summary> The web address of winapp3.ini </summary>
    Public ReadOnly Property wa3link As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/Winapp3/Winapp3.ini"
    '''<summary> The web address of archived entries.ini </summary>
    Public ReadOnly Property archivedLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/Winapp3/Archived%20entries.ini"
    '''<summary> The web address of java.ini </summary>
    Public ReadOnly Property javaLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/Winapp3/java.ini"
    '''<summary> The web address of the winapp2ool ReadMe file </summary>
    Public ReadOnly Property readMeLink As String = "https://raw.githubusercontent.com/MoscaDotTo/Winapp2/master/winapp2ool/Readme.md"
    '''<summary> Holds the path of any files to be saved by the Downloader </summary>
    Public Property downloadFile As iniFile = New iniFile(Environment.CurrentDirectory, "")

    ''' <summary> Indicates that the Downloader module's settings have been changed from their defaults </summary>
    Private Property ModuleSettingsChanged As Boolean = False

    ''' <summary> Restores the default state of the module's parameters </summary>
    Private Sub initDefaultSettings()
        downloadFile.resetParams()
        ModuleSettingsChanged = False
        restoreDefaultSettings(NameOf(Downloader), AddressOf createDownloadSettingsSection)
    End Sub

    ''' <summary> Loads values from disk into memory for the Downloader module settings </summary>
    Public Sub getSerializedDownloaderSettings()
        For Each kvp In settingsDict(NameOf(Downloader))
            Select Case kvp.Key
                Case NameOf(downloadFile) & "_Dir"
                    downloadFile.Dir = kvp.Value
                Case NameOf(ModuleSettingsChanged)
                    ModuleSettingsChanged = CBool(kvp.Value)
            End Select
        Next
    End Sub

    ''' <summary> Adds the current (typically default) state of the module's settings into the disk-writable settings representation </summary>
    Public Sub createDownloadSettingsSection()
        createModuleSettingsSection(NameOf(Downloader), {
                    getSettingIniKey(NameOf(Downloader), NameOf(downloadFile), downloadFile.Dir, isDir:=True),
                    getSettingIniKey(NameOf(Downloader), NameOf(ModuleSettingsChanged), ModuleSettingsChanged.ToString(CultureInfo.InvariantCulture))
        })
    End Sub

    ''' <summary> Handles the commandline args for the Downloader </summary>
    Public Sub handleCmdLine()
        Dim fileLink = ""
        If cmdargs.Count > 0 Then
            Select Case cmdargs(0).ToUpperInvariant
                Case "1", "2", "WINAPP2"
                    fileLink = If(Not cmdargs(0) = "2", wa2Link, nonccLink)
                    downloadFile.Name = "winapp2.ini"
                Case "3", "WINAPP2OOL"
                    fileLink = toolLink
                    downloadFile.Name = "winapp2ool.exe"
                Case "4", "REMOVED"
                    fileLink = removedLink
                    downloadFile.Name = "Removed Entries.ini"
                Case "5", "WINAPP3"
                    fileLink = wa3link
                    downloadFile.Name = "winapp3.ini"
                Case "6", "ARCHIVED"
                    fileLink = archivedLink
                    downloadFile.Name = "Archived Entries.ini"
                Case "7", "JAVA"
                    fileLink = javaLink
                    downloadFile.Name = "java.ini"
                Case "8", "README"
                    fileLink = readMeLink
                    downloadFile.Name = "readme.txt"
            End Select
            cmdargs.RemoveAt(0)
        End If
        getFileAndDirParams(downloadFile, New iniFile, New iniFile)
        If downloadFile.Dir = Environment.CurrentDirectory And downloadFile.Name = "winapp2ool.exe" Then autoUpdate()
        download(downloadFile, fileLink)
    End Sub

    ''' <summary> Prints the download menu to the user </summary>
    Public Sub printMenu()
        printMenuTop({"Download files from the winapp2 GitHub"})
        print(1, "Winapp2.ini", "Download the latest winapp2.ini")
        print(1, "Non-CCleaner", "Download the latest non-ccleaner winapp2.ini")
        print(1, "Winapp2ool", "Download the latest winapp2ool.exe")
        print(1, "Removed Entries.ini", "Download only entries used to create the non-ccleaner winapp2.ini", leadingBlank:=True)
        print(1, "Directory", "Change the save directory", trailingBlank:=True)
        print(1, "Advanced", "Settings for power users")
        print(1, "ReadMe", "The winapp2ool ReadMe")
        print(0, $"Save directory: {replDir(downloadFile.Dir)}", leadingBlank:=True, closeMenu:=Not ModuleSettingsChanged)
        print(2, NameOf(Downloader), cond:=ModuleSettingsChanged, closeMenu:=True)
    End Sub

    ''' <summary> Handles user input for the Downloader menu </summary>
    ''' <param name="input"> The user's input </param>
    Public Sub handleUserInput(input As String)
        Select Case input
            Case "0"
                exitModule()
            Case "1", "2"
                downloadFile.Name = "winapp2.ini"
                Dim link = If(input = "1", wa2Link, nonccLink)
                download(downloadFile, link)
                If downloadFile.Dir = Environment.CurrentDirectory Then checkedForUpdates = False
            Case "3"
                ' Feature gate downloading the executable behind .NET 4.6+
                If Not denyActionWithHeader(DotNetFrameworkOutOfDate, "This option requires a newer version of the .NET Framework") Then
                    If Not denyActionWithHeader(cantDownloadExecutable And downloadFile.Dir = Environment.CurrentDirectory,
                                                "Unable to download winapp2ool to the current directory, choose another directory before trying again") Then
                        If downloadFile.Dir = Environment.CurrentDirectory Then
                            autoUpdate()
                        Else
                            downloadFile.Name = "winapp2ool.exe"
                            download(downloadFile, toolExeLink)
                        End If
                    End If
                End If
            Case "4"
                downloadFile.Name = "Removed entries.ini"
                download(downloadFile, removedLink)
            Case "5"
                Dim tmp = downloadFile.Dir
                initModule("Directory Chooser", AddressOf downloadFile.printDirChooserMenu, AddressOf downloadFile.handleDirChooserInput)
                Dim headerTxt = "Directory change aborted"
                If Not tmp = downloadFile.Dir Then
                    headerTxt = "Save directory changed"
                    updateSettings(NameOf(Downloader), NameOf(downloadFile) & "_Dir", downloadFile.Dir)
                    ModuleSettingsChanged = True
                    updateSettings(NameOf(Downloader), NameOf(ModuleSettingsChanged), ModuleSettingsChanged.ToString(CultureInfo.InvariantCulture))
                    saveSettingsFile()
                End If
                setHeaderText(headerTxt)
            Case "6"
                initModule("Advanced Downloads", AddressOf printAdvMenu, AddressOf handleAdvInput)
            Case "7"
                ' It's actually a .md but the user doesn't need to know that  
                downloadFile.Name = "Readme.txt"
                download(downloadFile, readMeLink)
            Case "8"
                If ModuleSettingsChanged Then initDefaultSettings()
            Case Else
                setHeaderText(invInpStr, True)
        End Select
    End Sub

    ''' <summary> Returns the link to winapp2.ini of the apprpriate flavor for the current tool configuration </summary>
    Public Function winapp2link() As String
        Return If(RemoteWinappIsNonCC, nonccLink, wa2Link)
    End Function

    '''<summary> Returns the link to winapp2ool.exe on the appropriate branch for the current executable </summary>
    Public Function toolExeLink() As String
        Return If(isBeta, betaToolLink, toolLink)
    End Function

    ''' <summary> Returns the link to version.txt on the appropriate branch for the current executable </summary>
    Public Function toolVerTxtLink() As String
        Return If(isBeta, betaToolVerLink, toolVerLink)
    End Function

    ''' <summary> Returns the online download status (name) of winapp2.ini as a <c> String </c>, empty string if not downloading </summary>
    ''' <param name="shouldDownload"> Indicates that a module is configured to download a remote winapp2.ini </param>
    Public Function GetNameFromDL(shouldDownload As Boolean) As String
        Return If(shouldDownload, If(RemoteWinappIsNonCC, "Online (Non-CCleaner)", "Online"), "")
    End Function
End Module