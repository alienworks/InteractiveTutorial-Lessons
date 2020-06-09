using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

public class AlienFinder : SmartContract
{
    public class Alien
    {
        public static uint counter = 0; 
        public uint xna;
        public string alienName;
        public uint id; 
    }

    public static void Main(string alienName) 
    {
        uint xna = FindXna(RandomNumber());
        Alien.counter = Alien.counter + 1; 
        Alien someAlien = new Alien
        {
            xna = xna, 
            alienName = alienName, 
            id = Alien.counter
        };
        Runtime.Notify("Alien created, ID: " + someAlien.id);
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
    
}