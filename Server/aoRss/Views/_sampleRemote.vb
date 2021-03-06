﻿
Option Strict On
Option Explicit On


Imports Contensive.Addons.Rss
Imports Contensive.Addons.Rss.Controllers
Imports Contensive.BaseClasses
'
Namespace Views
    '
    Public Class GetProjectListNoDetailsClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        '
        Public Overrides Function Execute(ByVal cp As CPBaseClass) As Object
            Dim result As String = ""
            Dim sw As New Stopwatch : sw.Start()
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ae As New applicationController(cp, True)
                    '
                    ' -- optionally add a timer to report how long this section took
                    ae.packageProfileList.Add(New applicationController.packageProfileClass() With {.name = "applicationControllerConstructor", .time = sw.ElapsedMilliseconds})
                    If ae.packageErrorList.Count = 0 Then
                        '
                        ' -- get a request variable from either a querystring or a post
                        Dim integerValueFromUI As Integer = cp.Doc.GetInteger("integerValueFromUI")
                        '
                        ' -- get an object from the UI (javascript object stringified)
                        ' -- first inject the fake data to simpulate UI input, then read it
                        cp.Doc.SetProperty("objectValueFromUI", fakeData)
                        Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                        Dim objectValueFromUI As sampleStringifiedObject = json_serializer.Deserialize(Of sampleStringifiedObject)(cp.Doc.GetText("objectValueFromUI"))
                        Dim test As String = objectValueFromUI.firstname
                        '
                        ' -- create sample data
                        Dim personList As List(Of Rss.Models.Db.PeopleModel) = Models.Db.BaseModel.createList(Of Models.Db.PeopleModel)(cp, "", "")
                        '
                        ' -- add sample data to a node
                        ae.packageNodeList.Add(New applicationController.packageNodeClass With {
                            .dataFor = "nameOfThisDataForRemoteToRecognize",
                            .data = personList
                        })
                    End If
                    '
                    ' -- optionally add a timer to report how long this section took
                    ae.packageProfileList.Add(New applicationController.packageProfileClass() With {.name = "getProjectListNoDetailsClass", .time = sw.ElapsedMilliseconds})
                    result = ae.getSerializedPackage()
                End Using
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        Private Class sampleStringifiedObject
            Public firstname As String
            Public id As Integer
            Public friendList As List(Of String)
        End Class
        '
        Private fakeData As String = "{""firstname"":""nameData"", ""id"":""9"", ""friendList"":[""Tom"",""Dick"",""Harry""]}"
        '
    End Class
    '
    '
End Namespace
