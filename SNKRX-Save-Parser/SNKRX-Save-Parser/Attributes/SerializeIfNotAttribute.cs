using System;

namespace SNKRX_Save_Parser.Attributes
{
    /// <summary>
    /// Only serializes the specified field/property if its value does NOT match ANY of the provided values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SerializeIfNotAttribute : Attribute
    {
        public object? Value { get; private set; }

        public SerializeIfNotAttribute(object? value)
        {
            Value = value;
        }
    }
}