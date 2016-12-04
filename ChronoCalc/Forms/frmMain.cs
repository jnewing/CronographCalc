using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChronoCalc.Forms
{
    public partial class frmMain : Form
    {
        private double avgVelocity = 0;

        private double avgDeviation = 0;


        public frmMain()
        {
            InitializeComponent();
        }

        #region Calculations

        /// <summary>
        /// Calculates the avg velocity.
        /// </summary>
        private void calcAvgVelocity()
        {
            if (dataGridView1.RowCount < 2)
                return;

            // zero it
            avgVelocity = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value == null
                    || string.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                    continue;

                int rowVal = int.Parse(row.Cells[0].Value.ToString());
                avgVelocity += rowVal;
            }

            // calc avg
            avgVelocity = avgVelocity / (dataGridView1.RowCount - 1);

            // display
            lblAvgVelocity.Text = string.Format("{0} fps", Math.Round(avgVelocity, 2));
        }

        /// <summary>
        /// Calculate the MAD (mean avg. deviation)
        /// </summary>
        private void calcAvgDeviation()
        {
            if (dataGridView1.RowCount < 2)
                return;

            avgDeviation = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value == null || string.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                    continue;

                int rowVal = int.Parse(row.Cells[0].Value.ToString());
                int diff = rowVal - (int)avgVelocity;

                if (diff < 0)
                    diff = ~diff + 1;

                avgDeviation += diff;
            }

            // calc avg deviation
            avgDeviation = avgDeviation / (dataGridView1.RowCount - 1);

            // display
            lblDeviation.Text = string.Format("+/- {0} fps", Math.Round(avgDeviation, 2));
        }


        #endregion

        #region Data Grid Events

        /// <summary>
        /// When a user enters data into a cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            calcAvgVelocity();
            calcAvgDeviation();

            lblRoundCount.Text = string.Format("{0}", dataGridView1.RowCount - 1);
        }

        /// <summary>
        /// When a user removes a row with a velocity reading.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (dataGridView1.RowCount < 2)
                resetAll();

            calcAvgVelocity();
            calcAvgDeviation();

            lblRoundCount.Text = string.Format("{0}", dataGridView1.RowCount - 1);
        }

        /// <summary>
        /// When a user manually enters data into a cell, we check to make sure its numeric.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);

            if (dataGridView1.CurrentCell.ColumnIndex == 0)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
            }
        }

        /// <summary>
        /// Numeric check function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Column1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Displays the round number in the row header.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        #endregion

        #region Misc.

        /// <summary>
        /// Resets all data.
        /// </summary>
        private void resetAll()
        {
            avgVelocity = 0;
            avgDeviation = 0;

            lblAvgVelocity.Text = lblDeviation.Text = lblRoundCount.Text = "0";
        }

        /// <summary>
        /// Does all the calculations.
        /// </summary>
        private void reCalculate()
        {
            calcAvgVelocity();
            calcAvgDeviation();

            lblRoundCount.Text = string.Format("{0}", dataGridView1.RowCount - 1);
        }

        #endregion

        #region Menubar Functions

        /// <summary>
        /// Exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Import data from CSV file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofDlg = new OpenFileDialog())
            {
                ofDlg.Filter = "CSV files (*.csv)|*.csv";
                ofDlg.Title = "Select a CSV file to import from.";

                if (ofDlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        List<string> importData = Helpers.ImpEx.ImportData(ofDlg.FileName);

                        foreach (string i in importData)
                            dataGridView1.Rows.Add(i);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        reCalculate();
                    }
                }
            }
        }

        /// <summary>
        /// Export data to CSV file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfDlg = new SaveFileDialog())
            {
                sfDlg.Filter = sfDlg.Filter = "CSV files (*.csv)|*.csv";
                sfDlg.Title = "Select a CSV file to import from.";

                if (sfDlg.ShowDialog() == DialogResult.OK)
                {
                    Helpers.ImpEx.ExportData(sfDlg.FileName, dataGridView1);
                }
            }
        }

        /// <summary>
        /// Clear all data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to clear all current data?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();

                resetAll();
            }            
        }

        /// <summary>
        /// About this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAbout().Show();
        }

        #endregion

        
    }
}
