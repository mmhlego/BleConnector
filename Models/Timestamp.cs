using System;
/// <summary>
/// Standard representation of timestamps in bluetooth low energy communications
/// </summary>
namespace BleConnector.Models {
    class Timestamp {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }


        public int CompareTo(Timestamp obj) {
            return (new DateTime(Year, Month, Day, Hour, Minute, Second).CompareTo(new DateTime(obj.Year, obj.Month, obj.Day, obj.Hour, obj.Minute, obj.Second)));
        }
    }
}
