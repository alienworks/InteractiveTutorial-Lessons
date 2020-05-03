import {
    SmartContract,
    SerializableValueObject,
    MapStorage,
    constant,
    Blockchain,
    createEventNotifier,
    Address,
    Deploy
} from '@neo-one/smart-contract';

interface Alien extends SerializableValueObject {
    xna: number;
    readonly alienName: string;
    readonly blockHeight: number;
    readonly id: number;
}

const notifyCreation = createEventNotifier<number>(
    'generate',
    'id'
);

export class AlienFinder extends SmartContract {

    private readonly aliens = MapStorage.for<number, Alien>();

    private counter: number = 0; 

    public constructor(public readonly owner: Address = Deploy.senderAddress) {
        super();
    }

    public generateAlien(alienName: string) {
        if (!Address.isCaller(this.owner)) {
            throw new Error('Expected caller to be the owner');
        }

        let blockHeight: number = Blockchain.currentHeight;
        let xna: number = this.findXna(this.randomNumber(blockHeight));
        let id: number = ++this.counter;
        let someAlien: Alien = {xna: xna, alienName: alienName, blockHeight: blockHeight, id: id};
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

    @constant
    public query(id: number): Alien {
        const alien = this.aliens.get(id);
        if (alien === undefined) {
            throw new Error('Alien not found');
        }
        else {
            return alien;
        }
    }

    public mutate(id: number, attribute: number) {
        let a: Alien = this.query(id);
        let blockHeight: number = Blockchain.currentHeight;
        let randomDigit: number = this.randomNumber(blockHeight) % 100;

        switch(attribute) {
            case 0: {
                let right: number = a.xna % 1E6;
                a.xna = randomDigit * 1E6 + right;
                this.aliens.set(id, a);
                break;
            }
            case 1: {
                let left: number = a.xna / 1E6;
                let right: number = a.xna % 1E4;
                a.xna = left * 1E6 + randomDigit * 1E4 + right;
                this.aliens.set(id, a);
                break;
            }
            case 2: {
                let left: number = a.xna / 1E4;
                let right: number = a.xna % 1E2;
                a.xna = left * 1E4 + randomDigit * 1E2 + right;
                this.aliens.set(id, a);
                break;
            }
            case 3: {
                let left: number = a.xna / 1E2;
                a.xna = left * 1E2 + randomDigit;
                this.aliens.set(id, a);
                break;
            }
            default: {
                break;
            }
        }
    }
}