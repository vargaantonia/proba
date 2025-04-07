using IngatlanokBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IngatlanokBackend
{  
    public class Program
    {
        public static string ftpUrl = "ftp.nethely.hu";
        public static string ftpUserName = "ingatlan";
        public static string ftpPassword = "Ingatlanok12345";

        public static int SaltLength = 64;

        public static Dictionary<string, Felhasznalok> LoggedInUsers = new Dictionary<string, Felhasznalok>();

        public static string GenerateSalt()
        {
            Random random = new Random();
            const string karakterek = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var salt = new StringBuilder();
            for (int i = 0; i < SaltLength; i++)
            {
                salt.Append(karakterek[random.Next(karakterek.Length)]);
            }
            return salt.ToString();
        }

        public static async Task SendEmail(string mailAddressTo, string subject, string body, bool isHtml = false)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("ingatlanberlesiplatform@gmail.com");
            mail.To.Add(mailAddressTo);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isHtml; 

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("ingatlanberlesiplatform@gmail.com", "mhwhbcbihzzozqvc");
            SmtpServer.EnableSsl = true;

            await SmtpServer.SendMailAsync(mail);
        }


        public static string CreateSHA256(string input, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(input + salt));
                var sBuilder = new StringBuilder();
                foreach (var b in data)
                {
                    sBuilder.Append(b.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static void Main(string[] args)
        {   
           var builder = WebApplication.CreateBuilder(args);

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            builder.Services.AddDbContext<HalakDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddDbContext<IngatlanberlesiplatformContext>(options =>
                options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                                 .AllowAnyMethod()
                                 .AllowAnyHeader()
                                 .AllowAnyOrigin();
                });
            });

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"], 
                        ValidAudience = builder.Configuration["Jwt:Audience"], 
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) 
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowSpecificOrigin");
            app.MapControllers();
            app.Run();
        }
    }
}


//Scaffold-DbContext "SERVER=localhost;PORT=3306;DATABASE=ingatlanberlesiplatform;USER=root;PASSWORD=;SSL MODE=none;" mysql.entityframeworkcore -outputdir Models -f
