using System; 
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

using Helper = Neo.SmartContract.Framework.Helper;

public class AlienFinder_Ch4 : SmartContract
{
    public delegate void deleAlienEvent(BigInteger id); 
    public static event deleAlienEvent AlienGenerated; 
    public static event deleAlienEvent AlienDeleted; 

    public static event Action<string, uint> Rewarded; 

    public static event Action<string, uint> Punished; 

    public static event Action<string, byte[]> EncounterTriggered; 

    public class Alien
    {        
        public uint Xna
        { get; set; }
        public string AlienName
        { get; set; }
        public BigInteger Id 
        { get; set; }
    }

    public static object Main(string method, params object[] args) 
    {
        if (Runtime.Trigger == TriggerType.Application) 
        {
            switch (method) 
            {
                case "generateAlien":
                    return GenerateAlien((string)args[0], (byte[])args[1]); 
                case "query":
                    return Query((byte[])args[0]); 
                case "delete": 
                    return Delete((byte[])args[0], (byte[])args[1]); 
                case "forward": 
                    return Forward((byte[])args[0]); 
            }
        }
        return false; 
    }

    public static bool GenerateAlien(string alienName, byte[] owner) 
    {
        // Check if the owner is the same as one who invoked contract
        if (!Runtime.CheckWitness(owner)) return false; 

        uint xna = FindXna(RandomNumber());
        Alien someAlien = new Alien
        {
            Xna = xna,
            AlienName = alienName,
            Id = updateCounter()
        }; 
        
        // add the object to storage
        StorageMap alienMap = Storage.CurrentContext.CreateMap(nameof(alienMap)); 
        alienMap.Put(someAlien.Id.ToByteArray(), Helper.Serialize(someAlien)); 
        AlienGenerated(someAlien.Id); 
        return true; 
    }

    private static ulong RandomNumber()
    {
        uint blockHeight = Blockchain.GetHeight(); 
        return Blockchain.GetHeader(blockHeight).ConsensusData; 
    }

    private static uint FindXna(ulong randomNumber)
    { 
        return (uint)(randomNumber % 100000000);
    }

    // Read the counter from storage if it exists. Otherwise, default to 0. 
    private static BigInteger getCounter() 
    {
        BigInteger counter = 0; 
        byte[] value = Storage.Get("alienCount"); 
        if (value.Length != 0) 
            counter = value.ToBigInteger();
        return counter; 
    }

    private static BigInteger updateCounter()
    {
        BigInteger counter = getCounter(); 
        counter++; 
        Storage.Put(Storage.CurrentContext, "alienCount", counter); 
        return counter; 
    }

    public static Alien Query(byte[] id)
    {
        // deserialise the alien object then return it
        StorageMap alienMap = Storage.CurrentContext.CreateMap(nameof(alienMap)); 
        var result = alienMap.Get(id); 
        if (result.Length == 0) return null; 
        return Helper.Deserialize(result) as Alien; 
    }

    public static bool Delete(byte[] owner, byte[] id)
    {
        if (owner.Length != 20)
            throw new InvalidOperationException("The parameter owner SHOULD be 20-byte addresses.");
        // Check if the owner is the same as one who invoked contract
        if (!Runtime.CheckWitness(owner)) return false; 

        StorageMap alienMap = Storage.CurrentContext.CreateMap(nameof(alienMap)); 
        alienMap.Delete(id); 
        AlienDeleted(id.ToBigInteger()); 
        return true; 
    }

    public static uint D6() 
    {
        return (uint) RandomNumber() % 6; 
    }

    public static uint D10() 
    {
        return (uint) RandomNumber() % 10; 
    }

    public static uint D100() 
    {
        return (uint) RandomNumber() % 100; 
    }

    // Randomly selecting an enemy alien
    public static BigInteger DN(BigInteger n) 
    {
        var result = RandomNumber() % n + 1; 
        return result; 
    }

