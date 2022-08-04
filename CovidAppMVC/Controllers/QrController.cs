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
using CovidAppMVC.Hubs;
using ZXing;

namespace CovidAppMVC.Controllers
{
    public class QrController : Controller
    {
        private Covid19Entities db = new Covid19Entities();
        HttpClient client = new HttpClient();

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
            else
            {
                SendMessage("Ошибка, QR-код не прочитан!");
            }

            return Content("true");
        }

        private void GetData(string url)
        {
            string checkUrl = "https://www.gosuslugi.ru/covid-cert/status/";
            try
            {
                if (url.Substring(0, 43) == checkUrl && url.Length >= 79 && url.Length <= 87)
                {
                    url = "https://www.gosuslugi.ru/api/covid-cert-checker/v3/cert/" + url.Substring(36);
                    GetItems(url);
                }
                /*else
                {
                    SendMessage("Ошибка, QR-код не с сайта госуслуг!");
                }*/
            }
            catch (Exception e)
            {
                SendMessage("Ошибка, QR-код не с сайта госуслуг!");
            }
        }

        private void GetItems(string url)
        {           
            string response = client.GetStringAsync(url).Result;           
            Sertificate sertificate = JsonConvert.DeserializeObject<Sertificate>(response);

            var сертификаты = db.Сертификаты;
            foreach (var c in сертификаты)
            {
                if (c.Номер_сертификата == string.Format("{0:#### #### #### ####}", sertificate.certId))
                {
                    SendMessage("Сертификат успешно найден!");
                    return;
                }                    
            }
            SendMessage("Сертификат не найден!");
        }

        private class Sertificate
        {
            public long certId { get; set; }
            public string validFrom { get; set; }
        }

        private void SendMessage(string message)
        {
            // Получаем контекст хаба
            var context =
                Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            // отправляем сообщение
            context.Clients.All.displayMessage(message);
        }
    }
}