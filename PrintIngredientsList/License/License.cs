using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;



namespace PrintIngredientsList
{
    public class LicenseManager
    {
        //CreateKey()関数で以下の文字列を取得しています。
        private string KEY = "Xy3QJPFmQ82c9qrOpCIqd3vwMhoSKQJqOYFl/QPG/s4=";
        private string IV = "YR4QYUVC2kBFn81fS9pGag==";

        private char SEP = '\n';


        //ライセンス設定ファイル出力　項目キー
       // const string KEY_MAIL_ADDR = "MAIL_ADDR";
        const string KEY_MAC_ADDR = "MAC_ADDR";
        const string KEY_LIMIT_DATE = "LIMIT_DATE";

        public static string licenseFileName = "licence.dat";


        public class LicenseInfo
        {

            //--------------------------------------------------------------
            //Serialized パラメータ
            //--------------------------------------------------------------
            /// <summary>
            /// URL
            /// </summary>
            public string MacAddr { get; set; } = "";
            /// <summary>
            /// メールアドレス
            /// </summary>
            public string MailAddr { get; set; } = "";
            /// <summary>
            /// 有効期限
            /// </summary>
            public DateTime? LimitDate { get; set; } = null;
        }

        //--------------------------------------------------------------
        //No Serialized パラメータ
        //--------------------------------------------------------------

        private static LicenseManager _self = new LicenseManager();



        public static LicenseManager GetLicenseManager()
        {
            return _self;
        }

        private LicenseManager()
        {

        }

        public void Dispose()
        {
        }

        /// <summary>
        /// ライセンスチェック
        /// </summary>
        /// <returns>
        /// 0.. 正常
        /// -1..ライセンスファイルなし
        /// -2..MACアドレスチェックNG
        /// -3..有効期限切れ
        ///     </returns>
        public int CheckLicense()
        {
            var info = ReadLicenseFile();
            if (info == null)
            {
                //ライセンスファイルなしなど...
                return -1;
            }

            return CheckLicense(info);
        }
        public int CheckLicense(LicenseInfo info)
        {
            //MACアドレス情報がある場合はチェック
            if (!string.IsNullOrEmpty(info.MacAddr))
            {
                //現在のPCのＭＡＣアドレスを取得
                string phisycalMacAddr = Network.GetMacAddress();
                //MACアドレスチェック
                if (!info.MacAddr.Equals(phisycalMacAddr))
                {
                    return -2;
                }
            }
            //有効期限チェック
            if(info.LimitDate < DateTime.Today)
            {
                return -3;
            }

            //ライセンス情報合格！！
            return 0;
        }



        public int WriteLicenseFile(string filePath, string macAddress, DateTime? dtLimitDay = null)
        {
            if(dtLimitDay==null)
            {
                dtLimitDay = DateTime.MinValue;
            }
            if(macAddress==null)
            {
                macAddress = "";
            }
          
            //設定値→文字列
            string outputStr =
                        //$"{KEY_MAIL_ADDR}={macAddress.Trim()}" +
                        $"{SEP}{KEY_MAC_ADDR}={macAddress.Trim()}" +
                        $"{SEP}{KEY_LIMIT_DATE}={dtLimitDay.ToString()}"
                        ;

            //文字列→暗号化
            string encrypted = EncryptToBase64(outputStr, Convert.FromBase64String(KEY), Convert.FromBase64String(IV));

            //テキストファイル出力
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.Write(encrypted);
                    sw.Flush();
                }

            }
            catch
            {
                return -1;
            }

