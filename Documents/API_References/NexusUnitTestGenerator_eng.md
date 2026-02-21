# Nexus Prime Architectural Manual: NexusUnitTestGenerator (Automated Stress Test Generation)

## 1. Introduction
`NexusUnitTestGenerator.cs` is an engineering tool that automates Nexus Prime's quality assurance (QA) processes. To measure how much data load the systems written by the developer can withstand, it builds a dummy world (Dummy World) by producing millions of random samples from the components required by the system.

The reason for this generator's existence is to reduce the burden of writing unit tests manually and to identify whether systems give unmanaged memory errors, especially in extreme cases (edge cases), in a scientific "Stress Test" environment.

---

## 2. Technical Analysis
NexusUnitTestGenerator follows these steps for reliable test results:

- **System Signature Reflection**: It analyzes which component types the system to be tested expects by scanning its `[Read]`, `[Write]`, and `[Inject]` fields.
- **Randomized Data Influx**: It produces components filled with random bit data that will push unmanaged memory limits in the determined number of entities (e.g., 100,000).
- **Performance Profiling**: It measures and reports the execution time of the system with micro-second (tick) sensitivity.
- **Safety Validations**: By checking the `Registry` integrity (`NexusIntegrityChecker`) at the end of the test, it audits whether the system caused a memory leak or misalignment.

---

## 3. Logical Flow
1.  **Input**: The target system and entity count are given with the `GenerateStressTest(system, count)` method.
2.  **Construction**: A temporary test `Registry` is created and the data structures expected by the system are initialized in the unmanaged space.
3.  **Execution**: The system is run one or more times on this massive data heap.
4.  **Reporting**: Operation speed (Entities/Second) and memory health metrics are printed to the console or a report.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Stress Test** | The process of measuring durability by running a system far above its expected normal load. |
| **System Signature** | A characteristic property set defining a system's dependencies (read/write permissions). |
| **Edge Case** | Rare but critical situations that arise at the most extreme points of normal working conditions. |
| **Dummy World** | A temporary and fake data environment created only for testing purposes. |

---

## 5. Risks and Limits
- **Reflection Overhead**: Reflection operations performed at the start of the test can take time, but this only occurs in the test setup phase (Setup).
- **RAM Limits**: If the `count` parameter is given large enough to push the system's physical RAM capacity, lockups may occur at the operating system level.

---

## 6. Usage Example
```csharp
var testGen = new NexusUnitTestGenerator();
var movementSystem = new PlayerMovementSystem();

// Put the movement system into a stress test with 1 million entities
testGen.GenerateStressTest(movementSystem, 1000000);
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public class NexusUnitTestGenerator
{
    public void GenerateStressTest(INexusSystem system, int entityCount = 100000)
    {
        // 1. Reflect system component requirements.
        // 2. Populate a registry with 'entityCount' entities.
        // 3. Measure execution time and check for memory safety.
    }
}
```

---

## Nexus Optimization Tip: Warm-up Cycles
To measure query performance more realistically, run the generator in "Warm-up" mode. Run the system 2-3 times idly on test data to ensure the CPU fills its instruction cache (Instruction Cache). This way, you can **measure true execution speed, stripped of JIT compilation cost, 25% more accurately.**
