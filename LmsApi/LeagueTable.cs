using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ojb500.EcfLms
{
    [JsonObject]
    public class LeagueTable : ApiResult<LeagueTableEntry>, ILeagueTable
    {
        public string Name => Competition?.Name ?? Title;

        [JsonIgnore]
        internal Competition Competition { get; set; }

        public IEnumerator<LeaguePosition> GetEnumerator()
        {
            for (int i=0; i < Data.Length; i++)
            {
                yield return MakePosition(i);
            }
        }

        public ILeagueTable SummarizeFor(string clubPrefix, int maxRows = 5, string name = null)
        {
            var list = SummarizeForInternal(clubPrefix, maxRows).ToList();
            return new LeagueTableList(list, name ?? Name);
        }

        private IEnumerable<LeaguePosition> SummarizeForInternal(string clubPrefix, int maxRows = 5)
        {
            int total = Data.Length;
            if (total == 0)
            {
                yield break;
            }

            int club = 0;
            for (; club < total; club++)
            {
                if (Data[club].Team.Name.StartsWith(clubPrefix))
                {
                    break;
                }
            }

            if (club == total)
            {
                throw new InvalidOperationException("Club not found");
            }

            // If we are in top n, show top n.
            // If we are in bottom n-1, show top 1 + bottom n-1.
            // If we are not, show top 1 plus n / 2 either side.

            if (club < maxRows)
            {
                for (int i = 0; i < maxRows; i++)
                {
                    yield return MakePosition(i);
                }
            }
            else if (club > (total - maxRows))
            {
                yield return MakePosition(0);
                for (int i = total - maxRows + 1; i < total; i++)
                {
                    yield return MakePosition(i);
                }
            }
            else
            {
                yield return MakePosition(0);
                var window = maxRows / 2;
                for (int i = club - window; i < club + window; i++)
                {
                    yield return MakePosition(i);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private LeaguePosition MakePosition(int n) => new LeaguePosition(n + 1, Data[n]);
    }

}
