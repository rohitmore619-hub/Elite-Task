using Elite_Task.Microservice.Core;
using System;
using System.Collections.Generic;

namespace Elite_Task.Microservice.Models.Entities
{
    public class FollowUpUpdateDto
    {
        public int TaskId { get; set; }
        public bool FollowUp { get; set; }
    }
}