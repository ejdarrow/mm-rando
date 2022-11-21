import { CheckedState } from './CheckedState';
import { ItemListRepr } from './ConfigTypes';
import { isHexString, tuple, u32 } from './Utility';

abstract class AbstractItemListBitMask implements Iterable<[number, number]> {
  abstract at: (index: number) => [number, number];
  abstract complete: () => ItemListBitMask;
  abstract length: () => number;

  *[Symbol.iterator]() {
    for (let i = 0; i < this.length(); i++) {
      yield this.at(i);
    }
  }

  /// NOTE: This assumes both (index => value) mappings are sorted by index.
  equals(other: AbstractItemListBitMask) {
    if (this.length() !== other.length()) {
      return false;
    }

    for (let i = 0; i < this.length(); i++) {
      const [ourIndex, ourValue] = this.at(i);
      const [otherIndex, otherValue] = other.at(i);
      if (ourIndex !== otherIndex) {
        return false;
      }
      if (ourValue !== otherValue) {
        return false;
      }
    }

    return true;
  }
}

/**
 * Mutable `ItemListBitMask` which uses `number[]` for `maskChunks` instead of `Uint32Array`.
 *
 * This type is less efficient than `ItemListBitMask`, but the `maskChunks` array length may be modified in-place.
 * Thus it is useful for performing operations such as bitwise `OR` before "completing" into less mutable state.
 */
export class ItemListBitMaskMut extends AbstractItemListBitMask {
  readonly chunkIndexes: number[];
  readonly maskChunks: number[];

  constructor(chunkIndexes: number[], maskChunks: number[]) {
    super();
    this.chunkIndexes = chunkIndexes;
    this.maskChunks = maskChunks;
  }

  at = (index: number) => {
    return tuple(this.chunkIndexes[index], this.maskChunks[index]);
  };

  complete = () => {
    return new ItemListBitMask(this.chunkIndexes, Uint32Array.from(this.maskChunks));
  };

  length = () => {
    return this.chunkIndexes.length;
  };

  /** Apply bitwise `OR` with another bit mask. */
  bitwiseOr(other: AbstractItemListBitMask) {
    for (let [otherChunkIndex, otherMaskChunk] of other) {
      const localIndex = this.chunkIndexes.indexOf(otherChunkIndex);
      if (localIndex < 0) {
        this.chunkIndexes.push(otherChunkIndex);
        this.maskChunks.push(otherMaskChunk);
      } else {
        this.maskChunks[localIndex] |= otherMaskChunk;
      }
    }
  }
}

/** Plain-object representation of `ItemListBitMask`. */
export interface ItemListBitMaskObject {
  chunkIndexes: number[]
  maskChunks: number[]
}

/** Efficient bitmask container for use with `ItemListBits`. */
export class ItemListBitMask extends AbstractItemListBitMask {
  readonly chunkIndexes: number[];
  readonly maskChunks: Uint32Array;

  constructor(chunkIndexes: number[], maskChunks: Uint32Array) {
    super();
    this.chunkIndexes = chunkIndexes;
    this.maskChunks = maskChunks;
  }

  /** Convert from an `ItemListBitMaskObject` to an `ItemListBitMask`. */
  static fromPlainObject(obj: ItemListBitMaskObject): ItemListBitMask {
    return new ItemListBitMask(obj.chunkIndexes, Uint32Array.from(obj.maskChunks))
  }

  /** Convert to an `ItemListBitMaskObject`. */
  toPlainObject(): ItemListBitMaskObject {
    return {
      chunkIndexes: this.chunkIndexes,
      maskChunks: Array.from(this.maskChunks),
    }
  }

  at = (index: number) => {
    return tuple(this.chunkIndexes[index], this.maskChunks[index]);
  };

  complete = () => {
    return this;
  };

  /** Get the number of chunks. */
  length = (): number => {
    return this.chunkIndexes.length;
  };

  /** Clone into a new `ItemListBitMask`. */
  clone(): ItemListBitMask {
    return new ItemListBitMask(Array.from(this.chunkIndexes), Uint32Array.from(this.maskChunks));
  }

