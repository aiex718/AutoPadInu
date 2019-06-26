using AutoPadInu_UWP.Model.Datatype;
using AutoPadInu_UWP.Model.PanelTools.Voting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace AutoPadInu_UWP.Model.PanelTools
{
    class ParsedImgs:IDisposable
    {
        Dictionary<Beads, SoftwareBitmap> ResultImgs = new Dictionary<Beads, SoftwareBitmap>();

        public SoftwareBitmap SelectResult(Beads BeadType)
        {
            if (ResultImgs.TryGetValue(BeadType, out var img))
                return img;

            return null;
        }

        public void AddResult(Beads BeadType,SoftwareBitmap img)
        {
            ResultImgs.Add(BeadType,img);
        }

        public void Dispose()
        {
            foreach (var item in ResultImgs.Values)
                item?.Dispose();

            ResultImgs.Clear();
        }
    }

    class Parser
    {
        PanelVoting panelVoter;

        public bool Enable { get; set; } = false;

        public Parser()
        {
            panelVoter = new PanelVoting();
        }

        public void Clear()
        {
            panelVoter.Clear();
        }

        public ParsedImgs ParsePanelImage(SoftwareBitmap SourceBitmap)
        {
            if (Enable)
            {
                ParsedImgs parsedImgs = new ParsedImgs();
                List<PadPanel> AnalyzedPanels = new List<PadPanel>();
                //Parallel.ForEach(Filter.AllFilter, (filter) => 
                foreach (var filter in Filter.AllFilter)
                {
                    PadPanel p = filter.FiltePanel(SourceBitmap, out SoftwareBitmap filterBitmap);

                    AnalyzedPanels.Add(p);
                    parsedImgs.AddResult(filter.BeadType, filterBitmap);
                }
                //);

                lock (panelVoter)
                {
                    foreach (var p in AnalyzedPanels)
                        panelVoter.Add(p);
                }

                return parsedImgs;
            }

            return null;
        }

        public PadPanel GetParsedPanel()
        {
            return panelVoter?.GetPanel();
        }


    }
}
