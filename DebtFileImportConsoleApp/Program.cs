namespace DebtFileImportConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Validate command-line arguments, checks if the correct number of arguments is provided and if the first argument is "-input". If not, it displays an error message and usage instructions.
            if (args.Length != 2 || args[0].ToLower() != "-input")
            {
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine("Usage: DebtFileImportConsoleApp.exe -input \"filename.csv\"");
                Console.WriteLine("OR");
                Console.WriteLine("Usage: DebtFileImportConsoleApp.exe -input \"filename.txt\"");
                return; 
            }

            string filePath = args[1];


            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            string extension = Path.GetExtension(filePath).Trim();
            string[] allowedExtensions = { ".csv", ".txt" };

            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Error: File '{extension}' is not a valid .csv or .txt file.");
                Console.WriteLine("Error: The file must either be a .csv or .txt format.");
                return;
            }

            ProcessInputFile(filePath);
        }




        static void ProcessInputFile(string filePath)
        {
            


        }

    }
}
