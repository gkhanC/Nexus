using Nexus.Data;

namespace Nexus.Tests;

public unsafe class DataTests
{
    [Fact]
    public void NexusString32_Conversion_Works()
    {
        NexusString32 ns = "Hello Nexus";
        Assert.Equal("Hello Nexus", ns.ToString());
    }

    [Fact]
    public void NexusString32_Truncation_Works()
    {
        string longStr = new string('A', 50);
        NexusString32 ns = longStr;
        Assert.Equal(31, ns.ToString().Length);
    }

    [Fact]
    public void NexusMath_Add_SIMD_Works()
    {
        int count = 100;
        float[] a = new float[count];
        float[] b = new float[count];
        float[] res = new float[count];

        for (int i = 0; i < count; i++) { a[i] = i; b[i] = i * 2; }

        fixed (float* pa = a, pb = b, pr = res)
        {
            NexusMath.Add(pa, pb, pr, count);
        }

        for (int i = 0; i < count; i++)
        {
            Assert.Equal(i * 3, res[i]);
        }
    }
}
