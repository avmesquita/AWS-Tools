using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using MaterialSkin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aws_security_group_access_rule
{
    public partial class Form1 : MaterialSkin.Controls.MaterialForm
    {
        private string MyIp = "0.0.0.0";

        public Form1()
        {
            InitializeComponent();

            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            // Configure color schema
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Green900, Primary.Amber900,
                Primary.Amber500, Accent.Amber700,
                TextShade.WHITE
            );
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string ipEnpoint = ConfigurationManager.AppSettings["IP-ENDPOINT"].ToString();
            MyIp = get_external_ip(ipEnpoint);

            lblIP.Text = MyIp;
        }

        private string get_external_ip(string ipEndpoint)
        {
            string ret = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(ipEndpoint));
            request.UserAgent = "aws-security-group-access-rule app";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                ret = reader.ReadToEnd();
            }
            return ret;
        }
        private void create_security_group_rule(string key, string secret, Amazon.RegionEndpoint region, string security_group, string serverports, string ipEnpoint = "")
        {
            HttpStatusCode ret = HttpStatusCode.OK;
            string ip = "0.0.0.0";
            MailAddress fromMailer = null;
            MailAddress toMailer = null;
            string mailhost = "";
            int mailport = 0;
            bool mailssl = false;
            string mailuser = "";
            string mailpass = "";

            bool loadConfig = true;

            string partialmsg = "";

            if (ipEnpoint != string.Empty)
            {
                ip = get_external_ip(ipEnpoint);
            }

            if (loadConfig)
            {
                try
                {
                    fromMailer = new MailAddress(ConfigurationManager.AppSettings["MAIL-FROM"].ToString());
                    toMailer = new MailAddress(ConfigurationManager.AppSettings["MAIL-TO"].ToString());
                    mailhost = ConfigurationManager.AppSettings["MAIL-HOST"].ToString();
                    mailport = int.Parse(ConfigurationManager.AppSettings["MAIL-PORT"].ToString());
                    mailssl = bool.Parse(ConfigurationManager.AppSettings["MAIL-SSL"].ToString());
                    mailuser = ConfigurationManager.AppSettings["MAIL-USER"].ToString();
                    mailpass = ConfigurationManager.AppSettings["MAIL-PASS"].ToString();
                    ipEnpoint = ConfigurationManager.AppSettings["IP-ENDPOINT"].ToString();
                    ip = get_external_ip(ipEnpoint);
                }
                catch { loadConfig = false; }
            }

            if (ip != "")
            {
                try
                {
                    AmazonEC2Client ec2Client = null;

                    AWSCredentials credentials = new Amazon.Runtime.BasicAWSCredentials(key, secret);

                    ec2Client = new AmazonEC2Client(credentials, region);

                    IpRange ipRange = new IpRange
                    {
                        CidrIp = ip + "/32",
                        Description = "Rule created by aws-security-group-access-rule at " + DateTime.Now.ToString()
                    };

                    var ingressRequest = new AuthorizeSecurityGroupIngressRequest
                    {
                        GroupId = security_group
                    };

                    var listaPortas = serverports.Split(',');
                    foreach (var item in listaPortas)
                    {
                        var ipPermission = new IpPermission
                        {
                            IpProtocol = "tcp",
                            FromPort = Int16.Parse(item),
                            ToPort = Int16.Parse(item)
                        };
                        ipPermission.Ipv4Ranges.Add(ipRange);

                        ingressRequest.IpPermissions.Add(ipPermission);
                    }

                    var ingressResponse = ec2Client.AuthorizeSecurityGroupIngress(ingressRequest);
                    partialmsg += "[AWS Response :: " + ingressResponse.HttpStatusCode.ToString() + "]";
                    ret = ingressResponse.HttpStatusCode;

                    #region Debug Section
                    /*
                    switch (ingressResponse.HttpStatusCode)
                    {
                        case HttpStatusCode.Continue: partialmsg += ""; break;
                        case HttpStatusCode.SwitchingProtocols: partialmsg += ""; break;
                        case HttpStatusCode.OK: partialmsg += ""; break;
                        case HttpStatusCode.Created: partialmsg += ""; break;
                        case HttpStatusCode.Accepted: partialmsg += ""; break;
                        case HttpStatusCode.NonAuthoritativeInformation: partialmsg += ""; break;
                        case HttpStatusCode.NoContent: partialmsg += ""; break;
                        case HttpStatusCode.ResetContent: partialmsg += ""; break;
                        case HttpStatusCode.PartialContent: partialmsg += ""; break;
                        case HttpStatusCode.MultipleChoices: partialmsg += ""; break;
                        case HttpStatusCode.MovedPermanently: partialmsg += ""; break;
                        case HttpStatusCode.Found: partialmsg += ""; break;
                        case HttpStatusCode.SeeOther: partialmsg += ""; break;
                        case HttpStatusCode.NotModified: partialmsg += ""; break;
                        case HttpStatusCode.UseProxy: partialmsg += ""; break;
                        case HttpStatusCode.Unused: partialmsg += ""; break;
                        case HttpStatusCode.TemporaryRedirect: partialmsg += ""; break;
                        case HttpStatusCode.BadRequest: partialmsg += ""; break;
                        case HttpStatusCode.Unauthorized: partialmsg += ""; break;
                        case HttpStatusCode.PaymentRequired: partialmsg += ""; break;
                        case HttpStatusCode.Forbidden: partialmsg += ""; break;
                        case HttpStatusCode.NotFound: partialmsg += ""; break;
                        case HttpStatusCode.MethodNotAllowed: partialmsg += ""; break;
                        case HttpStatusCode.NotAcceptable: partialmsg += ""; break;
                        case HttpStatusCode.ProxyAuthenticationRequired: partialmsg += ""; break;
                        case HttpStatusCode.RequestTimeout: partialmsg += ""; break;
                        case HttpStatusCode.Conflict: partialmsg += ""; break;
                        case HttpStatusCode.Gone: partialmsg += ""; break;
                        case HttpStatusCode.LengthRequired: partialmsg += ""; break;
                        case HttpStatusCode.PreconditionFailed: partialmsg += ""; break;
                        case HttpStatusCode.RequestEntityTooLarge: partialmsg += ""; break;
                        case HttpStatusCode.RequestUriTooLong: partialmsg += ""; break;
                        case HttpStatusCode.UnsupportedMediaType: partialmsg += ""; break;
                        case HttpStatusCode.RequestedRangeNotSatisfiable: partialmsg += ""; break;
                        case HttpStatusCode.ExpectationFailed: partialmsg += ""; break;
                        case HttpStatusCode.UpgradeRequired: partialmsg += ""; break;
                        case HttpStatusCode.InternalServerError: partialmsg += ""; break;
                        case HttpStatusCode.NotImplemented: partialmsg += ""; break;
                        case HttpStatusCode.BadGateway: partialmsg += ""; break;
                        case HttpStatusCode.ServiceUnavailable: partialmsg += ""; break;
                        case HttpStatusCode.GatewayTimeout: partialmsg += ""; break;
                        case HttpStatusCode.HttpVersionNotSupported: partialmsg += ""; break;
                    }
                    */
                    #endregion
                }
                catch (AmazonEC2Exception ex)                
                {
                    ret = ex.StatusCode;

                    partialmsg += "[ERROR-CODE " + ex.ErrorCode + "] " + ex.Message + "<BR>";                    
                }
                finally
                {
                    if (loadConfig)
                    {
                        sendEmail(mailhost, mailport, mailssl, mailuser, mailpass, fromMailer, toMailer,
                                  "[aws-security-group-access-rule]" + ret.ToString(),
                                  string.Format("Access granted to ports {1} by IP {0}",
                                                ip, serverports)
                                  );
                    }
                }
            }

            void sendEmail(string hostname, int port, bool ssl, string user, string pass, MailAddress fromMail, MailAddress toMail, string subject, string message, bool ishtml = false)
            {
                MailMessage msg = new MailMessage
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = ishtml,
                    Priority = MailPriority.High,
                    From = fromMail
                };
                msg.To.Add(toMail);                

                SmtpClient mailClient = new SmtpClient
                {
                    Host = hostname,
                    Port = port,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = ssl
                };
                mailClient.Send(msg);
            }

        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            string awsKey = ConfigurationManager.AppSettings["AWS-KEY"].ToString();
            string awsSecret = ConfigurationManager.AppSettings["AWS-SECRET"].ToString();
            string awsSG = ConfigurationManager.AppSettings["AWS-SG_ID"].ToString();
            string ipEnpoint = ConfigurationManager.AppSettings["IP-ENDPOINT"].ToString();

            string serverports = txtPorts.Text;

            var listaPortas = serverports.Split(',');
            if ((awsKey != string.Empty) && (awsSecret != string.Empty) && (awsSG != string.Empty) && (listaPortas.Count() > 0) && (ipEnpoint != string.Empty))
            {
                create_security_group_rule(awsKey, awsSecret, Amazon.RegionEndpoint.USEast1, awsSG, serverports, ipEnpoint);
            }
        }
    }
}