  /** Clone to a mutable representation: `ItemListBitMaskMut`. */
  cloneToMut(): ItemListBitMaskMut {
    return new ItemListBitMaskMut(Array.from(this.chunkIndexes), Array.from(this.maskChunks));
  }

  /** Whether or not this bit mask contains no `1` bits. */
  isEmpty(): boolean {
    for (const value of this.maskChunks) {
      if (value !== 0) {
        return false;
      }
    }
    return true;
  }

  /** Merge multiple bit masks using bitwise `OR` and return the result. */
  static bitwiseOrAll(bitMasks: ItemListBitMask[]): ItemListBitMask {
    if (bitMasks.length <= 0) {
      return ItemListBitMask.empty();
    } else if (bitMasks.length === 1) {
      return bitMasks[0].clone();
    }

    let result = bitMasks[0].cloneToMut();
    for (let i = 1; i < bitMasks.length; i++) {
      result.bitwiseOr(bitMasks[i]);
    }
    return result.complete();
  }

  /** Create an empty bit mask. */
  static empty(): ItemListBitMask {
    return new ItemListBitMask([], new Uint32Array(0));
  }

  /** Create a bit mask from bit indexes. */
  static fromBits(bitIndexes: number[]): ItemListBitMask {
    const maskChunks = Array<number>();
    const chunkIndexes = Array<number>();

    // Sort the bit indexes, which should result in a sorted chunk index array.
    // We need to do this for efficient comparison later on.
    bitIndexes = bitIndexes.sort((a, b) => a - b);

    for (let i = 0; i < bitIndexes.length; i++) {
      const bitIndex = bitIndexes[i];

      // TODO: Calculate chunk/shift with helper function?
      const chunk = Math.floor(bitIndex / 32);
      const shift = bitIndex % 32;

      // Find our index which maps to the chunk to update, or otherwise insert.
      const localIndex = chunkIndexes.indexOf(chunk);
      if (localIndex < 0) {
        // Insert new mask chunk.
        chunkIndexes.push(chunk);
        maskChunks.push(1 << shift);
      } else {
        // Update existing mask chunk.
        maskChunks[localIndex] |= 1 << shift;
      }
    }

    return new ItemListBitMask(chunkIndexes, Uint32Array.from(maskChunks));
  }
}

/** Calculate number of 32-bit chunks required to store bits. */
const getChunkCount = (length: number): number => {
  return (length + 31) >>> 5
}

/** Plain-object representation of `ItemListBits`. */
export interface ItemListBitsObject {
  storage: number[]
  length: number
}

export class ItemListBits {
  storage: Uint32Array;
  length: number;

  constructor(storage: Uint32Array, length: number) {
    this.storage = storage;
    this.length = length;
  }

  /** Clone into a new `ItemListBits`. */
  clone(): ItemListBits {
    return new ItemListBits(Uint32Array.from(this.storage), this.length);
  }

  /** Return the resulting `ItemListBitMask` from a bitwise `AND` operation. */
  and(bitMask: ItemListBitMask): ItemListBitMask {
    const clonedMask = bitMask.clone();
    for (var i = 0; i < clonedMask.length(); i++) {
      const chunkIndex = clonedMask.chunkIndexes[i];
      clonedMask.maskChunks[i] &= this.storage[chunkIndex];
    }
    return clonedMask;
  }

  /** Apply a bitmask using bitwise `OR`. */
  applyMaskOr(bitMask: ItemListBitMask) {
    for (let [chunkIndex, maskChunk] of bitMask) {
      this.storage[chunkIndex] |= maskChunk;
    }
  }

  /** Immutably apply a bitmask using bitwise `OR` and return the result. */
  applyMaskOrImmut(bitMask: ItemListBitMask): ItemListBits {
    if (!this.and(bitMask).equals(bitMask)) {
      const clone = this.clone();
      clone.applyMaskOr(bitMask);
      return clone;
    }
    return this;
  }

