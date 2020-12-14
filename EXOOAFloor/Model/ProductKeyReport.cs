using System;

namespace EXOOAFloor
{
    internal class ProductKeyReport
    {
        private bool _isChecked;
        private int _reportID;
        private string _reportState;
        private string _productKey;
        private string _serialNumber;
        private string _productKeyVersion;
        private DateTime _dateConsumed;
        private DateTime? _dateBound;
        private string _source;
        private DateTime? _datePacked;
        private string _productKeyID;
        private string _productKeyState;
        private string _productKeyPartNumber;


        public bool IsChecked { get => _isChecked; set => _isChecked = value; }
        public int ReportID { get => _reportID; set => _reportID = value; }
        public string ReportState { get => _reportState; set => _reportState = value; }
        public string ProductKey { get => _productKey; set => _productKey = value; }
        public string SerialNumber { get => _serialNumber; set => _serialNumber = value; }
        public string ProductKeyVersion { get => _productKeyVersion; set => _productKeyVersion = value; }
        public DateTime DateConsumed { get => _dateConsumed; set => _dateConsumed = value; }
        public DateTime? DateBound { get => _dateBound; set => _dateBound = value; }
        public DateTime? DatePacked { get => _datePacked; set => _datePacked = value; }
        public string Source { get => _source; set => _source = value; }
        public string ProductKeyID { get => _productKeyID; set => _productKeyID = value; }
        public string ProductKeyState { get => _productKeyState; set => _productKeyState = value; }
        public string ProductKeyPartNumber { get => _productKeyPartNumber; set => _productKeyPartNumber = value; }
    }
}