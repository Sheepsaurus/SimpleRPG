using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRPG
{
    public class Location
    {
        public Location(int id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
        }

        public Location(int iD, string name, string description, Item itemRequiredToEnter = null, Quest questAvailableHere = null, Monster monsterLivingHere = null)
        {
            ItemRequiredToEnter = itemRequiredToEnter;
            QuestAvailableHere = questAvailableHere;
            MonsterLivingHere = monsterLivingHere;
            ID = iD;
            Name = name;
            Description = description;
        }

        public Item ItemRequiredToEnter { get; set; }
        public Quest QuestAvailableHere { get; set; }
        public Monster MonsterLivingHere { get; set; }
        public Location LocationToNorth { get; set; }
        public Location LocationToEast { get; set; }
        public Location LocationToSouth { get; set; }
        public Location LocationToWest { get; set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }       
    }
}
