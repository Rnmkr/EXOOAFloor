using System;

namespace EXOOAFloor
{
    internal class OAKeyReport
    {
        private bool _isChecked;
        private int _reportID;
        private string _oAKey;
        private string _serialNumber;
        private string _state;
        private string _source;
        private DateTime _dateConsumed;
        private DateTime? _dateBound;


        public bool IsChecked { get => _isChecked; set => _isChecked = value; }
        public int ReportID { get => _reportID; set => _reportID = value; }
        public string OAKey { get => _oAKey; set => _oAKey = value; }
        public string SerialNumber { get => _serialNumber; set => _serialNumber = value; }
        public string State { get => _state; set => _state = value; }
        public string Source { get => _source; set => _source = value; }
        public DateTime DateConsumed { get => _dateConsumed; set => _dateConsumed = value; }
        public DateTime? DateBound { get => _dateBound; set => _dateBound = value; }
    }
}