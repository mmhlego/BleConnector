/// <summary>
/// This class is used to represent custom range in models. For example for spO2 or pulse rate results from oximeter device;
/// </summary>
namespace BleConnector.Models {
    class Range<T> {
        public T Min { get; set; }
        public T Average { get; set; }
        public T Max { get; set; }

    }
}
