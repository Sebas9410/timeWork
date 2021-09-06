using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace timeWork.Functions.Entities
{
    class timeEntity: TableEntity
    {
        public int id { get; set; }

        public DateTime CreatedTime { get; set; }

        public int tipo { get; set; }

        public bool consolidado { get; set; }
    }
}
