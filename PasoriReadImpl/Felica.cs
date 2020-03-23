using System;
using System.Runtime.InteropServices;

namespace PasoriReadImpl
{
    public partial class Felica : IDisposable
    {
        #region DllImports
        const string DLLPATH = "libs/felicalib.dll";

        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr pasori_open(String dummy);

        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int pasori_close(IntPtr p);

        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int pasori_init(IntPtr p);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern void felica_free(IntPtr f);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr felica_polling(IntPtr p, ushort systemcode, byte rfu, byte time_slot);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int felica_getidm(IntPtr f, byte[] data);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int felica_getpmm(IntPtr f, byte[] data);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int felica_read_without_encryption02(IntPtr f, int servicecode, int mode, byte addr, byte[] data);
        #endregion

        /// <summary>
        /// パソリハンドルへの参照
        /// </summary>
        private IntPtr _Pasori = IntPtr.Zero;
        /// <summary>
        /// フェリカハンドルへの参照
        /// </summary>
        private IntPtr _Felica = IntPtr.Zero;

        /// <summary>
        /// felicalib.dllを取得します。
        /// </summary>
        /// <returns></returns>
        public FelicaMessage Open()
        {
            this._Pasori = pasori_open(null);
            if (_Pasori == IntPtr.Zero) return FelicaMessage.PasoriOpenFailure;
            return FelicaMessage.PasoriOpenSuccess;
        }
        /// <summary>
        /// Pasoriデバイスの初期化を行います。
        /// </summary>
        /// <returns></returns>
        public FelicaMessage Init()
        {
            if (pasori_init(this._Pasori) != 0) return FelicaMessage.PasoriInitFailure;
            return FelicaMessage.PasoriInitSuccess;
        }
        
        /// <summary>
        /// 終了処理を行います。
        /// </summary>
        public void Close()
        {
            if (this._Pasori == IntPtr.Zero) return;
            pasori_close(_Pasori);
            this._Pasori = IntPtr.Zero;
        }

        /// <summary>
        /// 引数とされたシステムコードを利用してポーリングを行います。
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public FelicaMessage Polling(SystemCode code)
        {
            return this.Polling((int)code);
        }
        public FelicaMessage Polling(int systemCode)
        {
            felica_free(this._Felica);
            this._Felica = felica_polling(this._Pasori, (ushort)systemCode, 0, 0);
            if (this._Felica == IntPtr.Zero)
            {
                return FelicaMessage.PasoriPollingFailure;
            }
            return FelicaMessage.PasoriPollingSuccess;
        }

        /// <summary>
        /// 非暗号化データを読み取ります。
        /// </summary>
        /// <param name="serviceCode"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public byte[] ReadWithoutEncryption(int serviceCode, int addr)
        {
            if (this._Felica == IntPtr.Zero)
            {
                return null;
            }
            byte[] data = new byte[16];
            var ret = felica_read_without_encryption02(this._Felica, serviceCode, mode: 0, (byte)addr, data);
            return data;
        }

        /// <summary>
        /// カードのIdmを取得します。
        /// </summary>
        /// <returns></returns>
        public byte[] GetIdm()
        {
            if (this._Felica == IntPtr.Zero)
            {
                return null;
            }
            byte[] data = new byte[8];
            var ret = felica_getidm(this._Felica, data);
            return data;
        }

        /// <summary>
        /// リソース廃棄処理
        /// </summary>
        public void Dispose() => this.Close();

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~Felica() => this.Dispose();
    }
}