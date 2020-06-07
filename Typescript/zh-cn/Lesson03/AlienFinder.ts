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

const notifyEvent = createEventNotifier<string, number>(
    'event',
    'name',
    'id'
);

const notifyReward = createEventNotifier<string, number>(
    'reward',
    'attribute',
    'value'
);

const notifyPunishment = createEventNotifier<string, number>(
    'punish',
    'attribute',
    'value'
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

    // event selection
    @constant
    public d6(): number {
        return this.randomNumber(Blockchain.currentHeight) % 6;
    }

    // xna promotion
    @constant
    public d10(): number {
        return this.randomNumber(Blockchain.currentHeight) % 10;
    }

    // event judgement
    @constant
    public d100(): number {
        return this.randomNumber(Blockchain.currentHeight) % 100;
    }

    // enemy selection
    @constant
    public dN(n: number): number {
        return this.randomNumber(Blockchain.currentHeight) % n + 1;
    }

    private reward(alien: Alien) {
        let attribute: number = this.d6() / 2;
        let value: number = this.d10();

        switch(attribute) {
            case 0: {
                alien.xna += value * 1E4;
                notifyReward('strength', value);
                this.aliens.set(alien.id, alien);
                break;
            }
            case 1: {
                alien.xna += value * 1E2;
                notifyReward('speed', value);
                this.aliens.set(alien.id, alien);
                break;
            }
            case 2: {
                alien.xna += value;
                notifyReward('weight', value);
                this.aliens.set(alien.id, alien);
                break;
            }
            default: {
                break;
            }
        }
    }

    private punish(alien: Alien) {
        let attribute: number = this.d6() / 2;
        let value: number = this.d10();

        switch(attribute) {
            case 0: {
                alien.xna -= this.d10() * 1E4;
                notifyPunishment('strength', value);
                this.aliens.set(alien.id, alien);
                break;
            }
            case 1: {
                alien.xna -= this.d10() * 1E2;
                notifyPunishment('speed', value);
                this.aliens.set(alien.id, alien);
                break;
            }
            case 2: {
                alien.xna -= this.d10();
                notifyPunishment('weight', value);
                this.aliens.set(alien.id, alien);
                break;
            }
            default: {
                break;
            }
        }
    }

    private judge(alien: Alien, value: number) {
        if (value + this.d100() > 100) {
            this.reward(alien);
        }
        else {
            this.punish(alien);
        }
    }

    private fight(alien: Alien, enemy: Alien) {
        let strength: number = alien.xna % 1E6 / 1E4;
        let speed: number = alien.xna % 1E4 / 1E2;
        let weight: number = alien.xna % 1E2;

        let enemyStrength: number = enemy.xna % 1E6 / 1E4;
        let enemySpeed: number = enemy.xna % 1E4 / 1E2;
        let enemyWeight: number = enemy.xna % 1E2;

        let score: number = 0;
        if (strength > enemyStrength) {
            score++;
        }
        if (speed > enemySpeed) {
            score++;
        }
        if (weight > enemyWeight) {
            score++;
        }
        
        if (score > 1) {
            this.reward(alien);
        }
        else {
            this.punish(alien);
        }
    }

    public forward(id: number) {
        let alien: Alien = this.query(id);
        let event: number = this.d6();

        switch(event) {
            case 0: {
                // strength event(digits 3-4), remove some obstacle in the way
                let strength: number = alien.xna % 1E6 / 1E4;
                notifyEvent('strength', id);
                this.judge(alien, strength);
                break;
            }
            case 1: {
                // speed event(digits 5-6), run through dangerous area
                let speed: number = alien.xna % 1E4 / 1E2;
                notifyEvent('speed', id);
                this.judge(alien, speed);
                break;
            }
            case 2: {
                // weight event(digits 7-8), get across a weak bridge
                let weight: number = alien.xna % 1E2;
                notifyEvent('weight', id);
                this.judge(alien, 100 - weight);
                break;
            }
            case 3: {
                // fight with another alien
                notifyEvent('battle', id);
                let enemy = this.query(this.dN(this.counter))
                this.fight(alien, enemy);
                break;
            }
            case 4: {
                // find treasure
                notifyEvent('reward', id);
                this.reward(alien);
                break;
            }
            case 5: {
                // fall into a trap
                notifyEvent('punish', id);
                this.punish(alien);
                break;
            }
            default: {
                break;
            }
        }
    }
}