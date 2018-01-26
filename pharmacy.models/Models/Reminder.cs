﻿using System;
using System.Collections.Generic;

namespace Pharmacy.Models
{
    public partial class Reminder
    {
        public Reminder()
        {
            ReminderOrder = new HashSet<ReminderOrder>();
        }

        public Guid ReminderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime SendTime { get; set; }
        public bool? Sent { get; set; }
        public DateTime CreatedOn { get; set; }

        public ICollection<ReminderOrder> ReminderOrder { get; set; }
    }
}
