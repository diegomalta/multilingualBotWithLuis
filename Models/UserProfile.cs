using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public DateTime CallbackTime { get; set; }

        public string PhoneNumber { get; set; }

        public string Bug { get; set; }
    }
}
