from boa.builtins import concat
from boa.interop.Neo.Header import GetConsensusData
from boa.interop.Neo.Runtime import Notify, CheckWitness
from boa.interop.Neo.Storage import Get, Put, GetContext
from boa.interop.Neo.Blockchain import GetHeader, GetHeight


counter = 0
context = GetContext()
Put(context, 'counter', 0)


def RandomNumber(blockHeight):
    return GetConsensusData(GetHeader(blockHeight))


def FindXna(randomNumber):
    return randomNumber % 100000000


def generateAlien(xna, alienName, blockHeight):

    context = GetContext()
    counter = Get(context, 'counter')
    if not counter:
        Put(context, 'counter', 1)
    else:
        Put(context, 'counter', counter + 1)

    some_alien = {
        'xna': xna,
        'alienName': alienName,
        'blockHeight': blockHeight,
        'id': counter
    }

    context = GetContext()
    Put(context, some_alien['id'], Serialize(some_alien))

    Notify(concat("Alien created, ID: ", some_alien['id']))
    return 1


def query(temp_id):
    context = GetContext()
    some_alien = Get(context, temp_id)

    if not some_alien:
        Notify("Not Found")
        return False

    de_some_alien = Deserialize(some_alien)
    Notify(concat('Name: ', de_some_alien['alienName']))
    Notify(concat('XNA: ', de_some_alien['xna']))
    return de_some_alien


def mutate(id, attribute):
    some_alien = query(id)
    context = GetContext()
    if not some_alien:
        Notify('Alien does not exist!')
        return False
    blockHeight = GetHeight()
    randomDigit = RandomNumber(blockHeight) % 10

    de_some_alien = Deserialize(some_alien)
    if attribute == 0:
        de_some_alien['xna'] += randomDigit * 2;
        de_some_alien['xna'] -= randomDigit * 100;
        de_some_alien['xna'] -= randomDigit * 10000;
        Put(context, de_some_alien['id'], Serialize(de_some_alien))
        return 1
    elif attribute == 1:
        de_some_alien['xna'] += randomDigit * 2 * 100;
        de_some_alien['xna'] -= randomDigit;
        de_some_alien['xna'] -= randomDigit * 10000;
        Put(context, de_some_alien['id'], Serialize(de_some_alien))
        return 1
    elif attribute == 2:
        de_some_alien['xna'] += randomDigit * 2 * 10000;
        de_some_alien['xna'] -= randomDigit;
        de_some_alien['xna'] -= randomDigit * 100;
        Put(context, de_some_alien['id'], Serialize(de_some_alien))
        return 1

    return False


def initialize():
    context = GetContext()
    ini_flag = Get(context, 'ini_flag')
    if not ini_flag:
        Put(context, 'counter', 0)
        Put(context, 'ini_flag', 1)
        Notify('Initialized')
        return 1
    else:
        return 1


def Main(operation, args):
    initialize()

    nargs = len(args)

    if nargs == 0:
        print("No args")
        return False

    elif operation == 'generateAlien':
        temp_alienName = args[0]
        temp_owner = args[1]
        if not CheckWitness(temp_owner):
            return False
        temp_blockHeight = GetHeight()
        temp_xna = FindXna(RandomNumber(temp_blockHeight))
        temp_alien = generateAlien(temp_xna, temp_alienName, temp_blockHeight)
        return temp_alien

    elif operation == 'query':
        temp_id = args[0]
        temp_alien = query(temp_id)
        return temp_alien

    elif operation == 'mutate':
        temp_id = args[0]
        temp_attribute = args[1]
        mutate(temp_id, temp_attribute)
        return 1
