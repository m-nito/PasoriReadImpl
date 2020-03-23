namespace PasoriReadImpl
{
    public partial class Felica
    {
        public enum SystemCode : int
        {
            Any = 0xffff,           // ANY
            Common = 0xfe00,        // 共通領域
            Cyberne = 0x0003,       // サイバネ領域

            Edy = 0xfe00,           // Edy (=共通領域)
            Suica = 0x0003,         // Suica (=サイバネ領域)
            QUICPay = 0x04c1,       // QUICPay
        }
    }
}