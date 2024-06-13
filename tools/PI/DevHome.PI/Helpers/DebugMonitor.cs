﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using DevHome.Common.Extensions;
using DevHome.PI.Models;
using DevHome.PI.ViewModels;
using Microsoft.UI.Xaml;

namespace DevHome.PI.Helpers;

public sealed class DebugMonitor : IDisposable
{
    private readonly Process targetProcess;
    private readonly EventWaitHandle stopEvent;
    private readonly string errorMessageText = CommonHelper.GetLocalizedString("WinLogsAlreadyRunningErrorMessage");

    private const string MutexName = "DevHome.PI.DebugMonitor.SingletonMutex";
    private const string StopEventName = "DebugMonitorStopEvent";
    private const string DBWinBufferReadyName = "DBWIN_BUFFER_READY";
    private const string DBWinDataReadyName = "DBWIN_DATA_READY";
    private const string DBWinBufferName = "DBWIN_BUFFER";

    private static readonly List<string> IgnoreLogList = [];

    public DebugMonitor(Process targetProcess)
    {
        this.targetProcess = targetProcess;
        this.targetProcess.Exited += TargetProcess_Exited;

        stopEvent = new EventWaitHandle(false, EventResetMode.AutoReset, StopEventName);
    }

    public void Start()
    {
        stopEvent.Reset();

        // Don't initiate if debugger is attached. It makes debugging very slow.
        if (Debugger.IsAttached)
        {
            return;
        }

        // Check for multiple instances. It is possible to have multiple debug monitors listen on OutputDebugString,
        // but the message would be randomly distributed among all running instances.
        using var singletonMutex = new Mutex(false, MutexName, out var createdNew);
        if (!createdNew)
        {
            throw new InvalidOperationException($"Failed to get the {MutexName} mutex.");
        }

        bool isNewBufferReadyEvent;
        using var bufferReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, DBWinBufferReadyName, out isNewBufferReadyEvent);
        bool isNewDataReadyEvent;
        using var dataReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, DBWinDataReadyName, out isNewDataReadyEvent);

        // Don't initiate if there is an existing OutputDebugString monitor running
        if (!isNewBufferReadyEvent || !isNewDataReadyEvent)
        {
            var winlogsViewModel = Application.Current.GetService<WinLogsPageViewModel>();
            winlogsViewModel.AddNewEntry(DateTime.Now, WinLogCategory.Error, errorMessageText, WinLogsHelper.DebugOutputLogsName);
            return;
        }

        using var memoryMappedFile = MemoryMappedFile.CreateNew(DBWinBufferName, 4096);
        while (true)
        {
            bufferReadyEvent.Set();
            var waitResult = WaitHandle.WaitAny(new[] { stopEvent, dataReadyEvent });

            // Stop listenting to OutputDebugString if the debugger is attached. It makes debugging very slow.
            if (Debugger.IsAttached)
            {
                break;
            }

            // Stop event is triggered.
            if (waitResult == 0)
            {
                break;
            }

            if (waitResult == 1)
            {
                var timeGenerated = DateTime.Now;

                // The first DWORD of the shared memory buffer contains
                // the process ID of the client that sent the debug string.
                using var viewStream = memoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                using BinaryReader binaryReader = new BinaryReader(viewStream);
                var pid = binaryReader.ReadUInt32();

                if (pid == targetProcess.Id)
                {
                    // Get the message from the stream.
                    var stringBuilder = new StringBuilder();
                    while (binaryReader.PeekChar() != '\0')
                    {
                        stringBuilder.Append(binaryReader.ReadChar());
                    }

                    var entryMessage = stringBuilder.ToString();

                    if (!string.IsNullOrWhiteSpace(entryMessage))
                    {
                        var hasIgnoreLog = false;
                        foreach (var ignoreLog in IgnoreLogList)
                        {
                            if (entryMessage.Contains(ignoreLog))
                            {
                                hasIgnoreLog = true;
                            }
                        }

                        if (!hasIgnoreLog)
                        {
                            var winlogsViewModel = Application.Current.GetService<WinLogsPageViewModel>();
                            winlogsViewModel.AddNewEntry(timeGenerated, WinLogCategory.Debug, entryMessage, WinLogsHelper.DebugOutputLogsName);
                        }
                    }
                }
            }
        }
    }

    public void Stop()
    {
        if (!stopEvent.SafeWaitHandle.IsClosed)
        {
            stopEvent.Set();
        }
    }

    public void Dispose()
    {
        stopEvent.Close();
        stopEvent.Dispose();

        GC.SuppressFinalize(this);
    }

    private void TargetProcess_Exited(object? sender, EventArgs e)
    {
        Stop();
        Dispose();
    }
}
