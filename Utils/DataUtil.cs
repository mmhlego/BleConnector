namespace BleConnector.Utils {
    public class DataUtil {
        /// <summary>
        /// This function allows us to convert a short number to 2 bytes. (Used in manager)
        /// </summary>
        /// <param name="paramShort"></param>
        /// <returns></returns>
        public static byte[] Short2Bytes(short paramShort) {
            return new byte[2]
            {
                (byte)(paramShort & 0xFF),
                (byte)((paramShort >> 8) & 0xFF)
            };
        }

        /// <summary>
        /// This function has been used to convert the received data from measurement characteristic to short array
        /// </summary>
        /// <param name="byteArr"></param>
        /// <returns></returns>
        public static short[] byteArray2ShortArray(byte[] byteArr) {
            short[] array = new short[byteArr.Length / 2];
            for (int i = 0; i < array.Length; i++) {
                array[i] = BitConverter.ToInt16(new byte[2]
                {
                    byteArr[2 * i],
                    byteArr[2 * i + 1]
                }, 0);
            }

            return array;
        }

        /// <summary>
        /// This function has been used to convert the received data from measurement characteristic to short array
        /// </summary>
        /// <param name="byteArr"></param>
        /// <returns></returns>
        public static short[] byteArray2ShortArray2(byte[] byteArr) {
            short[] array = new short[byteArr.Length / 2];
            for (int i = 0; i < array.Length; i++) {
                array[i] = (short)((byteArr[2 * i + 1] << 8) | byteArr[2 * i]);
            }

            return array;
        }
    }
}
