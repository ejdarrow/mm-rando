export class Matrix<T> {
  readonly data: T[];
  readonly colCount: number;
  readonly rowCount: number;

  constructor(colCount: number, rowCount: number) {
    this.data = Array<T>(colCount * rowCount);
    this.colCount = colCount;
    this.rowCount = rowCount;
  }

  calcFlatIndex(colIndex: number, rowIndex: number) {
    return rowIndex * this.colCount + colIndex;
  }

  get(colIndex: number, rowIndex: number) {
    const index = this.calcFlatIndex(colIndex, rowIndex);
    return this.data[index];
  }

  getFlat(index: number) {
    return this.data[index];
  }

  *getColumnGenerator(colIndex: number) {
    for (let rowIndex = 0; rowIndex < this.rowCount; rowIndex++) {
      yield this.get(colIndex, rowIndex);
    }
  }

  *getRowGenerator(rowIndex: number) {
    for (let colIndex = 0; colIndex < this.colCount; colIndex++) {
      yield this.get(colIndex, rowIndex);
    }
  }
}
