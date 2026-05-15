using MultiShock.PluginSdk.Flow;

namespace DynamicShock.Services;

/// <summary>
/// Manages Example Plugin event triggers for flow nodes.
/// </summary>
public class DynamicShockTriggerManager
{
    private readonly Dictionary<string, List<TriggerRegistration>> _registrations = [];
    private readonly object _lock = new();

    public DynamicShockTriggerManager()
    {
        // Subscribe to your service events here
        // Example: _myService.OnSomethingHappened += HandleSomethingHappened;
    }

    public void Register(string eventType, IFlowNodeInstance instance, Func<IFlowNodeInstance, Dictionary<string, object?>, Task> callback)
    {
        lock (_lock)
        {
            if (!_registrations.ContainsKey(eventType))
            {
                _registrations[eventType] = [];
            }
            _registrations[eventType].Add(new TriggerRegistration(instance, callback));
        }
    }

    public void Unregister(string eventType, IFlowNodeInstance instance)
    {
        lock (_lock)
        {
            if (_registrations.TryGetValue(eventType, out var list))
            {
                list.RemoveAll(r => r.Instance.InstanceId == instance.InstanceId);
            }
        }
    }

    public async Task FireEvent(string eventType, Dictionary<string, object?> outputs, Func<IFlowNodeInstance, bool>? filter = null)
    {
        List<TriggerRegistration> registrations;
        lock (_lock)
        {
            if (!_registrations.TryGetValue(eventType, out var list))
            {
                return;
            }
            registrations = list.ToList();
        }
        foreach (var reg in registrations)
        {
            try
            {
                if (filter == null || filter(reg.Instance))
                {
                    await reg.Callback(reg.Instance, outputs);
                }
            }
            catch
            {
                // Ignore errors in individual triggers
            }
        }
    }

    public async Task Fire_Signal_Event(string eventType, Dictionary<string, object?> outputs, string signal, Func<IFlowNodeInstance, string, bool> filter)
    {
        List<TriggerRegistration> registrations;
        lock (_lock)
        {
            if (!_registrations.TryGetValue(eventType, out var list))
            {
                return;
            }
            registrations = list.ToList();
        }
        foreach (var reg in registrations)
        {
            try
            {
                if (filter(reg.Instance, signal))
                {
                    await reg.Callback(reg.Instance, outputs);
                }
            }
            catch
            {
                // Ignore errors in individual triggers
            }
        }
    }

    // Example event handler
    // private void HandleSomethingHappened(SomeEventData e)
    // {
    //     _ = FireEvent("exampleplugin.example", new Dictionary<string, object?>
    //     {
    //         ["data"] = e.Data,
    //     });
    // }

    private record TriggerRegistration(IFlowNodeInstance Instance, Func<IFlowNodeInstance, Dictionary<string, object?>, Task> Callback);
}
