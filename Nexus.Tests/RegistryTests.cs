using Nexus.Registry;

namespace Nexus.Tests;

public unsafe class RegistryTests
{
    struct TestComp { public int Value; }

    [Fact]
    public void Registry_EntityLifecycle_Works()
    {
        using var registry = new Nexus.Registry.Registry();
        var e1 = registry.Create();
        Assert.True(registry.IsValid(e1));

        registry.Destroy(e1);
        Assert.False(registry.IsValid(e1));

        var e2 = registry.Create();
        Assert.NotEqual(e1, e2);
        Assert.Equal(e1.Index, e2.Index); // Index reused
        Assert.NotEqual(e1.Version, e2.Version); // Version incremented
    }

    [Fact]
    public void SparseSet_AddAndGet_Works()
    {
        using var registry = new Nexus.Registry.Registry();
        var e = registry.Create();
        
        registry.Add<TestComp>(e, new TestComp { Value = 100 });
        Assert.True(registry.Has<TestComp>(e));
        
        TestComp* comp = registry.Get<TestComp>(e);
        Assert.Equal(100, comp->Value);
    }

    [Fact]
    public void SparseSet_Remove_MaintainsContiguity()
    {
        using var registry = new Nexus.Registry.Registry();
        var e1 = registry.Create();
        var e2 = registry.Create();

        registry.Add<TestComp>(e1, new TestComp { Value = 1 });
        registry.Add<TestComp>(e2, new TestComp { Value = 2 });

        var set = registry.GetSet<TestComp>();
        Assert.Equal(2, set.Count);

        registry.Remove<TestComp>(e1);
        Assert.Equal(1, set.Count);
        Assert.False(registry.Has<TestComp>(e1));
        Assert.True(registry.Has<TestComp>(e2));
        
        // Verify e2 was swapped to index 0
        Assert.Equal(2, set.GetComponent(0)->Value);
    }
}