  /** Apply a bitmask using bitwise `AND`, `NOT`. */
  applyMaskNot(bitMask: ItemListBitMask) {
    for (let [chunkIndex, maskChunk] of bitMask) {
      this.storage[chunkIndex] &= ~maskChunk;
    }
  }

  /** Immutably apply a bitmask using bitwise `AND`, `NOT` and return the result. */
  applyMaskNotImmut(bitMask: ItemListBitMask): ItemListBits {
    if (!this.and(bitMask).isEmpty()) {
      const clone = this.clone();
      clone.applyMaskNot(bitMask);
      return clone;
    }
    return this;
  }

  /** Return the resulting `ItemListBitMask` from a bitwise `XOR` operation. */
  xor(bitMask: ItemListBitMask): ItemListBitMask {
    const clonedMask = bitMask.clone();
    for (var i = 0; i < clonedMask.length(); i++) {
      const chunkIndex = clonedMask.chunkIndexes[i];
      clonedMask.maskChunks[i] ^= (this.storage[chunkIndex] & bitMask.maskChunks[i]);
    }
    return clonedMask;
  }

  /** Get the `CheckedState` for all bits. */
  getCheckedStateAll(): CheckedState {
    const identity = this.createIdentityMask();
    return this.getCheckedState(identity);
  }

  /** Get the `CheckedState` for a given bitmask. */
  getCheckedState(bitMask: ItemListBitMask): CheckedState {
    let currentState: CheckedState | undefined;

    for (const [chunkIndex, maskChunk] of bitMask) {
      const applied = (this.storage[chunkIndex] & maskChunk) >>> 0;

      let checkedState;
      if (applied === 0) {
        checkedState = CheckedState.Unchecked;
      } else if (applied === maskChunk) {
        checkedState = CheckedState.Checked;
      } else {
        return CheckedState.Indeterminate;
      }

      if (currentState !== undefined) {
        if (checkedState !== currentState) {
          return CheckedState.Indeterminate;
        }
      } else {
        currentState = checkedState;
      }
    }

    return currentState === CheckedState.Checked ? currentState : CheckedState.Unchecked;
  }

  /** Get the number of bits which are set. */
  getEnabledCount(): number {
    // TODO: Optimize this?
    let count = 0;
    for (let i = 0; i < this.length; i++) {
      if (this.hasBit(i)) count++;
    }
    return count;
  }

  /** Create an identity bitmask for all possible bits. */
  createIdentityMask(): ItemListBitMask {
    return createIdentityMask(this.length);
  }

  /** Create an identity bitmask for the tail chunk. */
  createIdentityTailChunkMask(): number {
    return createIdentityTailChunkMask(this.length);
  }

  /** Clear a bit. */
  clearBit(index: number) {
    const [chunk, shift] = ItemListBits.calcChunkAndShift(index);
    this.clearBitInternal(chunk, shift);
  }

  /** Immutably clear a bit and return the result. */
  clearBitImmut(index: number): ItemListBits {
    const [chunk, shift] = ItemListBits.calcChunkAndShift(index);
    if (this.hasBitRaw(chunk, shift)) {
      const clone = this.clone();
      clone.clearBitInternal(chunk, shift);
      return clone;
    }
    return this;
  }

  clearBitInternal(chunk: number, shift: number) {
    this.storage[chunk] &= ~((1 << shift) >>> 0) >>> 0;
  }

  /** Get whether or not a bit is set. */
  hasBit(index: number): boolean {
    const [chunk, shift] = ItemListBits.calcChunkAndShift(index);
    return this.hasBitRaw(chunk, shift);
  }

  hasBitRaw(chunk: number, shift: number): boolean {
    return ((this.storage[chunk] >>> shift) & 1) === 1;
  }

  /** Set a bit. */
  setBit(index: number) {
    const [chunk, shift] = ItemListBits.calcChunkAndShift(index);
    this.storage[chunk] |= (1 << shift) >>> 0;
  }

