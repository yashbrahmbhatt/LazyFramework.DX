using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Icons
{
    public static class Helpers
    {
        public static void WriteIconToFile(string name)
        {
            // Get the icon as a byte array from the embedded resource
            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);

            if (iconStream != null)
            {
                // Specify the path where you want to save the icon file
                string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetFileName(name));

                // Create the output file and write the icon data
                using (var fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    iconStream.CopyTo(fileStream);
                }

                Console.WriteLine($"Icon saved to: {outputPath}");
            }
            else
            {
                Console.WriteLine("Error: Could not find the icon resource.");
            }
        }
    }
}
