using System;
using System.Numerics;
using Neo.Smartcontract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

public class AlienFinder : Smartcontract {
    public class Alien {
        public static uint counter = 0;
        public uint xna;
        public string alienName;
        public uint blockHeight;
        public uint id;

        public Alien (uint xna, string alienName, uint blockHeight) {
            this.xna = xna;
            this.alienName = alienName;
            this.blockHeight = blockHeight;
            counter = counter + 1;
            this.id = counter;
        }
    }

    public static void Main (string alienName) {
        uint blockHeight = Blockchain.GetHeight ();
        uint xna = FindXna (RandomNumber (blockHeight));
        Alien someAlien = new Alien (xna, alienName, blockHeight);
        Runtime.Notify ("Alien created, ID: " + someAlien.id);
    }

    private static ulong RandomNumber (uint blockHeight) {
        return Blockchain.GetHeader (blockHeight).ConsensusData;
    }

    private static uint FindXna (ulong randomNumber) {
        return (uint) (randomNumber % 100000000);
    }

}