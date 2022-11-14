using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;


namespace TestEmailOtp.Repositories
{
    public class Email_Otp_Module_Model
    {
        //the function will check the otp is valid or not
        public int CheckOtp(IConfiguration config,string email, int code)
        {
            String fileName = config.GetSection("Settings")["FileName"];
            String path = config.GetSection("Settings")["Path"];
            String filePath = path + @"\" + fileName;
            DateTime curDate = DateTime.Now;

            string[] lines = File.ReadAllLines(filePath);
            int status = 0;

            for (var i = 0; i < lines.Length; i++) 
            {
                String[] item = lines[i].Split(",").Select(x => x.Trim()).ToArray();
                String otpEmail = item[0];
                int otp = int.Parse(item[1]);
                TimeSpan ts = curDate - DateTime.Parse(item[2]);

                if (otpEmail.ToUpper().Equals(email.ToUpper()) && otp == code && ts.TotalMinutes <= 60)
                {
                    status = 0;
                    return status;
                }
                else if(ts.TotalMinutes > 60) {
                    status = 2;
                    return status;
                }

                //Console.WriteLine(lines[i] + " - " + otp + " - " + ts.TotalMinutes);
            }

            status = 1;
            return status;
        }

        public String Generate_Otp_Email(IConfiguration config, String email)
        {
            String otp = "";
            String fileName = config.GetSection("Settings")["FileName"];
            String path = config.GetSection("Settings")["Path"];
            String filePath = path + @"\" + fileName;

            if (!Directory.Exists(path)) 
            {
                Directory.CreateDirectory(path);
            }

            Random rnd = new Random();
            otp = (rnd.Next(100000, 999999)).ToString();
            String input = email + ", " + otp + ", " + DateTime.Now;

            if (!File.Exists(fileName))
            {
                File.Create(filePath).Close();
            }

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(input);
            }

            return otp;

        }

        public int SendEmail(IConfiguration config, string email, string otp)
        {
            try
            {
                String smtp = config.GetSection("EmailSettings")["SmtpServer"];
                String smtpAddress = config.GetSection("EmailSettings")["FromEmail"];
                String smtpPassword = config.GetSection("EmailSettings")["Pw"];
                String[] toMail = email.Split(",");
                String fromMail = config.GetSection("EmailSettings")["FromEmail"];
                int port = int.Parse(config.GetSection("EmailSettings")["Port"]);

                Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                RegexOptions.CultureInvariant | RegexOptions.Singleline);

                bool isValidEmail = regex.IsMatch(email);
                if (!isValidEmail) return 2;
                //if (!email.ToUpper().Contains(".DSO.ORG.SG")) return 2;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromMail);

                foreach (var recipent in toMail)
                {
                    if (!string.IsNullOrEmpty(recipent)) //Added for null validation
                        mail.To.Add(new MailAddress(recipent));
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"Your OTP code is {0}. The code is valid for 1 minute.", otp);
                String body = sb.ToString();

                mail.Subject = config.GetSection("EmailSettings")["Subject"];
                mail.IsBodyHtml = true;
                mail.Body = body;

                using SmtpClient client = new SmtpClient
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Host = smtp,
                    Port = port,
                    Credentials = new System.Net.NetworkCredential(smtpAddress, smtpPassword)
                };

                client.Send(mail);
                
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
                throw ex;
            }
        }
    }
}
