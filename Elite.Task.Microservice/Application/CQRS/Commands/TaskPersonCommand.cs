using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class TaskPersonCommand
    {
		public TaskPersonCommand(string uid, string displayName)
		{
			Uid = uid;
			DisplayName = displayName;
		}

		public string Uid { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
    }
}
