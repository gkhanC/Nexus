using System;

namespace Nexus.Attributes
{
    /// <summary>
    /// Forces a type (typically a struct) to be strictly unmanaged.
    /// The Nexus Constraint Checker will flag an error if a type marked with this
    /// attribute contains managed references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class MustBeUnmanagedAttribute : Attribute { }
}
