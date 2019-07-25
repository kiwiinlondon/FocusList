using log4net;
using Odey.Framework.Infrastructure.TaskManagement;
using Odey.Framework.Keeley.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.FocusList.FocusListService
{
    public class TaskRunnerFactory : ITaskRunnerFactory
    {
        public TaskTypeIds TaskTypeId => TaskTypeIds.FocusListService;

        public TaskRunner GetTaskRunner(int taskId, ILog logger)
        {
            switch ((TaskIds)taskId)
            {
                case TaskIds.RollFocusList:
                    return new TaskRunnerRollFocusList(logger);

                default:
                    throw new ApplicationException($"Unknown Task {taskId} on {TaskTypeId}");
            }
        }
    }
}