using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspie_Planner.Classes
{
    class WeekPlannerSettingContainer
    {
        public enum PlannerResolution
        {
            Quarterly,
            HalfHourly,
            Hourly
        }
        public List<KeyValuePair<DayOfWeek, TimeSpan>> PreferenceDayStart { get; private set; }
        public List<KeyValuePair<DayOfWeek, TimeSpan>> PreferenceDayEnd { get; private set; }
        public TimeSpan GlobalCooldown { get; private set; }
        public PlannerResolution ActivePlannerResolution { get; private set; }

        public WeekPlannerSettingContainer()
        {

        }

        private void RestoreDefault()
        {
            ActivePlannerResolution = PlannerResolution.Quarterly;

        }
    }
}
