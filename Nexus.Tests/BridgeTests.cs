using Nexus.Registry;
using Nexus.Bridge;

namespace Nexus.Tests;

public unsafe class BridgeTests
{
    struct TestComp { public int Value; }

    [Fact]
    public void BridgeHub_PushPull_Works()
    {
        using var registry = new Nexus.Registry.Registry();
        var bridge = new BridgeHub(registry);
        
        var e = registry.Create();
        registry.Add<TestComp>(e, new TestComp { Value = 1 });

        int engineValue = 0;
        bool pushCalled = false;

        bridge.Register<TestComp>(
            push: (ent, data) => { engineValue = data->Value; pushCalled = true; },
            pull: (ent, data) => { data->Value = engineValue + 10; }
        );

        // 1. Initial Push
        bridge.PushAll();
        Assert.True(pushCalled);
        Assert.Equal(1, engineValue);

        // 2. Modify engine value and Pull
        engineValue = 50;
        bridge.PullAll();

        // Verify Nexus value changed
        Assert.Equal(60, registry.Get<TestComp>(e)->Value);
        
        // Verify it's marked as dirty in Nexus
        Assert.True(registry.GetSet<TestComp>().IsDirty(0));

        // 3. Push back to see if it syncs
        pushCalled = false;
        bridge.PushAll();
        Assert.True(pushCalled);
        Assert.Equal(60, engineValue);
    }
}
