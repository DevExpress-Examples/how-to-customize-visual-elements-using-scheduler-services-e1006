﻿Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports DevExpress.Web.ASPxScheduler
Imports DevExpress.XtraScheduler
Imports DevExpress.Web.ASPxScheduler.Internal
Imports DevExpress.XtraScheduler.Services
Imports DevExpress.Web.ASPxScheduler.Services

Partial Public Class _Default
    Inherits System.Web.UI.Page

    Private ReadOnly Property Storage() As ASPxSchedulerStorage
        Get
            Return Me.ASPxScheduler1.Storage
        End Get
    End Property
    Public Shared RandomInstance As New Random()

    Private prevTimeRulerFormatStringService As ITimeRulerFormatStringService
    Private customTimeRulerFormatStringService As CustomTimeRulerFormatStringService
    Private prevAppointmentFormatStringService As IAppointmentFormatStringService
    Private customAppointmentFormatStringService As CustomAppointmentFormatStringService
    Private prevHeaderCaptionService As IHeaderCaptionService
    Private customHeaderCaptionService As CustomHeaderCaptionService
    Private prevHeaderToolTipService As IHeaderToolTipService
    Private customHeaderToolTipService As CustomHeaderToolTipService



    Protected Overrides Sub OnInit(ByVal e As EventArgs)
        MyBase.OnInit(e)
        CreateAppointmentFormatStringService()
        CreateTimeRulerFormatStringService()
        CreateHeaderCaptionService()
        CreateHeaderToolTipService()


        ASPxScheduler1.RemoveService(GetType(IAppointmentFormatStringService))
        ASPxScheduler1.AddService(GetType(IAppointmentFormatStringService), customAppointmentFormatStringService)

        ASPxScheduler1.RemoveService(GetType(ITimeRulerFormatStringService))
        ASPxScheduler1.AddService(GetType(ITimeRulerFormatStringService), customTimeRulerFormatStringService)

        ASPxScheduler1.RemoveService(GetType(IHeaderCaptionService))
        ASPxScheduler1.AddService(GetType(IHeaderCaptionService), customHeaderCaptionService)

        ASPxScheduler1.RemoveService(GetType(IHeaderToolTipService))
        ASPxScheduler1.AddService(GetType(IHeaderToolTipService), customHeaderToolTipService)

    End Sub
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

        SetupMappings()
        ResourceFiller.FillResources(Me.ASPxScheduler1.Storage, 3)

        ASPxScheduler1.AppointmentDataSource = appointmentDataSource
        ASPxScheduler1.DataBind()


    End Sub
    #Region "Service Creation"
    Public Sub CreateAppointmentFormatStringService()
        Me.prevAppointmentFormatStringService = DirectCast(ASPxScheduler1.GetService(GetType(IAppointmentFormatStringService)), IAppointmentFormatStringService)
        Me.customAppointmentFormatStringService = New CustomAppointmentFormatStringService(prevAppointmentFormatStringService)
    End Sub
    Public Sub CreateTimeRulerFormatStringService()
        Me.prevTimeRulerFormatStringService = DirectCast(ASPxScheduler1.GetService(GetType(ITimeRulerFormatStringService)), ITimeRulerFormatStringService)
        Me.customTimeRulerFormatStringService = New CustomTimeRulerFormatStringService(prevTimeRulerFormatStringService)
    End Sub
    Public Sub CreateHeaderCaptionService()
        Me.prevHeaderCaptionService = DirectCast(ASPxScheduler1.GetService(GetType(IHeaderCaptionService)), IHeaderCaptionService)
        Me.customHeaderCaptionService = New CustomHeaderCaptionService(prevHeaderCaptionService)
    End Sub
    Public Sub CreateHeaderToolTipService()
        Me.prevHeaderToolTipService = DirectCast(ASPxScheduler1.GetService(GetType(IHeaderToolTipService)), IHeaderToolTipService)
        Me.customHeaderToolTipService = New CustomHeaderToolTipService(prevHeaderToolTipService)
    End Sub
    #End Region



    #Region "Common Procedures"
    #Region "Data Fill"


    Private Function GetCustomEvents() As CustomEventList

        Dim events_Renamed As CustomEventList = TryCast(Session("ListBoundModeObjects"), CustomEventList)
        If events_Renamed Is Nothing Then
            events_Renamed = GenerateCustomEventList()
            Session("ListBoundModeObjects") = events_Renamed
        End If
        Return events_Renamed
    End Function

    #Region "Random events generation"
    Private Function GenerateCustomEventList() As CustomEventList
        Dim eventList As New CustomEventList()
        Dim count As Integer = Storage.Resources.Count
        For i As Integer = 0 To count - 1
            Dim resource As Resource = Storage.Resources(i)
            Dim subjPrefix As String = resource.Caption & "'s "

            eventList.Add(CreateEvent(resource.Id, subjPrefix & "meeting", 2, 5))
            eventList.Add(CreateEvent(resource.Id, subjPrefix & "travel", 3, 6))
            eventList.Add(CreateEvent(resource.Id, subjPrefix & "phone call", 0, 10))
        Next i
        Return eventList
    End Function
    Private Function CreateEvent(ByVal resourceId As Object, ByVal subject As String, ByVal status As Integer, ByVal label As Integer) As CustomEvent
        Dim customEvent As New CustomEvent()
        customEvent.Subject = subject
        customEvent.OwnerId = resourceId
        Dim rnd As Random = RandomInstance
        Dim rangeInHours As Integer = 48
        customEvent.StartTime = Date.Today + TimeSpan.FromHours(rnd.Next(0, rangeInHours))
        customEvent.EndTime = customEvent.StartTime.Add(TimeSpan.FromHours(rnd.Next(0, rangeInHours \ 8)))
        customEvent.Status = status
        customEvent.Label = label
        customEvent.Id = "ev" & customEvent.GetHashCode()
        Return customEvent
    End Function
    #End Region


    Private Sub SetupMappings()
        Dim mappings As ASPxAppointmentMappingInfo = Storage.Appointments.Mappings
        Storage.BeginUpdate()
        Try
            mappings.AppointmentId = "Id"
            mappings.Start = "StartTime"
            mappings.End = "EndTime"
            mappings.Subject = "Subject"
            mappings.AllDay = "AllDay"
            mappings.Description = "Description"
            mappings.Label = "Label"
            mappings.Location = "Location"
            mappings.RecurrenceInfo = "RecurrenceInfo"
            mappings.ReminderInfo = "ReminderInfo"
            mappings.ResourceId = "OwnerId"
            mappings.Status = "Status"
            mappings.Type = "EventType"
        Finally
            Storage.EndUpdate()
        End Try
    End Sub
    #End Region


    Protected Sub ASPxScheduler1_AppointmentFormShowing(ByVal sender As Object, ByVal e As AppointmentFormEventArgs)
        e.Container = New MyAppointmentFormTemplateContainer(DirectCast(sender, ASPxScheduler))

    End Sub

    Protected Sub ASPxScheduler1_AppointmentInserting(ByVal sender As Object, ByVal e As PersistentObjectCancelEventArgs)

        Dim storage_Renamed As ASPxSchedulerStorage = DirectCast(sender, ASPxSchedulerStorage)
        Dim apt As Appointment = CType(e.Object, Appointment)
        storage_Renamed.SetAppointmentId(apt, "a" & apt.GetHashCode())
    End Sub

    Protected Sub appointmentsDataSource_ObjectCreated(ByVal sender As Object, ByVal e As ObjectDataSourceEventArgs)
        e.ObjectInstance = New CustomEventDataSource(GetCustomEvents())
    End Sub
    #End Region

    Protected Sub ASPxScheduler1_BeforeExecuteCallbackCommand(ByVal sender As Object, ByVal e As SchedulerCallbackCommandEventArgs)
        If e.CommandId = SchedulerCallbackCommandId.AppointmentSave Then
            e.Command = New MyAppointmentSaveCallbackCommand(DirectCast(sender, ASPxScheduler))
        End If
    End Sub

End Class
