using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aws_getfroms3
{
    class Program
    {
        static string MODO = "A";
        static string EndPoint = "";
        static string Chave = "";
        static string Segredo = "";
        static string meuBucket = "";
        static string nome_arquivo = "";
        static string caminho_arquivo = "";
        public enum ModalidadeTransferencia { Download = 0, Upload = 1 }
        public enum Debug { Desligado = 0, Ligado = 1 }

        static TransferUtility transfer = null;

        public const int FIVE_MINUTES = 5 * 60 * 1000;

        public static System.IO.StreamWriter file = null;

        ~Program()
        {
            file.Flush();
        }

        static void Main(string[] args)
        {
            Debug modoDebug = Debug.Desligado;

            ModalidadeTransferencia modalidadeTransferencia = ModalidadeTransferencia.Download;

            file = new System.IO.StreamWriter(@"C:\TEMP\aws-getfroms3.log", true);

            string Sintaxe = "";
            Sintaxe = " " + Environment.NewLine +
                      "SINTAX:" + Environment.NewLine +
                      " " + Environment.NewLine +
                      " > aws-getfroms3.exe <mode>,<bucket>,<key>,<secret>,<full-path> " + Environment.NewLine +
                      " " + Environment.NewLine +
                      "PS.: IF <mode> = 'A' or null, config file will be considered." + Environment.NewLine +
                      " " + Environment.NewLine;

            PutObjectResponse resposta = null;

            file.WriteLine("Loading configs...");
            Console.WriteLine("Loading configs...");

            // Carrega os parâmetros ou exibe mensagem sobre sinxaxe
            if (!carregouParametros(args))
            {
                if (args.Length.Equals(0))
                {
                    Console.WriteLine(Sintaxe);
                    return;
                }
            }
            else
            {
                if (!MODO.Equals("A"))
                {
                    Console.WriteLine(Sintaxe);
                    return;
                }
            }

            if (!File.Exists(caminho_arquivo))
            {
                Console.WriteLine("File not found.");
                file.WriteLine("File not found.");
                return;
            }

            try
            {
                try
                {
                    transfer = new TransferUtility(Chave, Segredo);

                    switch (modalidadeTransferencia)
                    {
                        case ModalidadeTransferencia.Download:
                            file.WriteLine("Creating request to download from bucket " + meuBucket + "...");
                            Console.WriteLine("Creating request to download from bucket " + meuBucket + "...");

                            TransferUtilityDownloadRequest requestDownload = new TransferUtilityDownloadRequest()
                            {
                                BucketName = meuBucket,
                                FilePath = caminho_arquivo
                            };

                            Console.WriteLine("Starting download...");
                            file.WriteLine("Starting download...");
                            transfer.Download(requestDownload);
                            Console.WriteLine("... download done.");
                            file.WriteLine("... download done.");

                            Console.WriteLine("/nFile received.");
                            file.WriteLine("/nFile received.");

                            break;

                        case ModalidadeTransferencia.Upload:

                            file.WriteLine("Creating request to upload to bucket " + meuBucket + "...");
                            Console.WriteLine("Creating request to upload to bucket " + meuBucket + "...");

                            TransferUtilityUploadRequest requestUpload = new TransferUtilityUploadRequest()
                            {
                                BucketName = meuBucket,
                                FilePath = caminho_arquivo
                            };

                            Console.WriteLine("Starting sending...");
                            file.WriteLine("Starting sending...");
                            transfer.Upload(requestUpload);
                            Console.WriteLine("... file sent.");
                            file.WriteLine("... file sent.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    transfer.Dispose();
                }
            }
            catch (Exception ex)
            {
                string msgErro = "An error occourred." + Environment.NewLine +
                                  "[INTERNAL_MESSAGE=" + ex.Message.ToString() + Environment.NewLine;
                if (ex.InnerException != null)
                    msgErro += "] & [INNER_MESSAGE=" + ex.InnerException.Message.ToString() + Environment.NewLine;

                if (ex.StackTrace != null)
                    msgErro += "] & [STACK_TRACE=" + ex.StackTrace.ToString() + "]";

                if (ex.InnerException == null && ex.StackTrace == null)
                    msgErro += "]";

                Console.WriteLine(msgErro);
                file.WriteLine(msgErro);
            }
            finally
            {
                if (modoDebug == Debug.Ligado)
                {
                    Console.WriteLine("Awswer from S3...");
                    file.WriteLine("Awswer from S3...");
                    if (resposta == null)
                    {
                        Console.WriteLine("Awswer from S3 was null.");
                        file.WriteLine("Awswer from S3 was null.");
                    }
                    else
                    {
                        Console.WriteLine(resposta.ResponseMetadata.ToString());
                        file.WriteLine(resposta.ResponseMetadata.ToString());
                    }
                }
                file.Flush();
                file.Dispose();
            }
        }

        static bool carregouParametros(string[] args)
        {
            if (args.Length.Equals(0))
                MODO = ConfigurationManager.AppSettings["MODO"].ToString();
            else
                MODO = args[1];


            if (MODO.Equals("A"))
            {
                meuBucket = ConfigurationManager.AppSettings["BUCKET_NAME"].ToString(); ;
                Chave = ConfigurationManager.AppSettings["CHAVE"].ToString();
                Segredo = ConfigurationManager.AppSettings["SEGREDO"].ToString();
                nome_arquivo = ConfigurationManager.AppSettings["NOME_ARQUIVO"].ToString();
                caminho_arquivo = ConfigurationManager.AppSettings["CAMINHO_ARQUIVO"].ToString();
                EndPoint = ConfigurationManager.AppSettings["ENDPOINT"].ToString();
            }
            else
            {
                if (args.Length.Equals(0))
                    return false;

                meuBucket = args[2];
                Chave = args[3];
                Segredo = args[4];
                nome_arquivo = args[5];
                caminho_arquivo = args[6];
                EndPoint = args[7];
            }

            if (
                  !EndPoint.Equals("") &&
                  !Chave.Equals("") &&
                  !Segredo.Equals("") &&
                  !meuBucket.Equals("") &&
                  !nome_arquivo.Equals("") &&
                  !caminho_arquivo.Equals("")
                )
                return true;
            else
                return false;
        }
    }
}
