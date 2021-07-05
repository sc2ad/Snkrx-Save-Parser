using SNKRX_Save_Parser.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SNKRX_Save_Parser.Deserialization
{
    public class TypeParser
    {
        private static object PrimitiveParse(LuaSaveTokenStream tokenStream, Type t)
        {
            if (!PrimitiveTryParse(tokenStream, out var res, t))
                throw new InvalidOperationException($"Could not parse the primitive: {t}!");
            return res;
        }

        private static T PrimitiveParse<T>(LuaSaveTokenStream tokenStream)
        {
            return (T)PrimitiveParse(tokenStream, typeof(T));
        }

        private static bool PrimitiveTryParse(LuaSaveTokenStream tokenStream, out object result, Type t)
        {
            if (t == typeof(int))
            {
                var idxToken = tokenStream.Consume(Token.Digit);
                if (!int.TryParse(idxToken.Value, out var idx))
                    throw new InvalidOperationException($"Failed to convert a digit to an integer! Token: {idxToken.Token}, Value: {idxToken.Value}");
                // T is int
                result = idx;
            }
            else if (t == typeof(bool))
            {
                var peeked = tokenStream.Peek();
                if (peeked.Token != Token.True && peeked.Token != Token.False)
                    throw new InvalidOperationException($"Expected a boolean (true or false), but found: {peeked.Token}, Value: {peeked.Value}");
                tokenStream.Consume(peeked.Token);
                // T is bool
                result = peeked.Token == Token.True;
            }
            else if (t == typeof(string))
            {
                var literal = tokenStream.Consume(Token.StringLiteral);
                // Literals contain the prefix and suffix ", remove those.
                // T is string
                if (!literal.Value.StartsWith('"') || !literal.Value.EndsWith('"'))
                    throw new InvalidOperationException($"String must have double quotes: '{literal.Value}', this is not a valid string literal!");
                result = literal.Value[1..^1];
            }
            else if (t.IsEnum)
            {
                var literal = tokenStream.Consume(Token.StringLiteral);
                if (!literal.Value.StartsWith('"') || !literal.Value.EndsWith('"'))
                    throw new InvalidOperationException($"String must have double quotes: '{literal.Value}', this is not a valid string literal (for enum parsing into type: {t})!");
                var stringValue = literal.Value[1..^1];
                foreach (var m in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var conversion = m.GetCustomAttribute<SaveDataNameAttribute>();
                    if (conversion is not null && conversion.Name == stringValue)
                    {
                        result = m.GetValue(null)!;
                        return true;
                    } else if (m.Name.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                    {
                        result = m.GetValue(null)!;
                        return true;
                    }
                }
                throw new InvalidOperationException($"Expected the value: '{stringValue}' to be convertible to an enum of type: {t}, but it was not! Specify an EnumName attribute or ensure a member exists with that name!");
            }
            else
            {
                result = default!;
                return false;
            }
            return true;
        }

        private static bool PrimitiveTryParse<T>(LuaSaveTokenStream tokenStream, out T result)
        {
            if (PrimitiveTryParse(tokenStream, out var res, typeof(T)))
            {
                result = (T)res!;
                return true;
            }
            result = default!;
            return false;
        }

        private static List<object> ParseList(LuaSaveTokenStream tokenStream, Type t)
        {
            // Lists look like so: {[1] = x, ...}
            tokenStream.Consume(Token.LeftBrace);
            var lst = new List<object>();
            // Lua arrays start at 1
            int expectedIdx = 1;
            while (tokenStream.Peek().Token != Token.RightBrace)
            {
                // Read index
                tokenStream.Consume(Token.LeftBracket);
                var idx = PrimitiveParse<int>(tokenStream);
                if (idx != expectedIdx)
                    throw new InvalidOperationException($"Expected array idx: {expectedIdx} got: {idx}!");
                tokenStream.Consume(Token.RightBracket);

                // Read Assignment
                tokenStream.Consume(Token.Equals);

                // Perform checks for primitive parses
                if (!PrimitiveTryParse(tokenStream, out object item))
                {
                    item = Parse(tokenStream, t);
                }
                // Add item and increment index, item is always non-null at this point
                lst.Add(item!);
                // After the item there might be a comma. If there isn't, ensure it is a } and break.
                if (tokenStream.Peek().Token == Token.Comma)
                    tokenStream.Consume(Token.Comma);
                else
                    break;
                ++expectedIdx;
            }
            // Consume final }
            tokenStream.Consume(Token.RightBrace);
            return lst;
        }

        public static List<T> ParseList<T>(LuaSaveTokenStream tokenStream)
        {
            return (List<T>)(object)ParseList(tokenStream, typeof(T));
        }

        private class PropertyMapping
        {
            public PropertyInfo Property { get; }
            public bool AssignedTo { get; set; }
            public bool MaybeIgnored { get; }
            public PropertyMapping(PropertyInfo info, bool assigned, bool ignored)
            {
                Property = info;
                AssignedTo = assigned;
                MaybeIgnored = ignored;
            }
        }

        public static object Parse(LuaSaveTokenStream tokenStream, Type t)
        {
            // First try to see if we are a primtive type.
            if (PrimitiveTryParse(tokenStream, out var res, t))
                return res!;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                // Must be a generic type at this point and must be a list, so has only 1 generic parameter.
                return ParseList(tokenStream, t.GetGenericArguments()[0]);

            // Perform reflection over the type, check all properties
            // For each property, check to see if they have the SaveDataNameAttribute
            // Create a mapping from save name to PropertyInfo on the type.
            // Read the full object, parse to property names as necessary.
            var mapping = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in t.GetProperties())
            {
                var saveData = p.GetCustomAttribute<SaveDataNameAttribute>();
                var name = p.Name;
                if (saveData is not null)
                    name = saveData.Name;
                // Mappings are initialy not assigned to.
                // Ignorable properties are those that might not be serialized (or those with an ignorable attribute)
                bool ignorable = p.GetCustomAttribute<SerializeIfAttribute>() is not null
                    || p.GetCustomAttribute<SerializeIfNotAttribute>() is not null
                    || p.GetCustomAttribute<IgnoreAttribute>() is not null;
                mapping.Add(name, new(p, false, ignorable));
            }

            // Now we actually parse our type.
            var instance = Activator.CreateInstance(t);
            if (instance is null)
                throw new InvalidOperationException($"Cannot create a valid instance out of type: {t}!");
            tokenStream.Consume(Token.LeftBrace);
            while (tokenStream.Peek().Token != Token.RightBrace)
            {
                // Read property name
                tokenStream.Consume(Token.LeftBracket);
                var propName = PrimitiveParse<string>(tokenStream);
                tokenStream.Consume(Token.RightBracket);
                // Read assignment
                tokenStream.Consume(Token.Equals);
                if (!mapping.TryGetValue(propName, out var mappedTo))
                    throw new InvalidOperationException($"Encountered a property name that is not handled when deserializing: {t}! Property name: '{propName}'");
                if (mappedTo.AssignedTo)
                    throw new InvalidOperationException($"Attempting to assign property: '{propName}' but it has already been assigned to!");
                // Parse the property type then assign it here.
                var setter = mappedTo.Property.GetSetMethod(true);
                if (setter is null)
                    throw new InvalidOperationException($"Attempting to write to property: '{propName}' but it has no private or public setter!");
                setter.Invoke(instance, new object[] { Parse(tokenStream, mappedTo.Property.PropertyType) });
                mappedTo.AssignedTo = true;

                // After the item there might be a comma. If there isn't, ensure it is a } and break.
                if (tokenStream.Peek().Token == Token.Comma)
                    tokenStream.Consume(Token.Comma);
                else
                    break;
            }
            // After we are done with the instance, we should check the mapping for any unassigned properties.
            var lst = new List<Exception>();
            foreach (var item in mapping.Values.Where(p => !p.AssignedTo && !p.MaybeIgnored))
            {
                lst.Add(new InvalidOperationException($"Type: {t} has property: {item.Property.Name} that was not assigned to nor ignored (either implicitly or explicitly)!"));
            }
            if (lst.Count > 0)
                throw new AggregateException(lst);
            tokenStream.Consume(Token.RightBrace);
            return instance;
        }

        public static T Parse<T>(LuaSaveTokenStream tokenStream)
        {
            return (T)Parse(tokenStream, typeof(T));
        }
    }
}