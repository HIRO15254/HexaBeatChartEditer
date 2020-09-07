using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaBeatChartEditer.Core.Events
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class SplitLaneEvent : EventBase
    {

        [Newtonsoft.Json.JsonProperty]
        private bool[] lane = new bool[6]; 

        public bool[] Lane
        {
            get { return lane; }
            set { lane = value; }
        }

        public override string ToString() {
            int[] n = new int[6];
            for (int i = 0; i < 6; i++)
                n[i] = lane[i] == true ? 2 : 1; 
            return $"{n[0]}{n[1]}{n[2]}{n[3]}{n[4]}{n[5]}";
        }
    }
}
