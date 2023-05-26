// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Globalization;
using System.Text.Json.Nodes;
using CoreWidgetProvider.Helpers;
using CoreWidgetProvider.Widgets.Enums;
using Microsoft.Windows.Widgets.Providers;

namespace CoreWidgetProvider.Widgets;
internal class SystemNetworkUsageWidget : CoreWidget, IDisposable
{
    private static Dictionary<string, string> Templates { get; set; } = new ();

    private int networkIndex;

    protected static readonly new string Name = nameof(SystemNetworkUsageWidget);

    private readonly DataManager dataManager;

    public SystemNetworkUsageWidget()
        : base()
    {
        dataManager = new (DataType.Network, UpdateWidget);
    }

    private string SpeedToString(float cpuSpeed)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.00} GHz", cpuSpeed / 1000);
    }

    private string FloatToPercentString(float value)
    {
        return ((int)(value * 100)).ToString(CultureInfo.InvariantCulture) + "%";
    }

    private string BytesToBitsPerSecString(float value)
    {
        // Bytes to bits
        value *= 8;

        // bits to Kbits
        value /= 1024;
        if (value < 1024)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} Kbps", value);
        }

        // Kbits to Mbits
        value /= 1024;
        return string.Format(CultureInfo.InvariantCulture, "{0:0.0} Mbps", value);
    }

    public override void LoadContentData()
    {
        Log.Logger()?.ReportDebug(Name, ShortId, "Getting network Data");

        try
        {
            var networkData = new JsonObject();

            var currentData = dataManager.GetNetworkStats();

            var netName = currentData.GetNetworkName(networkIndex);
            var networkStats = currentData.GetNetworkUsage(networkIndex);

            networkData.Add("networkUsage", FloatToPercentString(networkStats.Usage));
            networkData.Add("netSent", BytesToBitsPerSecString(networkStats.Sent));
            networkData.Add("netReceived", BytesToBitsPerSecString(networkStats.Received));
            networkData.Add("networkName", netName);
            networkData.Add("netGraphUrl", currentData.CreateNetImageUrl(networkIndex));

            DataState = WidgetDataState.Okay;
            ContentData = networkData.ToJsonString();
        }
        catch (Exception e)
        {
            Log.Logger()?.ReportError(Name, ShortId, "Error retrieving data.", e);
            DataState = WidgetDataState.Failed;
            return;
        }
    }

    public override string GetTemplatePath(WidgetPageState page)
    {
        return page switch
        {
            WidgetPageState.Content => @"Widgets\Templates\SystemNetworkUsageTemplate.json",
            WidgetPageState.Loading => @"Widgets\Templates\SystemNetworkUsageTemplate.json",
            _ => throw new NotImplementedException(),
        };
    }

    public override string GetData(WidgetPageState page)
    {
        return page switch
        {
            WidgetPageState.Content => ContentData,
            WidgetPageState.Loading => EmptyJson,

            // In case of unknown state default to empty data
            _ => EmptyJson,
        };
    }

    private void HandlePrevNetwork(WidgetActionInvokedArgs args)
    {
        networkIndex = dataManager.GetNetworkStats().GetPrevNetworkIndex(networkIndex);
        UpdateWidget();
    }

    private void HandleNextNetwork(WidgetActionInvokedArgs args)
    {
        networkIndex = dataManager.GetNetworkStats().GetNextNetworkIndex(networkIndex);
        UpdateWidget();
    }

    public override void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
    {
        var verb = GetWidgetActionForVerb(actionInvokedArgs.Verb);
        Log.Logger()?.ReportDebug(Name, ShortId, $"ActionInvoked: {verb}");

        switch (verb)
        {
            case WidgetAction.PrevItem:
                HandlePrevNetwork(actionInvokedArgs);
                break;

            case WidgetAction.NextItem:
                HandleNextNetwork(actionInvokedArgs);
                break;

            case WidgetAction.Unknown:
                Log.Logger()?.ReportError(Name, ShortId, $"Unknown verb: {actionInvokedArgs.Verb}");
                break;
        }
    }

    protected override void SetActive()
    {
        ActivityState = WidgetActivityState.Active;
        Page = WidgetPageState.Content;
        if (ContentData == EmptyJson)
        {
            LoadContentData();
        }

        dataManager.Start();

        LogCurrentState();
        UpdateWidget();
    }

    protected override void SetInactive()
    {
        dataManager.Stop();

        ActivityState = WidgetActivityState.Inactive;

        LogCurrentState();
    }

    protected override void SetDeleted()
    {
        dataManager.Stop();

        SetState(string.Empty);
        ActivityState = WidgetActivityState.Unknown;
        LogCurrentState();
    }

    public void Dispose()
    {
        dataManager.Dispose();
    }
}
