/// <summary>
/// This class represents the defined results model provided by 'Beurer Gl50 evo' device documentation
/// </summary>
namespace BleConnector.Models {
    class Glucometer {
        public byte Flags { get; set; }
        public short SequenceNumber { get; set; }
        public Timestamp Timestamp { get; set; }
        public string Glucose { get; set; }
    }
}
