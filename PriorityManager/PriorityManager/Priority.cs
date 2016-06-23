using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityManager
{
	public class Priority
	{
		// member variables
		private int m_iID = -1;
		private string m_sName = "";
		private string m_sCategory = "";

		private int m_iBase = 0;
		private int m_iGrowth = 0;

		private DateTime m_pDateReset = DateTime.MinValue;

		// construction
		public Priority(int iID, string sName, string sCategory, int iBase, int iGrowth, DateTime pDateReset)
		{
			this.ID = iID;
			this.Name = sName;
			this.Category = sCategory;
			this.Base = iBase;
			this.Growth = iGrowth;
			this.DateReset = pDateReset;
		}

		// properties
		public int ID { get { return m_iID; } set { m_iID = value; } }
		public string Name { get { return m_sName; } set { m_sName = value; } }
		public string Category { get { return m_sCategory; } set { m_sCategory = value; } }
		public int Base { get { return m_iBase; } set { m_iBase = value; } }
		public int Growth { get { return m_iGrowth; } set { m_iGrowth = value; } }
		public DateTime DateReset { get { return m_pDateReset; } set { m_pDateReset = value; } }

		// methods
		public int CalculateCurrentPriority()
		{
			// find number of days since last reset
			double dDayDifference = (DateTime.Now - this.DateReset).TotalDays;
			int iDayDifference = (int)dDayDifference;

			return this.Base + this.Growth * iDayDifference;
		}

		public void Reset()
		{
			this.DateReset = DateTime.Now;
		}

		public override string ToString() { return this.ID + "|" + this.Name + "|" + this.Category + "|" + this.Base + "|" + this.Growth + "|" + this.DateReset.ToString(); }

		// static methods
		public static Priority Build(string sBuildString)
		{
			List<string> pParts = sBuildString.Split('|').ToList();
			return new Priority(Convert.ToInt32(pParts[0]), pParts[1], pParts[2], Convert.ToInt32(pParts[3]), Convert.ToInt32(pParts[4]), DateTime.Parse(pParts[5]));
		}
		public static Priority Create(int iID, string sName, string sCategory = "default", int iBase = 50, int iGrowth = 0)
		{
			return new Priority(iID, sName, sCategory, iBase, iGrowth, DateTime.Now);
		}
	}
}

