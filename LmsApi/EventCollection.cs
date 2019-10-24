
using System;
using System.Collections.Generic;
using System.Text;

namespace LmsApi
{
    public class EventCollection
    {
        internal EventCollection(List<Event> recent, List<Event> future)
        {
            Recent = recent;
            Future = future;
        }
        public List<Event> Recent { get; }
        public List<Event> Future { get; }


        public void Deconstruct(out List<Event> recent, out List<Event> upcoming)
        {
            recent = Recent;
            upcoming = Future;
        }
    }
}
