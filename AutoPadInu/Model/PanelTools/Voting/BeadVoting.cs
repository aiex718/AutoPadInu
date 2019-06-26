using AutoPadInu_UWP.Model;
using AutoPadInu_UWP.Model.Datatype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPadInu_UWP.Model.PanelTools.Voting
{
    class BeadVoting : List<Beads>
    {
        const int MinimumLength = 6;
        const int MaximumLength = 12;
        const double Confidence = 0.6;
        Beads LastResult = Beads.Notset;

        public new void Add(Beads bead)
        {
            if (bead!= Beads.Notset && LastResult == Beads.Notset)
                base.Add(bead);

            if (this.Count> MaximumLength)
            {
                this.RemoveAt(0);
            }
        }

        public new void Clear()
        {
            base.Clear();
            LastResult = Beads.Notset;
        }

        public Beads GetBead()
        {
            if (LastResult!= Beads.Notset)
            {
                return LastResult;
            }

            if (this.Count >= MinimumLength)
            {
                var groups = this.GroupBy(x => x).OrderByDescending(g => g.Count()).First();
                if (groups.Count() >= this.Count * Confidence)
                {
                    LastResult = groups.Key;
                    return groups.Key;
                }
            }
            return Beads.Notset;
        }
    }
}
