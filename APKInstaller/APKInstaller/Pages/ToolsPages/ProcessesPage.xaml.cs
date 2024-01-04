using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.ViewModels.ToolsPages;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.ToolsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProcessesPage : Page
    {
        private ProcessesViewModel Provider;

        public ProcessesPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new ProcessesViewModel(this);
            DataContext = Provider;
            Provider.TitleBar = TitleBar;
            MonitorHelper.Monitor.DeviceListChanged += OnDeviceListChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MonitorHelper.Monitor.DeviceListChanged -= OnDeviceListChanged;
        }

        private void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e) => _ = Provider.GetDevices();

        private void TitleBar_BackRequested(TitleBar sender, object e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearSort();
            _ = Provider.GetProcess();
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e)
        {
            ClearSort();
            _ = Provider.GetDevices().ContinueWith((Task) => _ = Provider.GetProcess());
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.DeviceComboBox = sender as ComboBox;
            _ = Provider.GetDevices();
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            if (!TitleBar.IsRefreshButtonVisible) { return; }

            DataGrid dataGrid = sender as DataGrid;
            // Clear previous sorted column if we start sorting a different column
            string previousSortedColumn = Provider.CachedSortedColumn;
            if (previousSortedColumn != string.Empty)
            {
                foreach (DataGridColumn dataGridColumn in dataGrid.Columns)
                {
                    if (dataGridColumn.Tag != null && dataGridColumn.Tag.ToString() == previousSortedColumn &&
                        (e.Column.Tag == null || previousSortedColumn != e.Column.Tag.ToString()))
                    {
                        dataGridColumn.SortDirection = null;
                    }
                }
            }

            // Toggle clicked column's sorting method
            if (e.Column.Tag != null)
            {
                if (e.Column.SortDirection == null)
                {
                    _ = Provider.SortData(e.Column.Tag.ToString(), true);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else if (e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    _ = Provider.SortData(e.Column.Tag.ToString(), false);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    _ = Provider.SortData(e.Column.Tag.ToString(), true);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
        }

        private void ClearSort()
        {
            // Clear previous sorted column if we start sorting a different column
            string previousSortedColumn = Provider.CachedSortedColumn;
            if (previousSortedColumn != string.Empty)
            {
                foreach (DataGridColumn dataGridColumn in DataGrid.Columns)
                {
                    dataGridColumn.SortDirection = null;
                }
            }
        }
    }
}
