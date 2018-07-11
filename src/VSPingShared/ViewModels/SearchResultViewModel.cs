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
    public class SearchResultViewModel : BindableBase
    {
        /// <summary>
        /// Retrieve the search result from a particular request
        /// </summary>
        public SearchResult SearchResult { get; set; } // SearchResult model object held by this ViewModel instance

        public SearchResultViewModel(SearchResult sr)
        {
            this.SearchResult = sr;
        }

        public override string ToString()
        {
            return this.SearchResult.ImageUri.ToString();
        }

    }

}
