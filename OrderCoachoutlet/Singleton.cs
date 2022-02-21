using OrderCoachoutlet.DataClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;

namespace OrderCoachoutlet
{
    internal static class Singleton
    {
        static Singleton()
        {
            Directory.CreateDirectory(LogDir);
            Directory.CreateDirectory(ProfileDir);
            Directory.CreateDirectory(ChromeDriverDir);
            Directory.CreateDirectory(ResultDir);
        }
        internal static string ExeDir { get; } = Directory.GetCurrentDirectory();
        internal static string LogDir { get; } = ExeDir + "\\Logs";
        internal static string AppDataDir { get; } = ExeDir + "\\AppData";
        internal static string ChromeDriverDir { get; } = ExeDir + "\\ChromeDriver";
        internal static string ProfileDir { get; } = ExeDir + "\\Profiles";
        internal static string ResultDir { get; } = ExeDir + "\\Results";

        internal static SaveSettingData<SettingData> Setting { get; } = new SaveSettingData<SettingData>(ExeDir + "\\Setting.json");
    }
}
