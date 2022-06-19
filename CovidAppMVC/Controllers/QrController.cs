using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CovidAppMVC.Models;
using ZXing;

namespace CovidAppMVC.Controllers
{
    public class QrController : Controller
    {
        public ActionResult QRScanView()
        {
            return View();
        }

        [HttpPost]
        public ContentResult SaveCapture(string data)
        {
            string fileName = DateTime.Now.ToString("dd-MM-yy hh-mm-ss");
            
            //Convert Base64 Encoded string to Byte Array.
            byte[] imageBytes = Convert.FromBase64String(data.Split(',')[1]);

            //Save the Byte Array as Image File.
            string filePath = Server.MapPath(string.Format("~/App_Data/{0}.jpg", fileName));
            System.IO.File.WriteAllBytes(filePath, imageBytes);

            string url = "";
            BarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode((Bitmap)Image.FromFile(filePath));
            if (result != null)
            {
                url = result.ToString();
                GetData(url);
            }

            return Content("true");
        }

        static async Task GetData(string url)
        {
            string checkUrl = "https://www.gosuslugi.ru/covid-cert/status/";

            /*url = "https://www.gosuslugi.ru/api/covid-cert-checker/v3/cert/" + url.Substring(36);
            //Console.WriteLine(url);
            //Console.WriteLine("https://www.gosuslugi.ru/api/covid-cert-checker/v3/cert/status/97092f87-c655-4894-a77d-ceaa98fa4cd4?");
            await program.GetItems(url);*/

            //Console.WriteLine("https://www.gosuslugi.ru/covid-cert/status/e598f485-2334-44a4-8633-b4af9527f95f?lang=ru".Length);
            try
            {
                if (url.Substring(0, 43) == checkUrl && url.Length >= 79 && url.Length <= 87)
                {
                    url = "https://www.gosuslugi.ru/api/covid-cert-checker/v3/cert/" + url.Substring(36);
                    //Console.WriteLine(url);
                    //Console.WriteLine("https://www.gosuslugi.ru/api/covid-cert-checker/v3/cert/status/97092f87-c655-4894-a77d-ceaa98fa4cd4?");
                    await GetItems(url);
                }
                else
                {
                    Console.WriteLine("Ошибка");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка");
            }
        }

        private static async Task GetItems(string url)
        {
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(url);

            Console.WriteLine(response);
            Sertificate sertificate = JsonConvert.DeserializeObject<Sertificate>(response);
            Console.WriteLine();
            Console.WriteLine(sertificate.certId);
            Console.WriteLine(sertificate.validFrom);
        }

        private class Sertificate
        {
            public string certId { get; set; }
            public string validFrom { get; set; }
        }
    }
}