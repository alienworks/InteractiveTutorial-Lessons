using System;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

using Helper = Neo.SmartContract.Framework.Helper;

public class AlienFinder : SmartContract {

    public delegate void deleAlienEvent (BigInteger id);
    public static event deleAlienEvent AlienGenerated;
    public static event deleAlienEvent AlienDeleted;

    public class Alien {
        public uint Xna { get; set; }
        public string AlienName { get; set; }
        public BigInteger Id { get; set; }
    }

    public static object Main (string method, params object[] args) {
        if (Runtime.Trigger == TriggerType.Application) {
            switch (method) {
                case "generateAlien":
                    return GenerateAlien ((string) args[0], (byte[]) args[1]);
                case "query":
                    return Query ((byte[]) args[0]);
                default: 
                    return false; 
            }
        }
        return false;
    }

    public static BigInteger GenerateAlien (string alienName, byte[] owner) {
        if (owner.Length != 20 && owner.Length != 33)
            throw new InvalidOperationException ("The parameter owner should be a 20-byte address or a 33-byte public key"); // Check if the owner is the same as one who invoked contract
        if (!Runtime.CheckWitness (owner)) return 0;

        uint xna = FindXna (RandomNumber ());
        Alien someAlien = new Alien {
            Xna = xna,
            AlienName = alienName,
            Id = updateCounter ()
        };

        // add the object to storage
        StorageMap alienMap = Storage.CurrentContext.CreateMap (nameof (alienMap));
        alienMap.Put (someAlien.Id.ToByteArray (), Helper.Serialize (someAlien));
        Runtime.Notify (someAlien.Id, "created");
        return someAlien.Id;
    }

    private static ulong RandomNumber () {
        uint blockHeight = Blockchain.GetHeight ();
        return Blockchain.GetHeader (blockHeight).ConsensusData;
    }

    private static uint FindXna (ulong randomNumber) {
        return (uint) (randomNumber % 100000000);
    }

    // Read the counter from storage if it exists. Otherwise, default to 0. 
    private static BigInteger getCounter () {
        BigInteger counter = 0;
        byte[] value = Storage.Get ("alienCount");
        if (value.Length != 0)
            counter = value.ToBigInteger ();
        return counter;
    }

    private static BigInteger updateCounter () {
        BigInteger counter = getCounter ();
        counter = counter + 1;
        Storage.Put (Storage.CurrentContext, "alienCount", counter);
        return counter;
    }

    public static Alien Query (byte[] id) {
        // deserialise the alien object then return it
        StorageMap alienMap = Storage.CurrentContext.CreateMap (nameof (alienMap));
        var result = alienMap.Get (id);
        if (result.Length == 0) return null;
        return Helper.Deserialize (result) as Alien;
    }

    public static bool Delete (byte[] owner, byte[] id) {
        if (owner.Length != 20 && owner.Length != 33)
            throw new InvalidOperationException ("The parameter owner should be a 20-byte address or a 33-byte public key"); // Check if the owner is the same as one who invoked contract
        // Check if the owner is the same as one who invoked contract
        if (!Runtime.CheckWitness (owner)) return false;

        StorageMap alienMap = Storage.CurrentContext.CreateMap (nameof (alienMap));
        alienMap.Delete (id);
        AlienDeleted (id.ToBigInteger ());
        return true;
    }
}