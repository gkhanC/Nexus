using System;

namespace Nexus.Data
{
    /// <summary>
    /// Flags a component for bit-level compression during serialization.
    /// The Bit-Level Compression Tool uses this to generate optimized wrappers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class BitPackedAttribute : Attribute
    {
        public int Bits { get; }

        public BitPackedAttribute(int bitsCount)
        {
            Bits = bitsCount;
        }
    }
}
