using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ChronoCalc.Helpers
{
    class ImpEx
    {
        /// <summary>
        /// Imports data, also kinda srubs the data, (it won't take anything that won't parse as an int)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static List<string> ImportData(string filename)
        {
            List<string> csvFileItems = new List<string>();

            if (!File.Exists(filename))
                throw new Exception("Invalid or unknown file.");

            string fileData = File.ReadAllText(filename);

            // make sure we have (somewhat) clean data
            foreach (string d in fileData.Split(','))
            {
                int buffer;

                if (int.TryParse(d, out buffer))
                    csvFileItems.Add(d);
            }

            return csvFileItems;
        }

        public static bool ExportData(string filename, DataGridView dataGrid)
        {
            if (dataGrid.RowCount < 2)
                return false;

            string csvData = "";

            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                int buff;

                if (row.Cells[0].Value != null && int.TryParse(row.Cells[0].Value.ToString(), out buff))
                    csvData += string.Format("{0},", row.Cells[0].Value);
            }
                

            // trim last ,
            File.WriteAllText(filename, csvData.TrimEnd(','));

            MessageBox.Show("All data was successfully exported.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return true;
        }
    }
}
