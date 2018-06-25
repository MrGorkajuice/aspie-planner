using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspie_Planner
{
    public class TaskNote
    {
        private DateTime noteDate;
        private string note;

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
