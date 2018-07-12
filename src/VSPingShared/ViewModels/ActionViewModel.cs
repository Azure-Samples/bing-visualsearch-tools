using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VSPing.Models;
using VSPing.Utils;

namespace VSPing.ViewModels
{
    public class ActionViewModel : BindableBase
    {
        /// <summary>
        /// Interact with the URI's of actions, if applicable
        /// </summary>
        public VSPing.Models.Action Action { get; protected set; } // // Action model object held by this ViewModel instance

        public bool HasUrls => this.Action.Urls.Count > 0;

        public IEnumerable<Tuple<string, ICommand>> MenuItemNameCommands => this.Action.Urls.Select(u => new Tuple<string, ICommand>(u.Key, this.UrlActionCommand)); // // List of possible options and their associated commands when an action is right clicked

        public ActionViewModel(VSPing.Models.Action a)
        {
            this.Action = a;
            this.UrlActionCommand = new RelayCommand(this.UrlsActionClicked);
        }

        // When an action is rightclicked, go to the corresponding URL
        protected void UrlsActionClicked(object o)
        {
            string actionName = o as string;

            if (string.IsNullOrEmpty(actionName) || !this.Action.Urls.ContainsKey(actionName)) return; // Sanity checks to ensure that url corresponding to action has been clicked


            string actionUri = this.Action.Urls[actionName];

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(actionUri));

        }

        public ICommand UrlActionCommand { get; protected set; }

    }

}
