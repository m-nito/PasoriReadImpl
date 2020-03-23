using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasoriReadImpl
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Pasori端末の初期化を行います...");
            using (var f = new Felica())
            {
                if (f.Open() == Felica.FelicaMessage.PasoriOpenFailure)
                {
                    Console.WriteLine("Pasoriとの接続に失敗しました。");
                    return;
                }
                if (f.Init() == Felica.FelicaMessage.PasoriInitFailure)
                {
                    Console.WriteLine("Pasoriの初期化に失敗しました。カードをかざした状態で初期化を行っていないか確認してください。");
                    return;
                }
                
                // 準備完了
                Console.WriteLine("Ready to read.");
                
                // Suica以外も受け付けるなら分岐にする
                ReadSuica(f);
            }
        }

        /// <summary>
        /// Suicaの読み取りを待機し、読み取りを行ったなら基本情報をコンソール出力します。
        /// </summary>
        /// <param name="f"></param>
        static void ReadSuica(Felica f)
        {
            int timeOut = 0;
            while (!Console.KeyAvailable)
            {
                // Suica用ポーリングを実施
                var pollingResult = f.Polling(Felica.SystemCode.Suica);

                // ポーリングに成功しないなら待機
                if (pollingResult == Felica.FelicaMessage.PasoriPollingFailure)
                {
                    ++timeOut;
                    if (timeOut > 9999)
                    {
                        Console.WriteLine("Timeout.");
                        break;
                    }
                    // タイムアウトしないなら継続
                    continue;
                }

                // ポーリング成功ならSuica情報の生成
                var s = new Suica(f);
                // カードのIdmを表示
                Console.WriteLine(BitConverter.ToString(s.Idm));
                // 利用履歴を生成してコンソール出力
                var histories = s.GetHistories();
                foreach (var h in histories) Console.WriteLine($"{h.Device},{h.Usage},{h.Payment},{string.Format("{0:yyyy/MM/dd}", h.Date)},{h.Balance}");
                break;
            }
            Console.ReadKey();
        }
    }
}