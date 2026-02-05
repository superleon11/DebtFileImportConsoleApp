using System.Collections;

namespace DebtFileImportConsoleApp
{

    public class ClientRecord
    {
        public int RowId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Telephone { get; set; }
    }

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


            //Checks if the file passed in as an arguement exists, if it doesnt then it displays an error
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            //Gets the file extension of the file to be parsed.
            string extension = Path.GetExtension(filePath).Trim();
            //Sets the allowed file extensions to be parsed
            string[] allowedExtensions = { ".csv", ".txt" };

            //Checks if the file extension is either .csv or .txt, if not then it displays an error message.
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Error: File '{extension}' is not a valid .csv or .txt file.");
                Console.WriteLine("Error: The file must either be a .csv or .txt format.");
                return;
            }


            var clientList = ProcessInputFile(filePath, extension);
        }




        static ArrayList ProcessInputFile(string filePath, string extension)
        {
            //Uses this to set what the delimiter will be based on the file extension.
            var delimiter = "";
            var rowId = 1;
            var clientList = new ArrayList();
            if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                delimiter = ",";
            }
            else if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                delimiter = "|";
            }


            var lines = File.ReadLines(filePath).ToList();
            if (lines.Count == 0)
            {
                Console.WriteLine("Error: File is empty. Please upload file with data.");
                return clientList; 
            }

            Console.WriteLine("Processing records...");
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(delimiter);


                string accountNumber = columns[0].Trim();
                string name = columns[1].Trim();
                decimal amount = decimal.Parse(columns[2].Trim());
                int phone = int.Parse(columns[3].Trim());

                ClientRecord record = new ClientRecord
                {
                    RowId = rowId,
                    AccountNumber = accountNumber,
                    Name = name,
                    Amount = amount,
                    Telephone = phone
                };
                clientList.Add(record);
                rowId++;
                Console.WriteLine($"Found: {name} ({accountNumber}) - Amount: {amount} - Phone number: {phone}");
            }

            return clientList;
        }
    }
}