  /** Immutably set a bit and return the result. */
  setBitImmut(index: number): ItemListBits {
    const [chunk, shift] = ItemListBits.calcChunkAndShift(index);
    if (!this.hasBitRaw(chunk, shift)) {
      const clone = this.clone();
      clone.setBitInternal(chunk, shift);
      return clone;
    }
    return this;
  }

  setBitInternal(chunk: number, shift: number) {
    this.storage[chunk] |= (1 << shift) >>> 0;
  }

  /** Either set or clear a bit. */
  modifyBit(operation: boolean, index: number) {
    if (operation) {
      this.setBit(index);
    } else {
      this.clearBit(index);
    }
  }

  /** Check if the tail chunk is valid, e.g. does not contain out-of-range bits. */
  isTailChunkValid(): boolean {
    const tailBitMask = this.createIdentityTailChunkMask();
    const tailChunk = this.storage[this.storage.length - 1];
    return (tailChunk & tailBitMask) >>> 0 === tailChunk;
  }

  /** Set all bits to 1. */
  setAll() {
    const tailBitMask = this.createIdentityTailChunkMask();
    this.storage[this.storage.length - 1] = tailBitMask;
    for (let i = 0; i < this.storage.length - 1; i++) {
      this.storage[i] = u32.MAX;
    }
  }

  /** Immutably set all bits to 1 and return the result. */
  setAllImmut(): ItemListBits {
    const clone = this.clone();
    clone.setAll();
    return clone;
  }

  /** Set all bits to 0. */
  setNone() {
    for (let i = 0; i < this.storage.length; i++) {
      this.storage[i] = 0;
    }
  }

  /** Immutably set all bits to 0 and return the result. */
  setNoneImmut(): ItemListBits {
    const clone = this.clone();
    clone.setNone();
    return clone;
  }

  /** Convert to an `ItemListBitsObject`. */
  toPlainObject(): ItemListBitsObject {
    return {
      storage: Array.from(this.storage),
      length: this.length,
    }
  }

  toString(): string {
    const sections = Array<string>(this.storage.length);
    for (let i = 0; i < sections.length; i++) {
      sections[sections.length - i - 1] = this.storage[i] !== 0 ? this.storage[i].toString(16) : '';
    }
    return sections.join('-');
  }

  /** Throw an `Error` if the tail chunk is invalid. */
  validateTailChunk() {
    if (this.isTailChunkValid() === false) {
      throw Error('Tail chunk contains out-of-range bits.');
    }
  }

  /** Calculate the chunk index and shift index for a given bit index. */
  static calcChunkAndShift(index: number): [number, number] {
    const chunk = index >>> 5;
    const shift = index % 32;
    return tuple(chunk, shift);
  }

  /** Create an empty `ItemListBits` with no chunks. */
  static empty(): ItemListBits {
    return new ItemListBits(new Uint32Array(), 0);
  }

  /** Convert from an `ItemListBitsObject` to an `ItemListBits`. */
  static fromPlainObject(obj: ItemListBitsObject): ItemListBits {
    return new ItemListBits(Uint32Array.from(obj.storage), obj.length)
  }

  /** Parse an `ItemListBits` from a string. */
  static fromString(str: string, length: number): ItemListBits {
    const sections = str.split('-');
    const chunkCount = getChunkCount(length);
    if (sections.length !== chunkCount) {
      throw Error(`Sections count does not match expected chunk count: ${sections.length} !== ${chunkCount}`);
    }

    let storage = new Uint32Array(chunkCount);
    for (let i = 0; i < chunkCount; i++) {
      const section = sections[sections.length - i - 1];
      if (!isHexString(section) || section.length > 8) {
        throw Error(`Section is not valid UInt32 hex: "${section}"`);
      }

      storage[i] = parseInt(section, 16);
    }

    const result = new ItemListBits(storage, length);
    result.validateTailChunk();
    return result;
  }

  /** Create an empty `ItemListBits` with a given bit count. */
  static withLength(length: number): ItemListBits {
    const chunkCount = getChunkCount(length)
    return new ItemListBits(new Uint32Array(chunkCount), length)
  }
}

