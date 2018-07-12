using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;


namespace VSPing.Utils
{
    public class GenericEventArgs<TIn, TOut> : EventArgs
    {
        private readonly TIn eventData;

        public GenericEventArgs(TIn eventData)
        {
            this.eventData = eventData;
        }

        public TIn Data { get { return this.eventData; } }

        public TOut ReturnValue { get; set; }
    }

    public abstract class BindableBase : INotifyPropertyChanged
    {
        // <summary>
        ///     Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Checks if a property already matches a desired value.  Sets the property and
        ///     notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        /// <returns>
        ///     True if the value was changed, false if the existing value matched the
        ///     desired value.
        /// </returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers
        ///     that support <see cref="CallerMemberNameAttribute" />.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MyObservableCollection : System.Collections.ObjectModel.ObservableCollection<object>
    {}

    public class MyJToken
    {
        public Newtonsoft.Json.Linq.JToken JToken { get; set; }
    }

    // Copied from Aether Client Code
    public static class ClipboardUtils
    {
        // Copied from Aether Client Code
        public static string CreateHtmlClipboardFormat(string sHtml)
        {
            // See MSDN for "http://msdn.microsoft.com/en-us/library/ms649015(VS.85).aspx" for
            // the details on the HTML Clipboard Format.

            // Magic header with 10 digits of space reserved for the header fields
            const string cClipboardMagic = @"Version:1.0
StartHTML:*********1
EndHTML:*********2
StartFragment:*********3
EndFragment:*********4
StartSelection:*********3
EndSelection:*********4
";
            //string sPre = "<html><head><title></title></head><body><!--StartFragment-->";
            const string cPre = "<html><body><!--StartFragment-->";
            const string cPost = "<!--EndFragment--></body></html>";

            var sbOut = new StringBuilder();

            // Concatenate the text, caching the offsets as we go.
            sbOut.Append(cClipboardMagic);
            int ibHtmlStart = sbOut.Length;
            sbOut.Append(cPre);
            int ibFragmentStart = sbOut.Length;
            sbOut.Append(sHtml);
            int ibFragmentEnd = sbOut.Length;
            sbOut.Append(cPost);
            int ibHtmlEnd = sbOut.Length;

            // Replace the placeholders with the appropriate offset.
            sbOut.Replace("*********1", ibHtmlStart.ToString("d10"));
            sbOut.Replace("*********2", ibHtmlEnd.ToString("d10"));
            sbOut.Replace("*********3", ibFragmentStart.ToString("d10"));
            sbOut.Replace("*********4", ibFragmentEnd.ToString("d10"));

            return sbOut.ToString();
        }

        public static string CreateHtmlLink(string sLink)
        {
            return string.Format("<a href=\"{0}\">{0}</a>", sLink);
        }
    }

    public static class StringUtils
    {
        public static string ToXmlEscapedString(this string inString)
        {
            if (string.IsNullOrWhiteSpace(inString))
                return inString;

            return 
                inString.Replace("\"", "&quot;")
                    .Replace("'", "&apos;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("&", "&amp;");   
        }

        public static string ToXmlUnescapedString(this string inString)
        {
            if (string.IsNullOrWhiteSpace(inString))
                return inString;

            return
                inString.Replace("&quot;", "\"")
                    .Replace("&apos;", "'")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", "<")
                    .Replace("&amp;", "&");
        }

    }

}