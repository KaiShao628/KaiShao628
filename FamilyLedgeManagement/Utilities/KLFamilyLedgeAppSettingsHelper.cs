using DatabaseCommon.Utilities;
using PaddleOCRSharp;
using System.Text;
using System.Text.Json;

namespace FamilyLedgeManagement.Utilities
{
    public class KLFamilyLedgeAppSettingsHelper
    {
        public static KLFamilyLedgeAppSettings KLFamilyLedgeAppSettings { get; set; } = new KLFamilyLedgeAppSettings();

        public static void Initialization()
        {
            string filePath = "";

#if DEBUG
            filePath = "appsettings.development.json";
#else
            filePath = "appsettings.json";
#endif
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            if (!File.Exists(filePath))
                return;

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                string content = streamReader.ReadToEnd();

                KLFamilyLedgeAppSettings = JsonSerializer.Deserialize<KLFamilyLedgeAppSettings>(content) ?? new KLFamilyLedgeAppSettings();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLineError("AppSettingsHelper ReadFile Error:" + ex.Message);
            }
        }
    }

    public class KLFamilyLedgeAppSettings
    {
        /// <summary>
        /// 数据库配置
        /// </summary>
        public KLFamilyLedgeDataBase DataBase { get; set; } = new KLFamilyLedgeDataBase();
    }

    public class KLFamilyLedgeDataBase
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 0;
    }
}