/** Create a bit mask which represents all potential bits in storage. */
export const createIdentityMask = (length: number): ItemListBitMask => {
  const storageLength = getChunkCount(length)

  const maskChunks = new Uint32Array(storageLength)
  for (let i = 0; i < maskChunks.length - 1; i++) {
    maskChunks[i] = u32.MAX
  }
  maskChunks[maskChunks.length - 1] = createIdentityTailChunkMask(length)

  const chunkIndexes = [...Array(maskChunks.length).keys()]
  return new ItemListBitMask(chunkIndexes, maskChunks)
}

/** Create a bit mask which represents all potential bits within the tail chunk. */
const createIdentityTailChunkMask = (length: number): number => {
  if (length % 32 !== 0) {
    return u32.MAX >>> (32 - (length % 32))
  } else {
    return u32.MAX
  }
}

export const getCellCheckedState = (list: ItemListBits, repr: ItemListRepr, colIndex: number, rowIndex: number) => {
  const cell = repr.matrix.get(colIndex, rowIndex)
  if (cell !== undefined && cell.items.length > 0) {
    return list.getCheckedState(cell.bitMask)
  }
}

export const getColumnCheckedState = (list: ItemListBits, repr: ItemListRepr, colIndex: number) => {
  return list.getCheckedState(repr.columns[colIndex].bitMask)
}

export const getRowCheckedState = (list: ItemListBits, repr: ItemListRepr, rowIndex: number) => {
  return list.getCheckedState(repr.rows[rowIndex].bitMask)
}

/** Get an `ItemListBits` from an `ItemListBitsObject`. */
export const asItemListBits = (obj: ItemListBitsObject): ItemListBits => ItemListBits.fromPlainObject(obj)

/** Perform an immutable operation on an `ItemListBitsObject`, returning a new instance. */
export const reduxMutation = (list: ItemListBitsObject, reducer: (_: ItemListBits) => ItemListBits): ItemListBitsObject => {
  const instance = ItemListBits.fromPlainObject(list)
  const result = reducer(instance)
  return result.toPlainObject()
}

/** Immutably clear all bits and return the result. */
export const performAllClear = (list: ItemListBitsObject): ItemListBitsObject => reduxMutation(list, x => x.setNoneImmut())

/** Immutably set all bits and return the result. */
export const performAllSet = (list: ItemListBitsObject): ItemListBitsObject => reduxMutation(list, x => x.setAllImmut())

/** Immutably clear a bit and return the result. */
export const performBitClear = (list: ItemListBitsObject, index: number): ItemListBitsObject => reduxMutation(list, x => x.clearBitImmut(index))

/** Immutably set a bit and return the result. */
export const performBitSet = (list: ItemListBitsObject, index: number): ItemListBitsObject => reduxMutation(list, x => x.setBitImmut(index))

/** Immutably clear using a mask and return the result. */
export const performMaskClear = (list: ItemListBitsObject, mask: ItemListBitMaskObject): ItemListBitsObject =>
  reduxMutation(list, x => x.applyMaskNotImmut(ItemListBitMask.fromPlainObject(mask)))

/** Immutably set using a mask and return the result. */
export const performMaskSet = (list: ItemListBitsObject, mask: ItemListBitMaskObject): ItemListBitsObject =>
  reduxMutation(list, x => x.applyMaskOrImmut(ItemListBitMask.fromPlainObject(mask)))

/** Parse `ItemListBitsObject` from a string. */
export const performFromString = (value: string, length: number): ItemListBitsObject => ItemListBits.fromString(value, length).toPlainObject()

/** Get an `ItemListBitsObject` with a given bits length. */
export const performWithLength = (length: number): ItemListBitsObject => ItemListBits.withLength(length).toPlainObject()

/** Get an empty `ItemListBitsObject`. */
export const getEmptyItemList = (): ItemListBitsObject => ItemListBits.empty().toPlainObject()
