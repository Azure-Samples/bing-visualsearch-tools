using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Data;
using System.Globalization;

namespace VSPing.ViewModels
{
    public class JTokenTokenToChildren : IValueConverter
    {
        /// <summary>
        /// Handles converting JSON tokens to children
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            JToken token = value as JToken;
            if (token != null)
            {
                //System.Diagnostics.Debug.WriteLine($"{token.GetType()}: {token.Path} : {token.Type}");
                var children = token.Children().ToArray();
                List<JToken> r = new List<JToken>();
                foreach (var child in children)
                {
                    if (child.Type == JTokenType.Property)


                    {
                        var c = (child as JProperty).Value;
                        r.Add(c);
                        continue;
                    }

                    r.Add(child);
                }
                return r;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class JTokenToPathDepthBoolean : IValueConverter
    {
        /// <summary>
        /// Handles converting JSON tokens to a path depth boolean
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            JToken token = value as JToken;
            if (token == null)
                return false;

            int threshold = int.MaxValue;

            if (parameter != null)
            {
                int.TryParse(parameter.ToString(), out threshold);
            }

            int level = token.Path?.Split('.').Length ?? 0;

            return level > threshold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class JTokenToLevel : IValueConverter
    {
        /// <summary>
        /// Handles converting JSON tokens to a level
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            JToken token = value as JToken;
            if (token == null)
                return 0;

            int level = token.Path?.Split('.').Length ?? 0;

            return level;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class JTokenToName : IValueConverter
    {
        /// <summary>
        /// Handles converting JSON tokens to a name
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            JToken token = value as JToken;
            if (token != null)
            {
                string arraysuffix = String.Empty;
                JToken parent = token.Parent;
                if (token.Parent is JArray)
                {
                    arraysuffix = token.Path.Substring(token.Path.LastIndexOf('['));
                    parent = token.Parent.Parent;

                    if (token is JValue)
                        return $"{arraysuffix}: {token?.ToString() ?? String.Empty}";

                    return $"{arraysuffix}{TokenTypeToString(token)}";
                }
                else if (token is JValue)
                {
                    return $"{((JProperty)(parent)).Name} : {token?.ToString() ?? String.Empty}";
                }
                else
                {
                    return $"{TokenTypeToString(token)}: {(parent as JProperty).Name}";
                }
            }

            return null;
        }

        private string TokenTypeToString(JToken t)
        {
            if (t.Type == JTokenType.Object) return "{}";
            if (t.Type == JTokenType.Array) return "[]";
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
