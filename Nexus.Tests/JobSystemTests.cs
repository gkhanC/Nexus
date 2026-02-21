using Nexus.Logic;
using Nexus.Registry;

namespace Nexus.Tests;

public unsafe class JobSystemTests
{
    struct CompA { }
    struct CompB { }

    class System1 : INexusSystem
    {
        [Write] public CompA* A;
        public bool Executed = false;
        public void Execute() => Executed = true;
    }

    class System2 : INexusSystem
    {
        [Read] public CompA* A;
        [Write] public CompB* B;
        public bool Executed = false;
        public void Execute() => Executed = true;
    }

    [Fact]
    public void JobSystem_DependencyAnalysis_ExecutesSystems()
    {
        using var registry = new Nexus.Registry.Registry();
        var jobs = new JobSystem(registry);
        
        var s1 = new System1();
        var s2 = new System2();

        jobs.AddSystem(s1);
        jobs.AddSystem(s2);

        jobs.Execute();

        Assert.True(s1.Executed);
        Assert.True(s2.Executed);
    }
}
