import { reorderTaskIds, resolveDropNeighbors } from './drop-neighbors';

describe('resolveDropNeighbors', () => {
  it('returns null neighbours for a single-item column', () => {
    const ids = ['a'];
    expect(resolveDropNeighbors(ids, 'a', 0)).toEqual({
      previousTaskId: null,
      nextTaskId: null
    });
  });

  it('calculates neighbours when dropping at the start', () => {
    const ids = reorderTaskIds(['a', 'b', 'c'], 'c', 0);
    expect(ids).toEqual(['c', 'a', 'b']);
    expect(resolveDropNeighbors(ids, 'c', 0)).toEqual({
      previousTaskId: null,
      nextTaskId: 'a'
    });
  });

  it('calculates neighbours when dropping in the middle', () => {
    const ids = reorderTaskIds(['a', 'b', 'c'], 'a', 1);
    expect(ids).toEqual(['b', 'a', 'c']);
    expect(resolveDropNeighbors(ids, 'a', 1)).toEqual({
      previousTaskId: 'b',
      nextTaskId: 'c'
    });
  });

  it('calculates neighbours when dropping at the end', () => {
    const ids = reorderTaskIds(['a', 'b', 'c'], 'a', 2);
    expect(ids).toEqual(['b', 'c', 'a']);
    expect(resolveDropNeighbors(ids, 'a', 2)).toEqual({
      previousTaskId: 'c',
      nextTaskId: null
    });
  });
});
