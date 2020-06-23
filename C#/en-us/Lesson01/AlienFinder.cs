using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

public class AlienFinder : SmartContract
{
    public class Alien
    {
        public uint xna;
        public string alienName;
    }

    public static void Main(string alienName) 
    {
        ulong randomNumber = RandomNumber(); 
        uint xna = FindXna(randomNumber);
        Alien someAlien = new Alien
        {
            xna = xna, 
            alienName = alienName, 
        };
        Runtime.Notify(alienName, "created!");
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