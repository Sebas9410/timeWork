using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace timeWork.Functions.Entities
{
    public class timeEntity: TableEntity
    {
        public string id { get; set; }

        public DateTime CreatedTime { get; set; }

        public int tipo { get; set; }

        public bool consolidado { get; set; }
    }
}
