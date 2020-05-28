from boa.builtins import concat
from boa.interop.Neo.Header import GetConsensusData
from boa.interop.Neo.Runtime import Notify
from boa.interop.Neo.Storage import Get, Put, GetContext
from boa.interop.Neo.Blockchain import GetHeader, GetHeight

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

    temp_alien = {
        'xna': xna,
        'alienName': alienName,
        'blockHeight': blockHeight,
        'id': counter
    }

    context = GetContext()
    Put(context, counter, temp_alien)
    counter += 1
    return temp_alien


def Main(temp_alienName):
    temp_blockHeight = GetHeight()
    temp_xna = FindXna(RandomNumber(temp_blockHeight))
    someAlien = generateAlien(temp_xna, temp_alienName, temp_blockHeight)
    Notify(concat("Alien created, ID: ", someAlien['id']))
