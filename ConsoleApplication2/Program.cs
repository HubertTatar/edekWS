using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication2
{
    class Program
    {


        static void Main(string[] args)
        {

            if (args.Length == 0 || args.Length > 3)
            {
                Console.WriteLine("Zła liczba parametrów");
                Console.WriteLine("Powinnieneś odpalić ConsoleApplication2.exe {katalog} {dokument} {operacja} - bez nawiasow");
                Console.WriteLine(" katalog - sciezka do katalogu zawierajacego wygenerowane i podpisane pliki xml");
                Console.WriteLine(" dokument - 1_3 dla IFT1 lub IFT3, 2 dla IFT2");
                Console.WriteLine(" operajca - wyslij, upo");
            }
            DirectoryInfo info = new DirectoryInfo(args[0]);
            if (!info.Exists)
            {
                Console.WriteLine("Nie znalazłem katalogu: " + args[0]);
                return;
            }


            RunMode runMode = RunMode.NONE;
            if ("1_3".Equals(args[1])) 
            {
                runMode = RunMode.IFT1_3;
            }
            else if ("2".Equals(args[1]))
            {
                runMode = RunMode.IFT2;
            }
            else
            {
                Console.WriteLine("Nie rozpoznany typ dokumentu " + args[1]);
                Console.WriteLine(" dokument - 1_3 dla IFT1 lub IFT3, 2 dla IFT2");
                return;
            }

            if ("wyslij".Equals(args[2])) 
            {
                WorkerSend send = new WorkerSend(info, runMode);
                send.doJob();
            }
            else if ("upo".Equals(args[2])) 
            {
                WorkerGetUPO upo = new WorkerGetUPO(info, runMode);
                upo.doJob();
            }
            else
            {
                Console.WriteLine("Nie rozpoznany typ operacji " + args[2]);
                Console.WriteLine(" operajca - wyslij, upo");
                return;
            }

 
            

            //Program p = new Program();
            //p.Invoke("sendDocument", false);
        }

 
    }
}
