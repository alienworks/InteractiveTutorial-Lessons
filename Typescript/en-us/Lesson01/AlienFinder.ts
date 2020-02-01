import { Blockchain, constant, ArrayStorage, createEventNotifier, SmartContract } from '@neo-one/smart-contract';

const notifyCreation = createEventNotifier<number>(
    'Alien created, ID: ',
    'id'
);

export class AlienFinder extends SmartContract {

    // array to keep track of all Alien tuples; 
    private readonly aliens = ArrayStorage.for<[number, string, number]>();

    public createAlien(alienName: string) {
        let blockHeight: number = Blockchain.currentHeight;
        let xna: number = this.findXna(this.randomNumber(blockHeight));
        let someAlien: [number, string, number] = [xna, alienName, blockHeight];
        this.aliens.push(someAlien);
        notifyCreation(this.aliens.length - 1);
    }

    @constant
    private randomNumber(blockHeight: number): number {
        return Blockchain.currentBlockTime * blockHeight;
    }

    @constant
    private findXna(randomNumber: number): number {
        return randomNumber % 100000000;
    }
}