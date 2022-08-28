/// <summary>
/// This class represents the defined results model provided by 'Beurer PO60 Glucometer' device documentation
/// </summary>
namespace BleConnector.Models {
    class Oximeter {
        public byte Header { get; set; }
        public int Packetnumber { get; set; }
        public Timestamp Start { get; set; }
        public Timestamp End { get; set; }

    }
}
