import { withContracts } from '../neo-one/test';

describe('AlienFinder', () => {
  test('exist', async () => {
    await withContracts(async ({ alienFinder }) => {
      expect(alienFinder).toBeDefined();
    });
  });
  test('invoke', async () => {
    await withContracts(async ({ alienFinder }) => {
      const receipt = await alienFinder.createAlien.confirmed('someone');
    });
  });
});