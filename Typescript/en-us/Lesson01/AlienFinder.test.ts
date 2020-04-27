import { withContracts } from '../neo-one/test';

describe('AlienFinder', () => {
  test('definition', async () => {
    await withContracts(async ({ alienFinder }) => {
      expect(alienFinder).toBeDefined();
    });
  });
  test('function', async () => {
    await withContracts(async ({ alienFinder }) => {
      const receipt = await alienFinder.generateAlien.confirmed('someone');
      if (receipt.result.state === 'FAULT') {
        throw new Error(receipt.result.message);
      }
      expect(receipt.result.state).toEqual('HALT');
      expect(receipt.result.value).toBeUndefined();
      expect(receipt.events).toHaveLength(1);
      let event = receipt.events[0];
      expect(event.name).toEqual('generate');
      expect(event.parameters.id.toNumber()).toEqual(1);
    });
  });
});