using AutoPadInu_UWP.Model;
using AutoPadInu_UWP.Model.Datatype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPadInu_UWP.Model.PanelTools.Voting
{
    class PanelVoting
    {
        public BeadVoting[,] BeadVotings;

        public PanelVoting()
        {
            BeadVotings = new BeadVoting[PadPanel.Width, PadPanel.Height];

            for (int x = 0; x < PadPanel.Width; x++)
            {
                for (int y = 0; y < PadPanel.Height; y++)
                    BeadVotings[x, y] = new BeadVoting();
            }
        }

        public void Add(PadPanel panel)
        {
            for (int x = 0; x < PadPanel.Width; x++)
            {
                for (int y = 0; y < PadPanel.Height; y++)
                    BeadVotings[x, y].Add(panel.beads[x, y]);
            }
        }

        public void Clear()
        {
            foreach (var item in BeadVotings)
                item.Clear();
        }

        public PadPanel GetPanel()
        {
            PadPanel p = new PadPanel();

            for (int x = 0; x < PadPanel.Width; x++)
            {
                for (int y = 0; y < PadPanel.Height; y++)
                    p.beads[x, y] = BeadVotings[x, y].GetBead();
            }

            return p;
        }
    }
}
