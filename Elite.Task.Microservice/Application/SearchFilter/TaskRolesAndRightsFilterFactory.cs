using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.SearchFilter;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.SearchFilter
{
    public class TaskRolesAndRightsFilterFactory
    {
        public static ITaskRolesAndRightFilterBuilder<EliteTask> GetTaskRolesAndRightsFilterObject(FilterActionEnum topicFilterActionEnum, IList<UserRolesAndRights> userRolesAndRights, IRequestContext context)
        {
            ITaskRolesAndRightFilterBuilder<EliteTask> rolesBuilder = null;
            switch (topicFilterActionEnum)
            {
                case FilterActionEnum.Delete:
                    rolesBuilder = new TaskDeleteRolesAndRightFilterBuilder<EliteTask>(userRolesAndRights, context);
                    break;
                case FilterActionEnum.None:
                    rolesBuilder = new TaskUserRolesAndRightFilterBuilder<EliteTask>(userRolesAndRights, context, false);
                    break;
            }
            return rolesBuilder;

        }
    }
}
