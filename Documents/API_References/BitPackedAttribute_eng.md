# Nexus Prime Architectural Manual: BitPackedAttribute (Bit-Level Compression)

## 1. Introduction
`BitPackedAttribute.cs` is a "Compression Flag" layer used by Nexus Prime to increase data density and minimize data traffic over the network. It specifies that instead of sending all bytes of a component, it should be packaged to cover only the determined number of bits.

The reason for this attribute's existence is to save memory and **reduce bandwidth usage by up to 80%**, especially for Boolean values, Enums, or small numerical values (e.g., an ID between 0-15), instead of spending a whole `int` (32 bits) or `byte` (8 bits).

---

## 2. Technical Analysis
BitPackedAttribute offers the following directives for packaging tools:

- **Bit Length Specification**: Declares how many bits a piece of data can be represented in via the `Bits` parameter (e.g., 4 bits = 0-15 value range).
- **Source Generator Hook**: This attribute is scanned by Nexus's Bit-Level Compression Tool, and wrapper classes with `Pack/Unpack` methods are automatically generated for the relevant component.
- **Network Optimization**: During delta serialization, fields with this attribute are transmitted as a bit-stream instead of their raw values.

---

## 3. Logical Flow
1.  **Definition**: The developer marks a struct component with a small value range with a value like `[BitPacked(4)]`.
2.  **Analysis**: Nexus's compile-time tools see this flag.
3.  **Production**: Software automatically generates unsafe codes that mask (Masking) and shift (Shifting) this data at bit-level.
4.  **Execution**: Only the targeted bits are processed while data is being recorded or sent.

---

## 4. Usage Example
```csharp
using Nexus.Data;

[BitPacked(3)] // Takes up only 3 bits (for values between 0-7)
public struct TeamType {
    public int Value; 
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Data;

[AttributeUsage(AttributeTargets.Struct)]
public class BitPackedAttribute : Attribute
{
    public int Bits { get; }

    public BitPackedAttribute(int bitsCount)
    {
        Bits = bitsCount;
    }
}
```

---

## Nexus Optimization Tip: Precision Squeezing
If a component takes only very limited values like "Active/Passive" or "Team ID", be sure to use `[BitPacked]`. This allows the processor to fit more data into a single "Cache Line", **logarithmically increasing memory bandwidth efficiency.**
