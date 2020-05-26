using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace StealthBotIpc
{
    [ProtoContract()]
    public class Pair<T, U>
    {
        [ProtoMember(1)]
        public T First;
        [ProtoMember(2)]
        public U Second;

        public Pair(T first, U second)
        {
            First = first;
            Second = second;
        }

        public Pair()
        {

        }
    }

    [ProtoContract()]
    public class Triple<T, U, V>
    {
        [ProtoMember(1)]
        public T First;
        [ProtoMember(2)]
        public U Second;
        [ProtoMember(3)]
        public V Third;

        public Triple(T first, U second, V third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public Triple()
        {

        }
    }
    
    [ProtoContract()]
    public class Quintuple<T, U, V, W>
    {
        [ProtoMember(1)]
        public T First;
        [ProtoMember(2)]
        public U Second;
        [ProtoMember(3)]
        public V Third;
        [ProtoMember(4)]
        public W Fourth;

        public Quintuple(T first, U second, V third, W fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }

        public Quintuple()
        {

        }
    }
}
