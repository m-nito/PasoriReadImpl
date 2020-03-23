using System;

#if false
- iphoneのSuicaを読み取り、そのままダンプした数値の例
Suica 乗降履歴情報 [0]  C7-46-00-00-28-70-A2-A5-E6-4F-7B-28-00-14-0E-00
Suica 乗降履歴情報 [1]  C7-46-00-00-28-6E-B5-65-E6-4E-92-2E-00-14-0D-00
Suica 乗降履歴情報 [2]  1B-02-3F-00-28-6E-02-FD-00-00-B2-32-00-14-0C-00
Suica 乗降履歴情報 [3]  C8-46-00-00-28-6D-73-4B-1B-2D-A2-0B-00-14-0B-00
Suica 乗降履歴情報 [4]  16-01-00-02-28-6D-C5-02-C5-04-C9-0E-00-14-0A-00
Suica 乗降履歴情報 [5]  16-01-00-02-28-6D-C5-04-C5-02-47-0F-00-14-08-00
Suica 乗降履歴情報 [6]  C7-46-00-00-28-6B-6D-8C-37-64-C5-0F-00-14-06-00
Suica 乗降履歴情報 [7]  C8-46-00-00-28-6B-6D-2A-AE-D0-E4-12-00-14-05-00
Suica 乗降履歴情報 [8]  C8-46-00-00-28-69-8B-2F-89-88-CE-14-00-14-04-00
Suica 乗降履歴情報 [9]  C7-46-00-00-28-69-8A-AA-B1-B9-3C-15-00-14-03-00
Suica 乗降履歴情報 [10]  C7-46-00-00-28-67-6E-CC-37-64-76-18-00-14-02-00
Suica 乗降履歴情報 [11]  C7-46-00-00-28-67-6E-2C-05-C7-CE-1C-00-14-01-00
Suica 乗降履歴情報 [12]  C7-46-00-00-28-67-34-45-E6-4E-24-2D-00-14-00-00
Suica 乗降履歴情報 [13]  1B-02-3F-00-28-67-02-FD-00-00-A4-30-00-13-FF-00
Suica 乗降履歴情報 [14]  C8-46-00-00-28-66-23-66-4A-3B-94-09-00-13-FE-00
Suica 乗降履歴情報 [15]  C8-46-00-00-28-65-1F-06-4A-3B-0B-0F-00-13-FD-00
Suica 乗降履歴情報 [16]  C7-46-00-00-28-64-40-47-C7-E5-35-14-00-13-FC-00
Suica 乗降履歴情報 [17]  C7-46-00-00-28-63-3B-E5-E6-4F-FB-16-00-13-FB-00
Suica 乗降履歴情報 [18]  C7-46-00-00-28-62-05-25-E6-4E-9B-1B-00-13-FA-00
Suica 乗降履歴情報 [19]  C8-46-00-00-28-5D-84-2B-1B-2D-64-21-00-13-F9-00
#endif

namespace PasoriReadImpl
{
    /// <summary>
    /// Suica利用履歴
    /// 仕様はhttps://www.wdic.org/w/RAIL/%E3%82%B5%E3%82%A4%E3%83%90%E3%83%8D%E8%A6%8F%E6%A0%BC%20%28IC%E3%82%AB%E3%83%BC%E3%83%89%29を参照
    /// </summary>
    public class SuicaHistory
    {
        /// <summary>
        /// この決済が利用した機器の種別です。
        /// </summary>
        public DeviceType Device = DeviceType.Unknown;
        /// <summary>
        /// この決済のサービス種別です。
        /// </summary>
        public ServiceType Usage = ServiceType.Unknown;
        /// <summary>
        /// この決済の決済種別です。
        /// </summary>
        public PaymentType Payment = PaymentType.Unknown;
        /// <summary>
        /// 決済の日付（時刻は含みません）
        /// </summary>
        public DateTime Date;
        /// <summary>
        /// 現在の残高です。
        /// </summary>
        public int Balance = -1;
        /// <summary>
        /// 支払金額です。前回履歴の残高から計算する外ない仕様であるため、最終履歴の支払金額は取得できません。
        /// </summary>
        public int Paid = 0;
        /// <summary>
        /// 履歴連番です。
        /// </summary>
        public int HistoryNumber = -1;
        /// <summary>
        /// 地域情報です。
        /// </summary>
        public AreaCode Area = AreaCode.Unknown;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data"></param>
        public SuicaHistory(byte[] data)
        {
            this.Device = ConvertTo<DeviceType>(data[0]);
            this.Usage = ConvertTo<ServiceType>(data[1]);
            this.Payment = ConvertTo<PaymentType>(data[2]);
            this.Date = ToDateTime(data);
            this.Balance = ReadLittle2Bytes(data, 10);
            this.SetSpecifics(data);
            this.HistoryNumber = ReadBig2Bytes(data, 13);
            this.Area = ConvertTo<AreaCode>(data[15]);
        }

