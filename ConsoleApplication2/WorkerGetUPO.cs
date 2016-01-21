using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication2
{
    class WorkerGetUPO
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(WorkerGetUPO));
        private DirectoryInfo resultDir = null;
        private AbstractSender sender;
        private RunMode currentRunMode;

        public WorkerGetUPO(DirectoryInfo resultDir, RunMode runMode)
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
                    getUPO(FormType.IFT1);
                    getUPO(FormType.IFT3);
                    break;
                case RunMode.PIT8C:
                    getUPO(FormType.PIT8C);
                    break;
                case RunMode.PIT11C:
                    getUPO(FormType.PIT11C);
                    break;
                default:
                    Console.WriteLine("Nieznany typ dokumentu " + currentRunMode.ToString());
                    break;
            }
        }

        private void getUPO(FormType typDok)
        {

            //if (Config.Instance.onlyOne)
            //{
            //    if (Config.Instance.selectedIds == null)
            //    {
            //        log.Warn("Nie podano żadnych ID");
            //        return;
            //    }

            //    string[] idsTab = Config.Instance.selectedIds.Split(',');
            //    getFew(typDok, idsTab);
            //}
            //else
            //{
                getAll(typDok);
            //}


        }

        private void getFew(FormType typDok, string[] idsTab)
        {
            Console.WriteLine("Pobieram UPO " + typDok.ToString());

            Status status = checkStatusFile(typDok);

            if (status == null)
            {
                Console.WriteLine("Brak pliku status w katalogu wynikowym.");
                return;
            }

            List<Deklaracja> uposToGet = new List<Deklaracja>();

            foreach (string clientId in idsTab)
            {
                foreach (Deklaracja dek in status.FileStatus)
                {
                    if (clientId.Equals(dek.clientId))
                    {
                        uposToGet.Add(dek);
                    }
                }
            }

            if (uposToGet.Count == 0)
            {
                Console.WriteLine("Nic do pobrania dla " + typDok.ToString());
                return;
            }

            int documentToSend = uposToGet.Count;
            int documentCnt = 0;
            foreach (Deklaracja dek in uposToGet)
            {
                documentCnt++;


                if ("200".Equals(dek.status))
                {
                    Console.WriteLine("Dla " + dek.clientId + " UPO już pobrane(status 200), pomijam.");
                }
                else
                {
                    Console.WriteLine("Pobieram " + dek.filePath);
                    string upo = sender.getUpo(dek);
                    if ("200".Equals(dek.status))
                    {
                        string upoPath = saveUPO(dek, upo, typDok);

                    }
                    else if (Convert.ToInt32(dek.status) >= 300 && Convert.ToInt32(dek.status) <= 399)
                    {
                        Console.WriteLine("Dla " + dek.clientId + "dokument w trakcie przetwarzania, status - " + dek.status + ".");
                    }
                }
            }
            saveStatus(status, typDok);
            saveReport(status, typDok, resultDir);
        }



        private void getAll(FormType typDok)
        {

            Console.WriteLine("Pobieram UPO " + typDok.ToString());

            Status status = checkStatusFile(typDok);

            if (status == null)
            {
                Console.WriteLine("Brak pliku status w katalogu wynikowym.");
                return;
            }

            int documentToSend = status.FileStatus.Count;
            int documentCnt = 0;
            foreach (Deklaracja dek in status.FileStatus)
            {
                documentCnt++;

                Console.WriteLine("Wysylam " + dek.filePath);

                if ("200".Equals(dek.status))
                {
                    Console.WriteLine("Dla " + dek.clientId + " UPO już pobrane(status 200), pomijam.");
                }
                else
                {
                    string upo = sender.getUpo(dek);
                    if ("200".Equals(dek.status))
                    {
                        string upoPath = saveUPO(dek, upo, typDok);
                        if (StatusHandler.checkIfUPOConfirmed(upoPath))
                        {
                            dek.upoPath = upoPath;
                            Console.WriteLine(String.Format("UPO dla klienta {0} : przyjęte", dek.clientId));
                        }
                        else
                        {
                            dek.upoPath = moveBadUPO(upoPath, typDok, dek.clientId);
                            Console.WriteLine(String.Format("UPO dla klienta {0} : nie przyjęte", dek.clientId));
                        }

                        // Potwierdzenie savedUpo = checkUPO(upoPath);
                        //log.Info(String.Format("UPO dla klienta {0} : przyjęto {1} : data {2}", dek.clientId, savedUpo.Przyjeto, savedUpo.DataWplyniecia));
                    }
                    else
                    {
                        Console.WriteLine("Dla kilenta " + dek.clientId + " nie udało sie pobrać UPO. Szczegóły: status= " + dek.status + " opis= " + dek.message);
                    }
                }
            }
            saveStatus(status, typDok);
            saveReport(status, typDok, resultDir);
        }

        private string moveBadUPO(string upoPath, FormType typDok, string clientID)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + typDok.ToString() + "\\UPO\\nieprzyjete");
            if (!directory.Exists)
            {
                directory.Create();
            }

            string pathToFile = directory + "\\" + clientID + "_" + typDok.ToString() + ".xml";

            File.Move(upoPath, pathToFile);

            return pathToFile;
        }

        private Status checkStatusFile(FormType typDok)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + typDok.ToString());
            if (!directory.Exists)
            {
                return null;
            }
            string pathToFile = directory + "\\status_" + typDok.ToString() + ".xml";
            FileInfo statusFile = new FileInfo(pathToFile);
            if (statusFile.Exists)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Status));
                FileStream fs = new FileStream(statusFile.FullName, FileMode.Open);
                Status status = (Status)serializer.Deserialize(fs);

                fs.Close();
                return status;
            }
            return null;
        }


        private string SerializeObject(Object toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            Utf8StringWriter textWriter = new Utf8StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        private string saveUPO(Deklaracja deklaracja, string upoString, FormType dokType)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + dokType.ToString() + "\\" + "UPO");
            if (!directory.Exists)
            {
                directory.Create();
            }

            string pathToFile = directory + "\\" + deklaracja.clientId + "_" + dokType.ToString() + "_" + "UPO.xml";
            FileInfo fi = new FileInfo(pathToFile);
            System.IO.File.WriteAllText(fi.FullName, upoString);

            return pathToFile;
        }

        private string saveStatus(Status status, FormType dokType)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + dokType.ToString());
            if (!directory.Exists)
            {
                directory.Create();
            }

            string pathToFile = directory + "\\status_" + dokType.ToString() + ".xml";
            FileInfo fi = new FileInfo(pathToFile);
            System.IO.File.WriteAllText(fi.FullName, SerializeObject(status));

            return pathToFile;
        }

        private void saveReport(Status status, FormType dokType, DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                Console.WriteLine("Nie udało sie zapisać raportu - katalog " + directory.FullName + " nie istnieje.");
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(directory.FullName + "\\" + dokType.ToString() + "_upo_status.csv"))
            {
                foreach (Deklaracja deklaracja in status.FileStatus)
                {
                    string line = deklaracja.clientId + "," + deklaracja.status;
                    file.WriteLine(line);
                }
            }
        }
    }
}
