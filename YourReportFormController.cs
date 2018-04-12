namespace TRFU2016.Controllers
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Web;
    using System.Web.Mvc;
    using YourSpaceName.Models;
    using umbraco;
    using Umbraco.Web;
    using Umbraco.Web.Mvc;
    using Xceed.Words.NET;
    using static YourSpaceName.Models.recaptchaModel;

    public class YourFormController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult ShowReportForm()
        {
            return this.PartialView("YourReportForm", new YourReportFormModel());
        }

        
        public static CaptchaResponse ValidateCaptcha(string response)
        {
            string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["recaptchaPrivateKey"];
            var client = new WebClient();
            //The following API URL is used to verify the user response
            var jsonResult = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));
            return JsonConvert.DeserializeObject<CaptchaResponse>(jsonResult.ToString());
        }

        [HttpPost]
        public ActionResult SubmitReportForm (RedYellowCardReportFormModel model)
        {
            //verify Google reCAPTCHA
            if (string.IsNullOrEmpty(Request["g-recaptcha-response"]))
            {
                ModelState.AddModelError("reCAPTCHA", "Please complete the reCAPTCHA");
                return CurrentUmbracoPage();
            }
            else
            {
                CaptchaResponse response = ValidateCaptcha(Request["g-recaptcha-response"]);
                if (!response.Success)
                {
                    ModelState.AddModelError("reCAPTCHA", "The reCAPTCHA is incorrect!");
                    return CurrentUmbracoPage();
                }
            }

            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            // Look for multiple receivers on Umbraco page
            List<string> managerEmailList = new List<string>();

            string managerEmail1 = CurrentPage.Parent.GetPropertyValue<string>("notifyEmail");
            managerEmailList.Add(managerEmail1);

            string managerEmail2 = CurrentPage.Parent.GetPropertyValue<string>("notifyEmail2");
            managerEmailList.Add(managerEmail2);

            string managerEmail3 = CurrentPage.Parent.GetPropertyValue<string>("notifyEmail3");
            managerEmailList.Add(managerEmail3);

            string managerEmail = "";

            for (int i = 0; i < managerEmailList.Count(); i++)
            {
                if ((managerEmailList[i].Trim() != "") && (i != managerEmailList.Count() - 1))
                {
                    managerEmail += managerEmailList[i] + ",";
                }
                if ((managerEmailList[i].Trim() != "") && (i == managerEmailList.Count() - 1))
                {
                    managerEmail += managerEmailList[i];
                }
            }

            SendNotificationToManager(model, managerEmail);

            SendAutoResponder(model);

            return CurrentUmbracoPage();
        }

        private void SendNotificationToManager (YourReportFormModel model, string managerEmail)
        {
            var subject = string.Format("New {0} form by {1} {2}", model.ReportType, model.RefereeType, model.RefereeName);

            var message = string.Format("Please find {0} form by {1} {2} in the attachment", model.ReportType, model.RefereeType, model.RefereeName);

            //MailMessage is Disposable ==> USE "using"!
            using (var email = new MailMessage
            {
                Subject = subject,
                From = new MailAddress(model.RefereeEmail),
                Body = message,
                IsBodyHtml = true
            })
            {
                var developerEmail = ConfigurationManager.AppSettings["DeveloperEmail"];

                email.To.Add(string.IsNullOrWhiteSpace(managerEmail) ? developerEmail : managerEmail);

                var documentFileName = CreateWordDoc(model);

                //Attachment is Disposable ==> USE "using"!
                using (Attachment refsReportDocx = new Attachment(documentFileName))
                {
                    email.Attachments.Add(refsReportDocx);

                    var isSendCopyToDev = bool.Parse(ConfigurationManager.AppSettings["SendEmailCopyToDev"]);

                    if (isSendCopyToDev)
                    {
                        email.Bcc.Add(developerEmail);
                    }

                    SmtpClient smtp = new SmtpClient();
                    smtp.Send(email);
                }

                // at this point Attachment has been disposed - so we can delete the cached file safely
                System.IO.File.Delete(documentFileName);
            }
        }

        private void SendAutoResponder (RedYellowCardReportFormModel model)
        {
            var subject = string.Format("Report form submission on {0}.", this.Request.Url.Host);

            var message = string.Format("Hi {0}, Thank you for your submission. We'll review your submission as soon as possible. <br/> Cheers, <br> Taranaki Rugby Referee Association <br/>", model.RefereeName);

            using (var email = new MailMessage
            {
                Subject = subject,
                To = { model.RefereeEmail },
                From = new MailAddress("noreply@trra.co.nz"),
                Body = message,
                IsBodyHtml = true
            })
            {
                var developerEmail = ConfigurationManager.AppSettings["DeveloperEmail"];

                var isSendCopyToDev = bool.Parse(ConfigurationManager.AppSettings["SendEmailCopyToDev"]);

                if (isSendCopyToDev)
                {
                    email.Bcc.Add(developerEmail);
                }

                //Send email
                try
                {
                    //Connect to SMTP credentials set in web.config
                    SmtpClient smtp = new SmtpClient();
                    //Try & send the email with the SMTP settings
                    smtp.Send(email);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private string CreateWordDoc(YourReportFormModel model)
        {
            //Try to find the path "~YOURPATH"
            if (!Directory.Exists(Server.MapPath("~/YOURPATH")))
            {
                //If it's not found --> create one
                Directory.CreateDirectory(Server.MapPath("~/YOURPATH"));
            }
            string docName = string.Format("{0} form by {1} {2}", model.ReportType, model.RefereeType, model.RefereeName);
            string fileName = Server.MapPath("~/YOURPATH/" + docName + ".docx");
            //Create docx file
            var doc = DocX.Create(fileName);

            //Add header image
            var image = doc.AddImage(Server.MapPath("~/YOURPATH/YourHeaderImage.jpg")); 
            var logo = image.CreatePicture(60, 120);
            doc.AddHeaders();
            doc.Headers.Odd.InsertParagraph().AppendPicture(logo).SpacingBefore(0).Alignment = Alignment.right;

            //Start building word doc
            doc.InsertParagraph("YOUR REPORT TITLE").FontSize(12).Font("Trebuchet MS").Alignment = Alignment.center;
            var p = doc.InsertParagraph(" ");
            
            //Create table 1
            var tb1 = doc.AddTable(7, 2);
            tb1.Alignment = Alignment.center;
            tb1.Rows[0].Cells[0].Paragraphs.First().Append("Home Team: " + model.HomeTeam).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb1.Rows[0].Cells[1].Paragraphs.First().Append("Visitor: " + model.Visitors).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb1.Rows[2].Cells[0].Paragraphs.First().Append("Venue: " + model.Venue).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb1.Rows[2].Cells[1].Paragraphs.First().Append("Date: " + model.FixtureDate).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb1.Rows[4].MergeCells(0, 1);
            tb1.Rows[4].Paragraphs.First().Append("Period of the match when called off: " + model.Period).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb1.Rows[6].MergeCells(0, 1);
            tb1.Rows[6].Paragraphs.First().Append("Elapsed time in half: " + model.TimeElapsed).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            
            //Configure the table 1
            tb1.SetWidths(new float[] { 200, 300, 100 });
            var blankBorder = new Border(Xceed.Words.NET.BorderStyle.Tcbs_none, 0, 0, Color.White);
            tb1.SetBorder(TableBorderType.Bottom, blankBorder);
            tb1.SetBorder(TableBorderType.Top, blankBorder);
            tb1.SetBorder(TableBorderType.Left, blankBorder);
            tb1.SetBorder(TableBorderType.Right, blankBorder);
            tb1.SetBorder(TableBorderType.InsideH, blankBorder);
            tb1.SetBorder(TableBorderType.InsideV, blankBorder);
            doc.InsertTable(tb1);

            doc.InsertParagraph(" ");
            
            //Create table 2
            var tb2 = doc.AddTable(3, 2);
            tb2.Alignment = Alignment.center;
            tb2.Rows[0].Cells[0].Paragraphs.First().Append("REFEREE’S NAME: " + model.RefereeName).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb2.Rows[0].Cells[1].Paragraphs.First().Append("UNION: " + model.RefereeUnion).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            tb2.Rows[2].MergeCells(0, 1);
            tb2.Rows[2].Paragraphs.First().Append("CONTACT PHONE: " + model.RefereePhone).FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            
            //Configure table 2
            tb2.SetWidths(new float[] { 200, 300, 100 });
            tb2.SetBorder(TableBorderType.Bottom, blankBorder);
            tb2.SetBorder(TableBorderType.Top, blankBorder);
            tb2.SetBorder(TableBorderType.Left, blankBorder);
            tb2.SetBorder(TableBorderType.Right, blankBorder);
            tb2.SetBorder(TableBorderType.InsideH, blankBorder);
            tb2.SetBorder(TableBorderType.InsideV, blankBorder);
            doc.InsertTable(tb2);

            doc.InsertParagraph(" ");
            doc.InsertParagraph("What were the circumstances in which the match was called off? ").FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            doc.InsertParagraph(model.Circumstances).FontSize(8).Font("Trebuchet MS").Alignment = Alignment.left;
            doc.InsertParagraph(" ");
            doc.InsertParagraph("What were the examples of the persistent or serious Foul Play or Misconduct that led to the match being called off and who committed these offences? ").FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            doc.InsertParagraph(model.Misconduct).FontSize(8).Font("Trebuchet MS").Alignment = Alignment.left;
            doc.InsertParagraph(" ");
            doc.InsertParagraph("Were one or both teams responsible for the match being called off (give details)? ").FontSize(10).Font("Trebuchet MS").Alignment = Alignment.left;
            doc.InsertParagraph(model.Details).FontSize(8).Font("Trebuchet MS").Alignment = Alignment.left;
            doc.InsertParagraph(" ");
            doc.InsertParagraph("REPORT TO BE LODGED WITH THE PROVINCIAL UNION WHERE THE MATCH WAS PLAYED WITHIN 48 HOURS OF THE MATCH").FontSize(12).Font("Trebuchet MS").Alignment = Alignment.center;

            //Save the document
            doc.Save();
                
            return fileName;
        }
    
    }
}