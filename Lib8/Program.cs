using System;
using System.Collections.Generic;
using System.IO;

namespace Inu.Liblarian
{
    class Program
    {
        public const int Failure = 1;
        public const int Success = 0;

        private static readonly List<string> Errors = new List<string>();
        private static readonly Library.Library Library = new Library.Library();

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("No target file.");
                return Failure;
            }

            var targetName = args[0];
            var directory = Path.GetDirectoryName(targetName);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }

            if (args.Length < 2)
            {
                Console.Error.WriteLine("No object file.");
                return Failure;
            }

            for (var i = 1; i < args.Length; ++i)
            {
                string objName = args[i];
                var objDirectory = Path.GetDirectoryName(objName);
                if (string.IsNullOrEmpty(objDirectory))
                {
                    objName = directory + Path.DirectorySeparatorChar + objName;
                }
                ReadObjectFile(objName);
            }
            if (Errors.Count > 0)
            {
                return Failure;
            }

            Library.Save(targetName);
            return Success;
        }

        private static void ShowError(string error)
        {
            Errors.Add(error);
            Console.Error.WriteLine(error);
        }

        private static void ReadObjectFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                ShowError("File not found: " + fileName);
                return;
            }

            var obj = new Assembler.Object();
            obj.Load(fileName);

            var error = 0;
            foreach (var objSymbol in obj.Symbols.Values)
            {
                if (!objSymbol.IsExternal())
                {
                    var name = objSymbol.Name;
                    var existing = Library.NameToObject(name);
                    if (existing != null)
                    {
                        ShowError("Duplicated symbol: " + name + "\n\t" + fileName + "\n\t" + existing.Name);
                        ++error;
                    }
                }
            }
            if (error == 0)
            {
                Library.Add(obj);
            }
        }
    }
}
