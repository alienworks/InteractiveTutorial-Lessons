import { SmartContract, SerializableValueObject, ArrayStorage, constant, Blockchain, createEventNotifier } from '@neo-one/smart-contract';

interface Alien extends SerializableValueObject {
    readonly xna: number;
    readonly alienName: string;
    readonly blockHeight: number;
}

const notifyCreation = createEventNotifier<number>(
    'Alien created, ID: ',
    'id'
);

export class AlienFinder extends SmartContract {

    // array to keep track of all Alien tuples; 
    private readonly aliens = ArrayStorage.for<Alien>();

    public generateAlien(alienName: string) {
        let blockHeight: number = Blockchain.currentHeight;
        let xna: number = this.findXna(this.randomNumber(blockHeight));
        let someAlien: Alien = {xna: xna, alienName: alienName, blockHeight: blockHeight};
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