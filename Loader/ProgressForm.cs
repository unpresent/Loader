using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Loader
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public string ProgressCaption
        {
            get
            {
                return mProgressPanel.Caption;
            }
            set
            {
                mProgressPanel.Caption = value;
                mProgressPanel.Invalidate();
            }
        }

        public string ProgressDescription
        {
            get
            {
                return mProgressPanel.Description;
            }
            set
            {
                mProgressPanel.Description = value;
                // mProgressPanel.Invalidate();
            }
        }


        public int ProgressValue
        {
            get
            {
                return mProgressBar.Value;
            }
            set
            {
                if ((value >= 0) && (value <= 100))
                {
                    mProgressBar.Value = value;
                    mProgressBar.Visible = true;
                    // mProgressBar.Invalidate();
                }
                else
                {
                    mProgressBar.Value = 0;
                    mProgressBar.Visible = false;
                }
            }
        }
    }
}
