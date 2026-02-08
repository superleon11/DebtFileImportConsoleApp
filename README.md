# DebtFileImportConsoleApp

Simple .NET 8 console app to parse and validate debt/client data from a delimited file, export cleaned records, export rejected records, and produce a small processing report.

## Project
- Main source: `Program.cs`
- Target framework: .NET 8 (`net8.0`)

## Prerequisites
- .NET 8 SDK installed
- Windows, macOS, or Linux command-line capable of running `dotnet` or Visual Studio 2026

## Build
From the repository root:
- dotnet CLI:
  - `dotnet build`
- Visual Studio:
  - Open the project and press __Run__ (F5) or __Start Without Debugging__.

## Usage (CLI)
The program expects exactly two arguments: the flag `-input` and the path to the file.

Examples:
- Using `dotnet run`:
  - `dotnet run -- -input "C:\path\to\file.csv"`
- Using the built exe:
  - `DebtFileImportConsoleApp.exe -input "C:\path\to\file.txt"`

Notes:
- The program validates the number of arguments. If invalid, it prints usage and exits.
- Supported extensions: `.csv` (comma-delimited) and `.txt` (pipe `|` delimited).

## Expected input format
- The first line must be a header (the program calls `Skip(1)`).
- Columns (in order):
  1. Account number (string)
  2. Name (string)
  3. Amount (decimal, positive)
  4. Telephone (string)

Example CSV file input:

Account Number,Name,Amount,Telephone Number
100001,John Doe,1250.75,07700900101
100002,Anna Smith,980.00,07700900102
100003,Michał Kowalski,150.50,07700900103
100004,Emily Johnson,0.00,07700900104
100005,Carlos García,3200.99,07700900105



Example TXT file input:

Account Number|Name|Amount|Telephone Number
AC-100001|john Wick|1250.75|+447700900101|Test
AC-100002|Anna Jane Smith|980.00|
AC-100003|Michał Kowalski|150.50|+447700900103

AC-100004|emily Johnson|0.00|07700900104
AC-100005|carlos garcía|3200.99|07700900105
AC-100006|Jonny Bravo|150.50|+447700900103
AC-100007|Frank Johnstone|5.00|0770090010454543543543

AC-1000!08|Laura Mansfield|3200.99|07700900105

## Output

-3 output files are generated in the same directory as the input file:

--CSV OUTPUT
1. `clean_[originalName].csv` - contains valid records with normalized names (title case).
2. `errors_[originalName].csv` - contains invalid records with an additional `Reason` column explaining the validation failure.
3. `reportTIMESTAMP.txt` - contains a summary of the processing, including total records, valid records, and rejected records.

--TXT OUTPUT
1. `clean_[originalName].txt` - contains valid records with normalized names (title case).
2. `errors_[originalName].txt` - contains invalid records with an additional `Reason` column explaining the validation failure.
3. `reportTIMESTAMP.txt` - contains a summary of the processing, including total records, valid records, and rejected records.


## To Do in future iterations
- Parse headers for dynamic column mapping (currently hardcoded).
- Add unit tests for validation logic.
- Implement improved logging for better traceability.



