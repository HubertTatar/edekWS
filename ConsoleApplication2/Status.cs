using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    public class Status
    {
        private List<Deklaracja> fileStatus = new List<Deklaracja>();

        public List<Deklaracja> FileStatus
        {
            get { return fileStatus; }
            set { fileStatus = value; }
        }

        public void addDek(Deklaracja deklaracja)
        {
            fileStatus.Add(deklaracja);
        }

    }
}
