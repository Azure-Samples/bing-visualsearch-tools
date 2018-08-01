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

    //making a non generic class so that we can reference this as DataType in XAML as XAML doesn't support DataType names to be generic collection names
    public class MyObservableCollection : System.Collections.ObjectModel.ObservableCollection<object>
    { }


    //making a container class so that we can reference this as DataType in XAML without incurring recursion (JToken can contain other JTokens)
    public class MyJToken
    {
        public Newtonsoft.Json.Linq.JToken JToken { get; set; }
    }

}