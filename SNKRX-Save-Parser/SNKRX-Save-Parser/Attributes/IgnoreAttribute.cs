using System;

namespace SNKRX_Save_Parser.Attributes
{
    /// <summary>
    /// An attribute that specifies the target property as ignored for serialization and deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}