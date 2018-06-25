using System;

namespace Aspie_Planner
{
    public class CalendarDateNote
    {
        private DateTime eventDate;
        private string eventNotes;

        public CalendarDateNote(DateTime newEventdate, string newEventNotes)
        {
            this.eventDate = newEventdate;
            this.eventNotes = newEventNotes;
        }

        public CalendarDateNote Clone()
        {
            CalendarDateNote result = new CalendarDateNote(this.eventDate, this.eventNotes);
            return result;
        }

        public string GetNote()
        {
            return this.eventNotes;
        }

        public DateTime GetDate()
        {
            return this.eventDate;
        }

        public void SetNote(string note)
        {
            this.eventNotes = note;
        }
    }
}