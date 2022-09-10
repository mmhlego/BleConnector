using mintti_sdk.ble;

namespace BleConnector.Utils {
    /// <summary>
    /// This class processes the raw data received from the stethoscope device and returns it to us.
    /// </summary>
    class HandleRawData {
        private short[] mAecRemainPkgMic = new short[256];

        private short[] mAecRemainPkgSpk = new short[256];

        private short[] aecMicIn = new short[256];

        private short[] aecSpkIn = new short[256];

        private int aecWritedCount = 0;

        private int aecRemainCount = 0;

        private bool canProcessAec = false;

        private short[] mGainRemainPkg = new short[800];

        private short[] mInputData = new short[800];

        private int gainWritedCount = 0;

        private int gainRemainCount = 0;

        private bool canProcessGain = false;

        private bool isOpenAes = true;

        private static readonly HandleRawData singleInstance = new HandleRawData();

        public static HandleRawData GetInstance => singleInstance;

        private HandleRawData() {
        }

        public void setOpenAes(bool openAes) {
            isOpenAes = openAes;
        }

        public short[] aecProcessData(short[] mic_data, short[] spk_data) {
            short[] array = new short[256];
            if (mic_data == null || spk_data == null) {
                return null;
            }

            if (aecWritedCount == 0 && aecRemainCount != 0) {
                int num = 0;
                while (num < aecRemainCount) {
                    aecMicIn[aecWritedCount] = mAecRemainPkgMic[num];
                    aecSpkIn[aecWritedCount] = mAecRemainPkgSpk[num];
                    num++;
                    aecWritedCount++;
                }

                aecRemainCount = 0;
            }

            int num2 = 0;
            while (num2 < mic_data.Length) {
                if (aecWritedCount == 256) {
                    aecRemainCount = mic_data.Length - num2;
                    aecWritedCount = 0;
                    for (int i = 0; i < aecRemainCount; i++) {
                        mAecRemainPkgMic[i] = mic_data[num2 + i];
                        mAecRemainPkgSpk[i] = spk_data[num2 + i];
                    }

                    canProcessAec = true;
                    break;
                }

                aecMicIn[aecWritedCount] = mic_data[num2];
                aecSpkIn[aecWritedCount] = spk_data[num2];
                num2++;
                aecWritedCount++;
            }

            if (canProcessAec) {
                if (isOpenAes) {
                    SmarthoAlgo.aes_process(aecMicIn, aecSpkIn, 256, array);
                } else {
                    Array.Copy(aecMicIn, 0, array, 0, array.Length);
                }

                canProcessAec = false;
                return array;
            }

            return null;
        }

        public short[] clipDistortionAndAutoGain(short[] inputData, EchoMode echoMode) {
            short[] array = new short[800];
            if (gainWritedCount == 0 && gainRemainCount != 0) {
                int num = 0;
                while (num < gainRemainCount) {
                    mInputData[gainWritedCount] = mGainRemainPkg[num];
                    num++;
                    gainWritedCount++;
                }

                gainRemainCount = 0;
            }

            int num2 = 0;
            while (num2 < inputData.Length) {
                if (gainWritedCount == 800) {
                    gainRemainCount = inputData.Length - num2;
                    gainWritedCount = 0;
                    for (int i = 0; i < gainRemainCount; i++) {
                        mGainRemainPkg[i] = inputData[num2 + i];
                    }

                    canProcessGain = true;
                    break;
                }

                mInputData[gainWritedCount] = inputData[num2];
                num2++;
                gainWritedCount++;
            }

            if (canProcessGain) {
                Array.Copy(mInputData, 0, array, 0, mInputData.Length);
                if (echoMode == EchoMode.MODE_DIAPHRAGM_ECHO) {
                    SmarthoAlgo.smarthoAlgo_agc_process(array, array.Length);
                } else {
                    SmarthoAlgo.smarthoAlgo_clip_distortion(array, array.Length);
                }

                canProcessGain = false;
                return array;
            }

            return null;
        }

        public void reset() {
            canProcessAec = false;
            canProcessGain = false;
            mAecRemainPkgMic = new short[256];
            mAecRemainPkgSpk = new short[256];
            aecMicIn = new short[256];
            aecSpkIn = new short[256];
            aecWritedCount = 0;
            aecRemainCount = 0;
            mGainRemainPkg = new short[800];
            mInputData = new short[800];
            gainWritedCount = 0;
            gainRemainCount = 0;
        }
    }
}
