using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EXOOAFloor
{
    internal class OAKeyReport : INotifyPropertyChanged
    {
        private bool _isChecked= false;
        private int _reportID;
        private string _oAKey;
        private string _serialNumber;
        private string _state;
        private DateTime _dateConsumed;
        private DateTime? _dateBound;

        public bool IsChecked
        { 
            get => _isChecked;

            set
            {
                _isChecked = value;
                if (value == _isChecked) return;

                OnPropertyChanged("SearchResults");
            }
        }
        public int ReportID { get => _reportID; set => _reportID = value; }
        public string OAKey { get => _oAKey; set => _oAKey = value; }
        public string SerialNumber { get => _serialNumber; set => _serialNumber = value; }
        public string State { get => _state; set => _state = value; }
        public DateTime DateConsumed { get => _dateConsumed; set => _dateConsumed = value; }
        public DateTime? DateBound { get => _dateBound; set => _dateBound = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}