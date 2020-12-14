using ClosedXML.Excel;
using EXOOAFloor.Helpers;
using EXOOAFloor.View.Dialog;
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
using System.Windows.Controls;
using System.Windows.Input;
/// <summary>
/// Checkbox 'Select All' adapted from
/// https://stackoverflow.com/a/56278962
/// </summary>
///
namespace EXOOAFloor.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region connectionstrings
        //private readonly string connectionString = @"data source=VM-FORREST;initial catalog=EXOOAKeys2020;persist security info=True;user id=BUBBASQL;password=12345678;MultipleActiveResultSets=True";
        private readonly string connectionString = @"data source=BUBBA;initial catalog=EXOOAKeys2020;persist security info=True;user id=BUBBASQL;password=12345678;MultipleActiveResultSets=True;";
        //private readonly string connectionString = @"data source=DESKTOP;initial catalog=EXOOAKeys2020; integrated security=True; MultipleActiveResultSets=True";
        #endregion

        #region icommands
        public ICommand DeleteSelectedCommand { get; set; }
        public ICommand ShowLogFileCommand { get; set; }
        public ICommand SetPackageStatusCommand { get; set; }
        public ICommand CheckReportCommand { get; set; }
        public ICommand CheckAllReportsCommand { get; set; }
        public ICommand ChangeViewCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ShowMissingCommand { get; set; }
        public ICommand SetAsConsumedCommand { get; set; }
        public ICommand EnableStoreModeCommand { get; set; }
        public ICommand EnableQueryModeCommand { get; set; }
        public ICommand ExportListCommand { get; set; }
        #endregion
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        #region ctor
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _snackbarMessageQueue = snackbarMessageQueue;

            SearchCommand = new RelayCommand(SelectSearchQueryAsync, SelectSearchQueryAsync_CanExecute);

            ShowMissingCommand = new RelayCommand(FindMissingNumbers, FindMissing_CanExecute);

            SetAsConsumedCommand = new RelayCommand(SetAsConsumed, DeleteSelectedQueryAsync_CanExecute);

            DeleteSelectedCommand = new RelayCommand(ShowPasswordDialogAsync, DeleteSelectedQueryAsync_CanExecute);

            ExportListCommand = new RelayCommand(ExportList, FindMissing_CanExecute);

            ShowLogFileCommand = new RelayCommand(ShowLogFileAsync, DeleteSelectedQueryAsync_CanExecute);

            SetPackageStatusCommand = new RelayCommand(SetPackageStatus, DeleteSelectedQueryAsync_CanExecute);

            CheckReportCommand = new RelayCommand(OnCheckReportAsync, (o) => true);

            CheckAllReportsCommand = new RelayCommand(OnCheckAllReportsAsync, CheckAllBox_CanExecute);

            _snackbarMessageQueue = snackbarMessageQueue;

            IsAllChecked = false;
        }


        private void SetPackageStatus(object obj)
        {
            throw new NotImplementedException();
        }
        private async void ShowLogFileAsync(object obj)
        {
            var a = new ServerFiles("1234567A00001");
            var confirmDialog = new LogsDialog();

            //var confirmDialog = new MessageDialog
            //{
            //    Message = { Text = a.BinFile + " " +  a.BitLogFile + " " +  a.EmbaladoOkFile + " " +  a.IdealFile + " "
            //    + a.MasterConfigFilePath + " " + a.OrderLogsBaseFolder + " " + a.PixelTrashFile + " " + a.SerialNumberLogsFolder
            //    + " " + a.ServerLogsBaseFolder + " " + a.TestLogFile + " " + a.TestOkFile + " " + a.XmlFile }
            //};

            await DialogHost.Show(confirmDialog, new DialogClosingEventHandler((object warningSenderDialog, DialogClosingEventArgs warningEventArgs) =>
            {

            }));
        }

        #endregion

        #region export test
        private void ExportList(object obj)
        {
            if (SearchResults != null)
            {
                var resultado = ExportarLista();
                if (resultado == null) return;
                var MessageDialog = new MessageDialog
                {
                    Message = { Text = resultado }
                };
                DialogHost.Show(MessageDialog);
            }
        }
        private string ExportarLista()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Resultados de Búsqueda";
            dlg.DefaultExt = ".xslx";
            dlg.Filter = "Documentos Excel (.xlsx)|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();

            string resultado = null;
            if (result == true)
            {

                string filename = dlg.FileName;

                var progressDialog = new ProgressDialog { };
                DialogHost.Show(progressDialog, new DialogOpenedEventHandler((object senderDialog, DialogOpenedEventArgs eventArgs) =>
                {
                    var workbook = new XLWorkbook();
                    DataTable dt = new DataTable();
                    //dt = LINQToDataTable(SearchResults);

                    dt.Columns.Add("Numero de Serie");
                    dt.Columns.Add("OA Key");
                    dt.Columns.Add("Estado");
                    dt.Columns.Add("Fecha Consumida");

                    foreach (var results in SearchResults)
                    {
                        DataRow dr = dt.NewRow();
                        dr["Numero de Serie"] = results.SerialNumber;
                        dr["OA Key"] = results.ProductKey;
                        dr["Estado"] = results.ReportState;
                        dr["Fecha Consumida"] = results.DateConsumed;
                        dt.Rows.Add(dr);
                    }
                    var worksheet = workbook.Worksheets.Add("Registro de claves");
                    worksheet.Cell(1, 1).InsertTable(dt);
                    worksheet.Columns().AdjustToContents();
                    try
                    {
                        workbook.SaveAs(filename);
                        resultado = "El archivo se guardó correctamente en:" + Environment.NewLine + filename;
                    }
                    catch (Exception e)
                    {
                        resultado = "Ocurrió un error intentando guardar el archivo:" + Environment.NewLine + e.Message;
                    }
                    workbook.Dispose();
                    eventArgs.Session.Close(false);
                }));



            };
            return resultado;
        }

        #endregion

        #region properties w/fields

        private bool _isOrderSearched = false;

        private string _resultCount;
        public string ResultCount
        {
            get { return _resultCount; }
            set
            {
                _resultCount = value;
                OnPropertyChanged();
            }
        }

        private string _resultConsumedCount;
        public string ResultConsumedCount
        {
            get { return _resultConsumedCount; }
            set
            {
                _resultConsumedCount = value;
                OnPropertyChanged();
            }
        }

        private string _resultBoundCount;
        public string ResultBoundCount
        {
            get { return _resultBoundCount; }
            set
            {
                _resultBoundCount = value;
                OnPropertyChanged();
            }
        }

        private bool _showLocalStoreButtons = true;
        public bool ShowLocalStoreButtons
        {
            get
            {
                return _showLocalStoreButtons;
            }
            set
            {
                _showLocalStoreButtons = value;
                OnPropertyChanged();
            }
        }

        private bool _isOnlyBoundChecked;
        public bool IsOnlyBoundChecked
        {
            get => _isOnlyBoundChecked;
            set
            {
                _isOnlyBoundChecked = value;
                OnPropertyChanged();
            }
        }

        private bool? _isAllChecked;
        public bool? IsAllChecked
        {
            get
            {
                return this._isAllChecked;
            }
            set
            {
                bool? nullable = value;
                bool? isAllChecked = this._isAllChecked;
                if (nullable.GetValueOrDefault() == isAllChecked.GetValueOrDefault() & nullable.HasValue == isAllChecked.HasValue)
                    return;
                this._isAllChecked = value;
                this.OnPropertyChanged(nameof(IsAllChecked));
            }
        }

        private object _selectedView;
        public object SelectedView
        {
            get => _selectedView;
            set
            {
                if (value == _selectedView) return;
                _selectedView = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ProductKeyReport> _searchResults;
        public ObservableCollection<ProductKeyReport> SearchResults
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

        #endregion

        #region canexecute funcs

        private bool FindMissing_CanExecute(object arg)
        {
            return _isOrderSearched;
        }

        private bool SelectSearchQueryAsync_CanExecute(object arg)
        {
            return string.IsNullOrEmpty(_searchKeyword) ? false : true;
        }

        //private bool RecycleSelectedQueryAsync_CanExecute(object arg)
        //{
        //    return IsOnlyBoundChecked;
        //}

        private bool CheckAllBox_CanExecute(object arg)
        {
            return (SearchResults == null) ? false : true;
        }

        private bool DeleteSelectedQueryAsync_CanExecute(object arg)
        {

            if (IsAllChecked == true || IsAllChecked == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //private bool ChangeView_CanExecute(object arg)
        //{
        //    return true;
        //}

        #endregion

        #region methods

        private async void SetAsConsumed(object obj)
        {
            var confirmDialog = new ConfirmDialog
            {
                Title = { Text = "Esta acción puede acarrear errores." },

                Message = { Text = "Revertiendo a 'Consumed' está deshabilitando el control\n" +
                                   "de licencias duplicadas de este número de serie.\n" },

                AcceptButton = { Content = "ACEPTAR" }
            };

            await DialogHost.Show(confirmDialog, new DialogClosingEventHandler((object warningSenderDialog, DialogClosingEventArgs warningEventArgs) =>
            {
                if ((bool)warningEventArgs.Parameter == false) return;
                warningEventArgs.Cancel();
                warningEventArgs.Session.UpdateContent(new ProgressDialog());

                Task.Run(() =>
                {
                    IEnumerable<ProductKeyReport> selectedRecords = SearchResults
                    .Where(w => w.IsChecked == true && w.ReportState != "Consumed")
                    .Select(s => s);
                    ExecuteSetAsConsumedQuery(selectedRecords);

                }).ContinueWith((t, _) => warningEventArgs.Session.Close(false), null,
                        TaskScheduler.FromCurrentSynchronizationContext());
            }));
        }

        private void ExecuteSetAsConsumedQuery(IEnumerable<ProductKeyReport> selectedRecords)
        {
            try
            {
                var sr = selectedRecords.Count();

                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                var sqlCommand = new SqlCommand("SetAsConsumedFromID", sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                Console.WriteLine(selectedRecords.Count());

                foreach (var item in selectedRecords)
                {
                    sqlCommand.Parameters.AddWithValue("@ReportID", item.ReportID);
                    sqlCommand.Parameters.AddWithValue("@Source", "Manual");
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Parameters.Clear();
                    var serverFiles = new ServerFiles(item.SerialNumber);
                    WriteChangesToLog(item, "SetAsConsumed", serverFiles);
                }

                ObservableCollection<ProductKeyReport> newSearchResults = new ObservableCollection<ProductKeyReport>();
                foreach (var item in SearchResults)
                {
                    if (item.IsChecked)
                    {
                        item.ReportState = "Consumed";
                        item.Source = "Manual";
                        newSearchResults.Add(item);
                    }
                    else
                    {
                        newSearchResults.Add(item);
                    }
                }
                SearchResults = newSearchResults;

                sqlConnection.Close();
                _snackbarMessageQueue.Enqueue(sr + " registro(s) actualizado(s).");
            }
            catch (Exception e)
            {
                _snackbarMessageQueue.Enqueue("Ocurrió un error intentando actualizar registros. \n" + e.Message);
            }
        }

        //private void RecycleSelectedQueryAsync(object obj)
        //{
        //    throw new NotImplementedException();
        //}

        //private void EnableStoreMode(object obj)
        //{
        //    ShowLocalStoreButtons = true;
        //}

        //private void EnableQueryMode(object obj)
        //{
        //    ShowLocalStoreButtons = false;
        //}

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
                _isOrderSearched = false;
            }

            Regex regexSerialNumber = new Regex(@"^\d{7}[a-gA-G]\d{5}$");
            if (regexSerialNumber.IsMatch(_searchKeyword))
            {
                storedProcedureName = "GetRecordFromSerialNumber";
                _isOrderSearched = false;
            }

            Regex regexOrderNumber = new Regex(@"^\d{7}[a-cA-C]$");
            if (regexOrderNumber.IsMatch(_searchKeyword))
            {
                storedProcedureName = "GetRecordFromOrderNumber";
                _isOrderSearched = true;
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
                ObservableCollection<ProductKeyReport> resultList = new ObservableCollection<ProductKeyReport>();
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
                        var reportEntity = new ProductKeyReport()
                        {
                            ReportID = (int)reader["ReportID"],
                            ProductKey = (string)reader["OAKey"],
                            SerialNumber = (string)reader["SerialNumber"],
                            ReportState = (string)reader["State"],
                            Source = (string)reader["Source"],
                            DateConsumed = (DateTime)reader["DateConsumed"],
                        };

                        if ((reader["DateBound"]) != DBNull.Value)
                            reportEntity.DateBound = (DateTime)reader["DateBound"];

                        resultList.Add(reportEntity);
                    }

                    IsAllChecked = false;

                    SearchResults = resultList;

                    int rcount = SearchResults.Count;
                    if (rcount == 0)
                    {
                        _isOrderSearched = false;
                    }
                    var rencontrado = " registros en TOTAL encontrados.";
                    if (rcount == 1) { rencontrado = " registro en TOTAL encontrado."; }
                    ResultCount = rcount.ToString() + rencontrado;

                    int rccount = SearchResults.Where(w => w.ReportState == "Consumed").Count();
                    var rcencontrado = " registros CONSUMED encontrados.";
                    if (rccount == 1) { rcencontrado = " registro CONSUMED encontrado."; }
                    ResultConsumedCount = rccount.ToString() + rcencontrado;

                    int rbcount = SearchResults.Where(w => w.ReportState == "Bound").Count();
                    var rbencontrado = " registros BOUND encontrados.";
                    if (rbcount == 1) { rbencontrado = " registro BOUND encontrado."; }
                    ResultBoundCount = rbcount.ToString() + rbencontrado;

                    _snackbarMessageQueue.Enqueue(_resultCount);
                }
                catch (SqlException ex)
                {
                    _snackbarMessageQueue.Enqueue("No se pudo conectar con la base de datos." + Environment.NewLine + ex.Message);
                }
                catch (Exception e)
                {
                    _snackbarMessageQueue.Enqueue("Error intentando realizar consulta." + Environment.NewLine + e.Message);
                }
                finally
                {
                    if (reader != null) reader.Close();
                    sqlConnection.Close();
                }
            }
        }

        private async void ShowPasswordDialogAsync(object obj)
        {
            var passwordDialog = new PasswordDialog();
            string exec = "false";
            await DialogHost.Show(passwordDialog, new DialogClosingEventHandler((object passwordSenderDialog, DialogClosingEventArgs passwordEventArgs) =>
            {
                string canexe = "false";
                switch (passwordEventArgs.Parameter)
                {
                    case false:
                        canexe = "cancel";
                        break;
                    default:
                        PasswordBox pb = (PasswordBox)passwordEventArgs.Parameter;
                        if (pb.Password == "Mundial1986")
                        {
                            canexe = "true";
                        }
                        break;
                }
                exec = canexe;

            }));
            if (exec == "true")
            {
                DeleteSelectedQueryAsync();
            }
            else if (exec == "false")
            {
                MessageDialog md = new MessageDialog { Message = { Text = "Contraseña incorrecta :c" } };
                await DialogHost.Show(md);
            }
            else
            {
                return;
            }
        }

        private async void DeleteSelectedQueryAsync()
        {

            var confirmDialog = new ConfirmDialog
            {
                Title = { Text = "Esta acción puede acarrear errores." },

                Message = { Text = "Antes de de eliminar registros, confirme que se encuentran \n" +
                                   "en estado 'ALLOCATED' en la aplicación de Microsoft. \n" },

                AcceptButton = { Content = "ELIMINAR" }
            };

            await DialogHost.Show(confirmDialog, new DialogClosingEventHandler((object warningSenderDialog, DialogClosingEventArgs warningEventArgs) =>
            {
                if ((bool)warningEventArgs.Parameter == false) return;

                warningEventArgs.Cancel();
                warningEventArgs.Session.UpdateContent(new ProgressDialog());
                Task.Run(() =>
                {
                    IEnumerable<ProductKeyReport> selectedRecords = SearchResults
                    .Where(w => w.IsChecked == true)
                    .Select(s => s);
                    ExecuteDeleteQuery(selectedRecords);
                })
                .ContinueWith((t, _) => warningEventArgs.Session.Close(false), null,
                              TaskScheduler.FromCurrentSynchronizationContext());
            }));
        }

        private void ExecuteDeleteQuery(IEnumerable<ProductKeyReport> selectedRecords)
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
                //var ped = selectedRecords.Select(s => s.SerialNumber).FirstOrDefault();
                //SearchKeyword = ped.Substring(0, 8);
                //SelectSearchQueryAsync(SearchKeyword);
                sqlConnection.Close();
                _snackbarMessageQueue.Enqueue(selectedRecords.Count() + " registro(s) eliminado(s).");
            }
            catch (Exception e)
            {
                _snackbarMessageQueue.Enqueue("Ocurrió un error intentando eliminar registros. \n" + e.Message);
            }
        }

        private void WriteChangesToLog(ProductKeyReport report, string action, ServerFiles serverFiles)
        {
            var timestamp = DateTime.Now.ToString("dd-MM-yyyy") + " - " + DateTime.Now.ToString("H:mm:ss");
            List<string> warning;
            switch (action)
            {
                case "DeleteKey":
                    warning = new List<string>
                    {
                        timestamp + "  !      LICENCIA ELIMINADA DEL FLOOR",
                        "                       ! KEY            : " + report.ProductKey,
                        "                       ! ESTADO         : " + report.ReportState,
                        "                       ! ORIGEN         : " + report.Source,
                        "                       ! CONSUMIDA      : " + report.DateConsumed,
                        "                       ! REPORTADA      : " + report.DateBound,
                        "                       ! ID             : " + report.ReportID,
                        "                       ! ProductKeyID   : " + report.ProductKeyID,
                        "                       ! PartNumber     : " + report.ProductKeyPartNumber,
                        "                       ! ProductKeyState: " + report.ProductKeyState
                    };
                    break;
                case "SetAsConsumed":
                    warning = new List<string> { timestamp + "  -= LICENCIA DEVUELTA A ESTADO 'CONSUMIDA' =-" };
                    break;
                default:
                    throw new Exception("No se especificó una acción para el método WriteChangesToLog");
            }
            File.AppendAllLines(serverFiles.TestLogFile, warning);
        }

        private void DeleteLogFiles(ProductKeyReport report)
        {
            var serverFiles = new ServerFiles(report.SerialNumber);
            if (File.Exists(serverFiles.XmlFile)) { File.Delete(serverFiles.XmlFile); }
            if (File.Exists(serverFiles.BinFile)) { File.Delete(serverFiles.BinFile); }
            WriteChangesToLog(report, "DeleteKey", serverFiles);
        }

        private void DeleteFromSearchResult(ProductKeyReport report)
        {
            if (report == null) throw new ArgumentNullException();
            //mm updatea en tiempo real...
            var newList = new ObservableCollection<ProductKeyReport>(SearchResults);
            newList.Remove(report);
            SearchResults = newList;
        }

        private async void OnCheckAllReportsAsync(object o)
        {
            var progressDialog = new ProgressDialog { };
            await DialogHost.Show(progressDialog, new DialogOpenedEventHandler((object senderDialog, DialogOpenedEventArgs eventArgs) =>
            {
                Task.Run(() =>
                {
                    var newList = new ObservableCollection<ProductKeyReport>();
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
                    {
                        IsAllChecked = true;
                    }
                    else if (SearchResults.All(x => !x.IsChecked))
                    {
                        IsAllChecked = false;

                    }
                    else
                    {
                        IsAllChecked = null;
                    }

                    //var selection = SearchResults.Where(w => w.IsChecked == true).Select(s => s);
                    //if (selection.All(s => s.State == "Bound"))
                    //    IsOnlyBoundChecked = true;
                    //else if (selection.Any())
                    //    IsOnlyBoundChecked = false;
                    //else
                    //    IsOnlyBoundChecked = false;

                }).ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                TaskScheduler.FromCurrentSynchronizationContext());
            }));
        }

        private async void FindMissingNumbers(object o)
        {
            var inputDialog = new InputDialog();

            await DialogHost.Show(inputDialog, new DialogClosingEventHandler((object inputSenderDialog, DialogClosingEventArgs inputEventArgs) =>
            {
                //Regex regex = new Regex("[^0-9]+");
                //if (!regex.IsMatch(inputEventArgs.Parameter.ToString()))
                //{
                //    return;
                //}
                Console.WriteLine("tamo ativo papi");
                switch (inputEventArgs.Parameter)
                {
                    case false:
                        return;
                    default:
                        var total = Convert.ToInt32(inputEventArgs.Parameter);
                        inputEventArgs.Cancel();
                        inputEventArgs.Session.UpdateContent(new ProgressDialog());
                        List<int> found = new List<int>();
                        foreach (var item in SearchResults)
                        {
                            var n = Convert.ToInt32(item.SerialNumber.Substring(8, 5));
                            found.Add(n);
                        }

                        var missingNumbers = Enumerable.Range(1, total).Except(found);
                        var mn = string.Join(", ", missingNumbers);
                        if (string.IsNullOrWhiteSpace(mn)) { mn = "Pedido Completo!"; }
                        if (total < SearchResults.Count) { mn = "Total inválido!"; }
                        if (total < found.Max()) { mn = "Total inválido!"; }

                        var messageDialog = new MessageDialog
                        {
                            Message = { Text = "Números faltantes: \n \n" + mn }
                        };
                        inputEventArgs.Session.UpdateContent(messageDialog);
                        break;
                }


            }
            ));
        }

        #endregion

        #region onpropertychanged event hanlder

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}


