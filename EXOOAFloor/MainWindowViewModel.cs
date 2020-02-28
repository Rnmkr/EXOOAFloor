using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EXOOAFloor
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        //private readonly string connectionString = @"data source=VM-FORREST;initial catalog=EXOOAKeys2020;persist security info=True;user id=BUBBASQL;password=12345678;MultipleActiveResultSets=True";
        private readonly string connectionString = @"data source=BUBBA;initial catalog=EXOOAKeys2020;persist security info=True;user id=BUBBASQL;password=12345678;MultipleActiveResultSets=True;";
        //private readonly string connectionString = @"data source=DESKTOP;initial catalog=EXOOAKeys2020; integrated security=True; MultipleActiveResultSets=True";
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            SearchCommand = new RelayCommand(SelectSearchQueryAsync, SelectSearchQueryAsync_CanExecute);
            DeleteSelectedCommand = new RelayCommand(DeleteSelectedQueryAsync, DeleteSelectedQueryAsync_CanExecute);
            CheckReportCommand = new RelayCommand(OnCheckReportAsync, CheckBox_CanExecute);
            CheckAllReportsCommand = new RelayCommand(OnCheckAllReportsAsync, CheckBox_CanExecute);
            IsAllChecked = false;
            _snackbarMessageQueue = snackbarMessageQueue;
        }

        #region REGIO

        private bool CheckBox_CanExecute(object arg)
        {
            return (SearchResults == null) ? false : true;
        }


        private bool DeleteSelectedQueryAsync_CanExecute(object arg)
        {
            if (IsAllChecked != false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SelectSearchQueryAsync_CanExecute(object arg)
        {
            return string.IsNullOrEmpty(_searchKeyword) ? false : true;
        }

        public ICommand SearchCommand { get; set; }
        public ICommand DeleteSelectedCommand { get; set; }

        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        private async void DeleteSelectedQueryAsync(object obj)
        {
            var confirmDialog = new ConfirmDialog
            {
                Title = { Text = "Esta acción puede acarrear errores." },

                Message = { Text = "Antes de de eliminar registros, confirme que se encuentran \n" +
                                   "en estado 'ALLOCATED' en la aplicación de Microsoft. \n" },

                AcceptButton = { Content = "ELIMINAR" }
            };

            await DialogHost.Show(confirmDialog, new DialogClosingEventHandler((object senderDialog, DialogClosingEventArgs eventArgs) =>
            {
                if ((bool)eventArgs.Parameter == false) return;
                eventArgs.Cancel();
                eventArgs.Session.UpdateContent(new ProgressDialog());

                Task.Run(() =>
                {
                    IEnumerable<OAKeyReport> selectedRecords = SearchResults
                    .Where(w => w.IsChecked == true)
                    .Select(s => s);
                    ExecuteDeleteQuery(selectedRecords);

                }).ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                TaskScheduler.FromCurrentSynchronizationContext());
            }));
        }

        private void ExecuteDeleteQuery(IEnumerable<OAKeyReport> selectedRecords)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                var sqlCommand = new SqlCommand("DeleteOAKeyFromID", sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };


                foreach (var item in selectedRecords)
                {
                    sqlCommand.Parameters.AddWithValue("@ReportID", item.ReportID);
                    sqlCommand.ExecuteNonQuery();
                    DeleteLogFiles(item);
                    DeleteFromSearchResult(item);
                }

                sqlConnection.Close();
                _snackbarMessageQueue.Enqueue(selectedRecords.Count() + " registro(s) eliminado(s).");
            }
            catch (Exception e)
            {
                _snackbarMessageQueue.Enqueue("Ocurrió un error intentando eliminar registros. \n" + e.Message);
            }
        }

        private void DeleteLogFiles(OAKeyReport report)
        {
            var logBasePath = @"\\bubba\ea2100dc89ae9fe21fa9b08ab1bf18662dca1e53a3eebd7d03afebcaf5d57515$";
            var formattedSerialNumber = Path.Combine(report.SerialNumber.Substring(0, 1), report.SerialNumber.Substring(1, 3),
                report.SerialNumber.Substring(4, 4), report.SerialNumber.Substring(8, 5));
            var fullPath = Path.Combine(logBasePath, formattedSerialNumber);
            var xmlFilePath = Path.Combine(fullPath, "OA3.XML");
            var binFilePath = Path.Combine(fullPath, "OA3.BIN");
            if (File.Exists(xmlFilePath)) { File.Delete(xmlFilePath); }
            if (File.Exists(binFilePath)) { File.Delete(binFilePath); }
        }

        private void DeleteFromSearchResult(OAKeyReport report)
        {
            if (report == null) throw new ArgumentNullException();
            //mm updatea en tiempo real...
            var newList = new ObservableCollection<OAKeyReport>(SearchResults);
            newList.Remove(report);
            SearchResults = newList;
        }


        private ObservableCollection<OAKeyReport> _searchResults;
        public ObservableCollection<OAKeyReport> SearchResults
        {
            get => _searchResults;
            set
            {
                if (value == _searchResults) return;
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (value == _searchKeyword) return;
                _searchKeyword = value;

                OnPropertyChanged();

            }
        }

        private async void SelectSearchQueryAsync(object o)
        {
            if (string.IsNullOrWhiteSpace(_searchKeyword))
            {
                return;
            }

            SearchKeyword = SearchKeyword.ToUpper();

            string storedProcedureName = null;

            Regex regexKey = new Regex(@"^([A-Za-z0-9]{5}-){4}[A-Za-z0-9]{5}$");
            if (regexKey.IsMatch(_searchKeyword))
            {
                storedProcedureName = "GetRecordFromKey";
            }

            Regex regexSerialNumber = new Regex(@"^\d{7}[a-cA-C]\d{5}$");
            if (regexSerialNumber.IsMatch(_searchKeyword))
            {
                storedProcedureName = "GetRecordFromSerialNumber";
            }

            Regex regexOrderNumber = new Regex(@"^\d{7}[a-cA-C]$");
            if (regexOrderNumber.IsMatch(_searchKeyword))
            {
                storedProcedureName = "GetRecordFromOrderNumber";
            }

            if (string.IsNullOrWhiteSpace(storedProcedureName))
            {
                _snackbarMessageQueue.Enqueue("Parámetros inválidos para una búsqueda.");
                return;
            }

            var progressDialog = new ProgressDialog { };
            await DialogHost.Show(progressDialog, new DialogOpenedEventHandler((object senderDialog, DialogOpenedEventArgs eventArgs) =>
            {
                Task.Run(() =>
                {
                    ExecuteSearchQuery(storedProcedureName, _searchKeyword);

                }).ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                TaskScheduler.FromCurrentSynchronizationContext());
            }));

        }

        private void ExecuteSearchQuery(string storedProcedure, string keyword)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            using (sqlConnection)
            {
                ObservableCollection<OAKeyReport> resultList = new ObservableCollection<OAKeyReport>();
                IDataReader reader = null;

                try
                {
                    sqlConnection.Open();
                    var sqlCommand = new SqlCommand(storedProcedure, sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    sqlCommand.Parameters.AddWithValue("@Keyword", keyword);
                    reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        var reportEntity = new OAKeyReport()
                        {
                            ReportID = (int)reader["ReportID"],
                            OAKey = (string)reader["OAKey"],
                            SerialNumber = (string)reader["SerialNumber"],
                            State = (string)reader["State"],
                            DateConsumed = (DateTime)reader["DateConsumed"],
                        };

                        if ((reader["DateBound"]) != DBNull.Value)
                            reportEntity.DateBound = (DateTime)reader["DateBound"];

                        resultList.Add(reportEntity);
                    }

                    SearchResults = resultList;
                    _snackbarMessageQueue.Enqueue(SearchResults.Count + " registro(s) encontrado(s).");
                }
                catch (Exception e)
                {
                    _snackbarMessageQueue.Enqueue("Error intentando realizar consulta" + Environment.NewLine + e.ToString());
                }
                finally
                {
                    if (reader != null) reader.Close();
                    sqlConnection.Close();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



        #region HACER BIEN TODO ESTO
        /// <summary>
        /// Adapted from
        /// https://stackoverflow.com/a/56278962
        /// </summary>
        /// 

        public ICommand CheckReportCommand { get; private set; }
        public ICommand CheckAllReportsCommand { get; private set; }

        private bool? _isAllChecked;
        public bool? IsAllChecked
        {
            get => _isAllChecked;
            set
            {
                if (value == _isAllChecked) return;
                _isAllChecked = value;

                OnPropertyChanged();
            }
        }

        private async void OnCheckAllReportsAsync(object o)
        {
            var progressDialog = new ProgressDialog { };
            await DialogHost.Show(progressDialog, new DialogOpenedEventHandler((object senderDialog, DialogOpenedEventArgs eventArgs) =>
            {
                Task.Run(() =>
                {
                    var newList = new ObservableCollection<OAKeyReport>();
                    if (IsAllChecked == true)
                    {
                        foreach (var item in SearchResults)
                        {
                            item.IsChecked = true;
                            newList.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var item in SearchResults)
                        {
                            item.IsChecked = false;
                            newList.Add(item);
                        }
                    }

                    SearchResults = newList;

                }).ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                TaskScheduler.FromCurrentSynchronizationContext());
            }));
        }

        private async void OnCheckReportAsync(object o)
        {
            var progressDialog = new ProgressDialog { };
            await DialogHost.Show(progressDialog, new DialogOpenedEventHandler((object senderDialog, DialogOpenedEventArgs eventArgs) =>
            {
                Task.Run(() =>
                {
                    if (SearchResults.All(x => x.IsChecked))
                        IsAllChecked = true;
                    else if (SearchResults.All(x => !x.IsChecked))
                        IsAllChecked = false;
                    else
                        IsAllChecked = null;
                }).ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                TaskScheduler.FromCurrentSynchronizationContext());
            }));
        }
        #endregion

    }
}


