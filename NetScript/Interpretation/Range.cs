using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Interpretation
{
    public class Range : IEnumerable<int>
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Step { get; set; }

        public Range(int end)
        {
            Start = 0;
            End = end;
            Step = 1;
        }

        public Range(int start, int end)
        {
            Start = start;
            End = end;
            Step = 1;
        }

        public Range(int start, int end, int step)
        {
            Start = start;
            End = end;
            Step = step;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new RangeEnumerator(Start, End, Step);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class RangeEnumerator : IEnumerator<int>
        {
            private readonly int Start;
            private readonly int End;
            private readonly int Step;
            private int Curr;

            public RangeEnumerator(int start, int end, int step)
            {
                Start = start;
                End = end;
                Step = step;
                Curr = start - (Step > 0 ? 1 : Step < 0 ? -1 : 0);
            }

            public int Current => Curr;

            object IEnumerator.Current => Curr;

            public void Dispose() { }

            public bool MoveNext()
            {
                Curr += Step;
                if (Step > 0 ? Curr >= End : Curr < End)
                {
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                Curr = Start;
            }
        }
    }
}
