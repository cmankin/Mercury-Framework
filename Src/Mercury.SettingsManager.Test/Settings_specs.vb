Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports System.Reflection
Imports Mercury.Settings
Imports System.Windows

<TestClass()>
Public Class Settings_specs

    Private testContextInstance As TestContext

    '''<summary>
    '''Gets or sets the test context which provides
    '''information about and functionality for the current test run.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    ' Use ClassInitialize to run code before running the first test in the class
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Use ClassCleanup to run code after all tests in a class have run
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Use TestInitialize to run code before running each test
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Use TestCleanup to run code after each test has run
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

    <TestMethod()>
    Public Sub Save_to_new_settings()
        ' Setup file path
        Dim filePath As String = String.Format("{0}\TestSettings.xml", GetExecutionPath())
        SettingsHostManager.OpenSettingsFile(filePath)

        ' Get data to save
        Dim winSection As WindowSettingsSection = New WindowSettingsSection()
        Dim winElement As WindowSettingsElement = New WindowSettingsElement()
        winElement.WindowLocation = New Point(10, 10)
        winElement.WindowState = WindowState.Normal
        winElement.WindowSize = New Size(800, 600)
        winSection.MainWindowSettings = winElement

        SettingsHostManager.Save("WindowSettings", winSection)

        ' Assert
        Assert.IsTrue(File.Exists(filePath))

        SettingsHostManager.RemoveSettings(filePath)
    End Sub

    <TestMethod()>
    Public Sub Save_and_read_section()
        ' Setup file path
        Dim filePath As String = String.Format("{0}\TestSettings.xml", GetExecutionPath())
        SettingsHostManager.OpenSettingsFile(filePath)

        ' Get data
        Dim winSection As WindowSettingsSection = GetWindowSettingsSectionConfig()

        ' Save
        SettingsHostManager.Save("WindowSettings", winSection)

        ' Assert
        Assert.IsTrue(File.Exists(filePath))

        ' Read
        Dim instance As Object = SettingsHostManager.GetSection("WindowSettings")
        Dim newSection As WindowSettingsSection = TryCast(instance, WindowSettingsSection)

        ' Assert
        Assert.IsNotNull(newSection)
        Assert.IsNotNull(newSection.MainWindowSettings)
        Assert.IsTrue(newSection.MainWindowSettings.WindowLocation = winSection.MainWindowSettings.WindowLocation)
        Assert.IsTrue(newSection.MainWindowSettings.WindowSize = winSection.MainWindowSettings.WindowSize)
        Assert.IsTrue(newSection.MainWindowSettings.WindowState = winSection.MainWindowSettings.WindowState)

        SettingsHostManager.RemoveSettings(filePath)
    End Sub

    <TestMethod()>
    Public Sub Save_multiple_sections_and_read()
        ' Setup file path
        Dim filePath As String = String.Format("{0}\TestSettings.xml", GetExecutionPath())
        SettingsHostManager.OpenSettingsFile(filePath)

        ' Get data
        Dim winSection As WindowSettingsSection = GetWindowSettingsSectionConfig()
        Dim querySection As QuerySettingsSection = GetQuerySettingsSectionConfig()

        ' Save
        SettingsHostManager.Save("WindowSettings", winSection)
        SettingsHostManager.Save("QuerySettings", querySection)

        ' Assert
        Assert.IsTrue(File.Exists(filePath))

        ' Read
        Dim instance As Object = SettingsHostManager.GetSection("QuerySettings")
        Dim newQuerySection As QuerySettingsSection = TryCast(instance, QuerySettingsSection)

        ' Assert
        Assert.IsNotNull(newQuerySection)
        Assert.IsNotNull(newQuerySection.Settings)
        Assert.IsTrue(newQuerySection.Settings.Query(1).Key = querySection.Settings.Query(1).Key)
        Assert.IsTrue(newQuerySection.Settings.Query(1).Timeout = querySection.Settings.Query(1).Timeout)

        ' Delete
        SettingsHostManager.RemoveSettings(filePath)
    End Sub

    <TestMethod()>
    Public Sub Update_section_on_settings_file()
        ' Setup file path
        Dim filePath As String = String.Format("{0}\TestSettings.xml", GetExecutionPath())
        SettingsHostManager.OpenSettingsFile(filePath)

        ' Get data
        Dim winSection As WindowSettingsSection = GetWindowSettingsSectionConfig()
        Dim querySection As QuerySettingsSection = GetQuerySettingsSectionConfig()

        ' Save
        SettingsHostManager.Append("WindowSettings", winSection)
        SettingsHostManager.Append("QuerySettings", querySection)
        SettingsHostManager.Save()

        ' Assert
        Assert.IsTrue(File.Exists(filePath))

        ' Read
        Dim instance As Object = SettingsHostManager.GetSection("QuerySettings")
        Dim newQuerySection As QuerySettingsSection = TryCast(instance, QuerySettingsSection)
        'CType(newQuerySection, ISettingsHost).Reset()

        ' Update field on query settings
        newQuerySection.Settings.Query(1).Timeout = 30

        ' Assert
        Assert.IsFalse(newQuerySection.IsCurrent)

        ' Save
        SettingsHostManager.Save()

        ' Read
        instance = SettingsHostManager.GetSection("QuerySettings")
        newQuerySection = TryCast(instance, QuerySettingsSection)

        ' Assert
        Assert.IsTrue(newQuerySection.IsCurrent)

        ' Delete
        SettingsHostManager.RemoveSettings(filePath)
    End Sub

    <TestMethod()>
    Public Sub Save_to_default_app_data_location()
        Dim appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        Dim relativePath = "\Mercury\Test\testSettings.xml"
        Dim fullPath = String.Format("{0}{1}", appDataPath, relativePath)

        ' Reset manager
        SettingsHostManager.Reset()
        SettingsHostManager.OpenSettingsFile(fullPath)

        ' Get window settings
        Dim sct As WindowSettingsSection = Nothing
        If Not (SettingsHostManager.IsInMemory("WindowSettings")) Then
            sct = SettingsHostManager.GetSection("WindowSettings")
            If sct Is Nothing Then
                sct = New WindowSettingsSection()
                sct.Invalidate()
                SettingsHostManager.Append("WindowSettings", sct)
            End If
        End If

        sct.MainWindowSettings = New WindowSettingsElement()
        sct.MainWindowSettings.WindowLocation = New Point(15, 15)
        sct.MainWindowSettings.WindowSize = New Size(1280, 720)
        sct.MainWindowSettings.WindowState = WindowState.Normal

        SettingsHostManager.Save()

        ' Assert
        Assert.IsTrue(File.Exists(SettingsHostManager.SettingsPath))

        ' Delete
        SettingsHostManager.RemoveSettings()

        ' Assert
        Assert.IsFalse(File.Exists(SettingsHostManager.SettingsPath))
    End Sub

    Private Function GetWindowSettingsSectionConfig() As WindowSettingsSection
        Dim winSection As WindowSettingsSection = New WindowSettingsSection()
        Dim winElement As WindowSettingsElement = New WindowSettingsElement()
        winElement.WindowLocation = New Point(10, 10)
        winElement.WindowState = WindowState.Normal
        winElement.WindowSize = New Size(800, 600)
        winSection.MainWindowSettings = winElement

        Return winSection
    End Function

    Private Function GetQuerySettingsSectionConfig() As QuerySettingsSection
        Dim querySection As QuerySettingsSection = New QuerySettingsSection()
        Dim queryElement As QuerySettingsElement = New QuerySettingsElement()
        queryElement.Key = "dax_table_permissions.xtpl"
        queryElement.Timeout = 60

        querySection.Settings.AddOrUpdate(queryElement)

        queryElement = New QuerySettingsElement()
        queryElement.Key = "axis_table_validator_hr.xtpl"
        queryElement.Timeout = 90

        querySection.Settings.AddOrUpdate(queryElement)

        Return querySection
    End Function

    Protected Function GetExecutionPath() As String
        Return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    End Function

End Class
