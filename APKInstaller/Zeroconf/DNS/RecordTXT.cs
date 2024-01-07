using System.Collections.Generic;
using System.Text;

#region TXT RDATA format
/*
3.3.14. TXT RDATA format

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                   TXT-DATA                    /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

TXT-DATA        One or more <character-string>s.

TXT RRs are used to hold descriptive text.  The semantics of the text
depends on the domain where it is found.
 * 
*/
#endregion

namespace Zeroconf.DNS
{
    internal class RecordTXT : Record
    {
        public List<string> TXT;

        public RecordTXT(RecordReader rr, int Length)
        {
            int pos = rr.Position;
            TXT = [];
            while (
                ((rr.Position - pos) < Length) &&
                (rr.Position < rr.Length)
                )
            {
                TXT.Add(rr.ReadString());
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (string txt in TXT)
            {
                sb.AppendFormat("\"{0}\" ", txt);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
