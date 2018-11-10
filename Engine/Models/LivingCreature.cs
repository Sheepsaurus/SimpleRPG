using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.ViewModels;
using Engine;

namespace Engine.Models
{
    public class LivingCreature
    {
        public int HitPoints { get; set; }
        public int MaximumHitPoints { get; set; }

        public LivingCreature(int hitPoints, int maximumHitPoints)
        {
            HitPoints = hitPoints;
            MaximumHitPoints = maximumHitPoints;
        }        
    }
}
