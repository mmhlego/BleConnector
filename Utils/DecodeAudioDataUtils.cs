namespace BleConnector.Utils {
    /// <summary>
    /// This class holds the microphone and speaker audio decoder instances as singleton inside itself.
    /// </summary>
    class DecodeAudioDataUtils {
        private DecodeAudioData decodeMicData = new DecodeAudioData();

        private DecodeAudioData decodeSpkData = new DecodeAudioData();

        private static readonly DecodeAudioDataUtils singleInstance = new DecodeAudioDataUtils();

        public static DecodeAudioDataUtils GetInstance => singleInstance;

        private DecodeAudioDataUtils() {
        }

        public short[] DecodeMicData(byte[] data) {
            return decodeMicData.decodeData(data);
        }

        public short[] DecodeSpkData(byte[] data) {
            return decodeSpkData.decodeData(data);
        }
    }
}
