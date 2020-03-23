using System.Collections.Generic;
using System.Linq;

namespace PasoriReadImpl
{
    /// <summary>
    /// Suica関連機能クラス
    /// </summary>
    public class Suica
    {
        private readonly Felica _Felica;
        /// <summary>
        /// このカードが保持するIdm(識別ID)
        /// </summary>
        public readonly byte[] Idm;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="f"></param>
        public Suica(Felica f)
        {
            this._Felica = f;
            this.Idm = f.GetIdm();
        }

        /// <summary>
        /// Suica利用履歴を取得して返します。
        /// </summary>
        /// <returns></returns>
        public SuicaHistory[] GetHistories ()
        {
            // card read may fail in the middle of operation, thus trying to finish asap.
            var datas = Enumerable.Range(0, 20).AsParallel().AsOrdered().
                Select(i => this._Felica.ReadWithoutEncryption((int)ServiceCode.History, i)).
                ToArray();
            return datas.Select(d => new SuicaHistory(d)).ToArray();
        }

        /// <summary>
        /// Suicaが利用するサービスコード
        /// </summary>
        private enum ServiceCode : int
        {
            Basic = 0x008B,
            History = 0x090F,
            Gate = 0x108F,
            StoredFare = 0x10CB,
        }
    }
}