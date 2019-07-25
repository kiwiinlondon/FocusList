using log4net;
using Odey.Framework.Infrastructure.TaskManagement;
using Odey.Framework.Keeley.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.FocusList.FocusListService
{
    public class TaskRunnerRollFocusList : TaskRunner
    {
        public TaskRunnerRollFocusList(ILog logger) : base(logger)
        {
        }

        protected override int TaskId => (int)TaskIds.RollFocusList;

        protected override void DoTask()
        {
            new Pricer().Reprice(Context);
        }
    }
}