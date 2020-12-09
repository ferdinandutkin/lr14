using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;


namespace Serializer
{






    class JsonReader : IReadOnlyDictionary<string, string>
    {
        public bool isArray { get; private set; }

        public IEnumerable<string> Keys => values.Keys;

        public IEnumerable<string> Values => values.Values;

        public int Count => values.Count;

        Dictionary<string, string> values;

        private IEnumerable<KeyValuePair<string, string>> SplitArray(string jsonArray)
        {
            var noBrackets = jsonArray.Substring(1, jsonArray.Length - 2);
            int open = 0, closed = 0;
            int quotationMarks = 0;
            var marked = string.Concat(noBrackets.Select(c =>
            {
                if (c == '{') open++;
                else if (c == '}') closed++;
                else if (c == '"') quotationMarks++;
                if (open == closed && open != 0 && c == ',' && quotationMarks % 2 == 0)
                {
                    return '`';
                }
                else return c;
            }));

            return marked.Split('`').Select((token, index) => new KeyValuePair<string, string>(index.ToString(), token));




        }

        private IEnumerable<KeyValuePair<string, string>> SplitObject(string jsonObject)
        {
            var noBrackets = jsonObject.Substring(1, jsonObject.Length - 2);
            int open = 0, closed = 0;
            int quotationMarks = 0;
            var marked = string.Concat(noBrackets.Select(c =>
            {
                if (c == '{') open++;
                else if (c == '}') closed++;
                else if (c == '"') quotationMarks++;
                if (open == closed && c == ',' && quotationMarks % 2 == 0)
                {
                    return '`';
                }
                else return c;
            }));

            return marked.Split('`').Select(token =>
            {
                var tokens = token.Split(':', 2);
                return new KeyValuePair<string, string>(tokens[0], tokens[1]);
            });




        }


        public JsonReader(string json, bool isArray = false) => values = new(isArray ? SplitArray(json) : SplitObject(json));
        
        public string this[string name]
        {
            get
            {
                if (values.TryGetValue(name, out var result))
                {
                    return result;
                }
                else if (values.TryGetValue('"' + name + '"', out var result1))
                {
                    return result1;
                }
                else
                {
                    Console.WriteLine($"No key for {name}");
                    return string.Empty;
                }
            }
        }

        public bool ContainsKey(string key) => values.ContainsKey(key);


        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => TryGetValue(key, out value);


        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => values.GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)values).GetEnumerator();

    }
    class JsonWriter
    {

        public bool IsArray { get; private set; }
        Dictionary<string, string> values;

        public JsonWriter(bool isArray = false) => (IsArray, values) = (isArray, new());

        public string this[string name]
        {
            get
            {
                if (values.TryGetValue(name, out var result))
                {
                    return result;
                }
                else return string.Empty;
            }
            set => values[name] = value;

        }

        public override string ToString() =>
            IsArray ?
            ('[' + string.Join(',', values.Values) + ']')
            :
            ('{' + string.Join(',', values.Select(kvp => $"\"{kvp.Key}\":{kvp.Value}")) + '}');

    }

    class JsonSerializer
    {
        public JsonSerializer()
        {

        }


        public T Deserialize<T>(string json) => (T)Deserialize(json, typeof(T));

        public object Deserialize(string json, Type type)
        {

            Console.WriteLine($"SER: {json} of type {type}");
            var jsonReader = new JsonReader(json, type.IsArray);


            if (type.IsArray)
            {

                var elementType = type.GetElementType();
                Array array = Array.CreateInstance(elementType, jsonReader.Count);
                foreach (var (index, value) in jsonReader)
                {
                    Console.WriteLine($"Элемент массива {(index, value)}");
                    array.SetValue(Deserialize(value, elementType), int.Parse(index));
                }

                return array;
            }

            else
            {
                object ret = Activator.CreateInstance(type);
                if (type is null)
                {
                    return null;
                }

                foreach (var member in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Cast<MemberInfo>().
                   Concat(type.GetProperties().Where(p => p.GetSetMethod(false) != null)))
                {

                    var memberType = member.GetMemberType();

                    if (memberType.IsPrimitive)
                    {
                        Console.WriteLine($"Примитив {member.Name} = {jsonReader[member.Name]}");
                        member.SetValue(ret, Convert.ChangeType(jsonReader[member.Name], memberType));
                    }
                    else if (memberType == typeof(string))
                    {
                        Console.WriteLine($"Строка {member.Name} = {jsonReader[member.Name]}");
                        var result = jsonReader[member.Name];
                        member.SetValue(ret, result.Substring(1, result.Length - 2));
                    }
                    else
                    {
                        member.SetValue(ret, Deserialize(jsonReader[member.Name], memberType));
                    }

                }

                return ret;

            }

        }

        public string Serialize(object obj)
        {

            var type = obj.GetType();

            var jsonWriter = new JsonWriter(type.IsArray);

            if (obj is Array arr)
            {
                int i = 0;
                foreach (var el in arr)
                {
                    jsonWriter[i.ToString()] = Serialize(el);
                    i++;
                }

            }

            else
            {

                foreach (var member in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Cast<MemberInfo>().
                    Concat(type.GetProperties().Where(p => p.GetSetMethod(false) != null)))
                {

                    var memberType = member.GetMemberType();
                    if (memberType.IsPrimitive)
                    {
                        jsonWriter[member.Name] = member.GetValue(obj).ToString();
                    }
                    else if (memberType == typeof(string))
                    {
                        jsonWriter[member.Name] = '"' + member.GetValue(obj).ToString() + '"';
                    }
                    else
                    {
                        jsonWriter[member.Name] = Serialize(member.GetValue(obj));
                    }

                }

            }

            return jsonWriter.ToString();


        }
    }
}
