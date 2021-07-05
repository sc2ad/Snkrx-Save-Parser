using System;

namespace SNKRX_Save_Parser.Attributes
{
    /// <summary>
    /// An attribute that is used for serialization and deserialization to convert to the correct name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SaveDataNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public SaveDataNameAttribute(string name)
        {
            Name = name;
        }
    }
}