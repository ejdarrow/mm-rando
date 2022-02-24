/// Extended ES6 Map type.
export class ExMap<TKey, TValue> extends Map<TKey, TValue> {
  getOrError(key: TKey) {
    const result = this.get(key);
    if (result === undefined) {
      throw Error(`Value cannot be found for key: ${key}`);
    }
    return result;
  }
}
