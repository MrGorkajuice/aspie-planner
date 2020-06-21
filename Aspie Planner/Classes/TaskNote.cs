using System;

namespace Aspie_Planner
{
    public class TaskNote
    {
        public DateTime noteDate { get; set; }
        public string note { get; set; }

        public TaskNote(DateTime noteDate, string note)
        {
            this.note = note;
            this.noteDate = noteDate;
        }

        public string GetNote()
        {
            return note;
        }

        public DateTime GetDate()
        {
            return this.noteDate;
        }

        public void UpdateNote(string newNote)
        {
            this.note = newNote;
        }
    }
}
