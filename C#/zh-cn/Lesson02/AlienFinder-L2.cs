using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

using Helper = Neo.SmartContract.Framework.Helper;

public class AlienFinder : SmartContract {
    // type name for the contract
    const string AlienMapName = nameof (AlienFinder);

    public class Alien {
        private static BigInteger counter = 0;

        public uint Xna { get; set; }
        public string AlienName { get; set; }
        public uint BlockHeight { get; private set; }
        public BigInteger Id { get; private set; }

        public Alien (uint xna, string alienName, uint blockHeight) {
            Xna = xna;
            AlienName = alienName;
            BlockHeight = blockHeight;

            byte[] value = Storage.Get (Storage.CurrentContext, "counter");
            if (value != null) { counter = value.ToBigInteger (); } else { counter = 0; }

            Id = ++counter;
            Storage.Put (Storage.CurrentContext, "counter", counter);
        }
    }

    public static object Main (string operation, params object[] args) {
        if (Runtime.Trigger == TriggerType.Verification) {
            return false;
        }
        switch (operation) {
            case "generateAlien":
                return GenerateAlien ((string) args[0], (byte[]) args[1]);
            case "query":
                return Query ((byte[]) args[0]);
            case "mutate":
                return Mutate ((byte[]) args[0], (uint) args[1]);
            default:
                return false;
        }
    }

    public static bool GenerateAlien (string alienName, byte[] owner) {
        // Check if the `owner` argument is the same as one who invoked contract
        if (!Runtime.CheckWitness (owner)) return false;

        uint blockHeight = Blockchain.GetHeight ();
        uint xna = FindXna (RandomNumber (blockHeight));
        Alien someAlien = new Alien (xna, alienName, blockHeight);
        // add the object to storage
        StorageMap alienMap = Storage.CurrentContext.CreateMap (AlienMapName);
        alienMap.Put (someAlien.Id.ToByteArray (), Helper.Serialize (someAlien));
        Runtime.Notify ("Alien created, ID: " + (someAlien.Id));
        return true;
    }

    private static ulong RandomNumber (uint blockHeight) {
        return Blockchain.GetHeader (blockHeight).ConsensusData;
    }

    private static uint FindXna (ulong randomNumber) {
        return (uint) (randomNumber % 100000000);
    }

    public static Alien Query (byte[] id) {
        // deserialise the alien object then return it
        StorageMap alienMap = Storage.CurrentContext.CreateMap (AlienMapName);
        var result = alienMap.Get (id);
        if (result.Length == 0) return null;
        return Helper.Deserialize (result) as Alien;
    }

    /*
     * Increase the desired digits of an alien xna by a random number
     * the other fields will decrease so that the sum of attributes remain constant. 
     * The first two digits (which represent life form) does not count as attribute. 
     */
    public static void Mutate (byte[] id, uint attribute) {
        Alien a = Query (id);
        if (a == null) {
            Runtime.Notify ("Alien does not exist! ");
            return;
        }
        uint blockHeight = Blockchain.GetHeight ();
        uint randomDigit = (uint) (RandomNumber (blockHeight) % 10);
        switch (attribute) {
            case 3: // weight
                /* 
                 * Add twice the rng to the desired field, then substract
                 * equal amount from other fields. 
                 */
                a.Xna += randomDigit * 2;
                a.Xna -= randomDigit * 100;
                a.Xna -= randomDigit * 10000;
                return;
            case 2: // speed
                a.Xna += randomDigit * 2 * 100;
                a.Xna -= randomDigit;
                a.Xna -= randomDigit * 10000;
                return;
            case 1: // strength 
                a.Xna += randomDigit * 2 * 10000;
                a.Xna -= randomDigit;
                a.Xna -= randomDigit * 100;
                return;
            default:
                return;
        }
    }
}