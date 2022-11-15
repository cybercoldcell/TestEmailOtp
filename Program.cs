using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using TestEmailOtp.Repositories;

namespace TestEmailOtp
{
    class Program
    {
        private static IConfiguration _config { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
             //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             //.AddEnvironmentVariables();
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false);
            _config = builder.Build();
            bool flag = true;
            string process = null;
            while (flag) 
            {
                ProcessInput();
                Console.Write("Do you want to continue (Y/N)?");
                process = Console.ReadLine();
                if (process.ToUpper() == "N") break;
            }
           
            Console.ReadLine();

        }

        public static void ProcessInput()
        {
            Console.Write("Enter email: ");
            var email = Console.ReadLine();
            Email_Otp_Module_Model repo = new Email_Otp_Module_Model();
            string otp = repo.Generate_Otp_Email(_config, email);
            int sent = repo.SendEmail(_config, email, otp);
            int result = 0;

            switch (sent)
            {
                case 0:
                    Console.WriteLine(EmailStatus.STATUS_EMAIL_OK.ToString() + " email containing OTP has been sent successfully.");

                    for (int i = 1; i <= 10; i++)
                    {
                        Console.Write(i + " Enter your code: ");
                        int code = int.Parse(Console.ReadLine());
                        result = repo.CheckOtp(_config, email, code);
                        if (result == 0 || result == 2) break;
                    }

                    //if (result != 0 && result != 1) result = 2;

                    switch (result)
                    {
                        case 0:
                            Console.WriteLine(OtpStatus.STATUS_OTP_OK.ToString() + " OTP is valid and checked.");
                            break;
                        case 1:
                            Console.WriteLine(OtpStatus.STATUS_OTP_FAIL.ToString() + " OTP is wrong after 10 tries.");
                            break;
                        default:
                            Console.WriteLine(OtpStatus.STATUS_OTP_TIMEOUT.ToString() + " timeout after 1 minute.");
                            break;

                    }
                    break;
                case 1:
                    Console.WriteLine(EmailStatus.STATUS_EMAIL_FAIL.ToString() + "  email address does not exist or sending to the email has failed.");
                    break;
                default:
                    Console.WriteLine(EmailStatus.STATUS_EMAIL_INVALID.ToString() + "  email address is invalid.");
                    break;
            }
        }

        public enum OtpStatus { 
            STATUS_OTP_OK = 0, STATUS_OTP_FAIL = 1, STATUS_OTP_TIMEOUT = 2
        }

        public enum EmailStatus
        {
            STATUS_EMAIL_OK = 0, STATUS_EMAIL_FAIL = 1, STATUS_EMAIL_INVALID = 2
        }
    }
}
