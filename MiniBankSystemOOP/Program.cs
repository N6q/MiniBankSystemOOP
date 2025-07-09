namespace MiniBankSystemOOP
{
    internal class Program
    {
        // =============================
        //        DATA STRUCTURES
        // =============================

        /// <summary>
        /// Represents a login user (admin or customer) with authentication info and role.
        /// </summary>
        public class User
        {
            /// <summary>Unique login username for the user.</summary>
            public string Username { get; set; }
            /// <summary>Hashed password string for secure authentication.</summary>
            public string Password { get; set; }
            /// <summary>Role of the user: "Admin" or "Customer".</summary>
            public string Role { get; set; }
            /// <summary>Flag for account lock status after failed attempts.</summary>
            public bool IsLocked { get; set; } = false;
            /// <summary>Count of consecutive failed login attempts.</summary>
            public int FailedAttempts { get; set; } = 0;
        }

        /// <summary>
        /// Represents a bank account record (separate from login user).
        /// </summary>
        public class Account
        {
            /// <summary>Unique account number.</summary>
            public int AccountNumber { get; set; }
            /// <summary>Username of the account owner (login name).</summary>
            public string Username { get; set; }
            /// <summary>Current account balance.</summary>
            public double Balance { get; set; }
            /// <summary>National ID of the account holder.</summary>
            public string NationalID { get; set; }
            /// <summary>Registered phone number for this account.</summary>
            public string Phone { get; set; }
            /// <summary>Registered address for this account.</summary>
            public string Address { get; set; }
        }

        /// <summary>
        /// Represents a single loan request for a user.
        /// </summary>
        public class LoanRequest
        {
            /// <summary>Username of the requester.</summary>
            public string Username { get; set; }
            /// <summary>Requested loan amount.</summary>
            public double Amount { get; set; }
            /// <summary>Reason for requesting the loan.</summary>
            public string Reason { get; set; }
            /// <summary>Status of the loan: "Pending", "Approved", "Rejected".</summary>
            public string Status { get; set; }
            /// <summary>Interest rate applied to this loan.</summary>
            public double InterestRate { get; set; }
        }

        /// <summary>
        /// Represents a single service feedback entry from a user.
        /// </summary>
        public class ServiceFeedback
        {
            /// <summary>Username of the feedback provider.</summary>
            public string Username { get; set; }
            /// <summary>Bank service that the feedback is about.</summary>
            public string Service { get; set; }
            /// <summary>Actual feedback text or review.</summary>
            public string Feedback { get; set; }
            /// <summary>Date and time the feedback was submitted.</summary>
            public DateTime Date { get; set; }
        }

        /// <summary>
        /// Represents an appointment request or approved appointment.
        /// </summary>
        public class Appointment
        {
            /// <summary>Username of the user booking the appointment.</summary>
            public string Username { get; set; }
            /// <summary>Bank service for the appointment (e.g., loan, consultation).</summary>
            public string Service { get; set; }
            /// <summary>Date of the appointment (as string for now).</summary>
            public string Date { get; set; }
            /// <summary>Time of the appointment.</summary>
            public string Time { get; set; }
            /// <summary>Optional reason or description for the appointment.</summary>
            public string Reason { get; set; }
            /// <summary>Status: "Pending" or "Approved".</summary>
            public string Status { get; set; }
        }

        /// <summary>
        /// Reads a password from the user and displays asterisks (*) for each character.
        /// </summary>
        public static string ReadMaskedPassword()
        {
            string pass = "";
            ConsoleKeyInfo key;

            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                pass += key.KeyChar;
                Console.Write("*");
            }
            Console.WriteLine();
            return pass;
        }

        /// <summary>
        /// Hashes a password string using SHA256 for secure storage.
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                string hash = "";
                foreach (byte b in bytes)
                    hash += b.ToString("x2");
                return hash;
            }
        }
        /// <summary>
        /// Prompts for National ID (digits only) and validates input.
        /// </summary>      
        public static string ReadDigitsOnly(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && input.All(char.IsDigit))
                    return input;
                PrintMessageBox("Please enter numbers only.", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// Prompts for a non-empty string and validates input.
        /// </summary>
        public static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    return input;
                PrintMessageBox("This field cannot be empty.", ConsoleColor.Yellow);
            }
        }

        // =============================
        //   GLOBAL DATA COLLECTIONS 
        // =============================

        /// <summary>All registered login users (admins and customers).</summary>
        public static List<User> Users = new List<User>();
        /// <summary>All bank accounts in the system.</summary>
        public static List<Account> Accounts = new List<Account>();
        /// <summary>All loan requests submitted by users.</summary>
        public static List<LoanRequest> LoanRequests = new List<LoanRequest>();
        /// <summary>All service feedback/reviews.</summary>
        public static List<ServiceFeedback> ServiceFeedbacks = new List<ServiceFeedback>();
        /// <summary>All pending appointment requests (not yet approved by admin).</summary>
        public static List<Appointment> AppointmentRequests = new List<Appointment>();
        /// <summary>All approved appointments for users.</summary>
        public static List<Appointment> ApprovedAppointments = new List<Appointment>();
        /// <summary>Queue of all pending customer account opening requests (for admin approval).</summary>
        public static Queue<string> accountOpeningRequests = new Queue<string>();
        /// <summary>Queue of all pending admin account signup requests (for admin approval).</summary>
        public static Queue<string> adminAccountRequests = new Queue<string>();
        /// <summary>Stack for complaints/reviews (can be expanded to per-user for advanced features).</summary>
        public static Stack<string> ReviewsS = new Stack<string>();
        /// <summary>Last issued account number (increment for new accounts).</summary>
        public static int lastAccountNumber = 1000;

        // =============================
        //         FILE STORAGE 
        // =============================

        /// <summary>Minimum allowed balance in any bank account.</summary>
        const double MinimumBalance = 50.0;
        /// <summary>File path for saving and loading all approved bank accounts.</summary>
        static string AccountsFilePath = "accounts.txt";
        /// <summary>File path for saving and loading all registered user login info.</summary>
        static string UsersFilePath = "users.txt";
        /// <summary>File path for saving and loading all submitted complaints/reviews.</summary>
        static string ReviewsFilePath = "reviews.txt";
        /// <summary>Directory path for all account transaction logs and receipts.</summary>
        static string TransactionsDir = "transactions";
        /// <summary>File path for saving and loading all loan requests.</summary>
        static string LoanRequestsFilePath = "loan_requests.txt";
        /// <summary>File path for saving and loading all service feedback submissions.</summary>
        static string ServiceFeedbackFile = "service_feedback.txt";
        /// <summary>File path for saving and loading all pending appointment requests.</summary>
        static string AppointmentRequestsFile = "appointments_pending.txt";
        /// <summary>File path for saving and loading all approved appointment records.</summary>
        static string ApprovedAppointmentsFile = "appointments_approved.txt";
        /// <summary>File path for saving and loading currency exchange rates used in the system.</summary>
        static string ExchangeRatesFile = "exchange_rates.txt";

        /// <summary>
        /// Currency conversion rates for OMR to other currencies.
        /// </summary>
        public static double Rate_USD = 2.60;   // 1 OMR = 2.60 USD
        public static double Rate_EUR = 2.45;   // 1 OMR = 2.45 EUR
        public static double Rate_SAR = 9.75;   // 1 OMR = 9.75 SAR



        // =============================
        //        DECORATIONS
        // =============================

        /// <summary>
        /// Prints the fancy ASCII logo/banner for the bank system at the top of every menu.
        /// Visually brands the system and gives a luxury/trusted feel.
        /// </summary>
        static void PrintBankLogo()
        {
            // Custom bank logo/banner 
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                 /\\                                         ║");
            Console.WriteLine("║                                /  \\                                        ║");
            Console.WriteLine("║                               / 🏦 \\                                       ║");
            Console.WriteLine("║                          /--------------\\                                  ║");
            Console.WriteLine("║                         /  KHALFANOVISKI \\                                 ║");
            Console.WriteLine("║                        /        BANK      \\                                ║");
            Console.WriteLine("║                       /____________________\\                               ║");
            Console.WriteLine("║   ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~      ║");
            Console.WriteLine("║   |    ___      ___      ___      ___      ___      ___     ___     |      ║");
            Console.WriteLine("║   |   |   |    |   |    |   |    |   |    |   |    |   |   |   |    |      ║");
            Console.WriteLine("║   |   |___|    |___|    |___|    |___|    |___|    |___|   |___|    |      ║");
            Console.WriteLine("║   |    ___      ___      ___      ___      ___      ___     ___     |      ║");
            Console.WriteLine("║   |   |   |    |   |    |   |    |   |    |   |    |   |   |   |    |      ║");
            Console.WriteLine("║   |   |___|    |___|    |___|    |___|    |___|    |___|   |___|    |      ║");
            Console.WriteLine("║   |                                                                 |      ║");
            Console.WriteLine("║   ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~      ║");
            Console.WriteLine("║                                                                            ║");
            Console.WriteLine("║            ||    Trusted | Luxury | Secure | Community    ||               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("");
        }

        /// <summary>
        /// Prints a standard header box for any dialog, menu, or popup.
        /// Includes an optional icon for context.
        /// </summary>
        /// <param name="title">The title or label for the box.</param>
        /// <param name="icon">The emoji/icon to display in the header (default: money bag).</param>
        static void PrintBoxHeader(string title, string icon = "💰")
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            " + icon + "  " + title.PadRight(40) + "║");
            Console.WriteLine("╠════════════════════════════════════════════════════════╣");
        }

        /// <summary>
        /// Prints the footer/closing line for dialog or sub-menu boxes to complete the visual frame.
        /// </summary>
        static void PrintBoxFooter()
        {
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Pauses the program, prompting the user to press Enter to continue.
        /// Used after important messages or actions.
        /// </summary>
        static void PauseBox()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ Press Enter to continue...                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.ReadLine();
        }

        /// <summary>
        /// Displays a colored message box with a framed border.
        /// Pauses after the message so the user has time to read it.
        /// </summary>
        /// <param name="message">The message to display inside the box.</param>
        /// <param name="color">The color for the message text (default: white).</param>
        public static void PrintMessageBox(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("  ╔════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║ " + message.PadRight(51) + "║");
            Console.WriteLine("  ╚════════════════════════════════════════════════════╝");
            Console.ResetColor();
            PauseBox();
        }


        // =============================
        //      FILE MANAGEMENT 
        // =============================

        /// <summary>
        /// Saves all bank accounts (as CSV: account number, username, balance, national ID, phone, address) to disk.
        /// </summary>
        public static void SaveAccountsInformationToFile()
        {
            using (StreamWriter writer = new StreamWriter(AccountsFilePath))
            {
                foreach (var acc in Accounts)
                    writer.WriteLine($"{acc.AccountNumber},{acc.Username},{acc.Balance},{acc.NationalID},{acc.Phone},{acc.Address}");
            }
        }

        /// <summary>
        /// Loads all saved bank accounts from disk into the Accounts list.
        /// </summary>
        public static void LoadAccountsInformationFromFile()
        {
            Accounts.Clear();
            if (!File.Exists(AccountsFilePath)) return;
            string[] lines = File.ReadAllLines(AccountsFilePath);
            foreach (var line in lines)
            {
                string[] p = line.Split(',');
                if (p.Length >= 6)
                {
                    var acc = new Account
                    {
                        AccountNumber = Convert.ToInt32(p[0]),
                        Username = p[1],
                        Balance = Convert.ToDouble(p[2]),
                        NationalID = p[3],
                        Phone = p[4],
                        Address = p[5]
                    };
                    Accounts.Add(acc);

                    // Update lastAccountNumber if needed
                    if (acc.AccountNumber > lastAccountNumber)
                        lastAccountNumber = acc.AccountNumber;
                }
            }
        }

        /// <summary>
        /// Saves all registered users to disk (Username,Password,Role,IsLocked,FailedAttempts).
        /// </summary>
        public static void SaveUsers()
        {
            using (StreamWriter writer = new StreamWriter(UsersFilePath))
            {
                foreach (var user in Users)
                {
                    writer.WriteLine($"{user.Username},{user.Password},{user.Role},{user.IsLocked},{user.FailedAttempts}");
                }
            }
        }

        /// <summary>
        /// Loads all registered users from disk into the Users list.
        /// </summary>
        public static void LoadUsers()
        {
            Users.Clear();
            if (!File.Exists(UsersFilePath)) return;
            string[] lines = File.ReadAllLines(UsersFilePath);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var user = new User
                    {
                        Username = parts[0],
                        Password = parts[1],
                        Role = parts[2],
                        IsLocked = (parts.Length > 3) ? bool.Parse(parts[3]) : false,
                        FailedAttempts = (parts.Length > 4) ? int.Parse(parts[4]) : 0
                    };
                    Users.Add(user);
                }
            }
        }

        /// <summary>
        /// Saves all loan requests to disk (Username|Amount|Reason|Status|InterestRate).
        /// </summary>
        public static void SaveLoanRequests()
        {
            using (StreamWriter sw = new StreamWriter(LoanRequestsFilePath))
            {
                foreach (var req in LoanRequests)
                    sw.WriteLine($"{req.Username}|{req.Amount}|{req.Reason}|{req.Status}|{req.InterestRate}");
            }
        }

        /// <summary>
        /// Loads all loan requests from disk into the LoanRequests list.
        /// </summary>
        public static void LoadLoanRequests()
        {
            LoanRequests.Clear();
            if (!File.Exists(LoanRequestsFilePath)) return;
            foreach (var line in File.ReadAllLines(LoanRequestsFilePath))
            {
                var parts = line.Split('|');
                if (parts.Length >= 5)
                {
                    LoanRequests.Add(new LoanRequest
                    {
                        Username = parts[0],
                        Amount = double.Parse(parts[1]),
                        Reason = parts[2],
                        Status = parts[3],
                        InterestRate = double.Parse(parts[4])
                    });
                }
            }
        }

        /// <summary>
        /// Saves all service feedbacks to disk (Username|Service|Feedback|Date).
        /// </summary>
        public static void SaveServiceFeedbacks()
        {
            using (StreamWriter sw = new StreamWriter(ServiceFeedbackFile))
            {
                foreach (var feedback in ServiceFeedbacks)
                    sw.WriteLine($"{feedback.Username}|{feedback.Service}|{feedback.Feedback}|{feedback.Date}");
            }
        }

        /// <summary>
        /// Loads all service feedbacks from disk into the ServiceFeedbacks list.
        /// </summary>
        public static void LoadServiceFeedbacks()
        {
            ServiceFeedbacks.Clear();
            if (File.Exists(ServiceFeedbackFile))
                foreach (var line in File.ReadAllLines(ServiceFeedbackFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                        ServiceFeedbacks.Add(new ServiceFeedback
                        {
                            Username = parts[0],
                            Service = parts[1],
                            Feedback = parts[2],
                            Date = DateTime.TryParse(parts[3], out var dt) ? dt : DateTime.Now
                        });
                }
        }

        /// <summary>
        /// Saves all pending appointment requests to disk (Username|Service|Date|Time|Reason|Status).
        /// </summary>
        public static void SaveAppointmentRequests()
        {
            using (StreamWriter sw = new StreamWriter(AppointmentRequestsFile))
                foreach (var appt in AppointmentRequests)
                    sw.WriteLine($"{appt.Username}|{appt.Service}|{appt.Date}|{appt.Time}|{appt.Reason}|{appt.Status}");
        }

        /// <summary>
        /// Loads all pending appointment requests from disk into AppointmentRequests.
        /// </summary>
        public static void LoadAppointmentRequests()
        {
            AppointmentRequests.Clear();
            if (File.Exists(AppointmentRequestsFile))
                foreach (var line in File.ReadAllLines(AppointmentRequestsFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 6)
                        AppointmentRequests.Add(new Appointment
                        {
                            Username = parts[0],
                            Service = parts[1],
                            Date = parts[2],
                            Time = parts[3],
                            Reason = parts[4],
                            Status = parts[5]
                        });
                }
        }

        /// <summary>
        /// Saves all approved appointments to disk.
        /// </summary>
        public static void SaveApprovedAppointments()
        {
            using (StreamWriter sw = new StreamWriter(ApprovedAppointmentsFile))
                foreach (var appt in ApprovedAppointments)
                    sw.WriteLine($"{appt.Username}|{appt.Service}|{appt.Date}|{appt.Time}|{appt.Reason}|{appt.Status}");
        }

        /// <summary>
        /// Loads all approved appointments from disk into ApprovedAppointments.
        /// </summary>
        public static void LoadApprovedAppointments()
        {
            ApprovedAppointments.Clear();
            if (File.Exists(ApprovedAppointmentsFile))
                foreach (var line in File.ReadAllLines(ApprovedAppointmentsFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 6)
                        ApprovedAppointments.Add(new Appointment
                        {
                            Username = parts[0],
                            Service = parts[1],
                            Date = parts[2],
                            Time = parts[3],
                            Reason = parts[4],
                            Status = parts[5]
                        });
                }
        }

        /// <summary>
        /// Saves all complaints/reviews (stack) to disk (one per line).
        /// </summary>
        public static void SaveReviews()
        {
            using (StreamWriter writer = new StreamWriter(ReviewsFilePath))
            {
                foreach (string s in ReviewsS)
                    writer.WriteLine(s);
            }
        }

        /// <summary>
        /// Loads all complaints/reviews (stack) from disk.
        /// </summary>
        public static void LoadReviews()
        {
            ReviewsS.Clear();
            if (!File.Exists(ReviewsFilePath)) return;
            string[] lines = File.ReadAllLines(ReviewsFilePath);
            for (int i = lines.Length - 1; i >= 0; i--)
                ReviewsS.Push(lines[i]);
        }

        /// <summary>
        /// Appends a transaction to a user's transaction log file.
        /// </summary>
        public static void LogTransaction(int accountIdx, string type, double amount, double balance)
        {
            if (!Directory.Exists(TransactionsDir)) Directory.CreateDirectory(TransactionsDir);
            var acc = Accounts[accountIdx];
            string fn = $"{TransactionsDir}/acc_{acc.AccountNumber}.txt";
            using (StreamWriter sw = new StreamWriter(fn, true))
                sw.WriteLine($"{DateTime.Now} | {type} | Amount: {amount} | Balance: {balance}");
        }

        /// <summary>
        /// Shows all transactions for a given account.
        /// </summary>
        public static void ShowTransactionHistory(int accountIdx)
        {
            var acc = Accounts[accountIdx];
            string fn = $"{TransactionsDir}/acc_{acc.AccountNumber}.txt";
            PrintBoxHeader("TRANSACTION HISTORY", "💸");
            if (!File.Exists(fn)) Console.WriteLine("|   No transactions found.                            |");
            else
            {
                string[] lines = File.ReadAllLines(fn);
                foreach (var line in lines)
                    Console.WriteLine("|   " + line.PadRight(48) + "|");
            }
            PrintBoxFooter();
        }

        /// <summary>
        /// After deposit/withdraw, prints a transaction receipt to a txt file with timestamp.
        /// </summary>
        public static void PrintReceipt(string type, int accIdx, double amt, double bal)
        {
            var acc = Accounts[accIdx];
            string fn = $"receipt_{acc.AccountNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            using (StreamWriter sw = new StreamWriter(fn))
            {
                sw.WriteLine("==== MiniBank Receipt ====");
                sw.WriteLine("Account Number: " + acc.AccountNumber);
                sw.WriteLine("Username: " + acc.Username);
                sw.WriteLine("Operation: " + type);
                sw.WriteLine("Amount: " + amt.ToString("F2"));
                sw.WriteLine("Balance: " + bal.ToString("F2"));
                sw.WriteLine("Date: " + DateTime.Now);
            }
            Console.WriteLine($"Receipt saved as {fn}");
        }

        /// <summary>
        /// Saves current currency exchange rates to a text file for persistence.
        /// </summary>
        public static void SaveExchangeRates()
        {
            using (StreamWriter sw = new StreamWriter(ExchangeRatesFile))
            {
                sw.WriteLine(Rate_USD);
                sw.WriteLine(Rate_EUR);
                sw.WriteLine(Rate_SAR);
            }
        }

        /// <summary>
        /// Loads currency exchange rates from a file, or uses defaults if file not found.
        /// </summary>
        public static void LoadExchangeRates()
        {
            if (!File.Exists(ExchangeRatesFile)) return;
            string[] lines = File.ReadAllLines(ExchangeRatesFile);
            if (lines.Length >= 3)
            {
                double.TryParse(lines[0], out Rate_USD);
                double.TryParse(lines[1], out Rate_EUR);
                double.TryParse(lines[2], out Rate_SAR);
            }
        }


        // =============================
        //         UTILITY LOGIC
        // =============================

        /// <summary>
        /// Returns true if the National ID is already used in an approved account or a pending account opening request.
        /// </summary>
        public static bool NationalIDExistsInRequestsOrAccounts(string nationalID)
        {
            // Check in all approved accounts (OOP version)
            if (Accounts.Any(a => a.NationalID == nationalID)) return true;

            // Check in all pending account requests (still as string for now)
            foreach (string req in accountOpeningRequests)
                if (req.Contains("National ID: " + nationalID))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns the index of the account for the given username, or -1 if not found.
        /// </summary>
        public static int GetAccountIndexForUser(string username)
        {
            for (int i = 0; i < Accounts.Count; i++)
                if (Accounts[i].Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        /// <summary>
        /// Returns the pending request string for this username, or null if none exists.
        /// </summary>
        public static string GetPendingRequestForUser(string username)
        {
            foreach (string req in accountOpeningRequests)
                if (req.Contains("Username: " + username))
                    return req;
            return null;
        }

        /// <summary>
        /// Reads user input with a timeout (auto-logout if time expires).
        /// Returns null if timed out.
        /// </summary>
        public static string TimedReadLine(int timeoutSeconds, out bool timedOut)
        {
            timedOut = false;
            string input = "";
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalSeconds < timeoutSeconds)
            {
                if (Console.KeyAvailable)
                {
                    input = Console.ReadLine();
                    return input;
                }
                System.Threading.Thread.Sleep(200); // Poll every 200ms
            }
            timedOut = true;
            return null;
        }

        /// <summary>
        /// Extracts a specific field's value from a request string (e.g., "Username").
        /// </summary>
        public static string ParseFieldFromRequest(string req, string field)
        {
            var parts = req.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith(field + ":"))
                    return trimmed.Substring(field.Length + 1).Trim();
            }
            return "";
        }


        // =============================
        //       MAIN NAVIGATION
        // =============================

        /// <summary>
        /// Program entry point. Loads all persistent data and launches the main menu.
        /// </summary>
        public static void Main(string[] args)
        {
            // Ensure transaction directory exists and launch the system
            if (!Directory.Exists(TransactionsDir)) Directory.CreateDirectory(TransactionsDir);
            StartSystem();
        }

        /// <summary>
        /// Loads all persistent data from files and starts the main welcome menu.
        /// </summary>
        public static void StartSystem()
        {
            LoadAccountsInformationFromFile();
            LoadReviews();
            LoadUsers();
            LoadLoanRequests();
            LoadServiceFeedbacks();
            LoadAppointmentRequests();
            LoadApprovedAppointments();
            LoadExchangeRates();
            DisplayWelcomeMessage();
        }

        /// <summary>
        /// Shows the main welcome menu for users to choose admin, customer, info, or exit.
        /// Handles navigation to role menus and key utilities.
        /// </summary>
        public static void DisplayWelcomeMessage()
        {
            while (true)
            {
                Console.Clear();
                PrintBankLogo();
                Console.WriteLine("╔════════════════════════════════════════════════════════╗");
                Console.WriteLine("║         [1] Admin Portal                               ║");
                Console.WriteLine("║         [2] Customer Portal                            ║");
                Console.WriteLine("║         [3] Login by National ID                       ║");
                Console.WriteLine("║         [4] About Bank                                 ║");
                Console.WriteLine("║         [5] Change Theme (Light/Dark)                  ║");
                Console.WriteLine("║         [0] Exit                                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════╝");
                Console.Write("Enter your choice: ");
                string input = Console.ReadLine();
                if (input == "1") ShowRoleAuthMenu("Admin");
                else if (input == "2") ShowRoleAuthMenu("Customer");
                else if (input == "3")
                {
                    // Login by National ID only (returns user index, or -1)
                    var (userIdx, username) = LoginByNationalID();
                    if (userIdx != -1) ShowCustomerMenu(userIdx, username);
                }
                else if (input == "4") ShowBankAbout();
                else if (input == "5") ToggleTheme();
                else if (input == "0") { ExitApplication(); break; }
                else { Console.WriteLine("Invalid choice! Try again."); PauseBox(); }
            }
        }

        /// <summary>
        /// Shows login/signup options for Admin or Customer roles, and launches the right menu.
        /// </summary>
        /// <param name="role">"Admin" or "Customer" for which menu to show.</param>
        public static void ShowRoleAuthMenu(string role)
        {
            while (true)
            {
                Console.Clear();
                string banner = (role == "Admin") ? "🏦 ADMIN AUTHENTICATION 🏦   " : "👤 CUSTOMER AUTHENTICATION 👤";
                PrintBoxHeader(banner);
                Console.WriteLine("║ [1] Login                                              ║");
                Console.WriteLine("║ [2] Signup                                             ║");
                Console.WriteLine("║ [0] Back                                               ║");
                PrintBoxFooter();
                Console.Write("Enter your choice: ");
                string input = Console.ReadLine();
                if (input == "1")
                {
                    var (userIdx, username) = LoginSpecificRole(role);
                    if (userIdx != -1)
                    {
                        if (role == "Admin") ShowAdminMenu(userIdx);
                        else ShowCustomerMenu(userIdx, username);
                    }
                }
                else if (input == "2") SignupSpecificRole(role);
                else if (input == "0") break;
                else { Console.WriteLine("Invalid choice! Try again."); PauseBox(); }
            }
        }

        /// <summary>
        /// Helper to return to the main welcome menu from anywhere in the program.
        /// </summary>
        public static void goBack()
        {
            Console.Clear();
            DisplayWelcomeMessage();
        }

        /// <summary>
        /// Saves all persistent data and cleanly exits the banking application.
        /// Displays a thank-you message before exiting.
        /// </summary>
        public static void ExitApplication()
        {
            SaveAccountsInformationToFile();
            SaveUsers();
            SaveReviews();
            SaveLoanRequests();
            SaveServiceFeedbacks();
            SaveAppointmentRequests();
            SaveApprovedAppointments();
            SaveExchangeRates();
            Console.Clear();
            PrintBoxHeader("Thank You For Banking With Us! 🏦");
            PrintBoxFooter();
            Environment.Exit(0);
        }

        /// <summary>
        /// Displays the about/info box for the bank, including developer, contact, and version info.
        /// </summary>
        public static void ShowBankAbout()
        {
            Console.Clear();
            PrintBoxHeader("ABOUT KHALFANOVISKI BANK", "ℹ️");
            Console.WriteLine("| Welcome to Khafanoviski Bank!                        |");
            Console.WriteLine("| We offer luxury, trust, and community for all.       |");
            Console.WriteLine("| Contact: +968 91119301                               |");
            Console.WriteLine("| Address: Muscat, Oman                                |");
            Console.WriteLine("| Developer: Samir Al-Bulushi                          |");
            Console.WriteLine("| Version: 2.0                                         |");
            Console.WriteLine("| Your future, your bank.                              |");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Toggles the console theme between light and dark modes for better user experience.
        /// </summary>
        public static void ToggleTheme()
        {
            if (Console.BackgroundColor == ConsoleColor.Black)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Clear();
            PrintBankLogo();
            Console.WriteLine("Theme changed!");
            PauseBox();
        }




        // =============================
        //       LOGIN & SIGNUP 
        // =============================

        /// <summary>
        /// Standard login by username and password for specified role, with hashed password and lockout after 3 failed tries.
        /// </summary>
        public static (int userIdx, string username) LoginSpecificRole(string role)
        {
            Console.Clear();
            PrintBoxHeader("LOGIN " + role.ToUpper(), role == "Admin" ? "🛡️" : "👤");
            EnsureAdminAccount();
            Console.Write("| Username: ");
            string username = Console.ReadLine();

            // Find user object (OOP)
            int foundIdx = Users.FindIndex(u => u.Username == username && u.Role == role);

            if (foundIdx == -1)
            {
                PrintMessageBox("No such user with this role.", ConsoleColor.Red);
                return (-1, username);
            }

            var user = Users[foundIdx];

            if (user.IsLocked)
            {
                PrintMessageBox("\nAccount is locked. Please contact admin to unlock.", ConsoleColor.Red);
                PrintBoxFooter(); PauseBox(); return (-1, username);
            }

            Console.Write("| Password: ");
            string password = ReadMaskedPassword();
            string hashedPassword = HashPassword(password);

            if (user.Password == hashedPassword)
            {
                user.FailedAttempts = 0; // Reset failed attempts on success
                SaveUsers();
                Console.WriteLine("\nLogin successful!");
                PrintBoxFooter(); PauseBox(); return (foundIdx, username);
            }
            else
            {
                // "q" admin is never locked out, but others can be!
                if (user.Username == "q" && user.Role == "Admin")
                {
                    Console.WriteLine("\nInvalid password! Try again.");
                    PrintBoxFooter(); PauseBox(); return (-1, username);
                }

                user.FailedAttempts++;
                if (user.FailedAttempts >= 3)
                {
                    user.IsLocked = true;
                    SaveUsers();
                    PrintMessageBox("\nAccount locked after 3 failed attempts!", ConsoleColor.Red);
                }
                else
                {
                    SaveUsers();
                    PrintMessageBox($"\nInvalid password! Attempts left: {3 - user.FailedAttempts}", ConsoleColor.Red);
                }

                PrintBoxFooter(); PauseBox(); return (-1, username);
            }
        }

        /// <summary>
        /// Signup for specified role, requiring unique username and saving a hashed password. 
        /// Creates a pending request for approval.
        /// </summary>
        public static void SignupSpecificRole(string role)
        {
            Console.Clear();
            PrintBoxHeader("SIGNUP " + role.ToUpper(), role == "Admin" ? "🛡️" : "👤");

            string name;
            while (true)
            {
                Console.Write("| Full Name: ");
                name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                    break;
                Console.WriteLine("Please write your full name.");
            }

            string username;
            while (true)
            {
                Console.Write("| Choose Username: ");
                username = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Please write a username.");
                    continue;
                }
                // Check uniqueness (OOP)
                if (Users.Any(u => u.Username == username))
                {
                    PrintMessageBox("| Username already exists! Try another.", ConsoleColor.Yellow);
                    continue;
                }
                break;
            }

            string password;
            while (true)
            {
                Console.Write("| Choose Password: ");
                password = ReadMaskedPassword();
                if (!string.IsNullOrWhiteSpace(password))
                    break;
                Console.WriteLine("Please write a password.");
            }

            string nationalID = "";
            while (true)
            {
                nationalID = ReadDigitsOnly("| National ID: ");
                if (role != "Admin" && NationalIDExistsInRequestsOrAccounts(nationalID))
                {
                    PrintMessageBox("National ID already exists or pending. Try again.", ConsoleColor.Yellow);
                    continue;
                }
                break;
            }

            string phone = ReadDigitsOnly("| Enter Phone Number: ");

            string address;
            while (true)
            {
                Console.Write("| Enter Address: ");
                address = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(address))
                    break;
                Console.WriteLine("Please write your address.");
            }

            string initialDeposit = "";
            if (role != "Admin")
            {
                while (true)
                {
                    Console.Write("| Initial Deposit Amount: ");
                    initialDeposit = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(initialDeposit))
                        break;
                    Console.WriteLine("Please write an initial deposit amount.");
                }
            }
            PrintBoxFooter();

            if (role == "Admin")
            {
                // Add admin request to queue (pending approval)
                string request = "Username: " + username + " | Name: " + name
                    + " | National ID: " + nationalID + " | Phone: " + phone
                    + " | Address: " + address + " | Role: Admin"
                    + " | Password: " + HashPassword(password);
                adminAccountRequests.Enqueue(request);
                Console.WriteLine("\nAdmin account request submitted for approval!");
            }
            else // Customer
            {
                string request = "Username: " + username + " | Name: " + name + " | National ID: " + nationalID
                    + " | Initial: " + initialDeposit + " | Phone: " + phone + " | Address: " + address
                    + " | Role: Customer"
                    + " | Password: " + HashPassword(password);
                accountOpeningRequests.Enqueue(request);
                Console.WriteLine("\nAccount request submitted!");
            }
            PauseBox();
        }

        /// <summary>
        /// Customer login using only National ID. Validates that account exists.
        /// </summary>
        public static (int userIdx, string username) LoginByNationalID()
        {
            Console.Clear();
            PrintBoxHeader("LOGIN BY NATIONAL ID", "🔑");
            Console.Write("| Enter your National ID: ");
            string nationalID = Console.ReadLine();
            PrintBoxFooter();

            // Find approved account by National ID (OOP)
            int accIdx = Accounts.FindIndex(a => a.NationalID == nationalID);
            if (accIdx == -1)
            {
                Console.WriteLine("No approved account with this National ID.");
                PauseBox();
                return (-1, nationalID);
            }
            string username = Accounts[accIdx].Username;

            // Find user object (OOP)
            int foundIdx = Users.FindIndex(u => u.Username == username && u.Role == "Customer");

            if (foundIdx == -1)
            {
                Console.WriteLine("No login linked to this National ID.");
                PauseBox();
                return (-1, username);
            }

            Console.WriteLine("Login successful! Welcome, " + username);
            PauseBox();
            return (foundIdx, username);
        }


        // =============================
        //          ADMIN MENU 
        // =============================

        /// <summary>
        /// Main admin dashboard with all admin functions.
        /// </summary>
        public static void ShowAdminMenu(int userIdx)
        {
            while (true)
            {
                Console.Clear();
                PrintBankLogo();
                Console.WriteLine("  ╔════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║         👑   ADMIN CONTROL CENTER   👑             ║");
                Console.WriteLine("  ╠════════════════════════════════════════════════════╣");
                Console.WriteLine("  ║  Welcome, " + Users[userIdx].Username.PadRight(38) + "  ║");
                Console.WriteLine("  ╠════════════════════════════════════════════════════╣");

                // === SECTION: User Management ===
                Console.WriteLine("  ║═══════════════ [ User Management ] ════════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [A1]  View Admin Requests                          ║");
                Console.WriteLine("  ║ [A2]  Process Admin Requests                       ║");
                Console.WriteLine("  ║ [1]  View Account Requests                         ║");
                Console.WriteLine("  ║ [2]  Process Account Requests                      ║");
                Console.WriteLine("  ║ [3]  View All Accounts                             ║");
                Console.WriteLine("  ║ [4]  Search Account                                ║");
                Console.WriteLine("  ║ [5]  Delete Account (by Account Number)            ║");
                Console.WriteLine("  ║ [6]  Unlock User Account                           ║");
                Console.WriteLine("  ║ [7]  Change Admin Password                         ║");
                Console.WriteLine("  ║ [8]  View Locked Accounts                          ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Loan Management ===
                Console.WriteLine("  ║════════════════ [ Loan Management ] ═══════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [9]  Process Loan Requests                         ║");
                Console.WriteLine("  ║ [10] View All Loan Requests                        ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Transaction Management ===
                Console.WriteLine("  ║════════ [ Transaction/Balance Management ] ════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [11] Export All Accounts                           ║");
                Console.WriteLine("  ║ [12] Show Top Three Richest                        ║");
                Console.WriteLine("  ║ [13] Show Total Bank Balance                       ║");
                Console.WriteLine("  ║ [14] View Reviews                                  ║");
                Console.WriteLine("  ║ [15] View All Transactions                         ║");
                Console.WriteLine("  ║ [16] Search User Transaction                       ║");
                Console.WriteLine("  ║ [17] Filter User Transactions                      ║");
                Console.WriteLine("  ║ [18] Show Accounts Above Specified Balance         ║");
                Console.WriteLine("  ║ [19] Average Balance                               ║");
                Console.WriteLine("  ║ [20] Richest User(s)                               ║");
                Console.WriteLine("  ║ [21] Total Customers                               ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Feedback & Appointments ===
                Console.WriteLine("  ║═══════════ [ Feedback & Appointments ] ════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [22] View Service Feedbacks                        ║");
                Console.WriteLine("  ║ [23] View/Process Appointment Requests             ║");
                Console.WriteLine("  ║ [24] View All Approved Appointments                ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Currency & Reports ===
                Console.WriteLine("  ║══════════════ [ Currency & Reports ] ══════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [25] Update Exchange Rates / Currency Report       ║");
                Console.WriteLine("  ║                                                    ║");

                // === SYSTEM SECTION ===
                Console.WriteLine("  ║════════════════════════════════════════════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [77] System Statistics                             ║");
                Console.WriteLine("  ║ [88] Backup All Data                               ║");

                // === DANGEROUS ACTIONS ===
                Console.Write("  ║ ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[99] Delete All Data");
                Console.ResetColor();
                Console.WriteLine("                               ║");
                Console.Write("  ║ ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[0] Logout");
                Console.ResetColor();
                Console.WriteLine("                                         ║");

                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ╚════════════════════════════════════════════════════╝");

                bool timedOut;
                Console.Write("  Choose: ");
                string ch = TimedReadLine(10, out timedOut);
                if (timedOut)
                {
                    Console.WriteLine("\nAuto-logout due to inactivity.");
                    PauseBox();
                    return;
                }

                switch (ch)
                {
                    // User Management
                    case "A1": ViewAdminRequests(); break;
                    case "A2": ProcessAdminRequests(); break;
                    case "1": ViewRequests(); break;
                    case "2": ProcessRequest(); break;
                    case "3": ViewAccounts(); break;
                    case "4": AdminSearchByNationalIDorName(); break;
                    case "5": AdminDeleteAccountByNumber(); break;
                    case "6": UnlockUserAccount(); break;
                    case "7": ChangeAdminPassword(); break;
                    case "8": AdminViewLockedAccounts(); break;
                    // Loan Management
                    case "9": ProcessLoanRequests(); break;
                    case "10": ViewAllLoanRequests(); break;
                    // Transaction Management
                    case "11": ExportAllAccountsToFile(); break;
                    case "12": ShowTopRichestCustomers(); break;
                    case "13": ShowTotalBankBalance(); break;
                    case "14": ViewReviews(); break;
                    case "15": ShowAllTransactionsForAllUsers(); break;
                    case "16": AdminSearchUserTransactions(); break;
                    case "17": AdminFilterUserTransactions(); break;
                    case "18": ShowAccountsAboveBalance(); break;
                    case "19": ShowAverageBalance(); break;
                    case "20": ShowRichestUserLINQ(); break;
                    case "21": ShowTotalCustomers(); break;
                    // Feedback & Appointments
                    case "22": AdminViewServiceFeedback(); break;
                    case "23": AdminProcessAppointments(); break;
                    case "24": AdminViewApprovedAppointments(); break;
                    // Currency & Reports
                    case "25": AdminUpdateExchangeRates(); break;
                    // System & Dangerous
                    case "26": AdminSystemStats(); break;
                    case "27": AdminBackupData(); break;
                    case "28": DeleteAllData(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid choice!"); PauseBox(); break;
                }
            }
        }

        /// <summary>
        /// Shows all pending account opening requests for admin review.
        /// </summary>
        public static void ViewRequests()
        {
            Console.Clear();
            PrintBoxHeader("PENDING ACCOUNT REQUESTS", "📝");
            if (accountOpeningRequests.Count == 0)
                Console.WriteLine("|   No requests.                                     |");
            else
                foreach (string r in accountOpeningRequests)
                    Console.WriteLine("|   " + r.PadRight(48) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows admin to approve or reject each pending account opening request.
        /// </summary>
        public static void ProcessRequest()
        {
            Console.Clear();
            PrintBoxHeader("PROCESS REQUESTS", "🔎");
            if (accountOpeningRequests.Count == 0)
            {
                Console.WriteLine("|   No requests.                                     |");
                PrintBoxFooter();
                PauseBox(); return;
            }
            while (accountOpeningRequests.Count > 0)
            {
                string req = accountOpeningRequests.Peek();
                Console.WriteLine("|   " + req.PadRight(48) + "|");
                PrintBoxFooter();
                Console.Write("Approve (A) / Reject (R): ");
                char k = Console.ReadKey().KeyChar;
                Console.WriteLine();

                // Parse request fields
                string username = ParseFieldFromRequest(req, "Username");
                string name = ParseFieldFromRequest(req, "Name");
                string nationalID = ParseFieldFromRequest(req, "National ID");
                double initDeposit = 0.0;
                double.TryParse(ParseFieldFromRequest(req, "Initial"), out initDeposit);
                string phone = ParseFieldFromRequest(req, "Phone");
                string address = ParseFieldFromRequest(req, "Address");
                string hashedPassword = ParseFieldFromRequest(req, "Password"); // Might be in string

                if (k == 'A' || k == 'a')
                {
                    int newAccountNumber = ++lastAccountNumber;
                    // Create new Account object
                    Accounts.Add(new Account
                    {
                        AccountNumber = newAccountNumber,
                        Username = username,
                        Balance = initDeposit,
                        NationalID = nationalID,
                        Phone = phone,
                        Address = address
                    });

                    // Create User if not already added (optional: check if already exists)
                    if (!Users.Any(u => u.Username == username))
                    {
                        Users.Add(new User
                        {
                            Username = username,
                            Password = hashedPassword, // From signup request
                            Role = "Customer"
                        });
                        SaveUsers();
                    }

                    SaveAccountsInformationToFile();
                    accountOpeningRequests.Dequeue();
                    Console.WriteLine("Account created. Number: " + newAccountNumber);
                }
                else if (k == 'R' || k == 'r')
                {
                    accountOpeningRequests.Dequeue();
                    Console.WriteLine("Request rejected.");
                }
                else { Console.WriteLine("Invalid input. Skipping..."); }
                if (accountOpeningRequests.Count == 0) break;
            }
            PauseBox();
        }

        /// <summary>
        /// Shows all approved bank accounts with details.
        /// </summary>
        public static void ViewAccounts()
        {
            Console.Clear();
            PrintBoxHeader("ALL ACCOUNTS", "📒");
            foreach (var acc in Accounts)
            {
                string info = $"Acc#: {acc.AccountNumber} | User: {acc.Username} | Bal: {acc.Balance} | NID: {acc.NationalID}";
                Console.WriteLine("|   " + info.PadRight(48) + "|");
            }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows admin to search by national ID or username. Shows account number and balance.
        /// </summary>
        public static void AdminSearchByNationalIDorName()
        {
            Console.Clear();
            PrintBoxHeader("SEARCH ACCOUNT", "🔍");
            Console.Write("| Enter National ID or Username: ");
            string query = Console.ReadLine();
            bool found = false;
            foreach (var acc in Accounts)
            {
                if (acc.NationalID == query || acc.Username.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Account#: {acc.AccountNumber} | Username: {acc.Username} | Balance: {acc.Balance}");
                    found = true;
                }
            }
            if (!found) Console.WriteLine("No account found.");
            PauseBox();
        }

        /// <summary>
        /// Admin deletes an account by account number.
        /// </summary>
        public static void AdminDeleteAccountByNumber()
        {
            Console.Clear();
            PrintBoxHeader("DELETE ACCOUNT BY NUMBER", "🗑️");
            Console.Write("| Enter Account Number: ");
            string accNumStr = Console.ReadLine();
            int accNum;
            if (!int.TryParse(accNumStr, out accNum))
            {
                Console.WriteLine("Invalid account number!");
                PauseBox();
                return;
            }
            int idx = Accounts.FindIndex(a => a.AccountNumber == accNum);
            if (idx == -1)
            {
                Console.WriteLine("Account not found.");
                PauseBox();
                return;
            }
            Console.WriteLine($"\nAre you sure you want to DELETE account number {accNum}? (yes/no): ");
            string confirm = Console.ReadLine().Trim().ToLower();
            if (confirm != "yes")
            {
                Console.WriteLine("Account deletion cancelled.");
                PauseBox();
                return;
            }
            // Remove account from Accounts list
            Accounts.RemoveAt(idx);
            SaveAccountsInformationToFile();
            Console.WriteLine("Account deleted.");
            PauseBox();
        }

        /// <summary>
        /// Export all account data as CSV/txt.
        /// </summary>
        public static void ExportAllAccountsToFile()
        {
            using (StreamWriter sw = new StreamWriter("accounts_export.txt"))
            {
                sw.WriteLine("AccountNumber,Username,NationalID,Balance");
                foreach (var acc in Accounts)
                    sw.WriteLine($"{acc.AccountNumber},{acc.Username},{acc.NationalID},{acc.Balance}");
            }
            Console.WriteLine("Exported to accounts_export.txt.");
            PauseBox();
        }

        /// <summary>
        /// Show the top 3 customers with the highest balances.
        /// </summary>
        public static void ShowTopRichestCustomers()
        {
            Console.Clear();
            PrintBoxHeader("TOP 3 RICHEST CUSTOMERS", "🏆");

            var top3 = Accounts
                .OrderByDescending(a => a.Balance)
                .Take(3)
                .ToList();

            for (int k = 0; k < top3.Count; k++)
            {
                var acc = top3[k];
                Console.WriteLine($"|   {k + 1}. User: {acc.Username} | Acc#: {acc.AccountNumber} | Bal: {acc.Balance:F2}   |");
            }
            if (top3.Count == 0)
                Console.WriteLine("|   No accounts found.                                 |");

            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Show the total sum of all customer balances (bank's holdings).
        /// </summary>
        public static void ShowTotalBankBalance()
        {
            double total = Accounts.Sum(a => a.Balance);
            PrintBoxHeader("TOTAL BANK BALANCE", "💰");
            Console.WriteLine("|   Bank holds a total of: " + total.ToString("F2").PadRight(28) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Shows all transactions from all users/accounts in the bank.
        /// Each account's transaction history is read from its dedicated file.
        /// Used by Admin to audit all activity.
        /// </summary>
        public static void ShowAllTransactionsForAllUsers()
        {
            Console.Clear();
            PrintBoxHeader("ALL TRANSACTIONS (ALL USERS)", "💸");

            bool found = false;
            foreach (var acc in Accounts)
            {
                string fn = $"{TransactionsDir}/acc_{acc.AccountNumber}.txt";
                if (File.Exists(fn))
                {
                    string[] lines = File.ReadAllLines(fn);
                    if (lines.Length > 0)
                    {
                        Console.WriteLine("| Username: " + acc.Username);
                        foreach (string line in lines)
                        {
                            Console.WriteLine("|   " + line.PadRight(48) + "|");
                        }
                        found = true;
                    }
                }
            }
            if (!found)
            {
                Console.WriteLine("|   No transactions found for any user.               |");
            }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows Admin to search and display all transactions for a specific user by username.
        /// Reads the transaction history file for the selected account.
        /// </summary>        
        public static void AdminSearchUserTransactions()
        {
            Console.Clear();
            PrintBoxHeader("SEARCH USER TRANSACTIONS", "🔎");
            Console.Write("| Enter username: ");
            string searchUser = Console.ReadLine();
            bool found = false;

            foreach (var acc in Accounts.Where(a => a.Username.Equals(searchUser, StringComparison.OrdinalIgnoreCase)))
            {
                string fn = $"{TransactionsDir}/acc_{acc.AccountNumber}.txt";
                Console.WriteLine("| Transactions for: " + acc.Username);
                if (File.Exists(fn))
                {
                    string[] lines = File.ReadAllLines(fn);
                    foreach (string line in lines)
                    {
                        Console.WriteLine("|   " + line.PadRight(48) + "|");
                    }
                    found = true;
                }
                else
                {
                    Console.WriteLine("|   No transactions found for this user.              |");
                    found = true;
                }
            }
            if (!found)
            {
                Console.WriteLine("|   No such username found in system.                 |");
            }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Admin tool to unlock a locked user account and reset failed login attempts.
        /// </summary>
        public static void UnlockUserAccount()
        {
            Console.Clear();
            PrintBoxHeader("UNLOCK USER ACCOUNT", "🔓");
            Console.Write("| Enter username to unlock: ");
            string username = Console.ReadLine();
            var user = Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                if (user.IsLocked)
                {
                    Console.WriteLine($"\nAre you sure you want to UNLOCK the account for '{username}'? (yes/no): ");
                    string confirm = Console.ReadLine().Trim().ToLower();
                    if (confirm != "yes")
                    {
                        Console.WriteLine("Unlock cancelled.");
                        PauseBox();
                        return;
                    }

                    user.IsLocked = false;
                    user.FailedAttempts = 0;
                    SaveUsers();
                    Console.WriteLine("| Account for '" + username + "' has been unlocked!");
                }
                else
                {
                    Console.WriteLine("| Account is not locked.");
                }
            }
            else
            {
                Console.WriteLine("| No such user found.");
            }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Ensures there is always one Admin account in the system with username "q" and password "q".
        /// If the admin account does not exist, it will be created and saved automatically.
        /// Prevents the system from being left without an admin.
        /// </summary>
        public static void EnsureAdminAccount()
        {
            // Look for admin user "q"
            if (Users.Any(u => u.Username == "q" && u.Role == "Admin"))
                return;

            // Create the admin user "q" with password "q"
            Users.Add(new User
            {
                Username = "q",
                Password = HashPassword("q"),
                Role = "Admin",
                IsLocked = false,
                FailedAttempts = 0
            });
            SaveUsers();
        }

        /// <summary>
        /// Deletes all persistent data files (accounts, users, reviews, transaction logs)
        /// and clears all data collections in memory.
        /// </summary>
        public static void DeleteAllData()
        {
            // Display a clear, scary warning
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nWARNING: This will permanently DELETE ALL DATA in the bank system!");
            Console.WriteLine("This includes all users, accounts, transactions, reviews, appointments, etc.");
            Console.ResetColor();
            Console.Write("Are you absolutely sure you want to proceed? (yes/no): ");
            string confirm = Console.ReadLine().Trim().ToLower();
            if (confirm != "yes")
            {
                Console.WriteLine("Delete operation cancelled. No data was deleted.");
                PauseBox();
                return;
            }
            // Delete files if they exist
            try
            {
                if (File.Exists(AccountsFilePath)) File.Delete(AccountsFilePath);
                if (File.Exists(UsersFilePath)) File.Delete(UsersFilePath);
                if (File.Exists(ReviewsFilePath)) File.Delete(ReviewsFilePath);
                if (File.Exists(LoanRequestsFilePath)) File.Delete(LoanRequestsFilePath);
                if (File.Exists(ServiceFeedbackFile)) File.Delete(ServiceFeedbackFile);
                if (File.Exists(AppointmentRequestsFile)) File.Delete(AppointmentRequestsFile);
                if (File.Exists(ApprovedAppointmentsFile)) File.Delete(ApprovedAppointmentsFile);

                // Delete all transaction files
                if (Directory.Exists(TransactionsDir))
                {
                    var files = Directory.GetFiles(TransactionsDir, "*.txt", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                        File.Delete(file);

                    Directory.Delete(TransactionsDir, true);
                }

                // Clear all OOP collections
                Users.Clear();
                Accounts.Clear();
                LoanRequests.Clear();
                ServiceFeedbacks.Clear();
                AppointmentRequests.Clear();
                ApprovedAppointments.Clear();
                accountOpeningRequests.Clear();
                adminAccountRequests.Clear();
                ReviewsS.Clear();
                lastAccountNumber = 1000;

                Console.WriteLine("All data deleted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting data: " + ex.Message);
            }

            PauseBox();
        }

        /// <summary>
        /// Lets the Admin process all pending loan requests one by one.
        /// Admin may Approve (add funds to customer account and mark as Approved) or Reject (mark as Rejected).
        /// Each decision updates status and persists changes. Approved loans are logged as transactions.
        /// </summary>
        public static void ProcessLoanRequests()
        {
            PrintBoxHeader("PROCESS LOAN REQUESTS", "💸");
            if (LoanRequests.Count == 0)
            {
                Console.WriteLine("No loan requests.");
                PauseBox();
                return;
            }

            foreach (var req in LoanRequests.Where(lr => lr.Status == "Pending").ToList())
            {
                Console.WriteLine($"User: {req.Username}, Amount: {req.Amount}, Reason: {req.Reason}");
                Console.Write("Approve (A) / Reject (R): ");
                char k = Console.ReadKey().KeyChar;
                Console.WriteLine();

                if (k == 'A' || k == 'a')
                {
                    var acc = Accounts.FirstOrDefault(a => a.Username == req.Username);
                    if (acc != null)
                    {
                        acc.Balance += req.Amount;
                        SaveAccountsInformationToFile();
                        int accIdx = Accounts.IndexOf(acc);
                        LogTransaction(accIdx, "Loan Approved", req.Amount, acc.Balance);
                        Console.WriteLine("Loan approved and amount added to user account.");
                    }
                    req.Status = "Approved";
                }
                else if (k == 'R' || k == 'r')
                {
                    Console.WriteLine("Loan rejected.");
                    req.Status = "Rejected";
                }
                else
                {
                    Console.WriteLine("Invalid input. Skipping...");
                }
            }
            SaveLoanRequests();
            PauseBox();
        }

        /// <summary>
        /// Allows the Admin to view a list of all loan requests submitted by customers.
        /// Shows username, amount, reason, and current status (Pending, Approved, Rejected) for each request.
        /// This function helps Admins monitor, review, and audit the loan system.
        /// </summary>
        public static void ViewAllLoanRequests()
        {
            PrintBoxHeader("ALL LOAN REQUESTS", "💸");
            if (LoanRequests.Count == 0)
            {
                Console.WriteLine("|   No loan requests found.                          |");
            }
            else
            {
                foreach (var req in LoanRequests)
                {
                    Console.WriteLine($"| User: {req.Username.PadRight(12)} | Amount: {req.Amount,8:F2} | Status: {req.Status.PadRight(9)} | Interest: {req.InterestRate * 100:F1}% | Reason: {req.Reason.PadRight(15)}|");
                }
            }

            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows admin to filter any user's transactions by username and then by date, type, or amount.
        /// Reads transactions from the user's file and displays the filtered results.
        /// </summary>
        public static void AdminFilterUserTransactions()
        {
            Console.Clear();
            PrintBoxHeader("ADMIN TRANSACTION FILTER", "🔎");
            Console.Write("| Enter username to filter: ");
            string user = Console.ReadLine();

            var acc = Accounts.FirstOrDefault(a => a.Username.Equals(user, StringComparison.OrdinalIgnoreCase));
            if (acc == null)
            {
                Console.WriteLine("No such username.");
                PrintBoxFooter();
                PauseBox();
                return;
            }

            string fn = $"{TransactionsDir}/acc_{acc.AccountNumber}.txt";
            if (!File.Exists(fn))
            {
                Console.WriteLine("No transactions found for this user.");
                PrintBoxFooter();
                PauseBox();
                return;
            }

            Console.WriteLine("Filter by: [1] Date Range  [2] Type  [3] Amount  [0] Cancel");
            Console.Write("Choose: ");
            string opt = Console.ReadLine();

            string[] lines = File.ReadAllLines(fn);
            List<string> filtered = new List<string>();

            if (opt == "1")
            {
                Console.Write("Start date (YYYY-MM-DD): ");
                DateTime start, end;
                if (!DateTime.TryParse(Console.ReadLine(), out start))
                {
                    Console.WriteLine("Invalid date.");
                    PauseBox();
                    return;
                }
                Console.Write("End date (YYYY-MM-DD): ");
                if (!DateTime.TryParse(Console.ReadLine(), out end))
                {
                    Console.WriteLine("Invalid date.");
                    PauseBox();
                    return;
                }

                foreach (var line in lines)
                {
                    string[] split = line.Split('|');
                    DateTime dt;
                    if (split.Length > 0 && DateTime.TryParse(split[0].Trim(), out dt))
                    {
                        if (dt >= start && dt <= end)
                            filtered.Add(line);
                    }
                }
            }
            else if (opt == "2")
            {
                Console.Write("Type (Deposit/Withdraw/Transfer Out/Transfer In/Loan Approved): ");
                string type = Console.ReadLine().Trim().ToLower();
                foreach (var line in lines)
                    if (line.ToLower().Contains(type)) filtered.Add(line);
            }
            else if (opt == "3")
            {
                Console.Write("Amount (e.g. 100.00): ");
                double amt;
                if (!double.TryParse(Console.ReadLine(), out amt))
                {
                    Console.WriteLine("Invalid amount.");
                    PauseBox();
                    return;
                }
                foreach (var line in lines)
                    if (line.Contains($"Amount: {amt}")) filtered.Add(line);
            }
            else if (opt == "0")
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid option.");
                PauseBox();
                return;
            }

            PrintBoxHeader($"FILTERED TRANSACTIONS for {user}", "🔍");
            if (filtered.Count == 0)
                Console.WriteLine("|   No transactions match the filter.                 |");
            else
                foreach (var s in filtered)
                    Console.WriteLine("|   " + s.PadRight(48) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows the unique admin to change their own password after verifying the current password.
        /// </summary>
        public static void ChangeAdminPassword()
        {
            PrintBoxHeader("CHANGE ADMIN PASSWORD", "🔑");
            var admin = Users.FirstOrDefault(u => u.Username == "q" && u.Role == "Admin");
            if (admin == null)
            {
                Console.WriteLine("Admin account not found.");
                PauseBox();
                return;
            }
            Console.Write("Enter current password: ");
            string oldPass = ReadMaskedPassword();
            if (HashPassword(oldPass) != admin.Password)
            {
                Console.WriteLine("Incorrect password.");
                PauseBox();
                return;
            }
            Console.Write("Enter new password: ");
            string newPass = ReadMaskedPassword();
            admin.Password = HashPassword(newPass);
            SaveUsers();
            Console.WriteLine("Admin password updated!");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Lets the admin view all submitted service feedback.
        /// Admin can filter by service type or view all.
        /// Shows username, service, feedback text, and date/time for each entry.
        /// </summary>
        public static void AdminViewServiceFeedback()
        {
            PrintBoxHeader("SERVICE FEEDBACKS", "📝");
            if (ServiceFeedbacks.Count == 0)
            {
                Console.WriteLine("|   No service feedback submitted.                   |");
                PrintBoxFooter();
                PauseBox();
                return;
            }

            Console.WriteLine("Filter by service: [1] All  [2] Account Opening  [3] Loans  [4] Transfers  [5] Other");
            Console.Write("Choose: ");
            string filter = Console.ReadLine();

            string filterService = filter switch
            {
                "2" => "Account Opening",
                "3" => "Loans",
                "4" => "Transfers",
                "5" => "Other",
                _ => "" // All
            };

            int num = 1;
            foreach (var fb in ServiceFeedbacks)
            {
                if (filterService == "" || fb.Service == filterService)
                {
                    Console.WriteLine($"| [{num}] [{fb.Service}] {fb.Username}: {fb.Feedback} ({fb.Date})");
                    num++;
                }
            }
            if (num == 1)
                Console.WriteLine("|   No service feedback found for this filter.       |");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Admin: Backup all important data files into a timestamped backup folder.
        /// Copies accounts, users, reviews, loans, feedback, appointments, and transactions.
        /// </summary>
        public static void AdminBackupData()
        {
            try
            {
                string backupDir = "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                System.IO.Directory.CreateDirectory(backupDir);

                // All main files
                string[] filesToBackup = new string[]
                {
            AccountsFilePath, UsersFilePath, ReviewsFilePath,
            LoanRequestsFilePath, ServiceFeedbackFile,
            AppointmentRequestsFile, ApprovedAppointmentsFile
                };

                foreach (var file in filesToBackup)
                    if (System.IO.File.Exists(file))
                        System.IO.File.Copy(file, System.IO.Path.Combine(backupDir, System.IO.Path.GetFileName(file)), true);

                // Transactions directory
                if (System.IO.Directory.Exists(TransactionsDir))
                {
                    string transBackupDir = System.IO.Path.Combine(backupDir, TransactionsDir);
                    System.IO.Directory.CreateDirectory(transBackupDir);
                    foreach (var file in System.IO.Directory.GetFiles(TransactionsDir, "*.txt"))
                        System.IO.File.Copy(file, System.IO.Path.Combine(transBackupDir, System.IO.Path.GetFileName(file)), true);
                }
                Console.WriteLine("All data backed up to folder: " + backupDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Backup failed: " + ex.Message);
            }
            PauseBox();
        }

        /// <summary>
        /// Admin: View and process all pending appointment requests.
        /// </summary>
        public static void AdminProcessAppointments()
        {
            Console.Clear();
            PrintBoxHeader("APPOINTMENT REQUESTS", "📅");
            if (AppointmentRequests.Count == 0)
            {
                Console.WriteLine("|   No appointment requests.                          |");
                PrintBoxFooter();
                PauseBox();
                return;
            }
            int n = AppointmentRequests.Count;
            for (int i = 0; i < n; i++)
            {
                var appt = AppointmentRequests[0];
                AppointmentRequests.RemoveAt(0);
                Console.WriteLine($"| User: {appt.Username} | Service: {appt.Service} | Date: {appt.Date} | Time: {appt.Time}");
                Console.WriteLine($"| Reason: {appt.Reason} | Status: {appt.Status}");
                Console.Write("Approve (A) / Reject (R): ");
                char k = Console.ReadKey().KeyChar;
                Console.WriteLine();
                if (k == 'A' || k == 'a')
                {
                    // Mark as approved, add to approved list
                    appt.Status = "Approved";
                    ApprovedAppointments.Add(appt);
                    Console.WriteLine("Appointment approved!");
                }
                else if (k == 'R' || k == 'r')
                {
                    Console.WriteLine("Appointment rejected.");
                    // Not added anywhere, simply skipped
                }
                else
                {
                    // If invalid input, keep in queue
                    AppointmentRequests.Add(appt);
                    Console.WriteLine("Skipped.");
                }
            }
            SaveAppointmentRequests();
            SaveApprovedAppointments();
            PauseBox();
        }

        /// <summary>
        /// Admin: View all approved appointments.
        /// </summary>
        public static void AdminViewApprovedAppointments()
        {
            PrintBoxHeader("ALL APPROVED APPOINTMENTS", "📅");
            if (ApprovedAppointments.Count == 0)
                Console.WriteLine("|   No approved appointments.                         |");
            else
                foreach (var appt in ApprovedAppointments)
                {
                    Console.WriteLine($"| {appt.Username}: {appt.Service} on {appt.Date} at {appt.Time} ({appt.Reason})");
                }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Admin: View and update currency exchange rates. Shows a report for all.
        /// </summary>
        public static void AdminUpdateExchangeRates()
        {
            PrintBoxHeader("CURRENCY RATES & REPORT", "💱");
            Console.WriteLine("Current Exchange Rates (1 OMR = )");
            Console.WriteLine("USD: {0}   EUR: {1}   SAR: {2}", Rate_USD, Rate_EUR, Rate_SAR);
            Console.Write("Change rates? (y/n): ");
            if (Console.ReadLine().Trim().ToLower() == "y")
            {
                Console.Write("New USD rate: ");
                double.TryParse(Console.ReadLine(), out Rate_USD);
                Console.Write("New EUR rate: ");
                double.TryParse(Console.ReadLine(), out Rate_EUR);
                Console.Write("New SAR rate: ");
                double.TryParse(Console.ReadLine(), out Rate_SAR);
                Console.WriteLine("Rates updated!");
            }
            Console.WriteLine("\n--- All Accounts in Other Currencies ---");
            foreach (var acc in Accounts)
            {
                Console.WriteLine(
                    "User: {0,-12} | OMR: {1,8:F2} | USD: {2,8:F2} | EUR: {3,8:F2} | SAR: {4,8:F2}",
                    acc.Username, acc.Balance,
                    acc.Balance * Rate_USD,
                    acc.Balance * Rate_EUR,
                    acc.Balance * Rate_SAR
                );
            }
            Console.WriteLine("\nTotal Bank Holdings in Other Currencies:");
            double totalOMR = Accounts.Sum(a => a.Balance);
            Console.WriteLine("Total OMR: {0:F2}", totalOMR);
            Console.WriteLine("Total USD: {0:F2}", totalOMR * Rate_USD);
            Console.WriteLine("Total EUR: {0:F2}", totalOMR * Rate_EUR);
            Console.WriteLine("Total SAR: {0:F2}", totalOMR * Rate_SAR);
            SaveExchangeRates();
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Shows all accounts with balance greater than the specified amount using LINQ.
        /// </summary>
        public static void ShowAccountsAboveBalance()
        {
            PrintBoxHeader("ACCOUNTS ABOVE BALANCE", "🔍");
            Console.Write("Enter minimum balance: ");
            double min;
            if (!double.TryParse(Console.ReadLine(), out min))
            {
                Console.WriteLine("Invalid amount.");
                PauseBox();
                return;
            }
            var query = Accounts.Where(a => a.Balance > min);

            int count = 0;
            foreach (var acc in query)
            {
                Console.WriteLine($"| {acc.Username} | Acc#: {acc.AccountNumber} | Bal: {acc.Balance} |");
                count++;
            }
            if (count == 0)
                Console.WriteLine("|   No accounts found.                                 |");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Shows the average balance of all accounts using LINQ.
        /// </summary>
        public static void ShowAverageBalance()
        {
            PrintBoxHeader("AVERAGE ACCOUNT BALANCE", "ℹ️");
            if (Accounts.Count == 0)
                Console.WriteLine("|   No accounts.                                      |");
            else
            {
                double avg = Accounts.Average(a => a.Balance);
                Console.WriteLine($"|   Average balance: {avg:F2}".PadRight(48) + "|");
            }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Shows the user(s) with the highest balance using LINQ.
        /// </summary>
        public static void ShowRichestUserLINQ()
        {
            PrintBoxHeader("RICHEST USER(S) (LINQ)", "💸");
            if (Accounts.Count == 0)
            {
                Console.WriteLine("|   No accounts.                                      |");
                PrintBoxFooter();
                PauseBox();
                return;
            }
            double max = Accounts.Max(a => a.Balance);
            foreach (var acc in Accounts.Where(a => a.Balance == max))
            {
                Console.WriteLine($"| {acc.Username} | Acc#: {acc.AccountNumber} | Bal: {acc.Balance} |");
            }
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Shows total number of customer users (LINQ).
        /// </summary>
        public static void ShowTotalCustomers()
        {
            PrintBoxHeader("TOTAL CUSTOMERS", "👥");
            int count = Users.Count(u => u.Role == "Customer");
            Console.WriteLine($"|   Total customers: {count}".PadRight(48) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Admin: Shows overall statistics about the bank system.
        /// </summary>
        public static void AdminSystemStats()
        {
            Console.Clear();
            PrintBoxHeader("SYSTEM STATISTICS", "📊");
            Console.WriteLine("| Total Registered Users:      " + Users.Count.ToString().PadRight(18) + "|");
            Console.WriteLine("| Total Approved Accounts:     " + Accounts.Count.ToString().PadRight(18) + "|");
            Console.WriteLine("| Total Loans (all):           " + LoanRequests.Count.ToString().PadRight(18) + "|");

            int approvedLoans = LoanRequests.Count(lr => lr.Status == "Approved");
            Console.WriteLine("|  - Approved Loans:           " + approvedLoans.ToString().PadRight(18) + "|");

            Console.WriteLine("| Total Appointments:          " + (AppointmentRequests.Count + ApprovedAppointments.Count).ToString().PadRight(18) + "|");
            Console.WriteLine("|  - Approved Appointments:    " + ApprovedAppointments.Count.ToString().PadRight(18) + "|");
            Console.WriteLine("| Total Reviews:               " + ReviewsS.Count.ToString().PadRight(18) + "|");
            Console.WriteLine("| Total Service Feedbacks:     " + ServiceFeedbacks.Count.ToString().PadRight(18) + "|");

            // Simple "profit" from loan interest
            double loanProfit = 0.0;
            foreach (var lr in LoanRequests)
                if (lr.Status == "Approved")
                    loanProfit += lr.Amount * lr.InterestRate;
            Console.WriteLine("| Total Bank Loan Interest:    " + loanProfit.ToString("F2").PadRight(18) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Admin: View all users who are currently locked out.
        /// </summary>
        public static void AdminViewLockedAccounts()
        {
            Console.Clear();
            PrintBoxHeader("LOCKED ACCOUNTS", "🔒");
            bool found = false;
            foreach (var user in Users)
            {
                if (user.IsLocked)
                {
                    Console.WriteLine($"| Username: {user.Username.PadRight(16)} | Role: {user.Role.PadRight(10)} |");
                    found = true;
                }
            }
            if (!found)
                Console.WriteLine("|   No locked accounts currently.                    |");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Displays all pending admin account requests (Role: Admin) for the bank system.
        /// </summary>
        public static void ViewAdminRequests()
        {
            Console.Clear();
            PrintBoxHeader("PENDING ADMIN ACCOUNT REQUESTS", "👑");
            bool found = false;
            foreach (var req in adminAccountRequests)
            {
                if (req.Contains("Role: Admin"))
                {
                    Console.WriteLine("|   " + req.PadRight(48) + "|");
                    found = true;
                }
            }
            if (!found)
                Console.WriteLine("|   No pending admin requests.                        |");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows admin to process (approve/reject) pending admin account requests.
        /// If approved, adds new admin to Users list with default password "admin123".
        /// </summary>
        public static void ProcessAdminRequests()
        {
            var requests = adminAccountRequests.ToList();
            bool any = false;
            for (int i = 0; i < requests.Count; i++)
            {
                var req = requests[i];
                if (req.Contains("Role: Admin"))
                {
                    Console.Clear();
                    PrintBoxHeader("ADMIN ACCOUNT REQUEST", "👑");
                    Console.WriteLine("|   " + req.PadRight(48) + "|");
                    PrintBoxFooter();
                    Console.Write("Approve (A) / Reject (R): ");
                    string action = Console.ReadLine().Trim().ToUpper();

                    // Parse fields
                    string username = ParseFieldFromRequest(req, "Username");
                    string nationalID = ParseFieldFromRequest(req, "National ID");
                    string phone = ParseFieldFromRequest(req, "Phone");
                    string address = ParseFieldFromRequest(req, "Address");
                    string password = HashPassword("admin123"); // Default password

                    if (action == "A")
                    {
                        // Only add if not already in Users
                        if (!Users.Any(u => u.Username == username && u.Role == "Admin"))
                        {
                            Users.Add(new User
                            {
                                Username = username,
                                Password = password,
                                Role = "Admin",
                                IsLocked = false,
                                FailedAttempts = 0
                            });
                            SaveUsers();
                        }
                        requests[i] = null;
                        Console.WriteLine($"Admin '{username}' approved. Default password: admin123");
                        any = true;
                    }
                    else if (action == "R")
                    {
                        requests[i] = null;
                        Console.WriteLine("Admin request rejected.");
                        any = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Skipping...");
                    }
                    PauseBox();
                }
            }
          
            adminAccountRequests = new Queue<string>(requests.Where(r => r != null));
            if (!any)
            {
                Console.WriteLine("No pending admin requests to process.");
                PauseBox();
            }
        }






        // =============================
        //        CUSTOMER MENU
        // =============================

        /// <summary>
        /// Main customer menu/dashboard. If account not approved, shows request status.
        /// </summary>
        public static void ShowCustomerMenu(int userIdx, string username)
        {
            while (true)
            {
                // --- Pending account logic ---
                if (userIdx == -1)
                {
                    string pendingReq = GetPendingRequestForUser(username);
                    if (pendingReq != null)
                    {
                        PrintBoxHeader("ACCOUNT REQUEST STATUS", "📝");
                        Console.WriteLine("| Your account request is pending approval.           |");
                        PrintBoxFooter();
                        PauseBox();
                        return;
                    }
                    else
                    {
                        RequestAccountOpening(username);
                        return;
                    }
                }

                // ---- APPROVED ACCOUNT MENU ----
                Console.Clear();
                PrintBankLogo();
                Console.WriteLine("  ╔════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║           💳   CUSTOMER DASHBOARD   💳            ║");
                Console.WriteLine("  ╠════════════════════════════════════════════════════╣");
                Console.WriteLine("  ║  Welcome, " + Usernames[userIdx].PadRight(38) + "║");


                // === SECTION: Account Operations ===
                Console.WriteLine("  ║═══════════════ [ Account Operations ] ═════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [1]  Check Balance                                 ║");
                Console.WriteLine("  ║ [2]  Deposit                                       ║");
                Console.WriteLine("  ║ [3]  Withdraw                                      ║");
                Console.WriteLine("  ║ [4]  Transaction History                           ║");
                Console.WriteLine("  ║ [5]  Account Details                               ║");
                Console.WriteLine("  ║ [6]  Transfer Between Accounts                     ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Complaints & Reviews ===
                Console.WriteLine("  ║═══════════ [ Complaints & Reviews ] ═══════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [7]  Submit Review                                 ║");
                Console.WriteLine("  ║ [8]  Undo Last Complaint                           ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Statement & Loans ===
                Console.WriteLine("  ║════════════ [ Statement & Loans ] ═════════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [9]  Print Monthly Statement                       ║");
                Console.WriteLine("  ║ [10] Update Account Information                    ║");
                Console.WriteLine("  ║ [11] Request Loan                                  ║");
                Console.WriteLine("  ║ [12] View My Loan Requests                         ║");
                Console.WriteLine("  ║ [13] Filter My Transactions                        ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Feedback & Appointments ===
                Console.WriteLine("  ║═══════════ [ Feedback & Appointments ] ════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [14] Give Service Feedback                         ║");
                Console.WriteLine("  ║ [15] Book Appointment                              ║");
                Console.WriteLine("  ║ [16] View My Appointments                          ║");
                Console.WriteLine("  ║                                                    ║");

                // === SECTION: Currency Tools ===
                Console.WriteLine("  ║═══════════════ [ Currency Tools ] ═════════════════║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ║ [17] Convert My Balance to Other Currency          ║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ╠════════════════════════════════════════════════════╣");
                Console.WriteLine("  ║                                                    ║");
                // Logout in Red
                Console.Write("  ║ ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[0] Logout");
                Console.ResetColor();
                Console.WriteLine("                                         ║");
                Console.WriteLine("  ║                                                    ║");
                Console.WriteLine("  ╚════════════════════════════════════════════════════╝");

                bool timedOut;
                Console.Write("  Choose: ");
                string ch = TimedReadLine(10, out timedOut);
                if (timedOut)
                {
                    PrintMessageBox("Auto-logout due to inactivity.", ConsoleColor.Red);
                    return;
                }

                switch (ch)
                {
                    case "1": PrintMessageBox("Balance: " + balancesL[userIdx], ConsoleColor.Cyan); break;
                    case "2": Deposit(userIdx); break;
                    case "3": Withdraw(userIdx); break;
                    case "4": ShowTransactionHistory(userIdx); PauseBox(); break;
                    case "5": AccountDetails(userIdx); break;
                    case "6": TransferBetweenAccounts(); break;
                    case "7": Reviews(userIdx); break;
                    case "8": UndoLastComplaint(userIdx); break;
                    case "9": PrintMonthlyStatement(userIdx); break;
                    case "10": UpdateAccountInfo(userIdx); break;
                    case "11": RequestLoan(userIdx); break;
                    case "12": ViewMyLoanRequests(userIdx); break;
                    case "13": FilterMyTransactions(userIdx); break;
                    case "14": SubmitServiceFeedback(userIdx); break;
                    case "15": BookAppointment(userIdx); break;
                    case "16": ViewMyAppointments(userIdx); break;
                    case "17": ConvertMyBalance(userIdx); break;
                    case "0": return;
                    default: PrintMessageBox("Invalid choice! Please try again.", ConsoleColor.Red); break;
                }
            }
        }

        /// <summary>
        /// Lets customer request to open a new account (goes to pending requests).
        /// </summary>
        public static void RequestAccountOpening(string username)
        {
            Console.Clear();
            PrintBoxHeader("REQUEST ACCOUNT OPENING", "📝");
            Console.Write("| Full Name: ");
            string name = Console.ReadLine();
            string nationalID = ReadDigitsOnly("| National ID: ");
            Console.Write("| Initial Deposit Amount: ");
            string initialDeposit = Console.ReadLine();
            PrintBoxFooter();
            if (NationalIDExistsInRequestsOrAccounts(nationalID))
            {
                Console.WriteLine("National ID already exists or pending.");
                PauseBox(); return;
            }
            string request = "Username: " + username + " | Name: " + name + " | National ID: " + nationalID + " | Initial: " + initialDeposit;
            accountOpeningRequests.Enqueue(request);
            Console.WriteLine("\nAccount request submitted!");
            PauseBox();
        }

        /// <summary>
        /// Deposit money for a customer account. Also prints a receipt.
        /// </summary>
        public static void Deposit(int idx)
        {
            Console.Write("Deposit amount: ");
            double amt;
            if (!double.TryParse(Console.ReadLine(), out amt) || amt <= 0) { Console.WriteLine("Invalid amount."); PauseBox(); return; }
            balancesL[idx] += amt;
            SaveAccountsInformationToFile();
            LogTransaction(idx, "Deposit", amt, balancesL[idx]);
            PrintReceipt("Deposit", idx, amt, balancesL[idx]);
            Console.WriteLine("Deposit successful. New Balance: " + balancesL[idx]);
            PauseBox();
        }

        /// <summary>
        /// Withdraw money for a customer account. Enforces minimum balance, prints receipt.
        /// </summary>
        public static void Withdraw(int idx)
        {
            Console.Write("Withdraw amount: ");
            double amt;
            if (!double.TryParse(Console.ReadLine(), out amt) || amt <= 0) { Console.WriteLine("Invalid amount."); PauseBox(); return; }
            if (balancesL[idx] - amt < MinimumBalance) { Console.WriteLine("Insufficient funds or below minimum balance."); PauseBox(); return; }
            balancesL[idx] -= amt;
            SaveAccountsInformationToFile();
            LogTransaction(idx, "Withdraw", amt, balancesL[idx]);
            PrintReceipt("Withdraw", idx, amt, balancesL[idx]);
            Console.WriteLine("Withdraw successful. New Balance: " + balancesL[idx]);
            PauseBox();
        }

        /// <summary>
        /// Allows transferring money between two account numbers.
        /// </summary>
        public static void TransferBetweenAccounts()
        {
            Console.Clear();
            PrintBoxHeader("TRANSFER BETWEEN ACCOUNTS", "💸");
            Console.Write("| From Account Number: ");
            int fromAcc = int.Parse(Console.ReadLine());
            int fromIdx = accountNumbersL.IndexOf(fromAcc);

            Console.Write("| To Account Number: ");
            int toAcc = int.Parse(Console.ReadLine());
            int toIdx = accountNumbersL.IndexOf(toAcc);

            if (fromIdx == -1 || toIdx == -1)
            {
                Console.WriteLine("Invalid account number(s)!");
                PauseBox();
                return;
            }

            Console.Write("| Amount to transfer: ");
            double amt = double.Parse(Console.ReadLine());

            if (amt <= 0 || balancesL[fromIdx] - amt < MinimumBalance)
            {
                Console.WriteLine("Insufficient funds or would drop below minimum balance!");
                PauseBox();
                return;
            }

            balancesL[fromIdx] -= amt;
            balancesL[toIdx] += amt;
            SaveAccountsInformationToFile();
            LogTransaction(fromIdx, "Transfer Out", amt, balancesL[fromIdx]);
            LogTransaction(toIdx, "Transfer In", amt, balancesL[toIdx]);
            Console.WriteLine("Transfer successful.");
            PauseBox();
        }

        /// <summary>
        /// Shows all account details (account number, username, national ID, balance).
        /// </summary>
        public static void AccountDetails(int idx)
        {
            PrintBoxHeader("ACCOUNT DETAILS", "🧾");
            Console.WriteLine("|   Account#: " + accountNumbersL[idx].ToString().PadRight(36) + "|");
            Console.WriteLine("|   Username: " + accountNamesL[idx].PadRight(41) + "|");
            Console.WriteLine("|   National ID: " + nationalIDsL[idx].PadRight(35) + "|");
            Console.WriteLine("|   Balance: " + balancesL[idx].ToString().PadRight(39) + "|");
            Console.WriteLine("|   Phone: " + phoneNumbersL[idx].PadRight(41) + "|");
            Console.WriteLine("|   Address: " + addressesL[idx].PadRight(35) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Lets a customer submit a new review or complaint (pushes onto stack).
        /// </summary>
        public static void Reviews(int userIdx)
        {
            Console.Clear();
            PrintBoxHeader("SUBMIT COMPLAINT/REVIEW", "✉️");
            Console.Write("| Your complaint/review: ");
            string review = Console.ReadLine();
            ReviewsS.Push(review);
            SaveReviews();
            Console.WriteLine("Complaint submitted.");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Removes the most recent review/complaint submitted by any user.
        /// </summary>
        public static void UndoLastComplaint(int userIdx)
        {
            if (ReviewsS.Count > 0)
            {
                ReviewsS.Pop();
                SaveReviews();
                Console.WriteLine("Last complaint removed!");
            }
            else
                Console.WriteLine("No complaint to remove.");
            PauseBox();
        }

        /// <summary>
        /// Shows all submitted reviews/complaints (from the stack).
        /// </summary>
        public static void ViewReviews()
        {
            Console.Clear();
            PrintBoxHeader("ALL COMPLAINTS/REVIEWS", "✉️");
            if (ReviewsS.Count == 0)
                Console.WriteLine("|   No reviews.                                       |");
            else
            {
                int num = 1;
                foreach (string s in ReviewsS)
                {
                    Console.WriteLine($"| [{num}] {s.PadRight(45)}|");
                    num++;
                }
            }

            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Generates a monthly statement for the user's account by year and month.
        /// Displays all transactions for the chosen period, and allows the user
        /// to save the statement as a text file. Shows "No transactions" if none found.
        /// </summary>
        public static void PrintMonthlyStatement(int idx)
        {
            PrintBoxHeader("PRINT MONTHLY STATEMENT", "🗓️");
            Console.Write("Enter year (YYYY): ");
            int year;
            if (!int.TryParse(Console.ReadLine(), out year))
            {
                Console.WriteLine("Invalid year.");
                PauseBox();
                return;
            }
            Console.Write("Enter month (1-12): ");
            int month;
            if (!int.TryParse(Console.ReadLine(), out month) || month < 1 || month > 12)
            {
                Console.WriteLine("Invalid month.");
                PauseBox();
                return;
            }

            string fn = TransactionsDir + "/acc_" + accountNumbersL[idx] + ".txt";
            if (!File.Exists(fn))
            {
                Console.WriteLine("No transactions found for this account.");
                PauseBox();
                return;
            }

            string[] lines = File.ReadAllLines(fn);
            List<string> result = new List<string>();
            foreach (var line in lines)
            {
                DateTime dt;
                // Each line starts with DateTime: "6/29/2024 3:43:34 PM | ..."
                string[] split = line.Split('|');
                if (split.Length > 0 && DateTime.TryParse(split[0].Trim(), out dt))
                {
                    if (dt.Year == year && dt.Month == month)
                        result.Add(line);
                }
            }

            // Print result
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║           MONTHLY STATEMENT                        ║");
            Console.WriteLine($"║    Account#: {accountNumbersL[idx]}  User: {accountNamesL[idx]}                        ║");
            Console.WriteLine($"║    Period: {month}/{year}                                  ║");
            Console.WriteLine("╠════════════════════════════════════════════════════╣");
            if (result.Count == 0)
                Console.WriteLine("║   No transactions in this period.                 ║");
            else
                foreach (var s in result)
                    Console.WriteLine("║   " + s.PadRight(48) + "║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");

            // Ask to save as file
            Console.Write("Save this statement as a file? (y/n): ");
            string save = Console.ReadLine().Trim().ToLower();
            if (save == "y")
            {
                string statementFile = $"statement_{accountNumbersL[idx]}_{year}_{month}.txt";
                using (StreamWriter sw = new StreamWriter(statementFile))
                {
                    sw.WriteLine("==== MONTHLY STATEMENT ====");
                    sw.WriteLine($"Account#: {accountNumbersL[idx]}");
                    sw.WriteLine($"Username: {accountNamesL[idx]}");
                    sw.WriteLine($"Period: {month}/{year}");
                    sw.WriteLine("===========================");
                    if (result.Count == 0)
                        sw.WriteLine("No transactions in this period.");
                    else
                        foreach (var s in result)
                            sw.WriteLine(s);
                }
                Console.WriteLine("Statement saved as " + statementFile);
            }

            PauseBox();
        }

        /// <summary>
        /// Allows the user to update their account information (username, password, or national ID)
        /// after verifying their current password. Updates both the Users list and account data.
        /// Passwords are securely handled and hashed. Username and National ID must remain unique.
        /// </summary>
        public static void UpdateAccountInfo(int userIdx)
        {
            // idx is the index in accounts lists (should match userIdx if 1-to-1, otherwise adjust mapping)
            int idx = userIdx;
            if (idx == -1)
            {
                Console.WriteLine("No approved account found.");
                PauseBox();
                return;
            }

            PrintBoxHeader("UPDATE ACCOUNT INFO", "✏️");
            Console.WriteLine("[1] Change Username");
            Console.WriteLine("[2] Change Password");
            Console.WriteLine("[3] Change National ID");
            Console.WriteLine("[4] Change Phone Number");
            Console.WriteLine("[5] Change Address");
            Console.WriteLine("[0] Cancel");
            Console.Write("Choose: ");
            string choice = Console.ReadLine();

            if (choice == "0") return;

            // Require current password for changes
            Console.Write("Enter current password: ");
            string oldPass = ReadMaskedPassword();
            if (HashPassword(oldPass) != Passwords[userIdx])
            {
                Console.WriteLine("Incorrect password.");
                PauseBox();
                return;
            }

            if (choice == "1")
            {
                Console.Write("Enter new username: ");
                string newUser = Console.ReadLine();
                // Check uniqueness
                foreach (var u in Usernames)
                    if (u == newUser)
                    {
                        Console.WriteLine("Username already taken.");
                        PauseBox();
                        return;
                    }
                Usernames[userIdx] = newUser;
                accountNamesL[idx] = newUser;
                Console.WriteLine("Username updated.");
            }
            else if (choice == "2")
            {
                Console.Write("Enter new password: ");
                string newPass = ReadMaskedPassword();
                Passwords[userIdx] = HashPassword(newPass);
                Console.WriteLine("Password updated.");
            }
            else if (choice == "3")
            {
                string newNID = ReadDigitsOnly("Enter new National ID: ");
                // Check uniqueness
                if (nationalIDsL.Contains(newNID))
                {
                    Console.WriteLine("National ID already in use.");
                    PauseBox();
                    return;
                }
                nationalIDsL[idx] = newNID;
                Console.WriteLine("National ID updated.");
            }
            else if (choice == "4")
            {
                string newPhone = ReadDigitsOnly("Enter new phone number: ");
                phoneNumbersL[idx] = newPhone;
                Console.WriteLine("Phone number updated.");
            }
            else if (choice == "5")
            {
                Console.Write("Enter new address: ");
                string newAddr = Console.ReadLine();
                addressesL[idx] = newAddr;
                Console.WriteLine("Address updated.");
            }
            else
            {
                Console.WriteLine("Invalid choice.");
                PauseBox();
                return;
            }

            SaveUsers();
            SaveAccountsInformationToFile();
            PauseBox();
        }

        /// <summary>
        /// Allows a customer to submit a new loan request.
        /// The user enters the amount and reason; the request is added to the LoanRequests queue with "Pending" status.
        /// Loan requests are automatically saved to disk after submission.
        /// </summary>
        public static void RequestLoan(int userIdx)
        {
            int idx = userIdx; // Assuming 1:1 mapping between users and accounts
            if (idx == -1)
            {
                Console.WriteLine("You need an approved account first.");
                PauseBox();
                return;
            }

            // 1. Check minimum balance
            if (balancesL[idx] < 5000)
            {
                Console.WriteLine("Your balance must be at least 5000 to request a loan.");
                PauseBox();
                return;
            }

            // 2. Check if user has active (pending or approved) loan
            bool hasActiveLoan = false;
            for (int i = 0; i < LoanReq_Usernames.Count; i++)
            {
                if (LoanReq_Usernames[i] == Usernames[userIdx] &&
                    (LoanReq_Status[i] == "Pending" || LoanReq_Status[i] == "Approved"))
                {
                    hasActiveLoan = true;
                    break;
                }
            }
            if (hasActiveLoan)
            {
                Console.WriteLine("You already have a pending or active loan. Only one loan allowed at a time.");
                PauseBox();
                return;
            }

            PrintBoxHeader("REQUEST LOAN", "💸");
            Console.Write("Enter loan amount: ");
            double amount;
            if (!double.TryParse(Console.ReadLine(), out amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                PauseBox();
                return;
            }
            Console.Write("Enter reason for loan: ");
            string reason = Console.ReadLine();

            double interestRate = 0.05; // 5% interest rate (adjust as you like)

            // Add to parallel lists
            LoanReq_Usernames.Add(Usernames[userIdx]);
            LoanReq_Amounts.Add(amount);
            LoanReq_Reasons.Add(reason);
            LoanReq_Status.Add("Pending");
            LoanReq_InterestRates.Add(interestRate);

            SaveLoanRequests(); // Always save after adding
            Console.WriteLine($"Loan request submitted for review (interest rate: {interestRate * 100:F1}%).");
            PauseBox();
        }

        /// <summary>
        /// Displays all loan requests submitted by the currently logged-in customer.
        /// Each request is shown with amount, reason, and status (Pending, Approved, Rejected).
        /// Lets users track their loan application status transparently.
        /// </summary>
        public static void ViewMyLoanRequests(int userIdx)
        {
            PrintBoxHeader("MY LOAN REQUESTS", "💸");
            bool found = false;
            string username = Usernames[userIdx];
            for (int i = 0; i < LoanReq_Usernames.Count; i++)
            {
                if (LoanReq_Usernames[i] == username)
                {
                    Console.WriteLine($"| User: {LoanReq_Usernames[i].PadRight(12)} | Amount: {LoanReq_Amounts[i],8:F2} | Status: {LoanReq_Status[i].PadRight(9)} | Interest: {LoanReq_InterestRates[i] * 100:F1}% | Reason: {LoanReq_Reasons[i].PadRight(15)}|");
                    found = true;
                }
            }
            if (!found)
                Console.WriteLine("You have no loan requests.");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Allows users to filter their own transaction history by date range, type, or amount.
        /// Results are displayed in a readable format, making it easy for users to audit or review
        /// their account activities quickly.
        /// </summary>
        public static void FilterMyTransactions(int idx)
        {
            string fn = TransactionsDir + "/acc_" + accountNumbersL[idx] + ".txt";
            if (!File.Exists(fn))
            {
                Console.WriteLine("No transactions found.");
                PauseBox();
                return;
            }

            // Ask user for filter options
            Console.WriteLine("Filter by: [1] Date Range  [2] Type  [3] Amount  [0] Cancel");
            Console.Write("Choose: ");
            string opt = Console.ReadLine();

            string[] lines = File.ReadAllLines(fn);
            List<string> filtered = new List<string>();

            if (opt == "1")
            {
                Console.Write("Start date (YYYY-MM-DD): ");
                DateTime start, end;
                if (!DateTime.TryParse(Console.ReadLine(), out start))
                {
                    Console.WriteLine("Invalid date.");
                    PauseBox();
                    return;
                }
                Console.Write("End date (YYYY-MM-DD): ");
                if (!DateTime.TryParse(Console.ReadLine(), out end))
                {
                    Console.WriteLine("Invalid date.");
                    PauseBox();
                    return;
                }

                foreach (var line in lines)
                {
                    string[] split = line.Split('|');
                    DateTime dt;
                    if (split.Length > 0 && DateTime.TryParse(split[0].Trim(), out dt))
                    {
                        if (dt >= start && dt <= end)
                            filtered.Add(line);
                    }
                }
            }
            else if (opt == "2")
            {
                Console.Write("Type (Deposit/Withdraw/Transfer Out/Transfer In/Loan Approved): ");
                string type = Console.ReadLine().Trim().ToLower();
                foreach (var line in lines)
                    if (line.ToLower().Contains(type)) filtered.Add(line);
            }
            else if (opt == "3")
            {
                Console.Write("Amount (e.g. 100.00): ");
                double amt;
                if (!double.TryParse(Console.ReadLine(), out amt))
                {
                    Console.WriteLine("Invalid amount.");
                    PauseBox();
                    return;
                }
                foreach (var line in lines)
                    if (line.Contains($"Amount: {amt}")) filtered.Add(line);
            }
            else if (opt == "0")
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid option.");
                PauseBox();
                return;
            }

            // Show results
            PrintBoxHeader("FILTERED TRANSACTIONS", "🔍");
            if (filtered.Count == 0)
                Console.WriteLine("|   No transactions match the filter.                 |");
            else
                foreach (var s in filtered)
                    Console.WriteLine("|   " + s.PadRight(48) + "|");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Lets a user submit feedback about a particular bank service (account opening, loans, etc.).
        /// The feedback is stored in ServiceFeedbacks and can be viewed by the admin.
        /// </summary>
        public static void SubmitServiceFeedback(int userIdx)
        {
            Console.WriteLine("Select service to give feedback about:");
            Console.WriteLine("[1] Account Opening\n[2] Loans\n[3] Transfers\n[4] Other");
            string opt = Console.ReadLine();
            string service = opt switch
            {
                "1" => "Account Opening",
                "2" => "Loans",
                "3" => "Transfers",
                _ => "Other"
            };

            Console.Write("Write your feedback: ");
            string text = Console.ReadLine();

            string record = $"{Usernames[userIdx]}|{service}|{text}|{DateTime.Now}";
            ServiceFeedbacks.Add(record);
            SaveServiceFeedbacks();
            Console.WriteLine("Service feedback submitted!");
            Console.WriteLine("Thank you for helping us improve our services!");
            PauseBox();
        }

        /// <summary>
        /// Customer: Book an appointment for a bank service.
        /// </summary>
        public static void BookAppointment(int userIdx)
        {
            Console.Clear();
            PrintBoxHeader("BOOK APPOINTMENT", "📅");
            Console.WriteLine("Services: [1] Open Account [2] Loan [3] Consultation [4] Other");
            Console.Write("Choose service: ");
            string s = Console.ReadLine();
            string service = s switch
            {
                "1" => "Open Account",
                "2" => "Loan",
                "3" => "Consultation",
                _ => "Other"
            };

            Console.Write("Preferred Date (YYYY-MM-DD): ");
            string date = Console.ReadLine();
            Console.Write("Preferred Time (e.g. 14:00): ");
            string time = Console.ReadLine();
            Console.Write("Reason (optional): ");
            string reason = Console.ReadLine();

            string req = $"{Usernames[userIdx]}|{service}|{date}|{time}|{reason}|Pending";
            AppointmentRequests.Enqueue(req);
            SaveAppointmentRequests();
            Console.WriteLine("Appointment request submitted! Wait for admin approval.");
            PauseBox();
        }

        /// <summary>
        /// Customer: View your own appointments (pending and approved).
        /// </summary>
        public static void ViewMyAppointments(int userIdx)
        {
            PrintBoxHeader("MY APPOINTMENTS", "📅");
            bool found = false;
            string username = Usernames[userIdx];

            foreach (var appt in AppointmentRequests)
                if (appt.StartsWith(username + "|"))
                {
                    var parts = appt.Split('|');
                    Console.WriteLine($"| Pending: {parts[1]} on {parts[2]} at {parts[3]} ({parts[4]})");
                    found = true;
                }
            foreach (var appt in ApprovedAppointments)
                if (appt.StartsWith(username + "|"))
                {
                    var parts = appt.Split('|');
                    Console.WriteLine($"| Approved: {parts[1]} on {parts[2]} at {parts[3]} ({parts[4]})");
                    found = true;
                }
            if (!found)
                Console.WriteLine("|   No appointments found.                            |");
            PrintBoxFooter();
            PauseBox();
        }

        /// <summary>
        /// Customer: Convert your account balance to another currency.
        /// </summary>
        public static void ConvertMyBalance(int idx)
        {
            PrintBoxHeader("CURRENCY CONVERSION", "💱");
            double omr = balancesL[idx];
            Console.WriteLine("Your Balance: {0} OMR", omr);
            Console.WriteLine("Convert to: [1] USD  [2] EUR  [3] SAR  [0] Cancel");
            Console.Write("Choose: ");
            string ch = Console.ReadLine();
            if (ch == "1")
                Console.WriteLine("= {0} USD", (omr * Rate_USD).ToString("F2"));
            else if (ch == "2")
                Console.WriteLine("= {0} EUR", (omr * Rate_EUR).ToString("F2"));
            else if (ch == "3")
                Console.WriteLine("= {0} SAR", (omr * Rate_SAR).ToString("F2"));
            else
                Console.WriteLine("Conversion cancelled.");
            PrintBoxFooter();
            PauseBox();
        }



    }

}
