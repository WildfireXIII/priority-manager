using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

namespace PriorityManager
{
	public class PriorityManager
	{
		// things I want to be able to do:
		// - list all current priorities (in order of importance
		// - add priority
		// - update priority
		// - reset priority
		// - delete priority

		//private string FILE_NAME = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "./priorities.db";
		//private string FILE_NAME = "/site/wwwroot/priorities.db";

		// member variables
		private List<Priority> m_pPriorityList = new List<Priority>();

		public PriorityManager() { }

		// functions
		public XElement ListPriorities()
		{
			// get the list
			LoadList();

			// order the priorities by current priority
			Dictionary<Priority, int> pPriorities = m_pPriorityList.ToDictionary(pPriority => pPriority, pPriority => pPriority.CalculateCurrentPriority());
			var pOrdered = (from entry in pPriorities
							orderby entry.Value ascending
							select entry);

			XElement pRoot = new XElement("Priorities");
			foreach (KeyValuePair<Priority, int> pPriorityPair in pOrdered)
			{
				Priority pPriority = pPriorityPair.Key;
				int iCurrentValue = pPriorityPair.Value;

				// make xml priority element from information and add to root
				XElement pCurrent = new XElement("Priority");
				pCurrent.SetAttributeValue("ID", pPriority.ID);
				pCurrent.SetAttributeValue("Name", pPriority.Name);
				pCurrent.SetAttributeValue("Category", pPriority.Category);
				pCurrent.SetAttributeValue("Base", pPriority.Base);
				pCurrent.SetAttributeValue("Growth", pPriority.Growth);
				pCurrent.SetAttributeValue("DateReset", pPriority.DateReset);
				pCurrent.SetValue(iCurrentValue);
				pRoot.Add(pCurrent);
			}
			//return pRoot.ToString();
			return pRoot;
		}

		public string AddPriority(string sName, string sCategory, int iBase, int iGrowth)
		{
			LoadList();

			// find new id
			int iID = 0;
			if (m_pPriorityList.Count == 0) { iID = 1; }
			else { iID = m_pPriorityList[m_pPriorityList.Count - 1].ID + 1; }

			Priority pPriority = Priority.Create(iID, sName, sCategory, iBase, iGrowth);
			m_pPriorityList.Add(pPriority);

			SaveList();
			return "success!";
		}

		public void ResetPriority(int iID)
		{
			LoadList();
			Priority pPriority = FindPriorityByID(iID);
			if (pPriority != null) { pPriority.Reset(); SaveList(); }
		}

		public void UpdatePriority(int iID, string sName, string sCategory, int iBase, int iGrowth)
		{
			LoadList();
			Priority pPriority = FindPriorityByID(iID);
			pPriority.Name = sName;
			pPriority.Category = sCategory;
			pPriority.Base = iBase;
			pPriority.Growth = iGrowth;
			SaveList();
		}

		public void DeletePriority(int iID)
		{
			LoadList();
			Priority pPriority = FindPriorityByID(iID);
			m_pPriorityList.Remove(pPriority);
			SaveList();
		}

		private Priority FindPriorityByID(int iID)
		{
			foreach (Priority pPriority in m_pPriorityList)
			{
				if (pPriority.ID == iID)
				{
					return pPriority;
				}
			}
			return null;
		}


		private CloudBlockBlob GetBlob()
		{
			// parse the connection string and return a reference to the storage account.
			CloudStorageAccount pStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
			CloudBlobClient pBlobClient = pStorageAccount.CreateCloudBlobClient();

			CloudBlobContainer pContainer = pBlobClient.GetContainerReference("wsblobs");

			// get blob
			return pContainer.GetBlockBlobReference("priorities");
		}
		private void LoadList()
		{
			CloudBlockBlob pBlob = GetBlob();
			if (!pBlob.Exists()) { pBlob.UploadText(""); }
			string sLines = pBlob.DownloadText();
			string[] aLines = sLines.Split('\n');
			foreach (string sLine in aLines)
			{
				if (sLine == "") continue;
				Priority pPriority = Priority.Build(sLine);
				m_pPriorityList.Add(pPriority);
			}

			/*if (!File.Exists(FILE_NAME)) { File.Create(FILE_NAME); }
			string[] aLines = File.ReadAllLines(FILE_NAME);
			foreach (string sLine in aLines)
			{
				Priority pPriority = Priority.Build(sLine);
				m_pPriorityList.Add(pPriority);
			}*/
		}
		private void SaveList()
		{
			string sSave = "";
			foreach (Priority pPriority in m_pPriorityList) { sSave += pPriority.ToString() + "\n"; }
			sSave = sSave.TrimEnd('\n'); // get rid of trailing newline
			GetBlob().UploadText(sSave);
			//File.WriteAllText(FILE_NAME, sSave);
		}
	}
}

