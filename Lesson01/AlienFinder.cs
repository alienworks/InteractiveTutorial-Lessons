using Neo.Smartcontract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

public class AlienFinder : Smartcontract
{
    struct Alien
    {
        private uint xna;
        private string alienName;
        private uint blockHeight;

        public Alien(uint xna, string alienName, uint blockHeight) 
        {
            this.xna = xna; 
            this.alienName = alienName;
            this.blockHeight = blockHeight;
        }
    }

    // list to keep track of all Alien structs; 
    static List<Alien> aliens = new List<Alien>(); 

    public static void Main(string alienName) 
    {
        uint blockHeight = Blockchain.GetHeight();
        uint xna = FinaXnaOnPlanet(GeneratePlanet(blockHeight));
        Alien someAlien = new Alien(xna, alienName, blockHeight);
        aliens.Add(someAlien);
        Runtime.Notify("Alien created, ID: " + ToString(aliens.count - 1));
    }

    private static ulong GeneratePlanet(uint blockHeight)
    {
        return Blockchain.GetHeader(blockHeight).ConsensusData; 
    }

    private static uint FindXnaOnPlanet(ulong planet)
    {
        return (uint)(planet % 100000000);
    }
    
}