        /// <summary>
        /// 受け取ったbyteを列挙型として返します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <returns></returns>
        private T ConvertTo<T>(byte b)
        {
            if (Enum.IsDefined(typeof(T), b))
                return (T)Enum.Parse(typeof(T), b.ToString());
            return default;
        }

        /// <summary>
        /// 日付値を取得してDateTimeを返します。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private DateTime ToDateTime(byte[] bytes)
        {
            if (bytes == null) return new DateTime();
            var date = ReadBig2Bytes(bytes, 4);
            if (date == 0) return new DateTime();
            int yy = (date >> 9) + 2000;
            int mm = (date >> 5) & 0xf;
            int dd = date & 0x1f;

            return new DateTime(yy, mm, dd);
        }

        /// <summary>
        /// 決済種別による固有情報をセットします。
        /// </summary>
        /// <param name="bytes"></param>
        private void SetSpecifics(byte[] bytes)
        {
            switch (this.Device)
            {
                case DeviceType.Gate:
                case DeviceType.Norikoshi:
                    this.SetTrainSpecifics(bytes);
                    break;
                case DeviceType.PurchaseA:
                case DeviceType.PurchaseB:
                    this.SetShoppingSpecifics(bytes);
                    break;
                default:
                    return;
            }
        }

        #region Specifics
        public int EntryStationCode = -1;
        public int ExitStationCode = -1;
        private void SetTrainSpecifics(byte[] bytes)
        {
            this.EntryStationCode = ReadBig2Bytes(bytes, 6);
            this.ExitStationCode = ReadBig2Bytes(bytes, 8);
        }
        public int StationCode = -1;
        public int TicketMachineCode = -1;
        private void SetTicketSpecifics(byte[] bytes)
        {
            this.StationCode = ReadBig2Bytes(bytes, 6);
            this.TicketMachineCode = ReadBig2Bytes(bytes, 8);
        }
        public int BusCompanyCode = -1;
        public int BusStopCode = -1;
        private void SetBusSpecifics(byte[] bytes)
        {
            this.BusCompanyCode = ReadBig2Bytes(bytes, 6);
            this.BusStopCode = ReadBig2Bytes(bytes, 8);
        }

        /// <summary>
        /// 決済時刻
        /// </summary>
        public DateTime CheckoutTime;
        /// <summary>
        /// 決済端末のID
        /// </summary>
        public int PaymentDeviceId = -1;
        private void SetShoppingSpecifics(byte[] bytes)
        {
            var time = ReadBig2Bytes(bytes, 6);
            // var hour = (time >> 11) & 0x17;
            var hour = (time >> 11);
            var min = (time >> 5) & 0x3B;
            var sec = (time & 0x1d) * 2;
            this.CheckoutTime = new DateTime(
                year: this.Date.Year,
                month: this.Date.Month,
                day: this.Date.Day,
                hour: hour,
                minute: min,
                second: sec);
            this.PaymentDeviceId = ReadBig2Bytes(bytes, 8);
        }
        #endregion

        /// <summary>
        /// posで指定した地点から2バイトをビッグエンディアンで読み込みます。
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int ReadBig2Bytes(byte[] b, int pos) => b[pos] << 8 | b[pos + 1];

        /// <summary>
        /// posで指定した地点から2バイトをリトルエンディアンで読み込みます。
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int ReadLittle2Bytes(byte[] b, int pos) => b[pos + 1] << 8 | b[pos];

        /// <summary>
        /// 機器コード
        /// </summary>
        public enum DeviceType : byte
        {
            Unknown = 0x00,
            Norikoshi = 0x03,
            Bus = 0x05,
            Ticket = 0x07,
            Gate = 0x16,
            PasoriApplePayEtc = 0x1B,
            PurchaseA = 0xC7,
            PurchaseB = 0xC8,
        }
        /// <summary>
        /// サービス種別コード
        /// </summary>
        public enum ServiceType : byte
        {
            Unknown = 0x00,
            Gate = 0x01,
            SFCharge = 0x02,
            Ticket = 0x03,
            PaymentA = 0x04,
            PaymentB = 0x05,
            PasoriEtc = 0x1B,
            PurchaseA = 0x46,
            PointCharge = 0x48,
        }
        /// <summary>
        /// 決済種別コード
        /// </summary>
        public enum PaymentType : byte
        {
            Unknown = 0xFF,
            Normal = 0x00,
            MobileSuica = 0x3F,
        }
        /// <summary>
        /// 地域コード
        /// </summary>
        public enum AreaCode : byte
        {
            Unknown = byte.MaxValue,
            JapanRail = 0,
            ChubuRegion = 1,
            KansaiRegion = 2,
            Other = 3,
        }
    }
}