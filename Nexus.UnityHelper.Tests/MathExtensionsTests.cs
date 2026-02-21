using Xunit;
using Nexus.UnityHelper.Core;
using UnityEngine;

namespace Nexus.UnityHelper.Tests
{
    public class MathExtensionsTests
    {
        [Fact]
        public void SnapToGrid_Works()
        {
            Vector3 v = new Vector3(1.2f, 2.7f, -0.1f);
            Vector3 snapped = v.SnapToGrid(1.0f);
            
            Assert.Equal(1.0f, snapped.x);
            Assert.Equal(3.0f, snapped.y);
            Assert.Equal(0.0f, snapped.z);
        }

        [Fact]
        public void Remap_Works()
        {
            float result = NexusMathUtils.Remap(5, 0, 10, 0, 100);
            Assert.Equal(50, result);
        }

        [Fact]
        public void GCD_Works()
        {
            Assert.Equal(6, NexusMathUtils.GCD(12, 18));
            Assert.Equal(1, NexusMathUtils.GCD(7, 13));
        }
    }
}
