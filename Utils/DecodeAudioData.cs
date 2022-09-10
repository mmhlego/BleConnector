namespace BleConnector {
    /// <summary>
    /// This class contains the audio decode functions that allows us to obtain meaningful audio from raw data.
    /// </summary>
    internal class DecodeAudioData {
        private sbyte[] indexTable = new sbyte[16] { -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8 };

        private short[] stepSizeTable = new short[89] { 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767 };

        private int m_predictedSample = 0;

        private int m_index = 0;

        public short[] decodeData(byte[] encodeData) {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = m_predictedSample;
            int num5 = m_index;
            int num6 = stepSizeTable[m_index];
            short[] array = new short[encodeData.Length * 2];
            byte b = 0;
            int num7 = 0;
            byte b2 = 4;
            byte b3 = 0;
            bool flag = true;
            for (num = 0; num < encodeData.Length; num++) {
                byte b4 = encodeData[num];
                for (num3 = 0; num3 < 2; num3++) {
                    b2 = 4;
                    num7 = 0;
                    if (flag) {
                        b = (byte)((b4 >> 4) & 0xF);
                        flag = false;
                    } else {
                        b = (byte)(b4 & 0xF);
                        flag = true;
                    }

                    b3 = (byte)(b & 7);
                    int num8 = num6 << 3;
                    for (num2 = 0; num2 < 3; num2++) {
                        if ((b & b2) != 0) {
                            num7 += num8;
                        }

                        b2 = (byte)(b2 >> 1);
                        num8 = (short)(num8 >> 1);
                    }

                    num7 >>= 3;
                    if ((b & 8) != 0) {
                        num7 = -num7;
                    }

                    array[num * 2 + num3] = (short)(num4 + num7);
                    int num9 = b3 * num6 + num6;
                    num9 >>= 2;
                    num4 = (((b & 8) == 0) ? (num4 + num9) : (num4 + -num9));
                    if (num4 > 32767) {
                        num4 = 32767;
                    }

                    if (num4 < -32768) {
                        num4 = -32768;
                    }

                    m_predictedSample = num4;
                    num5 += indexTable[b3];
                    if (num5 < 0) {
                        num5 = 0;
                    }

                    if (num5 > 88) {
                        num5 = 88;
                    }

                    m_index = num5;
                    num6 = stepSizeTable[num5];
                }
            }

            return array;
        }
    }
}
