using System.Collections;
using System.Diagnostics;

namespace DebtFileImportConsoleApp
{

    public class ClientRecord
    {
        public int RowId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Telephone { get; set; }

        public ClientRecord(int rowId, string accountNumber, string name, decimal amount, string telephone)
        {
            RowId = rowId;
            AccountNumber = accountNumber;
            Name = name;
            Amount = amount;
            Telephone = telephone;
        }
    }

    public class RejectedRecord
    {
        public int RowId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Telephone { get; set; }

        public RejectionReason RejectionReason { get; set; }

        public RejectedRecord(int rowId, string accountNumber, string name, decimal amount, string telephone, RejectionReason rejectionReason)
        {
            RowId = rowId;
            AccountNumber = accountNumber;
            Name = name;
            Amount = amount;
            Telephone = telephone;
            RejectionReason = rejectionReason;
        }
    }

    public enum RejectionReason
    {
        InvalidAccountNumber = 1,
        InvalidAmount = 2,
        InvalidPhoneNumber = 3,
        EmptyLine = 4,
        Other = 5

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

            Stopwatch stopwatch = Stopwatch.StartNew();
            var dataList = ProcessInputFile(filePath, extension);
            if (dataList.Count > 0 && dataList[0] is ArrayList validRecords)
            {
                DataExporter.ExportCompletedRecordsToCsv(validRecords, filePath, extension);
            }

            if (dataList.Count > 0 && dataList[1] is ArrayList errorRecords)
            {
                DataExporter.ExportErrorRecordsToCsv(errorRecords, filePath);
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;

            DataExporter.CreateProcessingReport(dataList, ts);

        }




        static ArrayList ProcessInputFile(string filePath, string extension)
        {

            var delimiter = "";
            var rowId = 1;
            var errorRowId = 1;
            RejectionReason rejectionReason = 0;
            var clientList = new ArrayList();
            var errorList = new ArrayList();
            var combinedList = new ArrayList();

            //Checks the file extension and then sets the delimiter accordingly
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
                Boolean isValid = true;
                if (string.IsNullOrWhiteSpace(line))
                {
                    RejectedRecord record = new RejectedRecord(errorRowId, "", "", 0, "", RejectionReason.EmptyLine);
                    errorList.Add(record);
                    errorRowId++;
                    continue;
                }

                var columns = line.Split(delimiter);



                string accountNumber = columns[0].Trim();
                string name = columns[1].Trim();
                decimal amount = decimal.Parse(columns[2].Trim());
                string phone = columns[3].Trim();

                // Validates the account number to ensure it only contains letters, numbers, and hyphens
                if (!checkAccountNumberValid(accountNumber))
                {
                    Console.WriteLine($"Warning: Invalid account number. Account: {accountNumber}, Name: {name}, Amount: {amount}, Phone: {phone}");
                    isValid = false;
                    rejectionReason = RejectionReason.InvalidAccountNumber;
                }

                string formattedName = FormatName(name);


                if (!checkAmountValid(amount))
                {
                    Console.WriteLine($"Warning: Invalid amount. Account: {accountNumber}, Name: {name}, Amount: {amount}, Phone: {phone}");
                    isValid = false;
                    rejectionReason = RejectionReason.InvalidAmount;
                }

                // Validates the phone number to ensure meets required format
                if (phone.Length > 0)
                { 
                    string cleanedPhone = cleanUpPhoneNumber(phone);
                    if (cleanedPhone == "-")
                    {
                        Console.WriteLine($"Warning: Invalid phone number. Account: {accountNumber}, Name: {name}, Amount: {amount}, Original Phone: {phone}");
                        isValid = false;
                        rejectionReason = RejectionReason.InvalidPhoneNumber;
                    }
                    else
                    {
                        phone = cleanedPhone;
                    }
                }



                if (isValid)
                {
                    ClientRecord record = new ClientRecord(rowId, accountNumber, formattedName, amount, phone);
                    clientList.Add(record);
                    rowId++;
                }
                else
                {
                   RejectedRecord record = new RejectedRecord(errorRowId, accountNumber, formattedName, amount, phone, rejectionReason);
                    errorList.Add(record);
                    errorRowId++;
                }
                    Console.WriteLine($"Found: {name} ({accountNumber}) - Amount: {amount} - Phone number: {phone}");
            }

            Console.WriteLine($"Total valid records processed: {clientList.Count}");
            foreach (ClientRecord record in clientList)
            {
                Console.WriteLine($"RowId: {record.RowId}, AccountNumber: {record.AccountNumber}, Name: {record.Name}, Amount: {record.Amount}, Telephone: {record.Telephone}");
            }
            Console.WriteLine($"Total invalid records processed: {errorList.Count}");
            foreach (RejectedRecord record in errorList)
            {
                Console.WriteLine($"RowId: {record.RowId}, AccountNumber: {record.AccountNumber}, Name: {record.Name}, Amount: {record.Amount}, Telephone: {record.Telephone}, RejectionReason: {record.RejectionReason}");
            }

         
            combinedList.Add(clientList);
            combinedList.Add(errorList);
            return combinedList;
        }




