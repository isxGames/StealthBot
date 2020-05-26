using System.Collections.Generic;

namespace StealthBot.Core
{
    public class ActionSet
    {
        public int Level { get; private set; }

        public List<Action> Actions = new List<Action>();

        public ActionSet(int level)
        {
            Level = level;
        }
    }
}
