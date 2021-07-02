using System;

namespace SNKRX_Save_Parser.Attributes
{
    /// <summary>
    /// Only serializes the property if its value matches ANY of the provided values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SerializeIfAttribute : Attribute
    {
        public object? Value { get; private set; }

        public SerializeIfAttribute(object? value)
        {
            Value = value;
        }
    }
}