            return 0;

        }
        public LicenseInfo ReadLicenseFile()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string filePath = Path.Combine(path, licenseFileName);

            return ReadLicenseFile(filePath);
        }

        public LicenseInfo ReadLicenseFile(string filePath)
        {
            if( !File.Exists(filePath))
            {
                return null;
            }
            //テキストファイル読込
            string encrypted = "";

            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    encrypted = sr.ReadToEnd();
                }
            }
            catch 
            {
            }
            if(string.IsNullOrEmpty(encrypted))
            {
                return null;
            }
            //暗号化→文字列
            string readStr = DecryptFromBase64(encrypted, Convert.FromBase64String(KEY), Convert.FromBase64String(IV));
            if(readStr==null)
            {
                return null;
            }
            
            var aryItems = readStr.Split(SEP);

            LicenseInfo info = new LicenseInfo();
            info.MacAddr = GetValue(aryItems, KEY_MAC_ADDR);
           // info.MailAddr = GetValue(aryItems, KEY_MAIL_ADDR, "");
            info.LimitDate = GetValue(aryItems, KEY_LIMIT_DATE, (DateTime?)null);


            return info;

        }

       

        string GetValue(string[] items, string strTag, string defaultValue="")
        {
            var item = items.FirstOrDefault(x => x.StartsWith(strTag + "="));
            if (string.IsNullOrEmpty(item)) return "";

            var values = item.Split('=');
            return values[1].Trim();

        }
        int GetValue(string[] items, string strTag, int defaultValue)
        {
            var value = GetValue(items, strTag);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            int retValue;
            if (int.TryParse(value, out retValue) == false) return defaultValue;

            return retValue;

        }
        bool GetValue(string[] items, string strTag, bool defaultValue)
        {
            var value = GetValue(items, strTag);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            bool retValue;
            if (bool.TryParse(value, out retValue) == false) return defaultValue;

            return retValue;

        }
        Color GetValue(string[] items, string strTag, Color defaultValue)
        {
            var value = GetValue(items, strTag);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            //カラーコード(ARGB)からColorを作成
            Color retValue = Color.FromArgb(int.Parse(value) );
            if (retValue.A == 0 && retValue.R == 0 && retValue.G == 0 && retValue.B == 0)
            {
                return defaultValue;
            }
            return retValue;

        }
        DateTime? GetValue(string[] items, string strTag, DateTime? defaultValue)
        {
            var value = GetValue(items, strTag);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            DateTime retValue;
            if (DateTime.TryParse(value, out retValue) == false) return defaultValue;

            return retValue;

        }




        /// <summary>
        /// 暗号化のKEYとIVを取得します。（開発用）
        /// </summary>
        public static void  CreateKey()
        {
            Aes aes = Aes.Create();
            Debug.WriteLine(Convert.ToBase64String(aes.Key));
            Debug.WriteLine(Convert.ToBase64String(aes.IV));


        }


        // 入力文字列をAES暗号化してBase64形式で返すメソッド
        private string EncryptToBase64(string plainText, byte[] key, byte[] iv)
        {
            // 入力文字列をバイト型配列に変換
            byte[] src = Encoding.Unicode.GetBytes(plainText);
            //WriteLine($"平文のバイト型配列の長さ={src.Length}");
            // 出力例：平文のバイト型配列の長さ=60

            // Encryptor（暗号化器）を用意する
            using (var aes = Aes.Create())
            using (var encryptor = aes.CreateEncryptor(key, iv))
            // ファイルを入力とするなら、ここでファイルを開く
            // using (FileStream inStream = new FileStream(FilePath, ……省略……
            // 出力ストリームを用意する
            using (var outStream = new MemoryStream())
            {
                // 暗号化して書き出す
                using (var cs = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(src, 0, src.Length);
                    // 入力がファイルなら、inStreamから一定量ずつバイトバッファーに読み込んで
                    // cse.Writeで書き込む処理を繰り返す（復号のサンプルコードを参照）
                }
                // 出力がファイルなら、以上で完了

                // Base64文字列に変換して返す
                byte[] result = outStream.ToArray();
                //WriteLine($"暗号のバイト型配列の長さ={result.Length}");
                // 出力例：暗号のバイト型配列の長さ=64
                // 出力サイズはBlockSize（既定値16バイト）の倍数になる
                return Convert.ToBase64String(result);
            }
        }

        // 暗号化されたBase64形式の入力文字列をAES復号して平文の文字列を返すメソッド
        private string DecryptFromBase64(string base64Text, byte[] key, byte[] iv)
        {
            try
            {
                // Base64文字列をバイト型配列に変換
                byte[] src = Convert.FromBase64String(base64Text);


                // Decryptor（復号器）を用意する
                using (var aes = Aes.Create())
                using (var decryptor = aes.CreateDecryptor(key, iv))
                // 入力ストリームを開く
                using (var inStream = new MemoryStream(src, false))
                // 出力ストリームを用意する
                using (var outStream = new MemoryStream())
                {
                    // 復号して一定量ずつ読み出し、それを出力ストリームに書き出す
                    using (var cs = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] buffer = new byte[4096]; // バッファーサイズはBlockSizeの倍数にする
                        int len = 0;
                        while ((len = cs.Read(buffer, 0, 4096)) > 0)
                            outStream.Write(buffer, 0, len);
                    }
                    // 出力がファイルなら、以上で完了

                    // 文字列に変換して返す
                    byte[] result = outStream.ToArray();
                    return Encoding.Unicode.GetString(result);
                }
            }catch
            {
                return null;
            }
        }
    }
}
