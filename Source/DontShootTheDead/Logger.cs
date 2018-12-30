using System;
using System.IO;



namespace DontShootTheDead
{
    public class Logger
    {
        static string filePath = $"{DontShootTheDead.ModDirectory}/DontShootTheDead.log";
        public static void LogError(Exception ex)
        {
            if (DontShootTheDead.DebugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[DontShootTheDead @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace + "" + Environment.NewLine);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        public static void LogLine(String line)
        {
            if (DontShootTheDead.DebugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[DontShootTheDead @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine(prefix + line);
                }
            }
        }
    }
}
