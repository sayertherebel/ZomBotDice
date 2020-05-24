using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZomBotDice.Models
{
    public enum DiceColour
    {
        Red,
        Yellow,
        Green

    }
    public enum DiceSide
    {
        Shotgun,
        Runner,
        Brain
    }
    public abstract class ZombieDice
    {
        public DiceColour colour;
        public int numberOfShotguns;
        public int numberOfFootsteps;
        public int numberOfBrains;
        public DiceSide rolledState;

        public void Roll()
        {
            Random random = new Random();
            int result =  random.Next(1, 7); // This looks unintuitive, but the upper bound is exclusive, so for a random between 1 and 6, we specifiy 1 and 7

            switch(result)
            {
                case int n when (n <= numberOfBrains):
                    this.rolledState = DiceSide.Brain;
                    break;
                case int n when (n <= numberOfBrains + numberOfFootsteps):
                    this.rolledState = DiceSide.Runner;
                    break;
                default:
                    this.rolledState = DiceSide.Shotgun;
                    break;

            }

        }

    }
    public class RedZombieDice : ZombieDice
    {
        public RedZombieDice()
            :base () { numberOfBrains = 1; numberOfFootsteps = 2; numberOfShotguns = 3; colour = DiceColour.Red; }
    }
    public class YellowZombieDice : ZombieDice
    {
        public YellowZombieDice()
            : base() { numberOfBrains = 2; numberOfFootsteps = 2; numberOfShotguns = 2; colour = DiceColour.Yellow; }
    }
    public class GreenZombieDice : ZombieDice
    {
        public GreenZombieDice()
            : base() { numberOfBrains = 3; numberOfFootsteps = 2; numberOfShotguns = 1; colour = DiceColour.Green; }
    }

    public class ZombieRound
    {
        List<ZombieDice> diceInTub;
        List<ZombieDice> playedDice;
        ZombieDice[] diceInHand;
        public int brainsWon { get; set; }
        int shotgunsWon;
        public bool brainDiceRecycled { get; set; }

        public ZombieRound()
        {
            diceInTub = new List<ZombieDice>();
            diceInTub.AddRange(new List<ZombieDice> { new RedZombieDice(), new RedZombieDice(), new RedZombieDice() });
            diceInTub.AddRange(new List<ZombieDice> { new YellowZombieDice(), new YellowZombieDice(), new YellowZombieDice(), new YellowZombieDice() });
            diceInTub.AddRange(new List<ZombieDice> { new GreenZombieDice(), new GreenZombieDice(), new GreenZombieDice(), new GreenZombieDice(), new GreenZombieDice(), new GreenZombieDice() });
            diceInHand = new ZombieDice[3];
            brainsWon = 0;
            shotgunsWon = 0;
            playedDice = new List<ZombieDice>();
        }

        private ZombieDice TakeOne()
        {
            if(diceInTub.Count == 0)
            {
                //Recycle ze brains

                foreach (ZombieDice dice in diceInHand.Where(x=>x.rolledState == DiceSide.Brain)) // Take Rolled Brains
                {
                    diceInTub.Add(dice); // Put them back in the tub
                }
            }

            Random random = new Random();
            int randomIndex = random.Next(1, diceInTub.Count+1);
            ZombieDice selected = diceInTub[randomIndex-1];
            diceInTub.RemoveAt(randomIndex-1);
            return selected;
        }

        public RoundResult Roll()
        {
            RoundResult result = new RoundResult();

            for (int i = 0; i < 3; i++ )
            {
                if(diceInHand[i] == null) { diceInHand[i] = TakeOne(); }
                diceInHand[i].Roll();
                result.dice[i] = diceInHand[i];
                if(diceInHand[i].rolledState != DiceSide.Runner)
                {
                    switch(diceInHand[i].rolledState)
                    {
                        case (DiceSide.Brain):
                            brainsWon++;
                            break;
                        case (DiceSide.Shotgun):
                            shotgunsWon++;
                            if(shotgunsWon>=3)
                            {
                                result.isDead = true;
                            }
                            break;
                    }
                    playedDice.Add(diceInHand[i]);
                    diceInHand[i] = null;
                }
            }

            result.shotguns = shotgunsWon;

            //if(result.isDead)
            //{
            //    result.brainsAdded = 0;
            //}
            //else
            //{
                result.brainsAdded = brainsWon;
            //}
            return result;
        }

    }

    public class RoundResult
    {
        public ZombieDice[] dice { get; set; }
        public bool isDead{ get; set; }
        public int brainsAdded{ get; set; }
        public int shotguns { get; set; }

        public string playerdisplayname { get; set; }
        public RoundResult()
        {
            dice = new ZombieDice[3];
        }

        public int brainsBanked { get; set; } 

    }
}
