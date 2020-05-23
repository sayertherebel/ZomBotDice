using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZomBotDice.Models
{
    public class CardIdMappingHandler
    {
        public Tuple<string, string>[] maps;
        public int cursor;
        public CardIdMappingHandler()
        {
            maps = new Tuple<string, string>[10];
            cursor = 0;
            for(int i = 0; i < 10; i++)
            {
                maps[i] = new Tuple<string, string>("", "");
            }
        }

        public void AddIdMapping(string map, string value)
        {
            maps[cursor] = new Tuple<string, string>(map, value);
            cursor = cursor == 9 ? 0 : cursor+1;
        }

        public bool HasIdMapping(string queryMap, out string value)
        {
            try
            {
                value = maps.Single(map => map.Item1.Equals(queryMap)).Item2;
                return true;
            }
            catch {
                value = null;
                return false;
            }
        }
    }
}
