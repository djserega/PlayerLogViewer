using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLogViewer.Models
{
    public class RowLowRowsData : BindableBase
    {
        public RowLowRowsData(string header)
        {
            Header = header;

            if (Header.StartsWith("Request"))
            {
                int posBracket = Header.IndexOf('[');
                int posBracket2 = Header.IndexOf('{');

                int pos = 0;
                if (posBracket > 0 && posBracket2 > 0)
                    pos = Math.Min(posBracket, posBracket2);
                else
                {
                    if (posBracket > 0)
                        pos = posBracket;
                    else if (posBracket2 > 0)
                        pos = posBracket2;
                }

                if (pos > 0)
                {
                    Data = Header.Substring(pos);
                    Header = Header.Substring(0, pos);
                }
            }
        }

        public string Header { get; set; }
        public string Data { get; set; }
    }
}
