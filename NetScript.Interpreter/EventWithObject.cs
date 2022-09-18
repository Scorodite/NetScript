using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace NetScript.Interpreter
{
    public class EventWithObject
    {
        public object Obj { get; }
        public EventInfo Event { get; }

        public EventWithObject(object obj, EventInfo ev)
        {
            Obj = obj;
            Event = ev;
        }

        public object Invoke(object[] args)
        {
            return Event.RaiseMethod?.Invoke(Obj, args);
        }

        public void Remove(Delegate deleg)
        {
            Event.RemoveMethod.Invoke(Obj, new object[] { deleg });
        }
    }
}
