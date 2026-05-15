using DynamicShock.Components.Config;
using DynamicShock.Models;
using DynamicShock.Nodes;
using DynamicShock.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiShock.PluginSdk;
using MultiShock.PluginSdk.Flow;

namespace DynamicShock;

public class DynamicShockPlugin : ReloadablePluginBase, IPlugin, IConfigurablePlugin, IPluginRouteProvider, IPluginWithStyles, IFlowNodeProvider
{
    // ========== PLUGIN METADATA ==========

    public static readonly string PluginId = "Aes-DynamicShock";

    public override string Id => PluginId;

    public override string Name => "Dynamic Shock";

    public override string Version => BuildStamp.Version;

    public override string Description => "Dynamic Shock using keyframes and interpolation";


    private static ILogger? _logger;
    private static DynamicShockTriggerManager? _triggerManager;

    // Expose logger to services and nodes
    internal static ILogger? Logger => _logger;
    internal static DynamicShockTriggerManager? TriggerManager => _triggerManager;


    public static void Log_Info(string data)
    {
        Logger?.LogInformation(data);
    }

    // ========== DEPENDENCY INJECTION ==========

    protected override void ConfigurePluginServices(IServiceCollection services)
    {

    }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DynamicShockTriggerManager>();
        services.AddSingleton<DynamicWebSocket>();
    }

    public void Initialize(IServiceProvider sp)
    {
        var host = sp.GetService(typeof(IPluginHost)) as IPluginHost;
        var actions = sp.GetService(typeof(IDeviceActions)) as IDeviceActions;

        

        // Initialize logger (must include "Plugin" for proper log routing)
        _logger = host?.CreateLogger("DynamicShock.Plugin");
        _logger?.LogInformation("Dynamic Shock plugin initialized");
        
        // Initialize the trigger manager (it subscribes to events in constructor)
        _triggerManager = sp.GetService(typeof(DynamicShockTriggerManager)) as DynamicShockTriggerManager;
        _ = sp.GetService(typeof(DynamicWebSocket)) as DynamicWebSocket;

        Config_Values.Load(host?.GetPluginDataPath(PluginId));

        DynamicWebSocket.Add_Endpoint(PluginId, "Raw", WS_Raw_TriggerNode.RawWebSocket_OnMessage);
        DynamicWebSocket.Add_Endpoint(PluginId, "Event", WS_EventName_TriggerNode.Event_WebSocket_OnMessage);

        // Initialization logic here
    }

    public override void OnUnloading()
    {
        DynamicWebSocket.Unload();
        base.OnUnloading();
    }


    // ========== CONFIGURATION (IConfigurablePlugin) ==========

    public Type? GetConfigurationComponentType() => typeof(PluginConfigComponent);

    public Dictionary<string, object?>? GetDefaultSettings() => new()
    {
        ["enabled"] = true,
    };

    

    public void OnConfigurationChanged(Dictionary<string, object?> settings)
    {
    }

    // ========== STYLES (IPluginWithStyles) ==========

    public string GetStylesheet() => "";

    public string? GetStylesheetId() => "exampleplugin-styles";

    // ========== NAVIGATION (IPluginRouteProvider) ==========

    public override IEnumerable<NavigationItem> GetNavigationItems() =>
    [
        new NavigationItem
        {
            Text = "Edit Events",
            Href = "/LoadEvents",
            Icon = "hard-drive",
            Order = 60
        },
        new NavigationItem
        {
            Text = "Edit Transforms",
            Href = "/LoadTransforms",
            Icon = "chart-network",
            Order = 70
        }
    ];

    // ========== FLOW NODES (IFlowNodeProvider) ==========

    public IEnumerable<IFlowNode> GetNodeTypes()
    {
        yield return new Await_All();
        yield return new Delay_Emitter();
        yield return new DynamicTransformNode();
        yield return new Falling_Edge_Emitter();
        yield return new Rising_Edge_Node();
        yield return new Shocker_Selector();
        yield return new Signal_Reciever_Trigger();
        yield return new Stored_Event_Node();
        yield return new WS_EventName_TriggerNode();
        yield return new WS_Raw_TriggerNode();
    }
}
