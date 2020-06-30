import { SmartContract, SerializableValueObject, MapStorage, constant, Blockchain, createEventNotifier } from '@neo-one/smart-contract';

interface Alien extends SerializableValueObject {
    readonly xna: number;
    readonly alienName: string;
    readonly blockHeight: number;
    readonly id: number;
}

const notifyCreation = createEventNotifier<number>(
    'generate',
    'id'
);

export class AlienFinder extends SmartContract {

    // array to keep track of all Alien tuples; 
    private readonly aliens = MapStorage.for<number, Alien>();

    private counter: number = 0;

    public generateAlien(alienName: string) {
        let blockHeight: number = Blockchain.currentHeight;
        let xna: number = this.findXna(this.randomNumber(blockHeight));
        let id: number = ++this.counter;
        let someAlien: Alien = { xna: xna, alienName: alienName, blockHeight: blockHeight, id: id };
        this.aliens.set(id, someAlien);
        notifyCreation(someAlien.id);
    }

    @constant
    private randomNumber(blockHeight: number): number {
        return Blockchain.currentBlockTime * blockHeight;
    }

    @constant
    private findXna(randomNumber: number): number {
        return randomNumber % 1E8;
    }
}