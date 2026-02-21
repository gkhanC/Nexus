using Nexus.Buffer;

namespace Nexus.Tests;

public unsafe class BufferTests
{
    [Fact]
    public void ChunkedBuffer_Expansion_MaintainsPointerStability()
    {
        using var buffer = new ChunkedBuffer<int>(1);
        int* p1 = (int*)buffer.Add();
        *p1 = 42;

        // Force expansion
        for (int i = 0; i < 10000; i++) buffer.Add();

        int* p1_again = (int*)buffer.GetPointer(0);
        Assert.Equal((long)p1, (long)p1_again);
        Assert.Equal(42, *p1_again);
    }

    [Fact]
    public void ChunkedBuffer_Alignment_Is64Bytes()
    {
        using var buffer = new ChunkedBuffer<byte>(1);
        void* p = buffer.Add();
        Assert.True((long)p % 64 == 0 || (long)p % 64 == 16); // Header + alignment check
        // Actually our header is 64 bytes and AlignedAlloc is 64 bytes.
        // byte* chunkBase = (byte*)_chunks[chunkIdx];
        // return chunkBase + _headerSize + (offset * sizeof(T));
        // chunkBase is 64-aligned, _headerSize is 64. Result should be 64-aligned.
        Assert.Equal(0, (long)p % 64);
    }
}
