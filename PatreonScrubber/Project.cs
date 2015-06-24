using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PatreonScrubber
{
    class Project
    {
        public string m_Name = "";
        public float m_Earnings = 0;
        public float m_SurplusEarnings = 0;

        public int m_PatronCount = 0;
        public List<Patron> m_Patrons = new List<Patron>();
        public List<Patron> m_TopPatrons = new List<Patron>();

        public int m_PledgeCount = 0;
        public List<Pledge> m_Pledges = new List<Pledge>();

        public int m_ProjectID = 0;
        public string m_ProjectURL;

        public Project(string ProjURL)
        {
            ScrubProject(ProjURL);
        }

        public bool isProject(HtmlDocument Doc)
        {
            bool t_isProject = false;

            var t_List = Doc.DocumentNode.Descendants("h2").Where(x => x.Id == "creatorTitle");

            if (t_List.Count() > 0)
                t_isProject = true;

            return t_isProject;
        }

        public void ScrubProject(string ProjURL)
        {
            HtmlWeb t_Web = new HtmlWeb();
            var t_Doc = t_Web.Load(ProjURL);

            Console.WriteLine("Scrubbing Project Details");

            m_ProjectURL = ProjURL;
            var t_NameStr = t_Doc.DocumentNode.Descendants("h2").Where(x => x.Id == "creatorTitle").First().InnerText;
            m_Name = t_NameStr.Remove(t_NameStr.IndexOf("is creating")).Trim();

            m_PatronCount = int.Parse(t_Doc.DocumentNode.Descendants("div").Where(x => x.Id == "totalPatrons").First().InnerText);

            m_Earnings = float.Parse(t_Doc.DocumentNode.Descendants("span").Where(x =>
                    x.Id == "totalEarnings"
                    && x.Attributes.Contains("class")
                    && x.Attributes["class"].Value.Split().Contains("unmet_goal_amount") == false
                ).First().InnerText, CultureInfo.InvariantCulture);

            Console.WriteLine("Project Name: " + m_Name);
            Console.WriteLine("Project URL: " + m_ProjectURL);
            Console.WriteLine("Project Patrons: " + m_PatronCount);
            Console.WriteLine("Project Earnings: " + m_Earnings);


            Console.WriteLine("Scrubbing Project Pledges");
            // Get Pledges
                ScrubPledges( t_Doc.DocumentNode.Descendants("div").Where(x => x.Id == "boxGrid").First() );

            Console.WriteLine("Scrubbing Project Patrons");
            // Get Patrons
                ScrubPatrons( t_Doc.DocumentNode.Descendants("nav").Where(x => x.Id == "page_nav").First() );
        }

        private void ScrubPatrons(HtmlNode Node)
        {
            // -- /userNext?p=2&ty=p&srt=2&u=503677 -- Get Pages of Patrons for a project

            // Parse Nav String
                var t_ProjectID = int.Parse( Node.ChildNodes.Where(x => x.Name == "a").First().Attributes["href"].Value.Split('&').Where(x => x.StartsWith("u=")).First().Split('=')[1] );
                m_ProjectID = t_ProjectID;

            // Get all Patrons
                HtmlWeb t_Web = new HtmlWeb();
                int t_Pages = 1;
                while (true)
                {
                    
                    // Construct URL
                        string t_URL = @"https://www.patreon.com/userNext?p=" + t_Pages + @"&ty=p&srt=2&u=" + t_ProjectID;
                    // Get Page
                        var t_Doc = t_Web.Load(t_URL);
                    // Parse Page
                        var t_PatronList = t_Doc.DocumentNode.ChildNodes;

                        if (t_PatronList.Count() > 0)
                        {
                            foreach (var t_Node in t_PatronList)
                            {
                                // Get Href Node
                                    var t_NodeHref = t_Node.Descendants("a").Where(x => x.Attributes.Contains("href") && x.Attributes.Contains("class") == false).First();
                                    string t_Name = t_NodeHref.InnerText.Trim();
                                    string t_PatronIDStr = "";
                                    int t_PatronID = -1;

                                    if (t_NodeHref.Attributes["href"].Value.IndexOf("=") >= 0) // Split
                                    {
                                        t_PatronID = int.Parse(t_NodeHref.Attributes["href"].Value.Split('=')[1]);
                                    }
                                    else
                                    { 
                                        t_PatronIDStr = t_NodeHref.Attributes["href"].Value;//.Split('/')[1];
                                    }

                                    

                                // Add Patron to Project
                                    Patron t_Pat = new Patron();
                                    t_Pat.m_Name = t_Name;
                                    t_Pat.m_ID = t_PatronID;
                                    t_Pat.m_CustomID = t_PatronIDStr;
                                    t_Pat.m_URL = t_NodeHref.Attributes["href"].Value;
                                   // t_Pat.m_IsCreator = isProject(t_Web.Load(t_Pat.m_URL));

                                    //Console.WriteLine("Patron Processed: " + t_Pat.m_Name);
                                    m_Patrons.Add(t_Pat);
                            }
                        }
                        else
                            break;

                    Console.WriteLine("Scrubbing Patron Page: " + t_Pages);
                    // Increment Page Index
                        t_Pages++;
                }
        }

        private void ScrubPledges(HtmlNode Node)
        {
            var t_Pledges = Node.ChildNodes.Where(x => x.Name == "div");

            foreach (var t_N in t_Pledges)
                ScrubPledge(t_N);
        }

        private void ScrubPledge(HtmlNode Node)
        {
            // Get Pledge Amount
                // Get Href
                    var t_A = Node.ChildNodes.Where(x => x.Name == "a").First().Attributes["href"].Value;
                // Parse Href
                    float t_PledgeAmount = float.Parse(t_A.Split('=').Where(x => x.Contains('#')).First().Split('#')[0], CultureInfo.InvariantCulture);
            // Get Patron Amount
                    var t_PatronCount = 0;
                    var t_PatList = Node.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.StartsWith("needIcon") && x.Attributes["class"].Value.EndsWith("needIcon"));
                    if(t_PatList.Count() > 0)
                    { 
                        var t_PatNode = Node.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.StartsWith("needIcon") && x.Attributes["class"].Value.EndsWith("needIcon")).First();
                        t_PatronCount = int.Parse( t_PatNode.InnerText.Remove(t_PatNode.InnerText.IndexOf("patron")).Trim() );
                    }
            // Add new Pledge to List
                var t_Pledge = new Pledge();

                t_Pledge.m_Earning = t_PledgeAmount*t_PatronCount;
                t_Pledge.m_PledgeAmount = t_PledgeAmount;
                t_Pledge.m_PatronsCount = t_PatronCount;
                t_Pledge.m_Name = "$" + t_PledgeAmount.ToString() + " per Month";

                Console.WriteLine("Pledge Name: " + t_Pledge.m_Name);
                Console.WriteLine("Pledge Amount: " + t_Pledge.m_PledgeAmount);
                Console.WriteLine("Pledge Patrons: " + t_Pledge.m_PatronsCount);
                Console.WriteLine("Pledge Earnings: " + t_Pledge.m_Earning);

                m_Pledges.Add(t_Pledge);
            // Update PledgeCount for Project
                m_PledgeCount = m_Pledges.Count;
        }


        public void ToXML(XmlWriter Writer)
        {
            // Write Project Details
                Writer.WriteStartElement("Project");
                    Writer.WriteElementString("Name",m_Name);
                    Writer.WriteElementString("URL",m_ProjectURL);
                    Writer.WriteElementString("ID",m_ProjectID.ToString());
                    Writer.WriteElementString("Earnings", m_Earnings.ToString("C", CultureInfo.CreateSpecificCulture("en-US")));
                    Writer.WriteElementString("SurplusEarnings", m_SurplusEarnings.ToString("C", CultureInfo.CreateSpecificCulture("en-US")));
                    Writer.WriteElementString("PatronCount", m_PatronCount.ToString());
                    Writer.WriteElementString("PledgeCount", m_PledgeCount.ToString());

            // Write Pledges
                foreach (var t_P in m_Pledges)
                {
                    Writer.WriteStartElement("Pledge");
                        Writer.WriteElementString("Name", t_P.m_Name);
                        Writer.WriteElementString("Amount", t_P.m_PledgeAmount.ToString("C", CultureInfo.CreateSpecificCulture("en-US")));
                        Writer.WriteElementString("Earnings", t_P.m_Earning.ToString("C", CultureInfo.CreateSpecificCulture("en-US")));
                        Writer.WriteElementString("PatronsCount", t_P.m_PatronsCount.ToString());
                    Writer.WriteEndElement();
                }

            // Write Patrons
                foreach (var t_P in m_Patrons)
                {
                    Writer.WriteStartElement("Patron");
                        Writer.WriteElementString("Name", t_P.m_Name);
                        Writer.WriteElementString("ID", t_P.m_ID.ToString());
                        Writer.WriteElementString("URL", t_P.m_URL);
                        Writer.WriteElementString("Creator", t_P.m_IsCreator.ToString());
                    Writer.WriteEndElement();
                }

            Writer.WriteEndElement();
        }

        public string ToDOT(string Str)
        {
            Str += m_ProjectID + " [label=\"" + m_Name + "\"];";
            Str += "Pledges" + m_ProjectID + " -- " + m_ProjectID + ";";
            Str += "Patreons" + m_ProjectID + " -- " + m_ProjectID + ";";

            foreach (var t_P in m_Pledges)
            {
                Str += t_P.GetHashCode() + " [label=\"" + t_P.m_Name + "\"];";
                Str += t_P.GetHashCode() + " -- Pledges" + m_ProjectID + ";";
            }

            foreach (var t_P in m_Patrons)
                Str += "\"" + t_P.m_Name + "\" -- Patreons" + m_ProjectID + ";";


            return Str;
        }
    }
}
