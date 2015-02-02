using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ExpireAlert
{
    public partial class Gsp_shouying_qyshb
    {
        public bool IsExpired { get; set; }
        public bool IsAlarmed { get; set; }

        public string Name { get { return String.Format("[{0}] {1} ({2})", this.xuhao, this.mingcheng, this.daima); } }
        public string Contact { get { return String.Format("{0} (TEL:{1})", this.dizhi, this.dianhua); } }
        public Brush Color
        {
            get
            {
                if (this.IsExpired) return Brushes.LightPink;
                else if (this.IsAlarmed) return Brushes.LightGoldenrodYellow;
                else return Brushes.LightBlue;
            } 
        }
    }
}
