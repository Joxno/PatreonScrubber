using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Xml;

namespace PatreonScrubber
{
    class Scubber
    {
        public List<Project> m_Projects = new List<Project>();

        public void Scrub()
        {

            m_Projects.Add(new Project("https://www.patreon.com/LewdWarfare"));


            //ToXML("Data_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml");

            ToGraph("GraphData_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".dot");

            
        }

        public void ScrubFile(string Filename)
        {
            using (var t_Reader = new StreamReader(Filename))
            {
                string t_Line = "";
                while ((t_Line = t_Reader.ReadLine()) != null)
                {
                    m_Projects.Add(new Project(t_Line));
                }
            }

            if (m_Projects.Count > 0)
            {
                ToXML("Data_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml");
                ToGraph("GraphData_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".dot");
            }
        }

        public void ToXML(string FilePath)
        {
            using (XmlWriter t_Writer = XmlWriter.Create(FilePath))
            {
                t_Writer.WriteStartDocument();
                    t_Writer.WriteStartElement("Projects");
                    foreach (var t_P in m_Projects)
                        t_P.ToXML(t_Writer);
                    t_Writer.WriteEndElement();
                t_Writer.WriteEndDocument();
            }
        }

        public void ToGraph(string FilePath)
        {
            string t_Dot = "graph Patreon {";
            foreach (var t_P in m_Projects)
                t_Dot = t_P.ToDOT(t_Dot);
            t_Dot += "}";

            File.WriteAllText(FilePath, t_Dot);
        }
    }
}
