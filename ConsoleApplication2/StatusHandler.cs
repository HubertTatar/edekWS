using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication2
{
    class StatusHandler
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(StatusHandler));

        public static Status checkStatusFile(FormType typDok, string extension, DirectoryInfo resultDir)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + typDok.ToString());
            Status status = null;
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
                Status oldstatus = (Status)serializer.Deserialize(fs);
                if (oldstatus.FileStatus.Count != 0)
                {
                    status = oldstatus;
                }

                fs.Close();
            }
            else
            {
                Status newStatus = new Status();
                Console.WriteLine("Szukam plikow z roszerzeniem xml w " + resultDir.FullName + "\\" + typDok.ToString());
                List<Deklaracja> listToSend = getXmlDeclarations(typDok, extension, resultDir);

                foreach (Deklaracja dek in listToSend)
                {
                    dek.message = "";
                    dek.refId = "";
                    dek.upoPath = "";
                }
                newStatus.FileStatus = listToSend;
                status = newStatus;
                StatusHandler.saveXML(newStatus, typDok, resultDir);
            }
            return status;
        }


        private static List<Deklaracja> getXmlDeclarations(FormType typDok, string ext, DirectoryInfo resultDir)
        {
            string[] filePaths = Directory.GetFiles(resultDir + "\\" + typDok.ToString(), "*." + ext);
            List<Deklaracja> files = new List<Deklaracja>();
            foreach (string filePath in filePaths)
            {
                Deklaracja deklaracja = new Deklaracja();
                string fileWithExt = Path.GetFileNameWithoutExtension(filePath);
                string[] stringSeparators = new string[] { "_" };
                string[] result = fileWithExt.Split(stringSeparators, StringSplitOptions.None);
                if (!FormType.PIT8C.Equals(typDok))
                {
                    deklaracja.clientId = result[0];
                }
                deklaracja.filePath = filePath;
                files.Add(deklaracja);
            }
            return files;
        }

        public static void saveReport(Status status, FormType dokType, DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                Console.WriteLine("Nie udało sie zapisać reportu - katalog " + directory.FullName + "nie istnieje.");
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(directory.FullName + "\\" + dokType.ToString() + "_wysylka_status.csv"))
            {
                foreach (Deklaracja deklaracja in status.FileStatus)
                {
                    string line = deklaracja.clientId + "," + deklaracja.status;
                    file.WriteLine(line);
                }
            }
        }

        public static string saveXML(object objToSerialization, FormType dokType, DirectoryInfo resultDir)
        {
            DirectoryInfo directory = new DirectoryInfo(resultDir + "\\" + dokType.ToString());
            if (!directory.Exists)
            {
                directory.Create();
            }

            string pathToFile = directory + "\\status_" + dokType.ToString() + ".xml";
            FileInfo fi = new FileInfo(pathToFile);
            System.IO.File.WriteAllText(fi.FullName, SerializeObject(objToSerialization));

            return pathToFile;
        }

        private static string SerializeObject(Object toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            Utf8StringWriter textWriter = new Utf8StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static bool checkIfUPOConfirmed(string upoPath)
        {
            bool confirmed = false;
            string[] lines = System.IO.File.ReadAllLines(upoPath);
            List<string> linesList = new List<string>(lines);
            string pattern = @"\s+<Przyjeto>(.*?)</Przyjeto>";
            foreach (string str in linesList)
            {
                Match match = Regex.Match(str, pattern);
                if (match.Success)
                {
                    try
                    {
                        confirmed = Convert.ToBoolean(match.Groups[1].ToString());
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Nie udało się sprawdzić UPO dla pliku " + upoPath + " \n Proszę sprawdzić ręcznie czy dokument został przyjęty.");
                        confirmed = false;
                    }
                }
            }
            return confirmed;
        }
    }
}
