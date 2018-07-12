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
    public class TagViewModel : BindableBase
    {
        /// <summary>
        /// View the names of tags and their associated actions
        /// </summary>
        public List<ActionViewModel> FilteredActions { get; private set; }
        public Tag Tag { get; private set; } // Tag model object held by this ViewModel instance
        public string Name { get; private set; }

        public TagViewModel(Tag t)
        {
            this.Tag = t;
            this.Name = t.ToString();
            this.FilteredActions = new List<ActionViewModel>();

            foreach (var a in t.FilteredActions)
            {
                this.FilteredActions.Add(new ActionViewModel(a));
            }
        }
    }
}
