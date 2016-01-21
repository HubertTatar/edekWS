using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    public class WorkerSend
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(WorkerSend));
        private DirectoryInfo resultDir = null;
        //private ProgressModel model = new ProgressModel();
        private AbstractSender sender;
        private RunMode currentRunMode;

        public WorkerSend(DirectoryInfo resultDir, RunMode runMode)
        {
            this.resultDir = resultDir;
            this.currentRunMode = runMode;
        }

        public void doJob()
        {
            sender = new edekSender();

            switch (currentRunMode)
            {
                case RunMode.IFT1_3:
                    sendFiles(FormType.IFT1, "xml.xades");
                    sendFiles(FormType.IFT3, "xml.xades");
                    break;
                case RunMode.IFT2:
                    sendFiles(FormType.IFT2, "xml.xades");
                    break;
                case RunMode.PIT11C:
                    sendFiles(FormType.PIT11C, "xml.xades");
                    break;
                case RunMode.PIT8C:
                    //sendFile(FormType.PIT8C, "xml.zip.xades");
                    sendFiles(FormType.PIT8C, "xml.xades");
                    break;
            }
        }

        private void sendFile(FormType typDok, string extension)
        {
            Console.WriteLine("Wysylam " + typDok.ToString());

            Status status = StatusHandler.checkStatusFile(typDok, extension, resultDir);


            if (status == null)
            {
                Console.WriteLine("Nie znaleziono plików do wyslania");
                return;
            }

            int documentToSend = status.FileStatus.Count;

            if (documentToSend == 0)
            {
                Console.WriteLine("Nie znaleziono plików do wyslania");
                return;
            }

            if (documentToSend > 1)
            {
                Console.WriteLine("Znaleziono więcej niż jeden plik do wysłania, proszę usunąc zbędne pliki z katalogu.");
                removeStatus(typDok);
            }
            else
            {
                Deklaracja dek = status.FileStatus[0];

                if (dek.refId.Equals(""))
                {
                    Console.WriteLine("Wysyłam dla " + dek.filePath);
                    sender.send(dek);
                }
                else
                {
                    Console.WriteLine("Juz wyslany z refId= " + dek.refId + " " + dek.filePath);
                }

                StatusHandler.saveXML(status, typDok, resultDir);
                StatusHandler.saveReport(status, typDok, resultDir);
            }
        }


        private void sendFiles(FormType typDok, string extension)
        {

            //if (Config.Instance.onlyOne)
            //{
            //    if (Config.Instance.selectedIds == null)
            //    {
            //        Console.WriteLine("Nie podano żadnych ID");
            //        return;
            //    }

            //    string[] idsTab = Config.Instance.selectedIds.Split(',');
            //    sendFew(typDok, extension, idsTab);
            //}
            //else
            //{
                sendAll(typDok, extension);
            //}
        }

        private void sendFew(FormType typDok, string extension, string[] idsTab)
        {
            Console.WriteLine("Wysylam " + typDok.ToString());

            Status status = StatusHandler.checkStatusFile(typDok, extension, resultDir);
            List<Deklaracja> filesToSend = new List<Deklaracja>();

            foreach (string clientId in idsTab)
            {
                foreach (Deklaracja dek in status.FileStatus)
                {
                    if (clientId.Equals(dek.clientId))
                    {
                        filesToSend.Add(dek);
                    }
                }
            }

            int documentToSend = filesToSend.Count;


            if (documentToSend == 0)
            {
                Console.WriteLine("Nie znaleziono plików do wyslania");
                return;
            }


            int documentCnt = 0;
            foreach (Deklaracja dek in filesToSend)
            {
                documentCnt++;

                Console.WriteLine("Wysylam " + documentCnt + " z " + documentToSend + " id: " + dek.clientId);

                if (dek.refId.Equals(""))
                {
                    Console.WriteLine("Wysylam " + dek.filePath);
                    sender.send(dek);
                }
                else
                {
                    Console.WriteLine("Juz wyslany z refId= " + dek.refId + " " + dek.filePath);
                }
            }
            StatusHandler.saveXML(status, typDok, resultDir);
            StatusHandler.saveReport(status, typDok, resultDir);
        }

        private void sendAll(FormType typDok, string extension)
        {

            Console.WriteLine("Wysylam " + typDok.ToString());

            Status status = StatusHandler.checkStatusFile(typDok, extension, resultDir);

            if (status == null)
            {
                Console.WriteLine("Nie znaleziono katalogu z plikami do wyslania");
                return;
            }

            int documentToSend = status.FileStatus.Count;

            if (documentToSend == 0)
            {
                Console.WriteLine("Nie znaleziono plików do wyslania");
                return;
            }

            int documentCnt = 0;
            foreach (Deklaracja dek in status.FileStatus)
            {
                documentCnt++;

                Console.WriteLine("Wysylam " + documentCnt + " z " + documentToSend + " id: " + dek.clientId);

                if (dek.refId.Equals(""))
                {
                    Console.WriteLine("Wysylam " + dek.filePath);
                    sender.send(dek);
                }
                else
                {
                    Console.WriteLine("Juz wyslany z refId= " + dek.refId + " " + dek.filePath);
                }
            }
            StatusHandler.saveXML(status, typDok, resultDir);
            StatusHandler.saveReport(status, typDok, resultDir);
        }

        private void removeStatus(FormType typDok)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + typDok.ToString());
            if (!directory.Exists)
            {
                return;
            }
            string pathToFile = directory + "\\status_" + typDok.ToString() + ".xml";
            FileInfo statusFile = new FileInfo(pathToFile);
            statusFile.Delete();
        }
    }
}