        static bool checkAccountNumberValid(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return false;

            return accountNumber.All(c => char.IsLetterOrDigit(c) || c == '-');

        }

        static bool checkAmountValid(decimal amount)
        {
            return amount > 0;
        }

        static string FormatName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                return name;

            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
            return string.Join(' ', words);
        }



        static string cleanUpPhoneNumber(string phoneNumber)
        {
            Console.WriteLine("============PHONE NUMBER PARSER===============");

            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            
            if (digitsOnly.Length >= 10 && digitsOnly.Length <= 15)
            {
                if (phoneNumber.Contains("+"))
                {
                    digitsOnly = "+" + digitsOnly;
                }
                else
                {
                    digitsOnly = "+44" + digitsOnly;
                }
                Console.WriteLine($"Cleaned phone number: {digitsOnly} from original input: {phoneNumber}");
                return digitsOnly;
            }
            else
            {
                Console.WriteLine($"Warning: Phone number '{phoneNumber}' is invalid after cleanup. Expected between 10 and 15 digits, got {digitsOnly.Length}.");
                return "-"; 
            }
        }

    }



    internal class DataExporter
    {


        public static void ExportCompletedRecordsToCsv(ArrayList clientList, string outputPath, string extension)
        {
            if(clientList.Count == 0 || clientList == null)
            {
                Console.WriteLine("No valid records to export.");
                return;
            }
            var delimiter = "";
            string newFileName = "clean_" + outputPath;
            //Checks the file extension and then sets the delimiter accordingly
            if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                delimiter = ",";
            }
            else if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                delimiter = "|";
            }


            using (var writer = new StreamWriter(newFileName))
            {
                writer.WriteLine($"RowId{delimiter}AccountNumber{delimiter}Name{delimiter}Amount{delimiter}Telephone");
                foreach (ClientRecord record in clientList)
                {
                    writer.WriteLine($"{record.RowId}{delimiter}{record.AccountNumber}{delimiter}{record.Name}{delimiter}{record.Amount}{delimiter}{record.Telephone}");
                }
            }
        }


        public static void ExportErrorRecordsToCsv(ArrayList clientList, string outputPath)
        {
            if (clientList.Count == 0 || clientList == null)
            {
                Console.WriteLine("No valid records to export.");
                return;
            }
            var delimiter = ",";
            string newFileName = "errors_" + Path.GetFileNameWithoutExtension(outputPath) + ".csv";
            ;
           


            using (var writer = new StreamWriter(newFileName))
            {
                writer.WriteLine($"RowId{delimiter}AccountNumber{delimiter}Name{delimiter}Amount{delimiter}Telephone{delimiter}Error Reason");
                foreach (RejectedRecord record in clientList)
                {
                    writer.WriteLine($"{record.RowId}{delimiter}{record.AccountNumber}{delimiter}{record.Name}{delimiter}{record.Amount}{delimiter}{record.Telephone}{delimiter}{record.RejectionReason}");
                }
            }
        }


        public static void CreateProcessingReport(ArrayList dataList, TimeSpan ts)
        {
            var compeletedRecords = new ArrayList();
            var errorRecords = new ArrayList();

            if (dataList.Count > 0 && dataList[0] is ArrayList validRecords)
            {
                compeletedRecords = validRecords;
            }

            if (dataList.Count > 0 && dataList[1] is ArrayList errorRecord)
            {
                errorRecords = errorRecord;
            }


            string reportFileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            int completedCount = compeletedRecords.Count;
            int errorCount = errorRecords.Count;
            int totalRecords = completedCount + errorCount;

            var counts = new Dictionary<RejectionReason, int>();

            foreach (RejectedRecord record in errorRecords)
            {
                if (counts.ContainsKey(record.RejectionReason))
                {
                    counts[record.RejectionReason]++;
                }
                else
                {
                    counts[record.RejectionReason] = 1;
                }
            }


            using (var writer = new StreamWriter(reportFileName))
            {
                writer.WriteLine("Processing Report");
                writer.WriteLine("=================");
                writer.WriteLine($"Total records processed: {totalRecords}");
                writer.WriteLine($"Valid records: {completedCount}");
                writer.WriteLine($"Invalid records: {errorCount}");
                writer.WriteLine($"Processing time: {ts.TotalSeconds} seconds");
                writer.WriteLine("=================");
                writer.WriteLine("Rejected Record Reasons");
                foreach (var entry in counts)
                {
                    writer.WriteLine($"{entry.Key} occurred {entry.Value} times.");
                }







                Console.WriteLine("Processing Report");
                Console.WriteLine("=================");
                Console.WriteLine($"Total records processed: {totalRecords}");
                Console.WriteLine($"Valid records: {completedCount}");
                Console.WriteLine($"Invalid records: {errorCount}");
                Console.WriteLine($"Processing time: {ts.TotalSeconds} seconds");
                Console.WriteLine("=================");
                Console.WriteLine("Rejected Record Reasons");
                foreach (var entry in counts)
                {
                    Console.WriteLine($"{entry.Key} occurred {entry.Value} times.");
                }


            }


        }

    }
}

