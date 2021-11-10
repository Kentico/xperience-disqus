using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Kentico.Xperience.Disqus
{
    /// <summary>
    /// JsonPathConverter; adapted from code located at: https://stackoverflow.com/a/33094930/398630
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class JsonPathConverter : Newtonsoft.Json.JsonConverter
    {
        public override object ReadJson
        (
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var jo = JObject.Load(reader);
            object targetObj = existingValue ?? Activator.CreateInstance(objectType);

            foreach (var prop in objectType.GetProperties().Where(p => p.CanRead))
            {
                var pathAttribute = prop.GetCustomAttributes(true).OfType<JsonPropertyAttribute>().FirstOrDefault();
                var converterAttribute = prop.GetCustomAttributes(true).OfType<Newtonsoft.Json.JsonConverterAttribute>().FirstOrDefault();

                string jsonPath = pathAttribute?.PropertyName ?? prop.Name;
                var token = jo.SelectToken(jsonPath);

                if (token != null && token.Type != JTokenType.Null)
                {
                    bool done = false;

                    if (converterAttribute != null)
                    {
                        var args = converterAttribute.ConverterParameters ?? Array.Empty<object>();
                        var converter = Activator.CreateInstance(converterAttribute.ConverterType, args) as Newtonsoft.Json.JsonConverter;
                        if (converter != null && converter.CanRead)
                        {
                            using (var sr = new StringReader(token.ToString()))
                            using (var jr = new JsonTextReader(sr))
                            {
                                var value = converter.ReadJson(jr, prop.PropertyType, prop.GetValue(targetObj), serializer);
                                if (prop.CanWrite)
                                {
                                    prop.SetValue(targetObj, value);
                                }
                                done = true;
                            }
                        }
                    }

                    if (!done)
                    {
                        if (prop.CanWrite)
                        {
                            object value = token.ToObject(prop.PropertyType, serializer);
                            prop.SetValue(targetObj, value);
                        }
                        else
                        {
                            using (var sr = new StringReader(token.ToString()))
                            {
                                serializer.Populate(sr, prop.GetValue(targetObj));
                            }
                        }
                    }
                }
            }

            return targetObj;
        }

        /// <remarks>
        /// CanConvert is not called when <see cref="JsonConverterAttribute">JsonConverterAttribute</see> is used.
        /// </remarks>
        public override bool CanConvert(Type objectType) => false;

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override void WriteJson
        (
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        )
        {
            throw new NotImplementedException();
        }
    }
}