    // Methods for getting the attributes of aliens
    public static int getStrength(Alien a) 
    {
        return (int) (a.Xna % 1000000 / 10000); // digits (3-4)
    }
    public static int getSpeed(Alien a)
    {
        return (int) (a.Xna % 10000 / 100); // digits (5-6)
    }
    public static int getWeight(Alien a) 
    {
        return (int) (a.Xna % 100); // digits (7-8)
    }

    // Increase a random attribute by 0-9
    private static Alien Reward(Alien a) 
    {
        uint attribute = D6() / 2; // Randomly determine which attribute to reward. 
        uint value = D10();     // Randomly determine the value of reward. 

        switch(attribute) 
        {
            case 0:
                a.Xna += value * 10000; 
                Rewarded("strength", value); 
                break; 
            case 1: 
                a.Xna += value * 100; 
                Rewarded("speed", value); 
                break; 
            case 2: 
                a.Xna += value; 
                Rewarded("weight", value); 
                break; 
            default: 
                break; 
        }
        return a; 
    }

    // Decrease a random attribute by 0-9
    private static Alien Punish(Alien a)
    {
        uint attribute = D6() / 2; 
        uint value = D10(); 

        switch(attribute)
        {
            case 0:
                a.Xna -= value * 10000; 
                Punished("strength", value); 
                break; 
            case 1: 
                a.Xna -= value * 100; 
                Punished("speed", value); 
                break; 
            case 2: 
                a.Xna -= value; 
                Punished("weight", value); 
                break; 
            default: 
                break; 
        }
        return a; 
    }

    // Determines the success of an action using a D100 and modifier
    private static Alien Check(Alien a, int modifier) {
        if (modifier + D100() > 100)
            return Reward(a); 
        else 
            return Punish(a); 
    }

    // Modify the alien depending on result, then return it. 
    private static Alien Fight(Alien alien, Alien enemy){
        int strength = getStrength(alien); 
        int speed = getSpeed(alien); 
        int weight = getWeight(alien); 

        int enemyStrength = getStrength(enemy);
        int enemySpeed = getSpeed(enemy);
        int enemyWeight = getWeight(enemy);

        int score = 0; 
        if (strength > enemyStrength) 
            score++; 
        if (speed > enemySpeed)
            score++; 
        if (weight > enemyWeight)
            score++; 

        if (score > 1) {
            alien = Reward(alien); 
        } else {
            alien = Punish(alien); 
        }

        return alien; 
    }

    public static bool Forward(byte[] id) {
        Alien alien = Query(id); 
        if (alien == null) 
        {
            Runtime.Notify("Invalid id"); 
            return false; 
        }

        uint encounter = D6();
        switch(encounter) 
        {
            case 0:  // Strength check, remove some obstacle in the way
                EncounterTriggered("strength", id); 
                alien = Check(alien, getStrength(alien)); 
                break; 
            case 1:  // Speed check, run through dangerous area
                EncounterTriggered("speed", id); 
                alien = Check(alien, getSpeed(alien)); 
                break; 
            case 2:  // Weight check, get across a weak bridge
                EncounterTriggered("weight", id); 
                int modifier = 100 - getWeight(alien); 
                alien = Check(alien, modifier); 
                break; 
            case 3:  // Battle encounter
                EncounterTriggered("battle", id); 
                byte[] enemyId = DN(getCounter()).ToByteArray(); 
                Alien enemy = Query(enemyId); 
                if (enemy == null) return false; 
                alien = Fight(alien, enemy); 
                break; 
            case 4:  // Find treasure
                EncounterTriggered("reward", id); 
                alien = Reward(alien); 
                break; 
            case 5: // Fall into trap
                EncounterTriggered("punish", id); 
                alien = Punish(alien); 
                break; 
            default: 
                return false; 
        }

        // Store the modified alien
        StorageMap alienMap = Storage.CurrentContext.CreateMap(nameof(alienMap)); 
        alienMap.Put(id, Helper.Serialize(alien)); 
        return true; 
    }

    /*
    * NEP 11 Implementation
    */ 
    public static BigInteger TotalSupply() => getCounter(); 

    public static BigInteger BalanceOf() 
    {

    }


}
