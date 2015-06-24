using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatreonScrubber
{
    class Patron
    {
        public string m_Name = "";
        public int m_ID = -1;
        public string m_CustomID = "";

        public string m_URL = "";
        public bool m_IsCreator = false;

        public int m_ProjectsBackedCount = 0;
        public List<Project> m_ProjectsBacked = new List<Project>();


    }
}
