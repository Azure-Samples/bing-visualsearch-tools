using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using VSPing.ViewModels;
using VSPing.Utils;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for QueryImageStoreControl.xaml
    /// </summary>
    public partial class QueryImageStoreControl : UserControl
    {
        private GridViewColumnHeader lastQueryListHeaderClicked = null;
        private ListSortDirection lastQueryListSortDirection = ListSortDirection.Ascending;

        public event EventHandler<GenericEventArgs<object, Dictionary<string, object>>> QueryDoubleClick;
        public event EventHandler<GenericEventArgs<object, Dictionary<string, object>>> LoadQueryListClick;


        public QueryImageStoreControl()
        {
            InitializeComponent();

            this.vm = DataContext as QueryImageStoreViewModel;
        }

        private QueryImageStoreViewModel vm;
        private QueryImageStoreViewModel VM
        {
            get
            {
                if (this.vm == null)
                {
                    this.vm = DataContext as QueryImageStoreViewModel;
                }
                return vm;
            }
        }

        private void queryListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) //This method allows images to be sent to the search section directly if double-clicked upon
        {
            ListBox box = sender as ListBox;

            if (box == null) return;

            var o = box.SelectedItem;           
            this.QueryDoubleClick?.Invoke(this, new GenericEventArgs<object, Dictionary<string, object>>(o));
        }

        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e) //This method handles the rearranging of items if one of the headers are clicked on
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;


            if (headerClicked != null)
            {
                if ((headerClicked.Content).ToString() != "Image" && (headerClicked.Content).ToString() != "Tags")
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != lastQueryListHeaderClicked)
                        {
                            direction = ListSortDirection.Ascending;
                        }
                        else
                        {
                            if (lastQueryListSortDirection == ListSortDirection.Ascending)
                            {
                                direction = ListSortDirection.Descending;
                            }
                            else
                            {
                                direction = ListSortDirection.Ascending;
                            }
                        }

                        var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                        var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                        //Sort(sortBy, direction);

                        //sort the view
                        //var dataView = CollectionViewSource.GetDefaultView(this.queryListView);
                        var dataView = this.queryListView.Items;

                        dataView.SortDescriptions.Clear();
                        var sd = new SortDescription(sortBy, direction);
                        dataView.SortDescriptions.Add(sd);
                        dataView.Refresh();

                        if (direction == ListSortDirection.Ascending)
                        {
                            headerClicked.Column.HeaderTemplate =
                              Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        }
                        else
                        {
                            headerClicked.Column.HeaderTemplate =
                              Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        }

                        if (lastQueryListHeaderClicked != null && lastQueryListHeaderClicked != headerClicked)
                        {
                            lastQueryListHeaderClicked.Column.HeaderTemplate = null;
                        }

                        lastQueryListHeaderClicked = headerClicked;
                        lastQueryListSortDirection = direction;
                    }
                }
            }
            
        }

        private void queryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) //This method detects the selection of any tags in the view
        {
            ListView lv = sender as ListView;

            if (lv == null)
                return;

            lv.ScrollIntoView(lv.SelectedItem);
        }
        
        private void copyQueryUrlLocations_Click(object sender, RoutedEventArgs e) //This method copies the URL's of all the loaded images in the view
        {
            VM.CopyAllQueryImageUrlsToClipboard();
        }

        private async void GetQueries_Click(object sender, RoutedEventArgs e) //This method populates the view with images from the query store in question
        {
            try
            {
                this.getQueriesBtn.IsEnabled = false;
                this.queryListStatusBarItem.Text = "Loading ...";

                //obtain the store refresh params. Different stores may have different params
                var propertyBag = GetQueryStoreRefreshParams();

                //No refresh to perform. Note: a parameter-less refresh is expressed by an empty property bag.
                if (propertyBag != null)
                {
                    await VM.GetImagesFromStore(propertyBag);
                }
                
                this.queryListStatusBarItem.Text = $"Loaded {this.queryListView.Items.Count} items";                    
            } catch (Exception)
            {                
                this.queryListStatusBarItem.Text = "Error (Check App.Config)";
            }
            finally
            {
                this.getQueriesBtn.IsEnabled = true;
            }
            
        }

        private Dictionary<string, object> GetQueryStoreRefreshParams()
        {
            var eventObj = new GenericEventArgs<object, Dictionary<string, object>>(this.DataContext);

            //raise an event to parent that Load button has been clicked. 
            //It should return back with a Dictionary property bag of key/object pairs
            this.LoadQueryListClick?.Invoke(this, eventObj);

            //return the property bag
            return eventObj.ReturnValue;                
        }

        private void QueryTags_MoveMouse(object sender, MouseEventArgs e) //This method detects when a user clicks on an image in the list and allows it to be dragged to other sections
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element != null && e.LeftButton == MouseButtonState.Pressed)
            {
                object dataContext = element.DataContext;

                DragDrop.DoDragDrop(
                            element,
                            dataContext,
                            DragDropEffects.Copy);
                e.Handled = true;

            }

        }

        private void QueryListView_ImagePopupOn(object sender, MouseEventArgs e) //When an image in the list is hovered over, this method creates a larger version of the image for viewing
        {       
            Image item = e.Source as Image;
            this.listViewPopup.DataContext = item.DataContext;
            this.listViewPopup.PlacementTarget = item;
            this.listViewPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
            this.listViewPopup.IsOpen = true;            
        }

        private void QueryListView_ImagePopupOff(object sender, MouseEventArgs e) //Works with the above method to stop creating a larger image once the user moves their mouse away
        {
            this.listViewPopup.DataContext = null;
            this.listViewPopup.PlacementTarget = null;
            this.listViewPopup.IsOpen = false;            
        }

    }
}

