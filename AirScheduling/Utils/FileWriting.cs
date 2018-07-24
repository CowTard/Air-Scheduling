using System.IO;

namespace AirScheduling.Utils
{
    public class FileWriting
    {

        public static void WriteToFile(string filename, string data)
        {
            if (!File.Exists(filename))
            {
                File.Create(filename).Dispose();
            }
            
            using (TextWriter tw = new StreamWriter(filename, true))
            {
                tw.WriteLine(data);
            }
        }


        public static void CreateTestFile(string filename, string header)
        {
            if (File.Exists(filename))
                return;
            
            File.Create(filename).Dispose();
            WriteToFile(filename, header);
        }
    }
}