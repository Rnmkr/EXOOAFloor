using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EXOOAFloor.ViewModel
{
    public class LogsDialogViewModel : INotifyPropertyChanged
    {
        public LogsDialogViewModel()
        {

            SelectedNumbersList = new List<string> { "00001", "00002", "00003", "00004", "00005" };
            OrderNumber = "1234567A";
        }


        public List<string> SelectedNumbersList { get; set; }

        public string OrderNumber { get; set; }


        #region onpropertychanged event hanlder

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
